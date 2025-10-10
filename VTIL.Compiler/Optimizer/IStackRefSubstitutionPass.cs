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
    /// Substitutes internal stack references with direct register references where possible.
    /// </summary>
    public class IStackRefSubstitutionPass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets the execution order for this pass (serial).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Serial;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "IStack Reference Substitution";

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return SubstituteIStackReferences(block, crossBlock);
            }
        }

        /// <summary>
        /// Substitutes internal stack references in a basic block.
        /// </summary>
        private int SubstituteIStackReferences(BasicBlock block, bool crossBlock)
        {
            var substitutedCount = 0;
            var istackReferences = new Dictionary<Operand, RegisterDescriptor>();

            // First pass: collect IStack references and their mappings
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var references = ExtractIStackReferences(instruction);

                foreach (var reference in references)
                {
                    if (!istackReferences.ContainsKey(reference))
                    {
                        // Allocate a new register for this IStack reference
                        var newRegister = block.Routine.AllocRegister(reference.SizeInBits);
                        istackReferences[reference] = newRegister;
                    }
                }
            }

            // Second pass: substitute IStack references with register references
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var modified = SubstituteInstructionReferences(instruction, istackReferences);

                if (modified)
                {
                    block.ReplaceInstruction(i, instruction);
                    substitutedCount++;
                }
            }

            return substitutedCount;
        }

        /// <summary>
        /// Extracts IStack references from an instruction.
        /// </summary>
        private List<Operand> ExtractIStackReferences(Instruction instruction)
        {
            var references = new List<Operand>();

            for (int i = 0; i < instruction.OperandCount; i++)
            {
                var operand = instruction.GetOperand(i);
                
                if (IsIStackReference(operand))
                {
                    references.Add(operand);
                }
            }

            return references;
        }

        /// <summary>
        /// Checks if an operand is an IStack reference.
        /// </summary>
        private bool IsIStackReference(Operand operand)
        {
            if (!operand.IsMemory)
                return false;

            return true;
        }

        /// <summary>
        /// Substitutes IStack references in an instruction with register references.
        /// </summary>
        private bool SubstituteInstructionReferences(Instruction instruction, Dictionary<Operand, RegisterDescriptor> istackReferences)
        {
            var modified = false;

            for (int i = 0; i < instruction.OperandCount; i++)
            {
                var operand = instruction.GetOperand(i);
                
                if (istackReferences.TryGetValue(operand, out var register))
                {
                    var newOperand = operand;
                    newOperand.SetRegister(register);
                    modified = true;
                }
            }

            return modified;
        }
    }
}
