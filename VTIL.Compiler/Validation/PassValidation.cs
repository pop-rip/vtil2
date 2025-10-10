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

namespace VTIL.Compiler.Validation
{
    /// <summary>
    /// Validates optimization passes to ensure they maintain program correctness.
    /// </summary>
    public static class PassValidation
    {
        /// <summary>
        /// Validates a routine after optimization passes.
        /// </summary>
        public static ValidationResult ValidateRoutine(Routine routine)
        {
            var result = new ValidationResult();

            // Check basic block integrity
            ValidateBasicBlocks(routine, result);

            // Check instruction integrity
            ValidateInstructions(routine, result);

            // Check control flow integrity
            ValidateControlFlow(routine, result);

            // Check register usage
            ValidateRegisterUsage(routine, result);

            // Check stack usage
            ValidateStackUsage(routine, result);

            return result;
        }

        /// <summary>
        /// Validates basic blocks in a routine.
        /// </summary>
        private static void ValidateBasicBlocks(Routine routine, ValidationResult result)
        {
            // Check that all blocks have valid VIPs
            foreach (var block in routine.ExploredBlocks.Values)
            {
                if (block.Vip.Value == 0)
                {
                    result.AddError($"Block at VIP 0 is invalid");
                }

                // Check that block has at least one instruction or is properly terminated
                if (block.InstructionCount == 0 && !block.IsExit)
                {
                    result.AddWarning($"Empty block at VIP {block.Vip.Value:X}");
                }
            }

            // Check that entry point exists
            if (routine.EntryPoint == null)
            {
                result.AddError("No entry point found in routine");
            }

            // Check that all blocks are reachable from entry point
            var reachableBlocks = GetReachableBlocks(routine.EntryPoint);
            var allBlocks = routine.ExploredBlocks.Values.ToHashSet();
            var unreachableBlocks = allBlocks.Except(reachableBlocks);

            foreach (var unreachable in unreachableBlocks)
            {
                result.AddWarning($"Unreachable block at VIP {unreachable.Vip.Value:X}");
            }
        }

        /// <summary>
        /// Validates instructions in a routine.
        /// </summary>
        private static void ValidateInstructions(Routine routine, ValidationResult result)
        {
            foreach (var block in routine.ExploredBlocks.Values)
            {
                for (int i = 0; i < block.InstructionCount; i++)
                {
                    var instruction = block.GetInstruction(i);
                    ValidateInstruction(instruction, block, i, result);
                }
            }
        }

        /// <summary>
        /// Validates a single instruction.
        /// </summary>
        private static void ValidateInstruction(Instruction instruction, BasicBlock block, int index, ValidationResult result)
        {
            // Check operand count
            if (instruction.OperandCount != instruction.Descriptor.OperandCount)
            {
                result.AddError($"Instruction at {block.Vip.Value:X}+{index} has incorrect operand count");
            }

            // Check operand types
            for (int i = 0; i < instruction.OperandCount; i++)
            {
                var operand = instruction.GetOperand(i);
                var expectedType = instruction.Descriptor.OperandTypes[i];

                if (!IsOperandTypeValid(operand, expectedType))
                {
                    result.AddError($"Instruction at {block.Vip.Value:X}+{index} operand {i} has incorrect type");
                }
            }

            // Check access size
            if (instruction.AccessSize != 0)
            {
                var accessSize = instruction.AccessSize.Value;
                if (accessSize <= 0 || accessSize > 512)
                {
                    result.AddError($"Instruction at {block.Vip.Value:X}+{index} has invalid access size");
                }
            }

            // Check for semantic errors
            if (instruction.Descriptor == InstructionSet.Div)
            {
                // Check for division by zero
                if (instruction.OperandCount >= 3)
                {
                    var divisor = instruction.GetOperand2();
                    if (divisor.IsImmediate && divisor.GetImmediate() == 0)
                    {
                        result.AddWarning($"Potential division by zero at {block.Vip.Value:X}+{index}");
                    }
                }
            }
        }

        /// <summary>
        /// Validates control flow in a routine.
        /// </summary>
        private static void ValidateControlFlow(Routine routine, ValidationResult result)
        {
            foreach (var block in routine.ExploredBlocks.Values)
            {
                // Check that predecessors and successors are consistent
                foreach (var successor in block.Successors)
                {
                    if (!successor.Predecessors.Contains(block))
                    {
                        result.AddError($"Inconsistent control flow: block {block.Vip.Value:X} -> {successor.Vip.Value:X}");
                    }
                }

                foreach (var predecessor in block.Predecessors)
                {
                    if (!predecessor.Successors.Contains(block))
                    {
                        result.AddError($"Inconsistent control flow: block {predecessor.Vip.Value:X} -> {block.Vip.Value:X}");
                    }
                }

                // Check that branching instructions match successors
                if (block.InstructionCount > 0)
                {
                    var lastInstruction = block.GetInstruction(block.InstructionCount - 1);
                    ValidateBranchingInstruction(lastInstruction, block, result);
                }
            }
        }

