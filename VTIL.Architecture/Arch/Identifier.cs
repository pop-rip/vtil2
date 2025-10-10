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
using VTIL.Common.Util;

namespace VTIL.Architecture
{
    /// <summary>
    /// Architecture identifiers for different CPU architectures.
    /// </summary>
    public enum ArchitectureIdentifier : byte
    {
        Invalid = 0,
        X86 = 1,
        Amd64 = 2,
        Arm64 = 3,
        Virtual = 4
    }

    /// <summary>
    /// Extension methods for architecture identifiers.
    /// </summary>
    public static class ArchitectureIdentifierExtensions
    {
        /// <summary>
        /// Gets the architecture name as a string.
        /// </summary>
        public static string GetName(this ArchitectureIdentifier arch)
        {
            return arch switch
            {
                ArchitectureIdentifier.X86 => "x86",
                ArchitectureIdentifier.Amd64 => "amd64",
                ArchitectureIdentifier.Arm64 => "arm64",
                ArchitectureIdentifier.Virtual => "virtual",
                _ => "invalid"
            };
        }

        /// <summary>
        /// Gets the pointer size in bits for the architecture.
        /// </summary>
        public static int GetPointerSize(this ArchitectureIdentifier arch)
        {
            return arch switch
            {
                ArchitectureIdentifier.X86 => 32,
                ArchitectureIdentifier.Amd64 => 64,
                ArchitectureIdentifier.Arm64 => 64,
                ArchitectureIdentifier.Virtual => 64, // Default to 64-bit for virtual
                _ => 0
            };
        }

        /// <summary>
        /// Gets the pointer size in bytes for the architecture.
        /// </summary>
        public static int GetPointerSizeBytes(this ArchitectureIdentifier arch)
        {
            return arch.GetPointerSize() / 8;
        }

        /// <summary>
        /// Checks if the architecture is 64-bit.
        /// </summary>
        public static bool Is64Bit(this ArchitectureIdentifier arch)
        {
            return arch.GetPointerSize() == 64;
        }

        /// <summary>
        /// Checks if the architecture is 32-bit.
        /// </summary>
        public static bool Is32Bit(this ArchitectureIdentifier arch)
        {
            return arch.GetPointerSize() == 32;
        }

        /// <summary>
        /// Gets the default calling convention for the architecture.
        /// </summary>
        public static string GetDefaultCallingConvention(this ArchitectureIdentifier arch)
        {
            return arch switch
            {
                ArchitectureIdentifier.X86 => "cdecl",
                ArchitectureIdentifier.Amd64 => "fastcall",
                ArchitectureIdentifier.Arm64 => "aapcs",
                ArchitectureIdentifier.Virtual => "virtual",
                _ => "unknown"
            };
        }
    }

    /// <summary>
    /// Virtual instruction pointer type.
    /// </summary>
    public readonly struct VipT : IEquatable<VipT>, IComparable<VipT>
    {
        private readonly ulong _value;

        public VipT(ulong value)
        {
            _value = value;
        }

        public ulong Value => _value;

        public static implicit operator VipT(ulong value) => new(value);
        public static implicit operator ulong(VipT vip) => vip._value;

        public static VipT operator +(VipT left, ulong right) => new(left._value + right);
        public static VipT operator -(VipT left, ulong right) => new(left._value - right);
        public static VipT operator +(VipT left, VipT right) => new(left._value + right._value);
        public static VipT operator -(VipT left, VipT right) => new(left._value - right._value);

        public static bool operator ==(VipT left, VipT right) => left._value == right._value;
        public static bool operator !=(VipT left, VipT right) => left._value != right._value;
        public static bool operator <(VipT left, VipT right) => left._value < right._value;
        public static bool operator >(VipT left, VipT right) => left._value > right._value;
        public static bool operator <=(VipT left, VipT right) => left._value <= right._value;
        public static bool operator >=(VipT left, VipT right) => left._value >= right._value;

        public bool Equals(VipT other) => _value == other._value;
        public int CompareTo(VipT other) => _value.CompareTo(other._value);

        public override bool Equals(object? obj) => obj is VipT other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => $"0x{_value:X}";
    }

    /// <summary>
    /// Real instruction pointer type.
    /// </summary>
    public readonly struct RipT : IEquatable<RipT>, IComparable<RipT>
    {
        private readonly ulong _value;

        public RipT(ulong value)
        {
            _value = value;
        }

        public ulong Value => _value;

        public static implicit operator RipT(ulong value) => new(value);
        public static implicit operator ulong(RipT rip) => rip._value;

        public static RipT operator +(RipT left, ulong right) => new(left._value + right);
        public static RipT operator -(RipT left, ulong right) => new(left._value - right);
        public static RipT operator +(RipT left, RipT right) => new(left._value + right._value);
        public static RipT operator -(RipT left, RipT right) => new(left._value - right._value);

        public static bool operator ==(RipT left, RipT right) => left._value == right._value;
        public static bool operator !=(RipT left, RipT right) => left._value != right._value;
        public static bool operator <(RipT left, RipT right) => left._value < right._value;
        public static bool operator >(RipT left, RipT right) => left._value > right._value;
        public static bool operator <=(RipT left, RipT right) => left._value <= right._value;
        public static bool operator >=(RipT left, RipT right) => left._value >= right._value;

        public bool Equals(RipT other) => _value == other._value;
        public int CompareTo(RipT other) => _value.CompareTo(other._value);

        public override bool Equals(object? obj) => obj is RipT other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => $"0x{_value:X}";
    }

    /// <summary>
    /// Bit count type for specifying bit widths.
    /// </summary>
    public readonly struct BitCntT : IEquatable<BitCntT>, IComparable<BitCntT>
    {
        private readonly int _value;

        public BitCntT(int value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Bit count cannot be negative");
            _value = value;
        }

        public int Value => _value;

        public static implicit operator BitCntT(int value) => new(value);
        public static implicit operator int(BitCntT bitCnt) => bitCnt._value;

        public static BitCntT operator +(BitCntT left, int right) => new(left._value + right);
        public static BitCntT operator -(BitCntT left, int right) => new(left._value - right);
        public static BitCntT operator +(BitCntT left, BitCntT right) => new(left._value + right._value);
        public static BitCntT operator -(BitCntT left, BitCntT right) => new(left._value - right._value);

        public static bool operator ==(BitCntT left, BitCntT right) => left._value == right._value;
        public static bool operator !=(BitCntT left, BitCntT right) => left._value != right._value;
        public static bool operator <(BitCntT left, BitCntT right) => left._value < right._value;
        public static bool operator >(BitCntT left, BitCntT right) => left._value > right._value;
        public static bool operator <=(BitCntT left, BitCntT right) => left._value <= right._value;
        public static bool operator >=(BitCntT left, BitCntT right) => left._value >= right._value;

        public bool Equals(BitCntT other) => _value == other._value;
        public int CompareTo(BitCntT other) => _value.CompareTo(other._value);

        public override bool Equals(object? obj) => obj is BitCntT other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => $"{_value} bits";
    }
}
