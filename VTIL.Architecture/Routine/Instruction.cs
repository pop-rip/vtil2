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
using System.Numerics;
using VTIL.Common.Math;

namespace VTIL.Architecture
{
    /// <summary>
    /// Represents a VTIL instruction with its operands.
    /// </summary>
    public sealed class Instruction : IEquatable<Instruction>
    {
        /// <summary>
        /// The instruction descriptor.
        /// </summary>
        public InstructionDescriptor Descriptor { get; }

        /// <summary>
        /// The operands for this instruction.
        /// </summary>
        public IReadOnlyList<Operand> Operands { get; }

        /// <summary>
        /// The access size for this instruction.
        /// </summary>
        public BitCntT AccessSize { get; }

        /// <summary>
        /// Creates a new instruction.
        /// </summary>
        public Instruction(InstructionDescriptor descriptor, IReadOnlyList<Operand> operands, BitCntT accessSize)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Operands = operands ?? throw new ArgumentNullException(nameof(operands));
            AccessSize = accessSize;

            // Validate operands match descriptor
            ValidateOperands();
        }

        /// <summary>
        /// Creates a new instruction with operands.
        /// </summary>
        public static Instruction Create(InstructionDescriptor descriptor, params Operand[] operands)
        {
            return new Instruction(descriptor, operands, GetDefaultAccessSize(descriptor, operands));
        }

        /// <summary>
        /// Creates a new instruction with explicit access size.
        /// </summary>
        public static Instruction Create(InstructionDescriptor descriptor, BitCntT accessSize, params Operand[] operands)
        {
            return new Instruction(descriptor, operands, accessSize);
        }

        /// <summary>
        /// Creates a NOP instruction.
        /// </summary>
        public static Instruction CreateNop()
        {
            return new Instruction(InstructionSet.Nop, Array.Empty<Operand>(), 0);
        }

        /// <summary>
        /// Creates a MOV instruction.
        /// </summary>
        public static Instruction CreateMov(Operand destination, Operand source, BitCntT accessSize)
        {
            return new Instruction(InstructionSet.Mov, new[] { destination, source }, accessSize);
        }

        /// <summary>
        /// Creates an ADD instruction.
        /// </summary>
        public static Instruction CreateAdd(Operand destination, Operand source, BitCntT accessSize)
        {
            return new Instruction(InstructionSet.Add, new[] { destination, source }, accessSize);
        }

        /// <summary>
        /// Creates a SUB instruction.
        /// </summary>
        public static Instruction CreateSub(Operand destination, Operand source, BitCntT accessSize)
        {
            return new Instruction(InstructionSet.Sub, new[] { destination, source }, accessSize);
        }


        /// <summary>
        /// Creates a SUB instruction.
        /// </summary>
        public static Instruction CreateMul(Operand destination, Operand source, BitCntT accessSize)
        {
            return new Instruction(InstructionSet.Mul, new[] { destination, source }, accessSize);
        }

        /// <summary>
        /// Creates a PUSH instruction.
        /// </summary>
        public static Instruction CreatePush(Operand source, BitCntT accessSize)
        {
            return new Instruction(InstructionSet.Push, new[] { source }, accessSize);
        }

        /// <summary>
        /// Creates a POP instruction.
        /// </summary>
        public static Instruction CreatePop(Operand destination, BitCntT accessSize)
        {
            return new Instruction(InstructionSet.Pop, new[] { destination }, accessSize);
        }

        /// <summary>
        /// Creates a READ instruction.
        /// </summary>
        public static Instruction CreateRead(Operand destination, Operand baseAddress, Operand offset, BitCntT accessSize)
        {
            return new Instruction(InstructionSet.Read, new[] { destination, baseAddress, offset }, accessSize);
        }

        /// <summary>
        /// Creates a WRITE instruction.
        /// </summary>
        public static Instruction CreateWrite(Operand baseAddress, Operand offset, Operand source, BitCntT accessSize)
        {
            return new Instruction(InstructionSet.Write, new[] { baseAddress, offset, source }, accessSize);
        }

        /// <summary>
        /// Creates a JMP instruction.
        /// </summary>
        public static Instruction CreateJmp(Operand target)
        {
            return new Instruction(InstructionSet.Jmp, new[] { target }, 0);
        }

        /// <summary>
        /// Creates a CALL instruction.
        /// </summary>
        public static Instruction CreateCall(Operand target)
        {
            return new Instruction(InstructionSet.Call, new[] { target }, 0);
        }

        /// <summary>
        /// Creates a RET instruction.
        /// </summary>
        public static Instruction CreateRet()
        {
            return new Instruction(InstructionSet.Ret, Array.Empty<Operand>(), 0);
        }

        /// <summary>
        /// Gets the number of operands.
        /// </summary>
        public int OperandCount => Operands.Count;

