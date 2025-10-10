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
    /// Removes every non-volatile instruction whose effects are ignored or overwritten.
    /// </summary>
    public class DeadCodeEliminationPass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();
        private readonly Dictionary<BasicBlock, CachedTracer> _tracerCache = new Dictionary<BasicBlock, CachedTracer>();

        /// <summary>
        /// Gets the execution order for this pass (parallel depth-first).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.ParallelDepthFirst;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "Dead Code Elimination";

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                if (!_tracerCache.TryGetValue(block, out var tracer))
                {
                    tracer = new CachedTracer();
                    _tracerCache[block] = tracer;
                }

                return EliminateDeadCode(block, tracer, crossBlock);
            }
        }

        /// <summary>
        /// Eliminates dead code from a basic block.
        /// </summary>
        private int EliminateDeadCode(BasicBlock block, CachedTracer tracer, bool crossBlock)
        {
            var removedCount = 0;
            var instructionsToRemove = new List<int>();

            // Analyze each instruction to determine if it's dead
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);

                if (IsDeadInstruction(instruction, block, i, tracer, crossBlock))
                {
                    instructionsToRemove.Add(i);
                }
            }

            // Remove dead instructions in reverse order to maintain indices
            for (int i = instructionsToRemove.Count - 1; i >= 0; i--)
            {
                block.RemoveInstruction(instructionsToRemove[i]);
                removedCount++;
            }

            return removedCount;
        }

        /// <summary>
        /// Determines if an instruction is dead (its result is never used).
        /// </summary>
        private bool IsDeadInstruction(Instruction instruction, BasicBlock block, int instructionIndex, CachedTracer tracer, bool crossBlock)
        {
            // Never remove volatile instructions
            if (instruction.IsVolatile)
                return false;

            // Never remove branching instructions
            if (instruction.Descriptor.IsBranching)
                return false;

            // Never remove VM exit instructions
            if (instruction.Descriptor == InstructionSet.VmExit)
                return false;

            // Check if the instruction has any side effects
            if (HasSideEffects(instruction))
            {
                // Only remove if we can prove the side effects are not observable
                return !HasObservableSideEffects(instruction, block, instructionIndex, tracer, crossBlock);
            }

            // For instructions that write to registers/memory, check if the written value is used
            if (instruction.OperandCount > 0)
            {
                var destination = instruction.GetOperand0();
                if (destination.IsWritable)
                {
                    return !IsValueUsed(destination, block, instructionIndex, tracer, crossBlock);
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if an instruction has side effects.
        /// </summary>
        private bool HasSideEffects(Instruction instruction)
        {
            // Instructions that modify memory or have other side effects
            return instruction.Descriptor.AccessesMemory ||
                   instruction.Descriptor.IsVolatile ||
                   instruction.Descriptor == InstructionSet.VmExit;
        }

        /// <summary>
        /// Checks if an instruction has observable side effects.
        /// </summary>
        private bool HasObservableSideEffects(Instruction instruction, BasicBlock block, int instructionIndex, CachedTracer tracer, bool crossBlock)
        {
            // For now, we'll be conservative and assume all side effects are observable
            // In a more sophisticated implementation, we would analyze the control flow
            // and memory dependencies to determine if side effects are actually observable
            return true;
        }

        /// <summary>
        /// Checks if a value written by an instruction is used later.
        /// </summary>
        private bool IsValueUsed(Operand destination, BasicBlock block, int instructionIndex, CachedTracer tracer, bool crossBlock)
        {
            // Check if the value is used in subsequent instructions in the same block
            for (int i = instructionIndex + 1; i < block.InstructionCount; i++)
            {
                var laterInstruction = block.GetInstruction(i);
                if (IsValueReferenced(destination, laterInstruction))
                    return true;
            }

            // If cross-block analysis is enabled, check successor blocks
            if (crossBlock)
            {
                foreach (var successor in block.Successors)
                {
                    if (IsValueUsedInBlock(destination, successor, tracer))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a value is referenced by an instruction.
        /// </summary>
        private bool IsValueReferenced(Operand value, Instruction instruction)
        {
            for (int i = 0; i < instruction.OperandCount; i++)
            {
                var operand = instruction.GetOperand(i);
                if (AreOperandsRelated(value, operand))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if two operands are related (e.g., same register, overlapping memory).
        /// </summary>
        private bool AreOperandsRelated(Operand op1, Operand op2)
        {
            if (op1.IsRegister && op2.IsRegister)
            {
                var reg1 = op1.GetRegister();
                var reg2 = op2.GetRegister();
                return reg1.Equals(reg2);
            }

            if (op1.IsMemory && op2.IsMemory)
            {
                // For now, assume all memory references might overlap
                // In a more sophisticated implementation, we would analyze memory addresses
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a value is used in a block (recursive analysis).
        /// </summary>
        private bool IsValueUsedInBlock(Operand value, BasicBlock block, CachedTracer tracer)
        {
            // Use cached tracer to avoid redundant analysis
            return tracer.IsValueUsedInBlock(value, block);
        }
    }

    /// <summary>
    /// Cached tracer for dead code elimination analysis.
    /// </summary>
    public class CachedTracer
    {
        private readonly Dictionary<(Operand, BasicBlock), bool> _usageCache = new Dictionary<(Operand, BasicBlock), bool>();

        /// <summary>
        /// Checks if a value is used in a block with caching.
        /// </summary>
        public bool IsValueUsedInBlock(Operand value, BasicBlock block)
        {
            var key = (value, block);
            if (_usageCache.TryGetValue(key, out var result))
                return result;

            result = AnalyzeValueUsage(value, block);
            _usageCache[key] = result;
            return result;
        }

        /// <summary>
        /// Analyzes if a value is used in a block.
        /// </summary>
        private bool AnalyzeValueUsage(Operand value, BasicBlock block)
        {
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                for (int j = 0; j < instruction.OperandCount; j++)
                {
                    var operand = instruction.GetOperand(j);
                    if (AreOperandsRelated(value, operand))
                        return true;
                }
            }

            // Check successor blocks
            foreach (var successor in block.Successors)
            {
                if (IsValueUsedInBlock(value, successor))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if two operands are related.
        /// </summary>
        private bool AreOperandsRelated(Operand op1, Operand op2)
        {
            if (op1.IsRegister && op2.IsRegister)
            {
                var reg1 = op1.GetRegister();
                var reg2 = op2.GetRegister();
                return reg1.Equals(reg2);
            }

            if (op1.IsMemory && op2.IsMemory)
            {
                // Conservative assumption: all memory references might overlap
                return true;
            }

            return false;
        }
    }
}
