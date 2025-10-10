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
    /// Extends basic blocks by merging with their successors where beneficial.
    /// </summary>
    public class BasicBlockExtensionPass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets the execution order for this pass (serial).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Serial;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "Basic Block Extension";

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return ExtendBasicBlock(block, crossBlock);
            }
        }

        /// <summary>
        /// Extends a basic block by merging with successors where beneficial.
        /// </summary>
        private int ExtendBasicBlock(BasicBlock block, bool crossBlock)
        {
            var extendedCount = 0;

            // Check if this block can be extended
            if (CanExtendBlock(block))
            {
                var successorsToMerge = GetMergeableSuccessors(block);
                
                foreach (var successor in successorsToMerge)
                {
                    if (ShouldMergeBlocks(block, successor))
                    {
                        MergeBlocks(block, successor);
                        extendedCount++;
                    }
                }
            }

            return extendedCount;
        }

        /// <summary>
        /// Checks if a basic block can be extended.
        /// </summary>
        private bool CanExtendBlock(BasicBlock block)
        {
            // Don't extend blocks that are too large
            if (block.InstructionCount > 1000)
                return false;

            // Don't extend blocks with complex control flow
            if (block.Successors.Count > 2)
                return false;

            // Don't extend blocks that are entry points (unless it's safe)
            if (block.IsEntry && block.Predecessors.Count > 0)
                return false;

            return true;
        }

        /// <summary>
        /// Gets successors that can be merged with the current block.
        /// </summary>
        private List<BasicBlock> GetMergeableSuccessors(BasicBlock block)
        {
            var mergeable = new List<BasicBlock>();

            foreach (var successor in block.Successors)
            {
                if (CanMergeWithSuccessor(block, successor))
                {
                    mergeable.Add(successor);
                }
            }

            return mergeable;
        }

        /// <summary>
        /// Checks if a block can be merged with its successor.
        /// </summary>
        private bool CanMergeWithSuccessor(BasicBlock block, BasicBlock successor)
        {
            // Can't merge if successor has multiple predecessors
            if (successor.Predecessors.Count > 1)
                return false;

            // Can't merge if successor is too large
            if (successor.InstructionCount > 500)
                return false;

            // Can't merge if successor has complex control flow
            if (successor.Successors.Count > 2)
                return false;

            // Can't merge if there are register conflicts
            if (HasRegisterConflicts(block, successor))
                return false;

            return true;
        }

        /// <summary>
        /// Checks if merging two blocks would cause register conflicts.
        /// </summary>
        private bool HasRegisterConflicts(BasicBlock block1, BasicBlock block2)
        {
            var block1Registers = GetUsedRegisters(block1);
            var block2Registers = GetUsedRegisters(block2);

            // Check for overlapping register usage that could cause conflicts
            return block1Registers.Intersect(block2Registers).Any();
        }

        /// <summary>
        /// Gets all registers used in a basic block.
        /// </summary>
        private HashSet<RegisterDescriptor> GetUsedRegisters(BasicBlock block)
        {
            var registers = new HashSet<RegisterDescriptor>();

            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);

                for (int j = 0; j < instruction.OperandCount; j++)
                {
                    var operand = instruction.GetOperand(j);
                    
                    if (operand.IsRegister)
                    {
                        registers.Add(operand.GetRegister());
                    }
                }
            }

            return registers;
        }

        /// <summary>
        /// Determines if two blocks should be merged.
        /// </summary>
        private bool ShouldMergeBlocks(BasicBlock block1, BasicBlock block2)
        {
            // Merge if the total instruction count is reasonable
            if (block1.InstructionCount + block2.InstructionCount > 2000)
                return false;

            // Merge if it eliminates a jump instruction
            if (HasUnconditionalJumpToSuccessor(block1, block2))
                return true;

            // Merge if it improves locality
            if (ImprovesLocality(block1, block2))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if block1 has an unconditional jump to block2.
        /// </summary>
        private bool HasUnconditionalJumpToSuccessor(BasicBlock block1, BasicBlock block2)
        {
            if (block1.InstructionCount == 0)
                return false;

            var lastInstruction = block1.GetInstruction(block1.InstructionCount - 1);
            
            if (lastInstruction.Descriptor == InstructionSet.Jmp)
            {
                // This would need more sophisticated analysis to determine
                // if the jump target matches block2's entry point
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if merging blocks would improve locality.
        /// </summary>
        private bool ImprovesLocality(BasicBlock block1, BasicBlock block2)
        {
            // This could involve analyzing memory access patterns,
            // register usage patterns, etc.
            return block1.InstructionCount < 100 && block2.InstructionCount < 100;
        }

        /// <summary>
        /// Merges two basic blocks.
        /// </summary>
        private void MergeBlocks(BasicBlock block1, BasicBlock block2)
        {
            // Remove the jump instruction from block1 if it exists
            if (block1.InstructionCount > 0)
            {
                var lastInstruction = block1.GetInstruction(block1.InstructionCount - 1);
                if (lastInstruction.Descriptor == InstructionSet.Jmp)
                {
                    block1.RemoveInstruction(block1.InstructionCount - 1);
                }
            }

            // Move all instructions from block2 to block1
            var instructionsToMove = new List<Instruction>();
            for (int i = 0; i < block2.InstructionCount; i++)
            {
                instructionsToMove.Add(block2.GetInstruction(i));
            }

            foreach (var instruction in instructionsToMove)
            {
                block1.AddInstruction(instruction);
            }

            // Update control flow
            block1.Successors.Remove(block2);
            block1.Successors.AddRange(block2.Successors);

            // Update predecessor relationships
            foreach (var successor in block2.Successors)
            {
                successor.Predecessors.Remove(block2);
                successor.Predecessors.Add(block1);
            }

            // Remove block2 from the routine
            block1.Routine.RemoveBlock(block2);
        }
    }
}