        /// <summary>
        /// Gets an operand by index.
        /// </summary>
        public Operand GetOperand(int index)
        {
            if (index < 0 || index >= Operands.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return Operands[index];
        }

        /// <summary>
        /// Gets the first operand (if it exists).
        /// </summary>
        public Operand GetOperand0() => Operands[0];

        /// <summary>
        /// Gets the second operand (if it exists).
        /// </summary>
        public Operand GetOperand1() => Operands[1];

        /// <summary>
        /// Gets the third operand (if it exists).
        /// </summary>
        public Operand GetOperand2() => Operands[2];

        /// <summary>
        /// Gets the fourth operand (if it exists).
        /// </summary>
        public Operand GetOperand3() => Operands[3];

        /// <summary>
        /// Checks if this instruction is a NOP.
        /// </summary>
        public bool IsNop => Descriptor == InstructionSet.Nop;

        /// <summary>
        /// Checks if this instruction is volatile.
        /// </summary>
        public bool IsVolatile => Descriptor.IsVolatile;

        /// <summary>
        /// Checks if this instruction is a branch.
        /// </summary>
        public bool IsBranch => Descriptor.IsBranching;

        /// <summary>
        /// Checks if this instruction is a virtual branch.
        /// </summary>
        public bool IsVirtualBranch => Descriptor.IsBranchingVirt;

        /// <summary>
        /// Checks if this instruction is a real branch.
        /// </summary>
        public bool IsRealBranch => Descriptor.IsBranchingReal;

        /// <summary>
        /// Checks if this instruction accesses memory.
        /// </summary>
        public bool AccessesMemory => Descriptor.AccessesMemory;

        /// <summary>
        /// Checks if this instruction reads memory.
        /// </summary>
        public bool ReadsMemory => Descriptor.ReadsMemory;

        /// <summary>
        /// Checks if this instruction writes memory.
        /// </summary>
        public bool WritesMemory => Descriptor.WritesMemory;

        /// <summary>
        /// Gets the symbolic operator for this instruction.
        /// </summary>
        public OperatorId SymbolicOperator => Descriptor.SymbolicOperator;

        /// <summary>
        /// Gets the instruction name with size suffix.
        /// </summary>
        public string GetNameWithSize() => Descriptor.GetNameWithSize(AccessSize);

        /// <summary>
        /// Validates that the operands match the descriptor.
        /// </summary>
        private void ValidateOperands()
        {
            if (Operands.Count != Descriptor.OperandCount)
            {
                throw new ArgumentException($"Operand count mismatch: expected {Descriptor.OperandCount}, got {Operands.Count}");
            }

            for (int i = 0; i < Operands.Count; i++)
            {
                var operand = Operands[i];
                var expectedType = Descriptor.OperandTypes[i];

                // Check operand type compatibility
                if (!IsOperandTypeCompatible(operand.Type, expectedType))
                {
                    throw new ArgumentException($"Operand {i} type mismatch: expected {expectedType}, got {operand.Type}");
                }
            }
        }

        /// <summary>
        /// Checks if an operand type is compatible with an expected type.
        /// </summary>
        private static bool IsOperandTypeCompatible(OperandType actual, OperandType expected)
        {
            return expected switch
            {
                OperandType.ReadAny => actual == OperandType.Read,
                OperandType.ReadImm => actual == OperandType.ReadImm,
                OperandType.ReadReg => actual == OperandType.ReadReg,
                OperandType.Write => actual == OperandType.Write,
                OperandType.ReadWrite => actual == OperandType.ReadWrite,
                _ => actual == expected
            };
        }

        /// <summary>
        /// Gets the default access size for an instruction.
        /// </summary>
        private static BitCntT GetDefaultAccessSize(InstructionDescriptor descriptor, IReadOnlyList<Operand> operands)
        {
            if (descriptor.AccessSizeIndex == 0)
                return 64; // Default to 64-bit

            if (descriptor.AccessSizeIndex > 0)
            {
                var operandIndex = descriptor.AccessSizeIndex - 1;
                if (operandIndex < operands.Count)
                {
                    return operands[operandIndex].AccessSize;
                }
            }

            return 64; // Default fallback
        }

        public bool Equals(Instruction? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Descriptor.Equals(other.Descriptor) &&
                   Operands.SequenceEqual(other.Operands) &&
                   AccessSize.Equals(other.AccessSize);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Instruction);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Descriptor);
            hashCode.Add(Operands);
            hashCode.Add(AccessSize);
            return hashCode.ToHashCode();
        }

        public static bool operator ==(Instruction? left, Instruction? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Instruction? left, Instruction? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            var parts = new List<string> { GetNameWithSize() };

            if (Operands.Count > 0)
            {
                parts.Add(string.Join(", ", Operands.Select(op => op.ToString())));
            }

            return string.Join(" ", parts);
        }
    }
}
