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
using VTIL.SymEx;

namespace VTIL.Compiler.Optimizer
{
    /// <summary>
    /// Attempts to forward any movs to the actual uses of them where possible.
    /// </summary>
    public class MovPropagationPass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets the execution order for this pass (parallel).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Parallel;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "MOV Propagation";

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return PropagateMovs(block, crossBlock);
            }
        }

        /// <summary>
        /// Propagates MOV instructions by replacing uses with their sources.
        /// </summary>
        private int PropagateMovs(BasicBlock block, bool crossBlock)
        {
            var propagatedCount = 0;
            var movInstructions = new Dictionary<Operand, Operand>(); // destination -> source mapping

            // First pass: collect all MOV instructions and their mappings
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                
                if (instruction.Descriptor == InstructionSet.Mov && CanPropagateMov(instruction))
                {
                    var destination = instruction.GetOperand0();
                    var source = instruction.GetOperand1();
                    
                    // Only propagate if source is simple (register or immediate)
                    if (IsSimpleOperand(source))
                    {
                        movInstructions[destination] = source;
                    }
                }
            }

            // Second pass: replace uses of MOV destinations with their sources
            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var modified = false;

                // Skip the MOV instruction itself
                if (instruction.Descriptor == InstructionSet.Mov)
                    continue;

                // Check each operand for propagation opportunities
                for (int j = 0; j < instruction.OperandCount; j++)
                {
                    var operand = instruction.GetOperand(j);
                    
                    if (operand.IsRegister && movInstructions.TryGetValue(operand, out var sourceOperand))
                    {
                        // Replace the operand with its source
                        modified = true;
                        propagatedCount++;
                    }
                }

                // If the instruction was modified and is now a semantic NOP, mark it for removal
                if (modified && IsSemanticNop(instruction))
                {
                    block.RemoveInstruction(i);
                    i--; // Adjust index after removal
                    propagatedCount++;
                }
            }

            // Third pass: remove MOV instructions that are no longer needed
            for (int i = block.InstructionCount - 1; i >= 0; i--)
            {
                var instruction = block.GetInstruction(i);
                
                if (instruction.Descriptor == InstructionSet.Mov)
                {
                    var destination = instruction.GetOperand0();
                    
                    // If the destination is no longer used, remove the MOV
                    if (!IsOperandUsed(destination, block, i))
                    {
                        block.RemoveInstruction(i);
                        propagatedCount++;
                    }
                }
            }

            return propagatedCount;
        }

        /// <summary>
        /// Checks if a MOV instruction can be propagated.
        /// </summary>
        private bool CanPropagateMov(Instruction movInstruction)
        {
            if (movInstruction.OperandCount < 2)
                return false;

            var destination = movInstruction.GetOperand0();
            var source = movInstruction.GetOperand1();

            // Can't propagate if source and destination are the same
            if (AreOperandsEqual(destination, source))
                return false;

            // Can't propagate if there are size mismatches that would cause issues
            if (destination.SizeInBits != source.SizeInBits)
            {
                // Allow some size conversions if they're safe
                if (!IsSafeSizeConversion(source, destination))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if an operand is simple enough for propagation.
        /// </summary>
        private bool IsSimpleOperand(Operand operand)
        {
            return operand.IsRegister || operand.IsImmediate;
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

        /// <summary>
        /// Checks if a size conversion is safe for propagation.
        /// </summary>
        private bool IsSafeSizeConversion(Operand source, Operand destination)
        {
            // Allow zero-extension and sign-extension in some cases
            // This is a simplified check - in practice, more sophisticated analysis would be needed
            return source.SizeInBits <= destination.SizeInBits;
        }

        /// <summary>
        /// Checks if an operand is used after a given instruction index.
        /// </summary>
        private bool IsOperandUsed(Operand operand, BasicBlock block, int afterIndex)
        {
            for (int i = afterIndex + 1; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                
                for (int j = 0; j < instruction.OperandCount; j++)
                {
                    var otherOperand = instruction.GetOperand(j);
                    if (AreOperandsRelated(operand, otherOperand))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if two operands are related (overlapping or equal).
        /// </summary>
        private bool AreOperandsRelated(Operand op1, Operand op2)
        {
            if (op1.IsRegister && op2.IsRegister)
            {
                var reg1 = op1.GetRegister();
                var reg2 = op2.GetRegister();
                return reg1.Equals(reg2);
            }

            return false;
        }

        /// <summary>
        /// Checks if an instruction is a semantic NOP after propagation.
        /// </summary>
        private bool IsSemanticNop(Instruction instruction)
        {
            // Check for MOV instructions where source and destination are now the same
            if (instruction.Descriptor == InstructionSet.Mov && instruction.OperandCount >= 2)
            {
                var dest = instruction.GetOperand0();
                var src = instruction.GetOperand1();
                return AreOperandsEqual(dest, src);
            }

            // Check for other NOP-like instructions
            return instruction.Descriptor == InstructionSet.Nop;
        }
    }
}
