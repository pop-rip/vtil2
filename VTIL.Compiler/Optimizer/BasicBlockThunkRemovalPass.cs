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
    /// Removes thunk basic blocks that only contain jumps to other blocks.
    /// </summary>
    public class BasicBlockThunkRemovalPass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets the execution order for this pass (serial).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Serial;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "Basic Block Thunk Removal";

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return RemoveThunks(block, crossBlock);
            }
        }

        /// <summary>
        /// Removes thunk blocks in a routine.
        /// </summary>
        private int RemoveThunks(BasicBlock block, bool crossBlock)
        {
            var removedCount = 0;

            // Find thunk blocks (blocks that only contain jumps)
            var thunkBlocks = FindThunkBlocks(block.Routine);

            foreach (var thunk in thunkBlocks)
            {
                if (CanRemoveThunk(thunk))
                {
                    RemoveThunk(thunk);
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// <summary>
        /// Finds all thunk blocks in a routine.
        /// </summary>
        private List<BasicBlock> FindThunkBlocks(Routine routine)
        {
            var thunks = new List<BasicBlock>();

            foreach (var block in routine.ExploredBlocks.Values)
            {
                if (IsThunkBlock(block))
                {
                    thunks.Add(block);
                }
            }

            return thunks;
        }

        /// <summary>
        /// Checks if a block is a thunk (only contains jumps).
        /// </summary>
        private bool IsThunkBlock(BasicBlock block)
        {
            // A thunk block should have minimal instructions
            if (block.InstructionCount > 3)
                return false;

            // Check if all instructions are jumps
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                if (!IsJumpInstruction(instruction))
                    return false;
            }

            // Should have exactly one successor
            if (block.Successors.Count != 1)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if an instruction is a jump instruction.
        /// </summary>
        private bool IsJumpInstruction(Instruction instruction)
        {
            return instruction.Descriptor == InstructionSet.Jmp ||
                   instruction.Descriptor == InstructionSet.Jcc ||
                   instruction.Descriptor == InstructionSet.Ret;
        }

        /// <summary>
        /// Checks if a thunk can be safely removed.
        /// </summary>
        private bool CanRemoveThunk(BasicBlock thunk)
        {
            // Can't remove entry or exit blocks
            if (thunk.IsEntry || thunk.IsExit)
                return false;

            // Can't remove blocks with complex control flow
            if (thunk.Predecessors.Count > 10)
                return false;

            // Can't remove blocks that are targets of indirect jumps
            if (IsTargetOfIndirectJump(thunk))
                return false;

            return true;
        }

        /// <summary>
        /// Checks if a block is the target of an indirect jump.
        /// </summary>
        private bool IsTargetOfIndirectJump(BasicBlock block)
        {
            // This would need more sophisticated analysis to determine
            // if a block is targeted by indirect jumps
            return false;
        }

        /// <summary>
        /// Removes a thunk block and updates control flow.
        /// </summary>
        private void RemoveThunk(BasicBlock thunk)
        {
            var target = thunk.Successors.FirstOrDefault();
            if (target == null)
                return;

            // Update all predecessors to point directly to the target
            foreach (var predecessor in thunk.Predecessors.ToList())
            {
                // Remove the thunk from predecessors' successors
                predecessor.Successors.Remove(thunk);
                
                // Add the target to predecessors' successors (if not already present)
                if (!predecessor.Successors.Contains(target))
                    predecessor.Successors.Add(target);

                // Update the target's predecessors
                target.Predecessors.Remove(thunk);
                if (!target.Predecessors.Contains(predecessor))
                    target.Predecessors.Add(predecessor);

                // Update any jump instructions that target the thunk
                UpdateJumpTargets(predecessor, thunk, target);
            }

            // Remove the thunk from the routine
            thunk.Routine.RemoveBlock(thunk);
        }

        /// <summary>
        /// Updates jump instructions that target the thunk to target the new block.
        /// </summary>
        private void UpdateJumpTargets(BasicBlock block, BasicBlock oldTarget, BasicBlock newTarget)
        {
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var updated = UpdateJumpTarget(instruction, oldTarget, newTarget);

                if (updated != null && !updated.Equals(instruction))
                {
                    block.ReplaceInstruction(i, updated);
                }
            }
        }

        /// <summary>
        /// Updates a jump instruction's target.
        /// </summary>
        private Instruction? UpdateJumpTarget(Instruction instruction, BasicBlock oldTarget, BasicBlock newTarget)
        {
            if (instruction.Descriptor == InstructionSet.Jmp && instruction.OperandCount > 0)
            {
                var currentTarget = instruction.GetOperand0();
                
                // Check if the current target matches the old target
                if (IsJumpToBlock(currentTarget, oldTarget))
                {
                    var newTargetOperand = CreateJumpTargetOperand(newTarget);
                    return Instruction.CreateJmp(newTargetOperand);
                }
            }
            else if (instruction.Descriptor == InstructionSet.Jcc && instruction.OperandCount > 1)
            {
                var condition = instruction.GetOperand0();
                var currentTarget = instruction.GetOperand1();
                
                // Check if the current target matches the old target
                if (IsJumpToBlock(currentTarget, oldTarget))
                {
                    var newTargetOperand = CreateJumpTargetOperand(newTarget);
                    return Instruction.CreateJcc(condition, newTargetOperand);
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a jump operand targets a specific block.
        /// </summary>
        private bool IsJumpToBlock(Operand jumpTarget, BasicBlock targetBlock)
        {
            // This would need more sophisticated analysis to determine
            // if a jump target matches a block's entry point
            return false;
        }

        /// <summary>
        /// Creates a jump target operand for a specific block.
        /// </summary>
        private Operand CreateJumpTargetOperand(BasicBlock targetBlock)
        {
            // This would create an appropriate operand for jumping to the target block
            // For now, we'll create a placeholder
            return Operand.CreateImmediate((long)targetBlock.Vip.Value, 64);
        }
    }
}
