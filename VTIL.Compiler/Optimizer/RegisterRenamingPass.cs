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
    /// Renames registers to minimize register pressure and improve optimization opportunities.
    /// </summary>
    public class RegisterRenamingPass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets the execution order for this pass (parallel).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Parallel;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "Register Renaming";

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return RenameRegisters(block, crossBlock);
            }
        }

        /// <summary>
        /// Renames registers in a basic block to improve optimization opportunities.
        /// </summary>
        private int RenameRegisters(BasicBlock block, bool crossBlock)
        {
            var renamedCount = 0;
            var registerMap = new Dictionary<RegisterDescriptor, RegisterDescriptor>();
            var liveRanges = AnalyzeLiveRanges(block);

            // Create register mapping based on live range analysis
            foreach (var range in liveRanges)
            {
                if (CanRenameRegister(range.Register))
                {
                    var newRegister = block.Routine.AllocRegister(range.Register.SizeInBits);
                    registerMap[range.Register] = newRegister;
                }
            }

            // Apply register renaming
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var modified = false;

                for (int j = 0; j < instruction.OperandCount; j++)
                {
                    var operand = instruction.GetOperand(j);
                    
                    if (operand.IsRegister)
                    {
                        var register = operand.GetRegister();
                        if (registerMap.TryGetValue(register, out var newRegister))
                        {
                            var newOperand = operand.Clone();
                            newOperand.SetRegister(newRegister);
                            instruction.ReplaceOperand(j, newOperand);
                            modified = true;
                            renamedCount++;
                        }
                    }
                }

                // If instruction was modified, update the instruction
                if (modified)
                {
                    block.UpdateInstructionAt(i, instruction);
                }
            }

            return renamedCount;
        }

        /// <summary>
        /// Analyzes live ranges of registers in a basic block.
        /// </summary>
        private List<LiveRange> AnalyzeLiveRanges(BasicBlock block)
        {
            var liveRanges = new List<LiveRange>();
            var registerUses = new Dictionary<RegisterDescriptor, List<int>>();

            // Collect all register uses
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);

                for (int j = 0; j < instruction.OperandCount; j++)
                {
                    var operand = instruction.GetOperand(j);
                    
                    if (operand.IsRegister)
                    {
                        var register = operand.GetRegister();
                        
                        if (!registerUses.ContainsKey(register))
                            registerUses[register] = new List<int>();
                        
                        registerUses[register].Add(i);
                    }
                }
            }

            // Create live ranges
            foreach (var kvp in registerUses)
            {
                var register = kvp.Key;
                var uses = kvp.Value;
                
                if (uses.Count > 0)
                {
                    var liveRange = new LiveRange
                    {
                        Register = register,
                        StartIndex = uses.Min(),
                        EndIndex = uses.Max(),
                        UseCount = uses.Count
                    };
                    
                    liveRanges.Add(liveRange);
                }
            }

            return liveRanges;
        }

        /// <summary>
        /// Checks if a register can be renamed.
        /// </summary>
        private bool CanRenameRegister(RegisterDescriptor register)
        {
            // Don't rename special registers
            if (register.IsSpecialPurpose)
                return false;

            // Don't rename registers that are used across block boundaries
            // (This would need more sophisticated analysis in a real implementation)
            return true;
        }
    }

    /// <summary>
    /// Represents a live range of a register.
    /// </summary>
    public class LiveRange
    {
        /// <summary>
        /// The register being analyzed.
        /// </summary>
        public RegisterDescriptor Register { get; set; }

        /// <summary>
        /// The first instruction index where the register is used.
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// The last instruction index where the register is used.
        /// </summary>
        public int EndIndex { get; set; }

        /// <summary>
        /// The number of times the register is used.
        /// </summary>
        public int UseCount { get; set; }

        /// <summary>
        /// The length of the live range.
        /// </summary>
        public int Length => EndIndex - StartIndex + 1;
    }
}
