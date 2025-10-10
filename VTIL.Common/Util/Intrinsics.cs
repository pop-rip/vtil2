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
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace VTIL.Common.Util
{
    /// <summary>
    /// Provides access to CPU intrinsics and low-level operations.
    /// </summary>
    public static class Intrinsics
    {
        /// <summary>
        /// Gets the current timestamp for timing measurements.
        /// </summary>
        public static ulong TimeStamp()
        {
            return (ulong)System.Diagnostics.Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Pauses execution for a short time (CPU pause instruction).
        /// </summary>
        public static void Pause()
        {
            System.Threading.Thread.SpinWait(1);
        }

        /// <summary>
        /// Memory fence to ensure all memory operations complete.
        /// </summary>
        public static void MemoryFence()
        {
            System.Threading.Thread.MemoryBarrier();
        }

        /// <summary>
        /// Compares and exchanges a value atomically.
        /// </summary>
        public static T CompareExchange<T>(ref T location, T value, T comparand) where T : class
        {
            return System.Threading.Interlocked.CompareExchange(ref location, value, comparand);
        }

        /// <summary>
        /// Compares and exchanges a value atomically.
        /// </summary>
        public static int CompareExchange(ref int location, int value, int comparand)
        {
            return System.Threading.Interlocked.CompareExchange(ref location, value, comparand);
        }

        /// <summary>
        /// Compares and exchanges a value atomically.
        /// </summary>
        public static long CompareExchange(ref long location, long value, long comparand)
        {
            return System.Threading.Interlocked.CompareExchange(ref location, value, comparand);
        }

        /// <summary>
        /// Exchanges a value atomically.
        /// </summary>
        public static T Exchange<T>(ref T location, T value) where T : class
        {
            return System.Threading.Interlocked.Exchange(ref location, value);
        }

        /// <summary>
        /// Exchanges a value atomically.
        /// </summary>
        public static int Exchange(ref int location, int value)
        {
            return System.Threading.Interlocked.Exchange(ref location, value);
        }

        /// <summary>
        /// Exchanges a value atomically.
        /// </summary>
        public static long Exchange(ref long location, long value)
        {
            return System.Threading.Interlocked.Exchange(ref location, value);
        }

        /// <summary>
        /// Adds a value atomically.
        /// </summary>
        public static int Add(ref int location, int value)
        {
            return System.Threading.Interlocked.Add(ref location, value);
        }

        /// <summary>
        /// Adds a value atomically.
        /// </summary>
        public static long Add(ref long location, long value)
        {
            return System.Threading.Interlocked.Add(ref location, value);
        }

        /// <summary>
        /// Increments a value atomically.
        /// </summary>
        public static int Increment(ref int location)
        {
            return System.Threading.Interlocked.Increment(ref location);
        }

        /// <summary>
        /// Increments a value atomically.
        /// </summary>
        public static long Increment(ref long location)
        {
            return System.Threading.Interlocked.Increment(ref location);
        }

        /// <summary>
        /// Decrements a value atomically.
        /// </summary>
        public static int Decrement(ref int location)
        {
            return System.Threading.Interlocked.Decrement(ref location);
        }

        /// <summary>
        /// Decrements a value atomically.
        /// </summary>
        public static long Decrement(ref long location)
        {
            return System.Threading.Interlocked.Decrement(ref location);
        }

        /// <summary>
        /// Reads a value with acquire semantics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadAcquire<T>(ref T location) where T : class
        {
            var value = location;
            MemoryFence();
            return value;
        }

        /// <summary>
        /// Writes a value with release semantics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRelease<T>(ref T location, T value) where T : class
        {
            MemoryFence();
            location = value;
        }

        /// <summary>
        /// Gets the CPU cycle count (if available).
        /// </summary>
        public static ulong CpuCycles()
        {
            return TimeStamp();
        }

        /// <summary>
        /// Checks if CPU intrinsics are available.
        /// </summary>
        public static bool HasIntrinsics => System.Runtime.Intrinsics.X86.X86Base.IsSupported;

        /// <summary>
        /// Gets the CPU vendor string.
        /// </summary>
        public static string GetCpuVendor()
        {
            if (System.Runtime.Intrinsics.X86.X86Base.IsSupported)
            {
                var cpuid = System.Runtime.Intrinsics.X86.X86Base.CpuId(0, 0);
                var vendor = new char[12];
                var eax = (uint)cpuid.Eax;
                var ebx = (uint)cpuid.Ebx;
                var ecx = (uint)cpuid.Ecx;
                var edx = (uint)cpuid.Edx;

                // Extract vendor string from CPUID result
                BitConverter.GetBytes(ebx).CopyTo(vendor, 0);
                BitConverter.GetBytes(edx).CopyTo(vendor, 4);
                BitConverter.GetBytes(ecx).CopyTo(vendor, 8);

                return new string(vendor);
            }
            return "Unknown";
        }

        /// <summary>
        /// Gets CPU feature flags.
        /// </summary>
        public static CpuFeatures GetCpuFeatures()
        {
            var features = new CpuFeatures();

            if (System.Runtime.Intrinsics.X86.X86Base.IsSupported)
            {
                features.X86Base = true;

                if (System.Runtime.Intrinsics.X86.Sse.IsSupported)
                    features.Sse = true;
                if (System.Runtime.Intrinsics.X86.Sse2.IsSupported)
                    features.Sse2 = true;
                if (System.Runtime.Intrinsics.X86.Sse3.IsSupported)
                    features.Sse3 = true;
                if (System.Runtime.Intrinsics.X86.Ssse3.IsSupported)
                    features.Ssse3 = true;
                if (System.Runtime.Intrinsics.X86.Sse41.IsSupported)
                    features.Sse41 = true;
                if (System.Runtime.Intrinsics.X86.Sse42.IsSupported)
                    features.Sse42 = true;
                if (System.Runtime.Intrinsics.X86.Avx.IsSupported)
                    features.Avx = true;
                if (System.Runtime.Intrinsics.X86.Avx2.IsSupported)
                    features.Avx2 = true;
                if (System.Runtime.Intrinsics.X86.Avx512F.IsSupported)
                    features.Avx512F = true;
                if (System.Runtime.Intrinsics.X86.Bmi1.IsSupported)
                    features.Bmi1 = true;
                if (System.Runtime.Intrinsics.X86.Bmi2.IsSupported)
                    features.Bmi2 = true;
                if (System.Runtime.Intrinsics.X86.Lzcnt.IsSupported)
                    features.Lzcnt = true;
                if (System.Runtime.Intrinsics.X86.Popcnt.IsSupported)
                    features.Popcnt = true;
            }

            return features;
        }

        /// <summary>
        /// CPU feature flags.
        /// </summary>
        public struct CpuFeatures
        {
            public bool X86Base;
            public bool Sse;
            public bool Sse2;
            public bool Sse3;
            public bool Ssse3;
            public bool Sse41;
            public bool Sse42;
            public bool Avx;
            public bool Avx2;
            public bool Avx512F;
            public bool Bmi1;
            public bool Bmi2;
            public bool Lzcnt;
            public bool Popcnt;
        }
    }
}
