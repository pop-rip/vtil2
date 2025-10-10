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
using System.Numerics;

namespace VTIL.Architecture
{
    /// <summary>
    /// Register type identifiers.
    /// </summary>
    public enum RegisterType : byte
    {
        /// <summary>
        /// Invalid register type.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// General purpose register.
        /// </summary>
        GeneralPurpose = 1,

        /// <summary>
        /// Stack pointer register.
        /// </summary>
        StackPointer = 2,

        /// <summary>
        /// Instruction pointer register.
        /// </summary>
        InstructionPointer = 3,

        /// <summary>
        /// Flags register.
        /// </summary>
        Flags = 4,

        /// <summary>
        /// Segment register.
        /// </summary>
        Segment = 5,

        /// <summary>
        /// Control register.
        /// </summary>
        Control = 6,

        /// <summary>
        /// Debug register.
        /// </summary>
        Debug = 7,

        /// <summary>
        /// Test register.
        /// </summary>
        Test = 8,

        /// <summary>
        /// Floating point register.
        /// </summary>
        FloatingPoint = 9,

        /// <summary>
        /// MMX register.
        /// </summary>
        Mmx = 10,

        /// <summary>
        /// XMM register.
        /// </summary>
        Xmm = 11,

        /// <summary>
        /// YMM register.
        /// </summary>
        Ymm = 12,

        /// <summary>
        /// ZMM register.
        /// </summary>
        Zmm = 13,

        /// <summary>
        /// Internal/VTIL virtual register.
        /// </summary>
        Internal = 14,

        /// <summary>
        /// Stack register (VTIL virtual stack).
        /// </summary>
        Stack = 15
    }

    /// <summary>
    /// Describes a register in VTIL.
    /// </summary>
    public readonly struct RegisterDescriptor : IEquatable<RegisterDescriptor>
    {
        /// <summary>
        /// The type of register.
        /// </summary>
        public RegisterType Type { get; }

        /// <summary>
        /// The register identifier/index.
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// The size of the register in bits.
        /// </summary>
        public BitCntT BitCount { get; }

        /// <summary>
        /// Creates a register descriptor.
        /// </summary>
        public RegisterDescriptor(RegisterType type, ulong id, BitCntT bitCount)
        {
            Type = type;
            Id = id;
            BitCount = bitCount;
        }

        /// <summary>
        /// Creates an internal register descriptor.
        /// </summary>
        public static RegisterDescriptor CreateInternal(ulong id, BitCntT bitCount)
        {
            return new RegisterDescriptor(RegisterType.Internal, id, bitCount);
        }

        /// <summary>
        /// Creates a stack register descriptor.
        /// </summary>
        public static RegisterDescriptor CreateStack(ulong offset, BitCntT bitCount)
        {
            return new RegisterDescriptor(RegisterType.Stack, offset, bitCount);
        }

        /// <summary>
        /// Creates a general purpose register descriptor.
        /// </summary>
        public static RegisterDescriptor CreateGeneralPurpose(ulong id, BitCntT bitCount)
        {
            return new RegisterDescriptor(RegisterType.GeneralPurpose, id, bitCount);
        }

        /// <summary>
        /// Checks if this is an internal register.
        /// </summary>
        public bool IsInternal => Type == RegisterType.Internal;

        /// <summary>
        /// Checks if this is a stack register.
        /// </summary>
        public bool IsStack => Type == RegisterType.Stack;

        /// <summary>
        /// Checks if this is a general purpose register.
        /// </summary>
        public bool IsGeneralPurpose => Type == RegisterType.GeneralPurpose;

        /// <summary>
        /// Checks if this is a floating point register.
        /// </summary>
        public bool IsFloatingPoint => Type is RegisterType.FloatingPoint or RegisterType.Mmx or RegisterType.Xmm or RegisterType.Ymm or RegisterType.Zmm;

        /// <summary>
        /// Checks if this is a special register (non-GPR).
        /// </summary>
        public bool IsSpecial => Type is not RegisterType.GeneralPurpose and not RegisterType.Internal and not RegisterType.Stack;

        /// <summary>
        /// Gets the size of the register in bytes.
        /// </summary>
        public int SizeInBytes => BitCount.Value / 8;

        /// <summary>
        /// Gets the size of the register in bits.
        /// </summary>
        public int SizeInBits => BitCount.Value;

        /// <summary>
        /// Gets the register name as a string.
        /// </summary>
        public string GetName()
        {
            return Type switch
            {
                RegisterType.Internal => $"v{Id}",
                RegisterType.Stack => $"sp[{Id}]",
                RegisterType.GeneralPurpose => $"r{Id}",
                RegisterType.StackPointer => "sp",
                RegisterType.InstructionPointer => "ip",
                RegisterType.Flags => "flags",
                RegisterType.Segment => $"seg{Id}",
                RegisterType.Control => $"cr{Id}",
                RegisterType.Debug => $"dr{Id}",
                RegisterType.Test => $"tr{Id}",
                RegisterType.FloatingPoint => $"st{Id}",
                RegisterType.Mmx => $"mm{Id}",
                RegisterType.Xmm => $"xmm{Id}",
                RegisterType.Ymm => $"ymm{Id}",
                RegisterType.Zmm => $"zmm{Id}",
                _ => $"reg{Id}({Type})"
            };
        }

        /// <summary>
        /// Gets the register type name as a string.
        /// </summary>
        public string GetTypeName()
        {
            return Type switch
            {
                RegisterType.Invalid => "invalid",
                RegisterType.GeneralPurpose => "gpr",
                RegisterType.StackPointer => "sp",
                RegisterType.InstructionPointer => "ip",
                RegisterType.Flags => "flags",
                RegisterType.Segment => "segment",
                RegisterType.Control => "control",
                RegisterType.Debug => "debug",
                RegisterType.Test => "test",
                RegisterType.FloatingPoint => "float",
                RegisterType.Mmx => "mmx",
                RegisterType.Xmm => "xmm",
                RegisterType.Ymm => "ymm",
                RegisterType.Zmm => "zmm",
                RegisterType.Internal => "internal",
                RegisterType.Stack => "stack",
                _ => $"unknown({Type})"
            };
        }

        public bool Equals(RegisterDescriptor other)
        {
            return Type == other.Type && Id == other.Id && BitCount.Equals(other.BitCount);
        }

        public override bool Equals(object? obj)
        {
            return obj is RegisterDescriptor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, Id, BitCount);
        }

