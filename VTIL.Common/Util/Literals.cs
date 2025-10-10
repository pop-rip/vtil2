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
using System.Numerics;

namespace VTIL.Common.Util
{
    /// <summary>
    /// Provides literal constants and utility functions for VTIL.
    /// </summary>
    public static class Literals
    {
        // Time constants
        public static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
        public static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

        // Numeric constants
        public const byte MaxByte = byte.MaxValue;
        public const sbyte MaxSByte = sbyte.MaxValue;
        public const ushort MaxUInt16 = ushort.MaxValue;
        public const short MaxInt16 = short.MaxValue;
        public const uint MaxUInt32 = uint.MaxValue;
        public const int MaxInt32 = int.MaxValue;
        public const ulong MaxUInt64 = ulong.MaxValue;
        public const long MaxInt64 = long.MaxValue;

        public const byte MinByte = byte.MinValue;
        public const sbyte MinSByte = sbyte.MinValue;
        public const ushort MinUInt16 = ushort.MinValue;
        public const short MinInt16 = short.MinValue;
        public const uint MinUInt32 = uint.MinValue;
        public const int MinInt32 = int.MinValue;
        public const ulong MinUInt64 = ulong.MinValue;
        public const long MinInt64 = long.MinValue;

        // Bit constants
        public const int BitsPerByte = 8;
        public const int BitsPerInt16 = 16;
        public const int BitsPerInt32 = 32;
        public const int BitsPerInt64 = 64;

        public const ulong AllBitsSet64 = ulong.MaxValue;
        public const uint AllBitsSet32 = uint.MaxValue;
        public const ushort AllBitsSet16 = ushort.MaxValue;
        public const byte AllBitsSet8 = byte.MaxValue;

        // Size constants
        public const int SizeOfByte = sizeof(byte);
        public const int SizeOfSByte = sizeof(sbyte);
        public const int SizeOfInt16 = sizeof(short);
        public const int SizeOfUInt16 = sizeof(ushort);
        public const int SizeOfInt32 = sizeof(int);
        public const int SizeOfUInt32 = sizeof(uint);
        public const int SizeOfInt64 = sizeof(long);
        public const int SizeOfUInt64 = sizeof(ulong);
        public const int SizeOfFloat = sizeof(float);
        public const int SizeOfDouble = sizeof(double);

        // Common bit patterns
        public const ulong Bit0 = 1UL << 0;
        public const ulong Bit1 = 1UL << 1;
        public const ulong Bit2 = 1UL << 2;
        public const ulong Bit3 = 1UL << 3;
        public const ulong Bit4 = 1UL << 4;
        public const ulong Bit5 = 1UL << 5;
        public const ulong Bit6 = 1UL << 6;
        public const ulong Bit7 = 1UL << 7;
        public const ulong Bit8 = 1UL << 8;
        public const ulong Bit15 = 1UL << 15;
        public const ulong Bit16 = 1UL << 16;
        public const ulong Bit31 = 1UL << 31;
        public const ulong Bit32 = 1UL << 32;
        public const ulong Bit63 = 1UL << 63;

        // Mask constants
        public const ulong Mask8 = 0xFF;
        public const ulong Mask16 = 0xFFFF;
        public const ulong Mask32 = 0xFFFFFFFF;
        public const ulong Mask64 = 0xFFFFFFFFFFFFFFFF;

        // Address space constants
        public const ulong MaxAddress32 = 0xFFFFFFFF;
        public const ulong MaxAddress64 = 0xFFFFFFFFFFFFFFFF;

        // Alignment constants
        public const int Align1 = 1;
        public const int Align2 = 2;
        public const int Align4 = 4;
        public const int Align8 = 8;
        public const int Align16 = 16;
        public const int Align32 = 32;
        public const int Align64 = 64;

        // Cache line constants
        public const int CacheLineSize = 64;

        // Page size constants
        public const int PageSize = 4096;
        public const int LargePageSize = 2 * 1024 * 1024; // 2MB

        // Architecture constants
        public const int MaxRegisterSize = 64;
        public const int MaxInstructionSize = 15; // x86-64 max instruction size
        public const int MaxOperandCount = 4;

        // VTIL specific constants
        public const int MaxBasicBlockSize = 1024 * 1024;
        public const int MaxRoutineSize = 100 * 1024 * 1024;
        public const int MaxExpressionDepth = 1024;

        // Error codes
        public const int Success = 0;
        public const int ErrorInvalidParameter = -1;
        public const int ErrorOutOfMemory = -2;
        public const int ErrorInvalidOperation = -3;
        public const int ErrorNotImplemented = -4;
        public const int ErrorInvalidState = -5;

        // Hash constants
        public const ulong Fnv1a64OffsetBasis = 0xcbf29ce484222325UL;
        public const ulong Fnv1a64Prime = 0x100000001b3UL;

        // Magic numbers
        public const uint Magic32 = 0xDEADBEEF;
        public const ulong Magic64 = 0xDEADBEEFCAFEBABEUL;

        /// <summary>
        /// Gets a bit mask for the specified number of bits.
        /// </summary>
        public static ulong GetMask(int bitCount)
        {
            if (bitCount <= 0) return 0;
            if (bitCount >= 64) return ulong.MaxValue;
            return (1UL << bitCount) - 1;
        }

        /// <summary>
        /// Gets a bit mask for the specified number of bits.
        /// </summary>
        public static uint GetMask32(int bitCount)
        {
            if (bitCount <= 0) return 0;
            if (bitCount >= 32) return uint.MaxValue;
            return (1U << bitCount) - 1;
        }

        /// <summary>
        /// Gets a bit at the specified position.
        /// </summary>
        public static ulong GetBit(int position)
        {
            if (position < 0 || position >= 64) return 0;
            return 1UL << position;
        }

        /// <summary>
        /// Gets a bit at the specified position.
        /// </summary>
        public static uint GetBit32(int position)
        {
            if (position < 0 || position >= 32) return 0;
            return 1U << position;
        }

        /// <summary>
        /// Checks if a value is a power of 2.
        /// </summary>
        public static bool IsPowerOf2(ulong value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }

        /// <summary>
        /// Checks if a value is a power of 2.
        /// </summary>
        public static bool IsPowerOf2(uint value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }

        /// <summary>
        /// Rounds up to the next power of 2.
        /// </summary>
        public static ulong RoundUpToPowerOf2(ulong value)
        {
            if (value == 0) return 1;
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;
            return value + 1;
        }

        /// <summary>
        /// Rounds up to the next power of 2.
        /// </summary>
        public static uint RoundUpToPowerOf2(uint value)
        {
            if (value == 0) return 1;
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }

        /// <summary>
        /// Aligns a value to the specified boundary.
        /// </summary>
        public static ulong AlignUp(ulong value, ulong alignment)
        {
            return (value + alignment - 1) & ~(alignment - 1);
        }

        /// <summary>
        /// Aligns a value to the specified boundary.
        /// </summary>
        public static uint AlignUp(uint value, uint alignment)
        {
            return (value + alignment - 1) & ~(alignment - 1);
        }

        /// <summary>
        /// Aligns a value down to the specified boundary.
        /// </summary>
        public static ulong AlignDown(ulong value, ulong alignment)
        {
            return value & ~(alignment - 1);
        }

        /// <summary>
        /// Aligns a value down to the specified boundary.
        /// </summary>
        public static uint AlignDown(uint value, uint alignment)
        {
            return value & ~(alignment - 1);
        }
    }
}
