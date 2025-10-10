/*
 * Copyright (c) 2020 Can Boluk and contributors of the VTIL Project   
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
using VTIL.Compiler.Optimizer;
using VTIL.SymEx;

namespace VTIL.Compiler.Common
{
    /// <summary>
    /// Flags for branch analysis.
    /// </summary>
    [Flags]
    public enum BranchAnalysisFlags
    {
        None = 0,
        CrossBlock = 1,
        Pack = 2,
        ResolveOpaque = 4
    }

    /// <summary>
    /// Information about a branch instruction.
    /// </summary>
    public class BranchInfo
    {
        /// <summary>
        /// Whether this is a VM exit.
        /// </summary>
        public bool IsVmExit { get; set; }

        /// <summary>
        /// Whether this is a conditional jump.
        /// </summary>
        public bool IsJcc { get; set; }

        /// <summary>
        /// The condition code expression for conditional jumps.
        /// </summary>
        public Expression? ConditionCode { get; set; }

        /// <summary>
        /// Possible destination expressions.
        /// </summary>
        public List<Expression> Destinations { get; set; } = new List<Expression>();

        /// <summary>
        /// Creates a new branch info instance.
        /// </summary>
        public BranchInfo()
        {
        }

        /// <summary>
        /// Creates a branch info for a VM exit.
        /// </summary>
        public static BranchInfo CreateVmExit()
        {
            return new BranchInfo { IsVmExit = true };
        }

        /// <summary>
        /// Creates a branch info for a conditional jump.
        /// </summary>
        public static BranchInfo CreateJcc(Expression conditionCode, params Expression[] destinations)
        {
            return new BranchInfo
            {
                IsJcc = true,
                ConditionCode = conditionCode,
                Destinations = destinations.ToList()
            };
        }

        /// <summary>
        /// Creates a branch info for an unconditional jump.
        /// </summary>
        public static BranchInfo CreateJump(params Expression[] destinations)
        {
            return new BranchInfo
            {
                Destinations = destinations.ToList()
            };
        }
    }

    /// <summary>
    /// Auxiliary functions for optimization passes.
    /// </summary>
    public static class Auxiliaries
    {
        /// <summary>
        /// Checks if the expression given is block-local.
        /// </summary>
        public static bool IsLocal(Expression expression)
        {
            if (expression.IsConstant)
                return true;

            if (expression.IsVariable)
            {
                var varName = expression.GetVariableName();
                // Variables starting with 'tmp_' are typically local temporaries
                return varName.StartsWith("tmp_") || varName.StartsWith("local_");
            }

            if (expression.IsOperation)
            {
                // Check all operands recursively
                var operands = expression.GetOperands();
                return operands.All(IsLocal);
            }

            return false;
        }

        /// <summary>
        /// Checks if an instruction is a semantic NOP.
        /// </summary>
        public static bool IsSemanticNop(Instruction instruction)
        {
            // Check for mov instructions that don't actually change anything
            if (instruction.Descriptor == InstructionSet.Mov)
            {
                var dest = instruction.GetOperand0();
                var src = instruction.GetOperand1();

                // If source and destination are the same register
                if (dest.IsRegister == true && src.IsRegister == true)
                {
                    var destReg = dest.Register;
                    var srcReg = src.Register;
                    
                    if (destReg.Equals(srcReg) && dest.SizeInBits == src.SizeInBits)
                        return true;
                }
            }

            // Check for other NOP-like instructions
            if (instruction.Descriptor == InstructionSet.Nop)
                return true;

            return false;
        }

        /// <summary>
        /// Removes all NOPs from a basic block.
        /// </summary>
        public static int RemoveNops(BasicBlock block, bool semanticNops = true, bool volatileNops = false)
        {
            var removedCount = 0;
            var instructionsToRemove = new List<int>();

            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                bool shouldRemove = false;

                // Check for semantic NOPs
                if (semanticNops && IsSemanticNop(instruction))
                    shouldRemove = true;

                // Check for volatile NOPs
                if (volatileNops && instruction.Descriptor == InstructionSet.Nop && instruction.IsVolatile)
                    shouldRemove = true;

                if (shouldRemove)
                    instructionsToRemove.Add(i);
            }

            // Remove instructions in reverse order to maintain indices
            for (int i = instructionsToRemove.Count - 1; i >= 0; i--)
            {
                block.RemoveInstruction(instructionsToRemove[i]);
                removedCount++;
            }

            return removedCount;
        }

        /// <summary>
        /// Removes all NOPs from a routine.
        /// </summary>
        public static int RemoveNops(Routine routine, bool semanticNops = true, bool volatileNops = false)
        {
            var totalRemoved = 0;

            foreach (var block in routine.ExploredBlocks.Values)
            {
                totalRemoved += RemoveNops(block, semanticNops, volatileNops);
            }

            return totalRemoved;
        }

        /// <summary>
        /// Applies all optimization passes to a routine.
        /// </summary>
        public static int ApplyAllPasses(Routine routine)
        {
            var totalOptimizations = 0;

            // Apply passes in the order defined in the original C++ code
            var passes = new IOptimizationPass[]
            {
                new StackPinningPass(),
                new IStackRefSubstitutionPass(),
                new BasicBlockExtensionPass(),
                new StackPropagationPass(),
                new DeadCodeEliminationPass(),
                new MovPropagationPass(),
                new RegisterRenamingPass(),
                new DeadCodeEliminationPass(),
                new SymbolicRewritePass(),
                new BranchCorrectionPass(),
                new CollectivePropagationPass(),
                new SymbolicRewritePass(),
                new BasicBlockThunkRemovalPass()
            };

            foreach (var pass in passes)
            {
                var optimizations = pass.CrossPass(routine);
                totalOptimizations += optimizations;

                if (optimizations > 0)
                {
                    Console.WriteLine($"Applied {pass.Name}: {optimizations} optimizations");
                }
            }

            return totalOptimizations;
        }
    }
}
