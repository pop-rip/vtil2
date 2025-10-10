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
using VTIL.Common.Math;

namespace VTIL.Architecture
{
    /// <summary>
    /// Describes a VTIL instruction with its operands and properties.
    /// </summary>
    public sealed class InstructionDescriptor : IEquatable<InstructionDescriptor>
    {
        /// <summary>
        /// Name of the instruction.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// List of the operand types.
        /// </summary>
        public IReadOnlyList<OperandType> OperandTypes { get; }

        /// <summary>
        /// Index of the operand that determines the instruction's access size property.
        /// If positive, bitcount of the operand, if negative the immediate stored inside the operand.
        /// </summary>
        public int AccessSizeIndex { get; }

        /// <summary>
        /// Whether the instruction is volatile or not meaning it should not be discarded 
        /// even if it is no-op or dead.
        /// </summary>
        public bool IsVolatile { get; }

        /// <summary>
        /// A pointer to the expression operator that describes the operation of this instruction if applicable.
        /// </summary>
        public OperatorId SymbolicOperator { get; }

        /// <summary>
        /// List of operands that are treated as branching destinations for real addresses.
        /// </summary>
        public IReadOnlyList<int> BranchOperandsRip { get; }

        /// <summary>
        /// List of operands that are treated as branching destinations for virtual addresses.
        /// </summary>
        public IReadOnlyList<int> BranchOperandsVip { get; }

        /// <summary>
        /// Operand that marks the beginning of a memory reference and whether it writes to the pointer or not.
        /// [Idx] must be a register and [Idx+1] must be an immediate.
        /// </summary>
        public int MemoryOperandIndex { get; }

        /// <summary>
        /// Whether the memory operand writes to memory.
        /// </summary>
        public bool MemoryWrite { get; }

        /// <summary>
        /// Creates an instruction descriptor.
        /// </summary>
        public InstructionDescriptor(
            string name,
            IReadOnlyList<OperandType> operandTypes,
            int accessSizeIndex,
            bool isVolatile,
            OperatorId symbolicOperator,
            IReadOnlyList<int> branchOperandsRip,
            IReadOnlyList<int> branchOperandsVip,
            int memoryOperandIndex,
            bool memoryWrite)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            OperandTypes = operandTypes ?? throw new ArgumentNullException(nameof(operandTypes));
            AccessSizeIndex = accessSizeIndex;
            IsVolatile = isVolatile;
            SymbolicOperator = symbolicOperator;
            BranchOperandsRip = branchOperandsRip ?? Array.Empty<int>();
            BranchOperandsVip = branchOperandsVip ?? Array.Empty<int>();
            MemoryOperandIndex = memoryOperandIndex;
            MemoryWrite = memoryWrite;

            // Validate operands
            ValidateOperands();
        }

        /// <summary>
        /// Creates an instruction descriptor with simpler parameters.
        /// </summary>
        public static InstructionDescriptor Create(
            string name,
            OperandType[] operandTypes,
            int accessSizeIndex = 0,
            bool isVolatile = false,
            OperatorId symbolicOperator = OperatorId.Invalid,
            int[]? branchOperandsRip = null,
            int[]? branchOperandsVip = null,
            int memoryOperandIndex = -1,
            bool memoryWrite = false)
        {
            return new InstructionDescriptor(
                name,
                operandTypes,
                accessSizeIndex,
                isVolatile,
                symbolicOperator,
                branchOperandsRip ?? Array.Empty<int>(),
                branchOperandsVip ?? Array.Empty<int>(),
                memoryOperandIndex,
                memoryWrite);
        }

        /// <summary>
        /// Number of operands this instruction has.
        /// </summary>
        public int OperandCount => OperandTypes.Count;

        /// <summary>
        /// Whether the instruction branches for virtual addresses.
        /// </summary>
        public bool IsBranchingVirt => BranchOperandsVip.Count > 0;

        /// <summary>
        /// Whether the instruction branches for real addresses.
        /// </summary>
        public bool IsBranchingReal => BranchOperandsRip.Count > 0;

        /// <summary>
        /// Whether the instruction branches at all.
        /// </summary>
        public bool IsBranching => IsBranchingVirt || IsBranchingReal;

        /// <summary>
        /// Whether the instruction reads memory.
        /// </summary>
        public bool ReadsMemory => AccessesMemory && !MemoryWrite;

        /// <summary>
        /// Whether the instruction writes memory.
        /// </summary>
        public bool WritesMemory => AccessesMemory && MemoryWrite;