        public static bool operator ==(RegisterDescriptor left, RegisterDescriptor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RegisterDescriptor left, RegisterDescriptor right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{GetName()}:{SizeInBits}";
        }
    }

    /// <summary>
    /// Extension methods for register types.
    /// </summary>
    public static class RegisterTypeExtensions
    {
        /// <summary>
        /// Gets the register type name as a string.
        /// </summary>
        public static string GetName(this RegisterType type)
        {
            return type switch
            {
                RegisterType.Invalid => "invalid",
                RegisterType.GeneralPurpose => "general_purpose",
                RegisterType.StackPointer => "stack_pointer",
                RegisterType.InstructionPointer => "instruction_pointer",
                RegisterType.Flags => "flags",
                RegisterType.Segment => "segment",
                RegisterType.Control => "control",
                RegisterType.Debug => "debug",
                RegisterType.Test => "test",
                RegisterType.FloatingPoint => "floating_point",
                RegisterType.Mmx => "mmx",
                RegisterType.Xmm => "xmm",
                RegisterType.Ymm => "ymm",
                RegisterType.Zmm => "zmm",
                RegisterType.Internal => "internal",
                RegisterType.Stack => "stack",
                _ => $"<unknown:{type}>"
            };
        }

        /// <summary>
        /// Checks if the register type is a floating point register.
        /// </summary>
        public static bool IsFloatingPoint(this RegisterType type)
        {
            return type is RegisterType.FloatingPoint or RegisterType.Mmx or RegisterType.Xmm or RegisterType.Ymm or RegisterType.Zmm;
        }

        /// <summary>
        /// Checks if the register type is a special register.
        /// </summary>
        public static bool IsSpecial(this RegisterType type)
        {
            return type is not RegisterType.GeneralPurpose and not RegisterType.Internal and not RegisterType.Stack;
        }

        /// <summary>
        /// Checks if the register type is a control register.
        /// </summary>
        public static bool IsControl(this RegisterType type)
        {
            return type is RegisterType.Control or RegisterType.Debug or RegisterType.Test;
        }
    }
}
