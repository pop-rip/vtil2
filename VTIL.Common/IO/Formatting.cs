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
using System.Globalization;
using System.Numerics;
using System.Text;

namespace VTIL.Common.IO
{
    /// <summary>
    /// Provides formatting utilities for VTIL output.
    /// </summary>
    public static class Formatting
    {
        /// <summary>
        /// Suffix map for size formatting.
        /// </summary>
        public static readonly char[] SuffixMap = new char[256];

        static Formatting()
        {
            SuffixMap[1] = 'b';   // 8-bit
            SuffixMap[2] = 'w';   // 16-bit
            SuffixMap[4] = 'd';   // 32-bit
            SuffixMap[8] = 'q';   // 64-bit
            SuffixMap[16] = 'o';  // 128-bit
            SuffixMap[32] = 'h';  // 256-bit
            SuffixMap[64] = 'x';  // 512-bit
        }

        /// <summary>
        /// Formats a string with parameters.
        /// </summary>
        public static string Str(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Formats a value as a string.
        /// </summary>
        public static string AsString(object? value)
        {
            if (value is null)
                return "null";

            return value switch
            {
                bool b => b ? "true" : "false",
                byte b => $"0x{b:X2}",
                sbyte sb => sb.ToString(CultureInfo.InvariantCulture),
                short s => s.ToString(CultureInfo.InvariantCulture),
                ushort us => us.ToString(CultureInfo.InvariantCulture),
                int i => i.ToString(CultureInfo.InvariantCulture),
                uint ui => ui.ToString(CultureInfo.InvariantCulture),
                long l => l.ToString(CultureInfo.InvariantCulture),
                ulong ul => ul.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString("G", CultureInfo.InvariantCulture),
                double d => d.ToString("G", CultureInfo.InvariantCulture),
                decimal dec => dec.ToString(CultureInfo.InvariantCulture),
                BigInteger bi => bi.ToString(CultureInfo.InvariantCulture),
                string s => s,
                _ => value.ToString() ?? "null"
            };
        }

        /// <summary>
        /// Fixes parameter formatting for output.
        /// </summary>
        public static object FixParameter<T>(T parameter)
        {
            return parameter switch
            {
                bool b => b ? 1 : 0,
                byte b => b,
                sbyte sb => sb,
                short s => s,
                ushort us => us,
                int i => i,
                uint ui => ui,
                long l => l,
                ulong ul => ul,
                float f => f,
                double d => d,
                decimal dec => dec,
                BigInteger bi => bi,
                string s => s,
                null => "null",
                _ => (object)parameter,
            };
        }

        /// <summary>
        /// Formats a value in hexadecimal.
        /// </summary>
        public static string Hex(ulong value, int digits = 0)
        {
            return value.ToString("X");
        }

        /// <summary>
        /// Formats a value in hexadecimal with leading zeros.
        /// </summary>
        public static string HexPad(ulong value, int digits)
        {
            return value.ToString("X");
        }

        /// <summary>
        /// Formats a value in binary.
        /// </summary>
        public static string Binary(ulong value, int bits = 64)
        {
            var sb = new StringBuilder();
            sb.Append("0b");
            
            for (int i = bits - 1; i >= 0; i--)
            {
                sb.Append((value & (1UL << i)) != 0 ? '1' : '0');
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Formats a size with appropriate suffix.
        /// </summary>
        public static string Size(ulong bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F1} {suffixes[suffixIndex]}";
        }

        /// <summary>
        /// Formats a time span in a human-readable format.
        /// </summary>
        public static string TimeSpan(TimeSpan ts)
        {
            if (ts.TotalDays >= 1)
                return $"{ts.Days}d {ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
            if (ts.TotalHours >= 1)
                return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
            if (ts.TotalMinutes >= 1)
                return $"{ts.Minutes}m {ts.Seconds}s";
            if (ts.TotalSeconds >= 1)
                return $"{ts.Seconds}s {ts.Milliseconds}ms";
            return $"{ts.Milliseconds}ms";
        }
    }
}
