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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VTIL.Common.IO
{
    /// <summary>
    /// Provides assertion utilities for debugging and validation.
    /// </summary>
    public static class Asserts
    {
        /// <summary>
        /// Asserts that a condition is true, throwing an exception if false.
        /// </summary>
        /// <param name="condition">The condition to assert</param>
        /// <param name="message">Optional error message</param>
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string? message = null)
        {
            if (!condition)
            {
                throw new AssertionException(message ?? "Assertion failed");
            }
        }

        /// <summary>
        /// Asserts that a condition is true, throwing an exception if false.
        /// </summary>
        /// <param name="condition">The condition to assert</param>
        /// <param name="message">Error message with format parameters</param>
        /// <param name="args">Format arguments</param>
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message, params object[] args)
        {
            if (!condition)
            {
                throw new AssertionException(string.Format(message, args));
            }
        }

        /// <summary>
        /// Marks code as unreachable, throwing an exception if executed.
        /// </summary>
        /// <param name="message">Optional error message</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Unreachable(string? message = null)
        {
            throw new UnreachableException(message ?? "Unreachable code executed");
        }

        /// <summary>
        /// Marks code as unreachable, throwing an exception if executed.
        /// </summary>
        /// <param name="message">Error message with format parameters</param>
        /// <param name="args">Format arguments</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Unreachable(string message, params object[] args)
        {
            throw new UnreachableException(string.Format(message, args));
        }

        /// <summary>
        /// Asserts that an object is not null.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="paramName">The parameter name for the exception</param>
        /// <returns>The value if not null</returns>
        public static T NotNull<T>(T? value, string? paramName = null) where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(paramName);
            }
            return value;
        }

        /// <summary>
        /// Asserts that a nullable value is not null.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="paramName">The parameter name for the exception</param>
        /// <returns>The value if not null</returns>
        public static T NotNull<T>(T? value, string? paramName = null) where T : struct
        {
            if (!value.HasValue)
            {
                throw new ArgumentNullException(paramName);
            }
            return value.Value;
        }
    }

    /// <summary>
    /// Exception thrown when an assertion fails.
    /// </summary>
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
        public AssertionException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when unreachable code is executed.
    /// </summary>
    public class UnreachableException : Exception
    {
        public UnreachableException(string message) : base(message) { }
        public UnreachableException(string message, Exception innerException) : base(message, innerException) { }
    }
}
