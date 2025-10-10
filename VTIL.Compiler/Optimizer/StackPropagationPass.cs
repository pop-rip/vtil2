/*
 * Copyright (c) 2020 pop-rip and the contributors of the VTIL2 Project   
 * All rights reserved.   
 *    
 * Redistribution and use in source and binary forms, with or without   
 * modification, are permitted provided that the following conditions are met: 
 *    
 * 1. Redistributions of source code must retain the above copyright notice,   
 *    this list of conditions and the following disclaimer.   
 * 2. Redistributions in binary form must reproduce the above copyright   
 *    notice, this list of conditions and the following disclaimer in the   
 *    documentation and/or other materials provided with the distribution.   
 * 3. Neither the name of VTIL Project nor the names of its contributors
 *    may be used to endorse or promote products derived from this software 
 *    without specific prior written permission.   
 *    
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE   
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE  
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE   
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR   
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF   
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS   
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN   
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)   
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE  
 * POSSIBILITY OF SUCH DAMAGE.        
 */

using System;
using System.Collections.Generic;
using System.Linq;
using VTIL.Architecture;
using VTIL.SymEx;

namespace VTIL.Compiler.Optimizer
{
    /// <summary>
    /// Propagates stack operations and optimizes stack usage.
    /// </summary>
    public class StackPropagationPass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets the execution order for this pass (parallel).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Parallel;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "Stack Propagation";

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return PropagateStackOperations(block, crossBlock);
            }
        }

        /// <summary>
        /// Propagates stack operations and optimizes stack usage.
        /// </summary>
        private int PropagateStackOperations(BasicBlock block, bool crossBlock)
        {
            var optimizedCount = 0;

            // Track stack operations and their effects
            var stackOperations = new StackOperationTracker();

            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var optimizations = ProcessInstruction(instruction, stackOperations, block, i);

                if (optimizations > 0)
                {
                    optimizedCount += optimizations;
                    // Re-analyze from the beginning if we made changes
                    i = -1; // Will be incremented to 0 in the next iteration
                    stackOperations.Clear();
                }
            }

            return optimizedCount;
        }

        /// <summary>
        /// Processes a single instruction for stack optimizations.
        /// </summary>
        private int ProcessInstruction(Instruction instruction, StackOperationTracker tracker, BasicBlock block, int instructionIndex)
        {
            var optimizations = 0;

            switch (instruction.Descriptor)
            {
                case var desc when desc == InstructionSet.Push:
                    optimizations += ProcessPush(instruction, tracker, block, instructionIndex);
                    break;

                case var desc when desc == InstructionSet.Pop:
                    optimizations += ProcessPop(instruction, tracker, block, instructionIndex);
                    break;

                case var desc when desc == InstructionSet.Write:
                    optimizations += ProcessStore(instruction, tracker, block, instructionIndex);
                    break;

                case var desc when desc == InstructionSet.Read:
                    optimizations += ProcessLoad(instruction, tracker, block, instructionIndex);
                    break;
            }

            return optimizations;
        }

        /// <summary>
        /// Processes a PUSH instruction.
        /// </summary>
        private int ProcessPush(Instruction pushInstruction, StackOperationTracker tracker, BasicBlock block, int instructionIndex)
        {
            var optimizations = 0;
            var value = pushInstruction.GetOperand0();

            // Check if we can eliminate redundant pushes
            if (tracker.CanEliminatePush(value))
            {
                block.RemoveInstruction(instructionIndex);
                return 1;
            }

            // Check if we can combine with subsequent operations
            var nextInstruction = GetNextInstruction(block, instructionIndex);
            if (nextInstruction != null && CanCombinePushWithNext(pushInstruction, nextInstruction))
            {
                optimizations += CombinePushWithNext(pushInstruction, nextInstruction, block, instructionIndex);
            }

            tracker.RecordPush(value);
            return optimizations;
        }

        /// <summary>
        /// Processes a POP instruction.
        /// </summary>
        private int ProcessPop(Instruction popInstruction, StackOperationTracker tracker, BasicBlock block, int instructionIndex)
        {
            var optimizations = 0;
            var destination = popInstruction.GetOperand0();

            // Check if we can eliminate redundant pops
            if (tracker.CanEliminatePop(destination))
            {
                block.RemoveInstruction(instructionIndex);
                return 1;
            }

            // Check if we can forward the popped value
            var poppedValue = tracker.GetTopValue();
            if (poppedValue != null && CanForwardValue(poppedValue, destination))
            {
                optimizations += ForwardValue(poppedValue, destination, block, instructionIndex);
            }

            tracker.RecordPop(destination);
            return optimizations;
        }

        /// <summary>
        /// Processes a STACK_ALLOC instruction.
        /// </summary>
        private int ProcessStackAlloc(Instruction allocInstruction, StackOperationTracker tracker, BasicBlock block, int instructionIndex)
        {
            var size = allocInstruction.GetOperand0();
            tracker.RecordStackAlloc(size);
            return 0;
        }

        /// <summary>
        /// Processes a STACK_FREE instruction.
        /// </summary>
        private int ProcessStackFree(Instruction freeInstruction, StackOperationTracker tracker, BasicBlock block, int instructionIndex)
        {
            var size = freeInstruction.GetOperand0();
            tracker.RecordStackFree(size);
            return 0;
        }

        /// <summary>
        /// Processes a STORE instruction.
        /// </summary>
        private int ProcessStore(Instruction storeInstruction, StackOperationTracker tracker, BasicBlock block, int instructionIndex)
        {
            var optimizations = 0;
            var address = storeInstruction.GetOperand0();
            var value = storeInstruction.GetOperand1();

            // Check if we can optimize stack stores
            if (IsStackAddress(address) && tracker.CanOptimizeStackStore(address, value))
            {
                optimizations += OptimizeStackStore(storeInstruction, tracker, block, instructionIndex);
            }

            tracker.RecordStore(address, value);
            return optimizations;
        }

        /// <summary>
        /// Processes a LOAD instruction.
        /// </summary>
        private int ProcessLoad(Instruction loadInstruction, StackOperationTracker tracker, BasicBlock block, int instructionIndex)
        {
            var optimizations = 0;
            var address = loadInstruction.GetOperand0();
            var destination = loadInstruction.GetOperand1();

            // Check if we can optimize stack loads
            if (IsStackAddress(address) && tracker.CanOptimizeStackLoad(address))
            {
                var value = tracker.GetStackValue(address);
                if (value != null)
                {
                    optimizations += ReplaceLoadWithValue(loadInstruction, value.Value, block, instructionIndex);
                }
            }

            return optimizations;
        }

        /// <summary>
        /// Gets the next instruction in the block.
        /// </summary>
        private Instruction? GetNextInstruction(BasicBlock block, int currentIndex)
        {
            var nextIndex = currentIndex + 1;
            return nextIndex < block.InstructionCount ? block.GetInstruction(nextIndex) : null;
        }

        /// <summary>
        /// Checks if a PUSH can be combined with the next instruction.
        /// </summary>
        private bool CanCombinePushWithNext(Instruction pushInstruction, Instruction nextInstruction)
        {
            // Check for push-pop pairs that can be eliminated
            if (nextInstruction.Descriptor == InstructionSet.Pop)
            {
                var pushedValue = pushInstruction.GetOperand0();
                var poppedDestination = nextInstruction.GetOperand0();
                
                // If we're pushing and immediately popping to the same location, we can eliminate both
                return AreOperandsEqual(pushedValue, poppedDestination);
            }

            return false;
        }

        /// <summary>
        /// Combines a PUSH with the next instruction.
        /// </summary>
        private int CombinePushWithNext(Instruction pushInstruction, Instruction nextInstruction, BasicBlock block, int pushIndex)
        {
            // Remove both push and pop instructions
            block.RemoveInstruction(pushIndex + 1); // Remove pop first (higher index)
            block.RemoveInstruction(pushIndex);     // Then remove push
            return 2;
        }

        /// <summary>
        /// Checks if a value can be forwarded to a destination.
        /// </summary>
        private bool CanForwardValue(Operand value, Operand destination)
        {
            // Check if the types are compatible
            return value.SizeInBits == destination.SizeInBits;
        }

        /// <summary>
        /// Forwards a value to a destination.
        /// </summary>
        private int ForwardValue(Operand value, Operand destination, BasicBlock block, int instructionIndex)
        {
            // Replace the pop with a mov from the tracked value
            var movInstruction = Instruction.CreateMov(destination, value, value.SizeInBits);
            block.ReplaceInstruction(instructionIndex, movInstruction);
            return 1;
        }

        /// <summary>
        /// Checks if an address refers to the stack.
        /// </summary>
        private bool IsStackAddress(Operand address)
        {
            // This would need more sophisticated analysis to determine if an address
            // refers to the stack vs. other memory regions
            return address.IsMemory;
        }

        /// <summary>
        /// Optimizes a stack store operation.
        /// </summary>
        private int OptimizeStackStore(Instruction storeInstruction, StackOperationTracker tracker, BasicBlock block, int instructionIndex)
        {
            // Implementation would depend on specific stack optimization strategies
            return 0;
        }

        /// <summary>
        /// Replaces a load instruction with a direct value.
        /// </summary>
        private int ReplaceLoadWithValue(Instruction loadInstruction, Operand value, BasicBlock block, int instructionIndex)
        {
            var destination = loadInstruction.GetOperand1();
            var movInstruction = Instruction.CreateMov(destination, value, value.SizeInBits);
            block.ReplaceInstruction(instructionIndex, movInstruction);
            return 1;
        }

        /// <summary>
        /// Checks if two operands are equal.
        /// </summary>
        private bool AreOperandsEqual(Operand op1, Operand op2)
        {
            if (op1.IsRegister && op2.IsRegister)
            {
                var reg1 = op1.GetRegister();
                var reg2 = op2.GetRegister();
                return reg1.Equals(reg2);
            }

            if (op1.IsImmediate && op2.IsImmediate)
            {
                var imm1 = op1.GetImmediate();
                var imm2 = op2.GetImmediate();
                return imm1 == imm2;
            }

            return false;
        }
    }

    /// <summary>
    /// Tracks stack operations for optimization.
    /// </summary>
    public class StackOperationTracker
    {
        private readonly Stack<Operand> _stackValues = new Stack<Operand>();
        private readonly Dictionary<Operand, Operand> _stackMemory = new Dictionary<Operand, Operand>();
        private long _stackOffset = 0;

        /// <summary>
        /// Records a PUSH operation.
        /// </summary>
        public void RecordPush(Operand value)
        {
            _stackValues.Push(value);
        }

        /// <summary>
        /// Records a POP operation.
        /// </summary>
        public void RecordPop(Operand destination)
        {
            if (_stackValues.Count > 0)
            {
                _stackValues.Pop();
            }
        }

        /// <summary>
        /// Records a STACK_ALLOC operation.
        /// </summary>
        public void RecordStackAlloc(Operand size)
        {
            if (size.IsImmediate)
            {
                _stackOffset += (long)size.GetImmediate();
            }
        }

        /// <summary>
        /// Records a STACK_FREE operation.
        /// </summary>
        public void RecordStackFree(Operand size)
        {
            if (size.IsImmediate)
            {
                _stackOffset -= (long)size.GetImmediate();
            }
        }

        /// <summary>
        /// Records a STORE operation.
        /// </summary>
        public void RecordStore(Operand address, Operand value)
        {
            _stackMemory[address] = value;
        }

        /// <summary>
        /// Checks if a PUSH can be eliminated.
        /// </summary>
        public bool CanEliminatePush(Operand value)
        {
            // Check if we're pushing the same value that's already on top
            return _stackValues.Count > 0 && AreOperandsEqual(_stackValues.Peek(), value);
        }

        /// <summary>
        /// Checks if a POP can be eliminated.
        /// </summary>
        public bool CanEliminatePop(Operand destination)
        {
            // Check if we're popping to a location that already has the correct value
            return false; // Simplified implementation
        }

        /// <summary>
        /// Gets the value on top of the stack.
        /// </summary>
        public Operand GetTopValue()
        {
            return _stackValues.Peek();
        }

        /// <summary>
        /// Gets a value from stack memory.
        /// </summary>
        public Operand? GetStackValue(Operand address)
        {
            return _stackMemory.TryGetValue(address, out var value) ? value : null;
        }

        /// <summary>
        /// Checks if a stack store can be optimized.
        /// </summary>
        public bool CanOptimizeStackStore(Operand address, Operand value)
        {
            // Implementation would depend on specific optimization strategies
            return false;
        }

        /// <summary>
        /// Checks if a stack load can be optimized.
        /// </summary>
        public bool CanOptimizeStackLoad(Operand address)
        {
            return _stackMemory.ContainsKey(address);
        }

        /// <summary>
        /// Clears the tracker state.
        /// </summary>
        public void Clear()
        {
            _stackValues.Clear();
            _stackMemory.Clear();
            _stackOffset = 0;
        }

        /// <summary>
        /// Checks if two operands are equal.
        /// </summary>
        private bool AreOperandsEqual(Operand op1, Operand op2)
        {
            if (op1.IsRegister && op2.IsRegister)
            {
                var reg1 = op1.GetRegister();
                var reg2 = op2.GetRegister();
                return reg1.Equals(reg2);
            }

            if (op1.IsImmediate && op2.IsImmediate)
            {
                var imm1 = op1.GetImmediate();
                var imm2 = op2.GetImmediate();
                return imm1 == imm2;
            }

            return false;
        }
    }
}
