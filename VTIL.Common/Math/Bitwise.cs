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

namespace VTIL.Common.Math
{
    /// <summary>
    /// Provides bitwise operations and utilities.
    /// </summary>
    public static class Bitwise
    {
        /// <summary>
        /// Counts the number of set bits in a value.
        /// </summary>
        public static int PopCount(ulong value)
        {
            return BitOperations.PopCount(value);
        }

        /// <summary>
        /// Counts the number of set bits in a value.
        /// </summary>
        public static int PopCount(uint value)
        {
            return BitOperations.PopCount(value);
        }

        /// <summary>
        /// Counts the number of set bits in a value.
        /// </summary>
        public static int PopCount(ushort value)
        {
            return BitOperations.PopCount(value);
        }

        /// <summary>
        /// Counts the number of set bits in a value.
        /// </summary>
        public static int PopCount(byte value)
        {
            return BitOperations.PopCount(value);
        }

        /// <summary>
        /// Finds the index of the first set bit (least significant bit).
        /// </summary>
        public static int BitScanForward(ulong value)
        {
            if (value == 0) return -1;
            return BitOperations.TrailingZeroCount(value);
        }

        /// <summary>
        /// Finds the index of the first set bit (least significant bit).
        /// </summary>
        public static int BitScanForward(uint value)
        {
            if (value == 0) return -1;
            return BitOperations.TrailingZeroCount(value);
        }

        /// <summary>
        /// Finds the index of the last set bit (most significant bit).
        /// </summary>
        public static int BitScanReverse(ulong value)
        {
            if (value == 0) return -1;
            return 63 - BitOperations.LeadingZeroCount(value);
        }

        /// <summary>
        /// Finds the index of the last set bit (most significant bit).
        /// </summary>
        public static int BitScanReverse(uint value)
        {
            if (value == 0) return -1;
            return 31 - BitOperations.LeadingZeroCount(value);
        }

        /// <summary>
        /// Tests if a specific bit is set.
        /// </summary>
        public static bool BitTest(ulong value, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 64) return false;
            return (value & (1UL << bitIndex)) != 0;
        }

        /// <summary>
        /// Tests if a specific bit is set.
        /// </summary>
        public static bool BitTest(uint value, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 32) return false;
            return (value & (1U << bitIndex)) != 0;
        }

        /// <summary>
        /// Sets a specific bit.
        /// </summary>
        public static ulong BitSet(ulong value, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 64) return value;
            return value | (1UL << bitIndex);
        }

        /// <summary>
        /// Sets a specific bit.
        /// </summary>
        public static uint BitSet(uint value, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 32) return value;
            return value | (1U << bitIndex);
        }

        /// <summary>
        /// Clears a specific bit.
        /// </summary>
        public static ulong BitClear(ulong value, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 64) return value;
            return value & ~(1UL << bitIndex);
        }

        /// <summary>
        /// Clears a specific bit.
        /// </summary>
        public static uint BitClear(uint value, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 32) return value;
            return value & ~(1U << bitIndex);
        }

        /// <summary>
        /// Toggles a specific bit.
        /// </summary>
        public static ulong BitToggle(ulong value, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 64) return value;
            return value ^ (1UL << bitIndex);
        }

        /// <summary>
        /// Toggles a specific bit.
        /// </summary>
        public static uint BitToggle(uint value, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 32) return value;
            return value ^ (1U << bitIndex);
        }

        /// <summary>
        /// Creates a mask with the specified number of bits set.
        /// </summary>
        public static ulong Mask(int bitCount)
        {
            if (bitCount <= 0) return 0;
            if (bitCount >= 64) return ulong.MaxValue;
            return (1UL << bitCount) - 1;
        }

        /// <summary>
        /// Creates a mask with the specified number of bits set.
        /// </summary>
        public static uint Mask32(int bitCount)
        {
            if (bitCount <= 0) return 0;
            if (bitCount >= 32) return uint.MaxValue;
            return (1U << bitCount) - 1;
        }

        /// <summary>
        /// Rotates a value left by the specified number of bits.
        /// </summary>
        public static ulong RotateLeft(ulong value, int rotation)
        {
            return BitOperations.RotateLeft(value, rotation);
        }

        /// <summary>
        /// Rotates a value left by the specified number of bits.
        /// </summary>
        public static uint RotateLeft(uint value, int rotation)
        {
            return BitOperations.RotateLeft(value, rotation);
        }

        /// <summary>
        /// Rotates a value right by the specified number of bits.
        /// </summary>
        public static ulong RotateRight(ulong value, int rotation)
        {
            return BitOperations.RotateRight(value, rotation);
        }

        /// <summary>
        /// Rotates a value right by the specified number of bits.
        /// </summary>
        public static uint RotateRight(uint value, int rotation)
        {
            return BitOperations.RotateRight(value, rotation);
        }

        /// <summary>
        /// Counts the number of leading zero bits.
        /// </summary>
        public static int LeadingZeroCount(ulong value)
        {
            return BitOperations.LeadingZeroCount(value);
        }

        /// <summary>
        /// Counts the number of leading zero bits.
        /// </summary>
        public static int LeadingZeroCount(uint value)
        {
            return BitOperations.LeadingZeroCount(value);
        }

        /// <summary>
        /// Counts the number of trailing zero bits.
        /// </summary>
        public static int TrailingZeroCount(ulong value)
        {
            return BitOperations.TrailingZeroCount(value);
        }

        /// <summary>
        /// Counts the number of trailing zero bits.
        /// </summary>
        public static int TrailingZeroCount(uint value)
        {
            return BitOperations.TrailingZeroCount(value);
        }

        /// <summary>
        /// Checks if a value is a power of 2.
        /// </summary>
        public static bool IsPow2(ulong value)
        {
            return BitOperations.IsPow2(value);
        }

        /// <summary>
        /// Checks if a value is a power of 2.
        /// </summary>
        public static bool IsPow2(uint value)
        {
            return BitOperations.IsPow2(value);
        }

        /// <summary>
        /// Rounds up to the next power of 2.
        /// </summary>
        public static ulong RoundUpToPowerOf2(ulong value)
        {
            return BitOperations.RoundUpToPowerOf2(value);
        }

        /// <summary>
        /// Rounds up to the next power of 2.
        /// </summary>
        public static uint RoundUpToPowerOf2(uint value)
        {
            return BitOperations.RoundUpToPowerOf2(value);
        }

        /// <summary>
        /// Gets the bit count for a specific integer type.
        /// </summary>
        public static int GetBitCount<T>() where T : unmanaged
        {
            return typeof(T) switch
            {
                Type t when t == typeof(byte) || t == typeof(sbyte) => 8,
                Type t when t == typeof(ushort) || t == typeof(short) => 16,
                Type t when t == typeof(uint) || t == typeof(int) => 32,
                Type t when t == typeof(ulong) || t == typeof(long) => 64,
                _ => throw new ArgumentException($"Unsupported type: {typeof(T)}")
            };
        }

        /// <summary>
        /// Gets the bit count for a specific value.
        /// </summary>
        public static int GetBitCount<T>(T value) where T : unmanaged
        {
            return GetBitCount<T>();
        }
    }
}