        /// <summary>
        /// Validates a branching instruction.
        /// </summary>
        private static void ValidateBranchingInstruction(Instruction instruction, BasicBlock block, ValidationResult result)
        {
            if (instruction.Descriptor.IsBranching)
            {
                switch (instruction.Descriptor)
                {
                    case var desc when desc == InstructionSet.Jmp:
                        if (block.Successors.Count != 1)
                        {
                            result.AddError($"Unconditional jump at {block.Vip.Value:X} should have exactly 1 successor");
                        }
                        break;

                    case var desc when desc == InstructionSet.Ret:
                        if (block.Successors.Count != 0)
                        {
                            result.AddError($"Return at {block.Vip.Value:X} should have no successors");
                        }
                        break;
                }
            }
            else
            {
                // Non-branching instruction should fall through to next block
                if (block.Successors.Count > 1)
                {
                    result.AddError($"Non-branching instruction at {block.Vip.Value:X} has multiple successors");
                }
            }
        }

        /// <summary>
        /// Validates register usage in a routine.
        /// </summary>
        private static void ValidateRegisterUsage(Routine routine, ValidationResult result)
        {
            var registerUsage = new Dictionary<RegisterDescriptor, List<(BasicBlock block, int index)>>();

            foreach (var block in routine.ExploredBlocks.Values)
            {
                for (int i = 0; i < block.InstructionCount; i++)
                {
                    var instruction = block.GetInstruction(i);

                    for (int j = 0; j < instruction.OperandCount; j++)
                    {
                        var operand = instruction.GetOperand(j);

                        if (operand.IsRegister)
                        {
                            var register = operand.GetRegister();

                            if (!registerUsage.ContainsKey(register))
                                registerUsage[register] = new List<(BasicBlock, int)>();

                            registerUsage[register].Add((block, i));
                        }
                    }
                }
            }

            // Check for undefined register usage
            foreach (var kvp in registerUsage)
            {
                var register = kvp.Key;
                var usages = kvp.Value;

                // Check if register is used before being defined
                var firstUsage = usages.First();
                if (IsRegisterReadBeforeWrite(register, firstUsage.block, firstUsage.index))
                {
                    result.AddWarning($"Register {register.GetName()} may be used before being defined");
                }
            }
        }

        /// <summary>
        /// Validates stack usage in a routine.
        /// </summary>
        private static void ValidateStackUsage(Routine routine, ValidationResult result)
        {
            var stackBalance = 0;
            var maxStackDepth = 0;

            foreach (var block in routine.ExploredBlocks.Values)
            {
                for (int i = 0; i < block.InstructionCount; i++)
                {
                    var instruction = block.GetInstruction(i);

                    switch (instruction.Descriptor)
                    {
                        case var desc when desc == InstructionSet.Push:
                            stackBalance++;
                            maxStackDepth = Math.Max(maxStackDepth, stackBalance);
                            break;

                        case var desc when desc == InstructionSet.Pop:
                            stackBalance--;
                            if (stackBalance < 0)
                            {
                                result.AddError($"Stack underflow at {block.Vip.Value:X}+{i}");
                            }
                            break;
                    }
                }
            }

            if (stackBalance != 0)
            {
                result.AddWarning($"Unbalanced stack: {stackBalance} items remaining");
            }

            if (maxStackDepth > 1000)
            {
                result.AddWarning($"Excessive stack depth: {maxStackDepth} items");
            }
        }

        /// <summary>
        /// Checks if an operand type is valid for the expected type.
        /// </summary>
        private static bool IsOperandTypeValid(Operand operand, OperandType expectedType)
        {
            return Enum.IsDefined(typeof(OperandType), expectedType);
        }

        /// <summary>
        /// Checks if a register is read before being written.
        /// </summary>
        private static bool IsRegisterReadBeforeWrite(RegisterDescriptor register, BasicBlock block, int instructionIndex)
        {
            // Simplified check - in practice, this would need more sophisticated analysis
            for (int i = 0; i < instructionIndex; i++)
            {
                var instruction = block.GetInstruction(i);
                
                for (int j = 0; j < instruction.OperandCount; j++)
                {
                    var operand = instruction.GetOperand(j);
                    
                    if (operand.IsRegister && operand.GetRegister().Equals(register))
                    {
                        // Check if this is a write operation
                        if (operand.IsWritable)
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets all blocks reachable from a starting block.
        /// </summary>
        private static HashSet<BasicBlock> GetReachableBlocks(BasicBlock? startBlock)
        {
            var reachable = new HashSet<BasicBlock>();
            var toVisit = new Queue<BasicBlock>();

            if (startBlock != null)
            {
                toVisit.Enqueue(startBlock);
            }

            while (toVisit.Count > 0)
            {
                var current = toVisit.Dequeue();
                if (reachable.Add(current))
                {
                    foreach (var successor in current.Successors)
                    {
                        toVisit.Enqueue(successor);
                    }
                }
            }

            return reachable;
        }
    }

    /// <summary>
    /// Result of validation.
    /// </summary>
    public class ValidationResult
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public bool IsValid => Errors.Count == 0;
        public bool HasWarnings => Warnings.Count > 0;

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        public override string ToString()
        {
            var result = new System.Text.StringBuilder();

            if (Errors.Count > 0)
            {
                result.AppendLine($"Errors ({Errors.Count}):");
                foreach (var error in Errors)
                {
                    result.AppendLine($"  ERROR: {error}");
                }
            }

            if (Warnings.Count > 0)
            {
                result.AppendLine($"Warnings ({Warnings.Count}):");
                foreach (var warning in Warnings)
                {
                    result.AppendLine($"  WARNING: {warning}");
                }
            }

            if (IsValid && !HasWarnings)
            {
                result.AppendLine("Validation passed with no issues.");
            }

            return result.ToString();
        }
    }
}
