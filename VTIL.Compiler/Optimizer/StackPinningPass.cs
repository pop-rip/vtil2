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

namespace VTIL.Compiler.Optimizer
{
    /// <summary>
    /// Pins stack operations to improve performance and enable further optimizations.
    /// </summary>
    public class StackPinningPass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets the execution order for this pass (serial).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Serial;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "Stack Pinning";

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return PinStackOperations(block, crossBlock);
            }
        }

        /// <summary>
        /// Pins stack operations in a basic block.
        /// </summary>
        private int PinStackOperations(BasicBlock block, bool crossBlock)
        {
            var pinnedCount = 0;
            var stackOperations = new List<StackOperationInfo>();

            // First pass: collect stack operations
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var stackOp = AnalyzeStackOperation(instruction, i);

                if (stackOp != null)
                {
                    stackOperations.Add(stackOp);
                }
            }

            // Second pass: optimize stack operations
            foreach (var stackOp in stackOperations)
            {
                var optimized = OptimizeStackOperation(stackOp, block);
                if (optimized > 0)
                {
                    pinnedCount += optimized;
                }
            }

            return pinnedCount;
        }

        /// <summary>
        /// Analyzes a stack operation instruction.
        /// </summary>
        private StackOperationInfo AnalyzeStackOperation(Instruction instruction, int index)
        {
            switch (instruction.Descriptor)
            {
                case var desc when desc == InstructionSet.Push:
                    return new StackOperationInfo
                    {
                        Type = StackOperationType.Push,
                        Instruction = instruction,
                        Index = index,
                        Value = instruction.OperandCount > 0 ? instruction.GetOperand0() : null
                    };

                case var desc when desc == InstructionSet.Pop:
                    return new StackOperationInfo
                    {
                        Type = StackOperationType.Pop,
                        Instruction = instruction,
                        Index = index,
                        Destination = instruction.OperandCount > 0 ? instruction.GetOperand0() : null
                    };

                case var desc when desc == InstructionSet.Write && IsStackStore(instruction):
                    return new StackOperationInfo
                    {
                        Type = StackOperationType.Store,
                        Instruction = instruction,
                        Index = index,
                        Address = instruction.OperandCount > 0 ? instruction.GetOperand0() : null,
                        Value = instruction.OperandCount > 1 ? instruction.GetOperand1() : null
                    };

                case var desc when desc == InstructionSet.Read && IsStackLoad(instruction):
                    return new StackOperationInfo
                    {
                        Type = StackOperationType.Load,
                        Instruction = instruction,
                        Index = index,
                        Address = instruction.OperandCount > 0 ? instruction.GetOperand0() : null,
                        Destination = instruction.OperandCount > 1 ? instruction.GetOperand1() : null
                    };

                default:
                    return null;
            }
        }

        /// <summary>
        /// Checks if a store instruction is a stack store.
        /// </summary>
        private bool IsStackStore(Instruction instruction)
        {
            if (instruction.OperandCount == 0)
                return false;

            var address = instruction.GetOperand0();
            return IsStackAddress(address);
        }

        /// <summary>
        /// Checks if a load instruction is a stack load.
        /// </summary>
        private bool IsStackLoad(Instruction instruction)
        {
            if (instruction.OperandCount == 0)
                return false;

            var address = instruction.GetOperand0();
            return IsStackAddress(address);
        }

        /// <summary>
        /// Checks if an address refers to the stack.
        /// </summary>
        private bool IsStackAddress(Operand address)
        {
            // This would need more sophisticated analysis to determine
            // if an address refers to the stack vs. other memory regions
            return address.IsMemory;
        }

        /// <summary>
        /// Optimizes a stack operation.
        /// </summary>
        private int OptimizeStackOperation(StackOperationInfo stackOp, BasicBlock block)
        {
            switch (stackOp.Type)
            {
                case StackOperationType.Alloc:
                    return OptimizeStackAlloc(stackOp, block);

                case StackOperationType.Free:
                    return OptimizeStackFree(stackOp, block);

                case StackOperationType.Push:
                    return OptimizePush(stackOp, block);

                case StackOperationType.Pop:
                    return OptimizePop(stackOp, block);

                case StackOperationType.Store:
                    return OptimizeStackStore(stackOp, block);

                case StackOperationType.Load:
                    return OptimizeStackLoad(stackOp, block);

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Optimizes a stack allocation.
        /// </summary>
        private int OptimizeStackAlloc(StackOperationInfo allocOp, BasicBlock block)
        {
            // Check if we can combine with subsequent allocations
            var nextAlloc = FindNextStackOperation(block, allocOp.Index + 1, StackOperationType.Alloc);
            if (nextAlloc != null && CanCombineAllocations(allocOp, nextAlloc))
            {
                return CombineAllocations(allocOp, nextAlloc, block);
            }

            return 0;
        }

        /// <summary>
        /// Optimizes a stack free.
        /// </summary>
        private int OptimizeStackFree(StackOperationInfo freeOp, BasicBlock block)
        {
            // Check if we can combine with previous frees
            var prevFree = FindPreviousStackOperation(block, freeOp.Index - 1, StackOperationType.Free);
            if (prevFree != null && CanCombineFrees(freeOp, prevFree))
            {
                return CombineFrees(prevFree, freeOp, block);
            }

            return 0;
        }

        /// <summary>
        /// Optimizes a push operation.
        /// </summary>
        private int OptimizePush(StackOperationInfo pushOp, BasicBlock block)
        {
            // Check if we can eliminate redundant pushes
            var nextPop = FindNextStackOperation(block, pushOp.Index + 1, StackOperationType.Pop);
            if (nextPop != null && CanEliminatePushPop(pushOp, nextPop))
            {
                return EliminatePushPop(pushOp, nextPop, block);
            }

            return 0;
        }

        /// <summary>
        /// Optimizes a pop operation.
        /// </summary>
        private int OptimizePop(StackOperationInfo popOp, BasicBlock block)
        {
            // Check if we can eliminate redundant pops
            var prevPush = FindPreviousStackOperation(block, popOp.Index - 1, StackOperationType.Push);
            if (prevPush != null && CanEliminatePushPop(prevPush, popOp))
            {
                return EliminatePushPop(prevPush, popOp, block);
            }

            return 0;
        }

        /// <summary>
        /// Optimizes a stack store.
        /// </summary>
        private int OptimizeStackStore(StackOperationInfo storeOp, BasicBlock block)
        {
            // Check if we can eliminate redundant stores
            var nextStore = FindNextStackStore(block, storeOp.Index + 1, storeOp.Address.Value);
            if (nextStore != null && CanEliminateRedundantStore(storeOp, nextStore))
            {
                return EliminateRedundantStore(storeOp, block);
            }

            return 0;
        }

        /// <summary>
        /// Optimizes a stack load.
        /// </summary>
        private int OptimizeStackLoad(StackOperationInfo loadOp, BasicBlock block)
        {
            // Check if we can forward a recent store
            var recentStore = FindRecentStackStore(block, loadOp.Index - 1, loadOp.Address.Value);
            if (recentStore != null && CanForwardStore(recentStore, loadOp))
            {
                return ForwardStore(recentStore, loadOp, block);
            }

            return 0;
        }

        /// <summary>
        /// Finds the next stack operation of a specific type.
        /// </summary>
        private StackOperationInfo? FindNextStackOperation(BasicBlock block, int startIndex, StackOperationType type)
        {
            for (int i = startIndex; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var stackOp = AnalyzeStackOperation(instruction, i);

                if (stackOp != null && stackOp.Type == type)
                    return stackOp;
            }

            return null;
        }

        /// <summary>
        /// Finds the previous stack operation of a specific type.
        /// </summary>
        private StackOperationInfo? FindPreviousStackOperation(BasicBlock block, int startIndex, StackOperationType type)
        {
            for (int i = startIndex; i >= 0; i--)
            {
                var instruction = block.GetInstruction(i);
                var stackOp = AnalyzeStackOperation(instruction, i);

                if (stackOp != null && stackOp.Type == type)
                    return stackOp;
            }

            return null;
        }

        /// <summary>
        /// Finds the next stack store to the same address.
        /// </summary>
        private StackOperationInfo? FindNextStackStore(BasicBlock block, int startIndex, Operand address)
        {
            if (address == null)
                return null;

            for (int i = startIndex; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var stackOp = AnalyzeStackOperation(instruction, i);

                if (stackOp != null && stackOp.Type == StackOperationType.Store && 
                    AreOperandsEqual(stackOp.Address.Value, address))
                    return stackOp;
            }

            return null;
        }

        /// <summary>
        /// Finds the most recent stack store to the same address.
        /// </summary>
        private StackOperationInfo? FindRecentStackStore(BasicBlock block, int startIndex, Operand address)
        {
            if (address == null)
                return null;

            for (int i = startIndex; i >= 0; i--)
            {
                var instruction = block.GetInstruction(i);
                var stackOp = AnalyzeStackOperation(instruction, i);

                if (stackOp != null && stackOp.Type == StackOperationType.Store && 
                    AreOperandsEqual(stackOp.Address.Value, address))
                    return stackOp;
            }

            return null;
        }

        // Optimization helper methods
        private bool CanCombineAllocations(StackOperationInfo alloc1, StackOperationInfo alloc2) => false;
        private int CombineAllocations(StackOperationInfo alloc1, StackOperationInfo alloc2, BasicBlock block) => 0;
        private bool CanCombineFrees(StackOperationInfo free1, StackOperationInfo free2) => false;
        private int CombineFrees(StackOperationInfo free1, StackOperationInfo free2, BasicBlock block) => 0;
        private bool CanEliminatePushPop(StackOperationInfo push, StackOperationInfo pop) => false;
        private int EliminatePushPop(StackOperationInfo push, StackOperationInfo pop, BasicBlock block) => 0;
        private bool CanEliminateRedundantStore(StackOperationInfo store1, StackOperationInfo store2) => false;
        private int EliminateRedundantStore(StackOperationInfo store, BasicBlock block) => 0;
        private bool CanForwardStore(StackOperationInfo store, StackOperationInfo load) => false;
        private int ForwardStore(StackOperationInfo store, StackOperationInfo load, BasicBlock block) => 0;

        /// <summary>
        /// Checks if two operands are equal.
        /// </summary>
        private bool AreOperandsEqual(Operand op1, Operand op2)
        {
            if (op1 == null || op2 == null)
                return op1 == op2;

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
    /// Information about a stack operation.
    /// </summary>
    public class StackOperationInfo
    {
        public StackOperationType Type { get; set; }
        public Instruction Instruction { get; set; } = null!;
        public int Index { get; set; }
        public Operand? Size { get; set; }
        public Operand? Value { get; set; }
        public Operand? Destination { get; set; }
        public Operand? Address { get; set; }
    }

    /// <summary>
    /// Types of stack operations.
    /// </summary>
    public enum StackOperationType
    {
        Alloc,
        Free,
        Push,
        Pop,
        Store,
        Load
    }
}