        /// <summary>
        /// Whether the instruction accesses memory at all.
        /// </summary>
        public bool AccessesMemory => MemoryOperandIndex != -1;

        /// <summary>
        /// Gets the instruction name with size suffix.
        /// </summary>
        public string GetNameWithSize(BitCntT accessSize)
        {
            var suffix = GetSizeSuffix(accessSize);
            return suffix != '\0' ? Name + suffix : Name;
        }

        /// <summary>
        /// Gets the size suffix character for the given access size.
        /// </summary>
        private static char GetSizeSuffix(BitCntT accessSize)
        {
            return accessSize.Value switch
            {
                8 => 'b',   // 8-bit
                16 => 'w',  // 16-bit
                32 => 'd',  // 32-bit
                64 => 'q',  // 64-bit
                128 => 'o', // 128-bit
                256 => 'h', // 256-bit
                512 => 'x', // 512-bit
                _ => '\0'
            };
        }

        /// <summary>
        /// Validates the instruction descriptor operands.
        /// </summary>
        private void ValidateOperands()
        {
            // Validate access size index
            if (AccessSizeIndex != 0 && (AccessSizeIndex < -OperandCount || AccessSizeIndex >= OperandCount))
            {
                throw new ArgumentException($"Access size index {AccessSizeIndex} is out of range for operand count {OperandCount}");
            }

            // Validate branch operands
            foreach (var index in BranchOperandsRip)
            {
                if (index < 0 || index >= OperandCount)
                {
                    throw new ArgumentException($"Branch operand RIP index {index} is out of range for operand count {OperandCount}");
                }
            }

            foreach (var index in BranchOperandsVip)
            {
                if (index < 0 || index >= OperandCount)
                {
                    throw new ArgumentException($"Branch operand VIP index {index} is out of range for operand count {OperandCount}");
                }
            }

            // Validate memory operand
            if (MemoryOperandIndex != -1)
            {
                if (MemoryOperandIndex < 0 || MemoryOperandIndex >= OperandCount - 1)
                {
                    throw new ArgumentException($"Memory operand index {MemoryOperandIndex} is out of range for operand count {OperandCount}");
                }

                // Memory operand must be a register
                if (OperandTypes[MemoryOperandIndex] != OperandType.ReadReg && OperandTypes[MemoryOperandIndex] != OperandType.ReadWrite)
                {
                    throw new ArgumentException($"Memory operand at index {MemoryOperandIndex} must be a register operand");
                }

                // Next operand must be an immediate (offset)
                if (OperandTypes[MemoryOperandIndex + 1] != OperandType.ReadImm)
                {
                    throw new ArgumentException($"Memory operand offset at index {MemoryOperandIndex + 1} must be an immediate operand");
                }
            }
        }

        public bool Equals(InstructionDescriptor? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Name == other.Name &&
                   OperandTypes.SequenceEqual(other.OperandTypes) &&
                   AccessSizeIndex == other.AccessSizeIndex &&
                   IsVolatile == other.IsVolatile &&
                   SymbolicOperator == other.SymbolicOperator &&
                   BranchOperandsRip.SequenceEqual(other.BranchOperandsRip) &&
                   BranchOperandsVip.SequenceEqual(other.BranchOperandsVip) &&
                   MemoryOperandIndex == other.MemoryOperandIndex &&
                   MemoryWrite == other.MemoryWrite;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as InstructionDescriptor);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Name);
            hashCode.Add(OperandTypes);
            hashCode.Add(AccessSizeIndex);
            hashCode.Add(IsVolatile);
            hashCode.Add((int)SymbolicOperator);
            hashCode.Add(BranchOperandsRip);
            hashCode.Add(BranchOperandsVip);
            hashCode.Add(MemoryOperandIndex);
            hashCode.Add(MemoryWrite);
            return hashCode.ToHashCode();
        }

        public static bool operator ==(InstructionDescriptor? left, InstructionDescriptor? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InstructionDescriptor? left, InstructionDescriptor? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            var parts = new List<string> { Name };
            
            if (OperandCount > 0)
            {
                parts.Add($"({string.Join(", ", OperandTypes)})");
            }

            if (IsBranching)
            {
                parts.Add($"[branch: rip={string.Join(",", BranchOperandsRip)}, vip={string.Join(",", BranchOperandsVip)}]");
            }

            if (AccessesMemory)
            {
                parts.Add($"[mem: {MemoryOperandIndex}, write={MemoryWrite}]");
            }

            if (IsVolatile)
            {
                parts.Add("[volatile]");
            }

            return string.Join(" ", parts);
        }
    }
}
