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
using System.Threading;
using Microsoft.Extensions.Logging;

namespace VTIL.Common.IO
{
    /// <summary>
    /// Console colors for logging output.
    /// </summary>
    public enum ConsoleColor
    {
        Bright = 15,
        Yellow = 14,
        Purple = 13,
        Red = 12,
        Cyan = 11,
        Green = 10,
        Blue = 9,
        Default = 7
    }

    /// <summary>
    /// Logger state management for VTIL.
    /// </summary>
    public sealed class LoggerState
    {
        private readonly object _lock = new object();
        private volatile bool _mute = false;
        private volatile int _padding = -1;
        private volatile int _paddingCarry = 0;
        private volatile bool _ansiEscapeCodes = true;

        /// <summary>
        /// Whether logging output is muted.
        /// </summary>
        public bool Mute
        {
            get => _mute;
            set => _mute = value;
        }

        /// <summary>
        /// Current padding level for indentation.
        /// </summary>
        public int Padding
        {
            get => _padding;
            set => _padding = value;
        }

        /// <summary>
        /// Padding leftover from previous print.
        /// </summary>
        public int PaddingCarry
        {
            get => _paddingCarry;
            set => _paddingCarry = value;
        }

        /// <summary>
        /// Whether to use ANSI escape codes for colors.
        /// </summary>
        public bool AnsiEscapeCodes
        {
            get => _ansiEscapeCodes;
            set => _ansiEscapeCodes = value;
        }

        /// <summary>
        /// Locks the logger state for thread-safe operations.
        /// </summary>
        public void Lock()
        {
            Monitor.Enter(_lock);
        }

        /// <summary>
        /// Unlocks the logger state.
        /// </summary>
        public void Unlock()
        {
            Monitor.Exit(_lock);
        }

        /// <summary>
        /// Attempts to lock the logger state.
        /// </summary>
        public bool TryLock()
        {
            return Monitor.TryEnter(_lock);
        }

        /// <summary>
        /// Attempts to lock the logger state with a timeout.
        /// </summary>
        public bool TryLock(TimeSpan maxWait)
        {
            return Monitor.TryEnter(_lock, maxWait);
        }
    }

    /// <summary>
    /// VTIL logging functionality.
    /// </summary>
    public static class Logger
    {
        private static readonly LoggerState _state = new LoggerState();
        private static readonly char LogPaddingChar = '|';
        private static readonly int LogPaddingStep = 2;

        /// <summary>
        /// Global error hook for custom error handling.
        /// </summary>
        public static Action<string>? ErrorHook { get; set; }

        /// <summary>
        /// Changes the console color where possible.
        /// </summary>
        public static void SetColor(ConsoleColor color)
        {
            if (_state.AnsiEscapeCodes)
            {
                var colorCode = color switch
                {
                    ConsoleColor.Bright => "\x1b[97m",  // Bright white
                    ConsoleColor.Yellow => "\x1b[93m",  // Bright yellow
                    ConsoleColor.Purple => "\x1b[95m",  // Bright magenta
                    ConsoleColor.Red => "\x1b[91m",     // Bright red
                    ConsoleColor.Cyan => "\x1b[96m",    // Bright cyan
                    ConsoleColor.Green => "\x1b[92m",   // Bright green
                    ConsoleColor.Blue => "\x1b[94m",    // Bright blue
                    ConsoleColor.Default => "\x1b[0m",  // Reset
                    _ => "\x1b[0m"
                };
                Console.Write(colorCode);
            }
        }

        /// <summary>
        /// RAII-style scope for managing padding indentation.
        /// </summary>
        public sealed class ScopePadding : IDisposable
        {
            private bool _active = true;
            private readonly int _prevPadding;

            public ScopePadding(int padding)
            {
                _state.Lock();
                _prevPadding = _state.Padding;
                _state.Padding += padding;
            }

            public void Dispose()
            {
                if (_active)
                {
                    _state.Padding = _prevPadding;
                    _state.Unlock();
                    _active = false;
                }
            }
        }

        /// <summary>
        /// RAII-style scope for managing verbosity.
        /// </summary>
        public sealed class ScopeVerbosity : IDisposable
        {
            private bool _active = true;
            private readonly bool _prevMute;

            public ScopeVerbosity(bool verboseOutput)
            {
                _state.Lock();
                _prevMute = _state.Mute;
                _state.Mute |= !verboseOutput;
            }

            public void Dispose()
            {
                if (_active)
                {
                    _state.Mute = _prevMute;
                    _state.Unlock();
                    _active = false;
                }
            }
        }

        /// <summary>
        /// Logs a message with specified color.
        /// </summary>
        public static void Log(ConsoleColor color, string format, params object[] args)
        {
            lock (_state)
            {
                if (_state.Mute) return;

                int outputCount = 0;

                // Handle padding
                if (_state.Padding > 0)
                {
                    int padBy = _state.Padding - _state.PaddingCarry;
                    if (padBy > 0)
                    {
                        for (int i = 0; i < padBy; i++)
                        {
                            if ((i + 1) == padBy)
                            {
                                Console.Write(new string(' ', LogPaddingStep - 1));
                                if (format.Length > 0 && format[0] == ' ')
                                    Console.Write(LogPaddingChar);
                            }
                            else
                            {
                                Console.Write(new string(' ', LogPaddingStep - 1));
                                Console.Write(LogPaddingChar);
                            }
                        }
                    }

                    // Set or clear carry for next
                    if (format.EndsWith("\n"))
                        _state.PaddingCarry = 0;
                    else
                        _state.PaddingCarry = _state.Padding;
                }

                // Set color and output
                SetColor(color);

                if (args.Length == 0)
                    Console.Write(format);
                else
                    Console.Write(format, args);

                SetColor(ConsoleColor.Default);
            }
        }

        /// <summary>
        /// Logs a message with default color.
        /// </summary>
        public static void Log(string format, params object[] args)
        {
            Log(ConsoleColor.Default, format, args);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public static void Warning(string format, params object[] args)
        {
            string message = string.Format(format, args);

            bool locked = _state.TryLock(TimeSpan.FromMilliseconds(100));

            SetColor(ConsoleColor.Yellow);
            Console.Error.WriteLine($"[!] Warning: {message}");

            if (locked)
                _state.Unlock();
        }

        /// <summary>
        /// Logs an error message and throws an exception.
        /// </summary>
        public static void Error(string format, params object[] args)
        {
            string message = string.Format(format, args);

            // Call error hook if present
            ErrorHook?.Invoke(message);

            bool locked = _state.TryLock(TimeSpan.FromMilliseconds(100));

            SetColor(ConsoleColor.Red);
            Console.Error.WriteLine($"[*] Error: {message}");

            // Don't unlock since we're about to throw
            Asserts.Unreachable(message);
        }
    }
}
