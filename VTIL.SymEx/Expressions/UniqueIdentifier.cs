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

namespace VTIL.SymEx
{
    /// <summary>
    /// Unique identifier type to be used within symbolic expression context.
    /// </summary>
    public sealed class UniqueIdentifier : IEquatable<UniqueIdentifier>, IComparable<UniqueIdentifier>
    {
        /// <summary>
        /// Hash of the identifier.
        /// </summary>
        public ulong HashValue { get; }

        /// <summary>
        /// The value stored in this identifier.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Creates a unique identifier from a string.
        /// </summary>
        public UniqueIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            Value = name;
            HashValue = (ulong)name.GetHashCode();
        }

        /// <summary>
        /// Creates a unique identifier from a value with an optional name.
        /// </summary>
        public UniqueIdentifier(object value, string? name = null)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            
            if (!string.IsNullOrEmpty(name))
            {
                HashValue = (ulong)name.GetHashCode();
            }
            else
            {
                HashValue = (ulong)Value.GetHashCode();
            }
        }

        /// <summary>
        /// Creates a unique identifier from a numeric value.
        /// </summary>
        public UniqueIdentifier(BigInteger value)
        {
            Value = value;
            HashValue = (ulong)value.GetHashCode();
        }

        /// <summary>
        /// Creates a unique identifier from an integer value.
        /// </summary>
        public UniqueIdentifier(long value)
        {
            Value = value;
            HashValue = (ulong)value.GetHashCode();
        }

        /// <summary>
        /// Creates a unique identifier from an unsigned integer value.
        /// </summary>
        public UniqueIdentifier(ulong value)
        {
            Value = value;
            HashValue = value; // Use the value itself as hash for simple numeric types
        }

        /// <summary>
        /// Gets the string representation of this identifier.
        /// </summary>
        public string ToString()
        {
            return Value switch
            {
                string str => str,
                BigInteger bi => bi.ToString(),
                long l => l.ToString(),
                ulong ul => ul.ToString(),
                int i => i.ToString(),
                uint ui => ui.ToString(),
                short s => s.ToString(),
                ushort us => us.ToString(),
                byte b => b.ToString(),
                sbyte sb => sb.ToString(),
                _ => Value.ToString() ?? "null"
            };
        }

        /// <summary>
        /// Gets the value as a specific type.
        /// </summary>
        public T GetValue<T>()
        {
            if (Value is T t)
                return t;
            
            if (typeof(T) == typeof(string))
                return (T)(object)ToString();
            
            throw new InvalidCastException($"Cannot convert {Value.GetType()} to {typeof(T)}");
        }

        /// <summary>
        /// Tries to get the value as a specific type.
        /// </summary>
        public bool TryGetValue<T>(out T value)
        {
            try
            {
                value = GetValue<T>();
                return true;
            }
            catch
            {
                value = default(T)!;
                return false;
            }
        }

        /// <summary>
        /// Checks if this identifier represents a string value.
        /// </summary>
        public bool IsString => Value is string;

        /// <summary>
        /// Checks if this identifier represents a numeric value.
        /// </summary>
        public bool IsNumeric => Value is BigInteger or long or ulong or int or uint or short or ushort or byte or sbyte;

        /// <summary>
        /// Gets the value as a BigInteger if it's numeric.
        /// </summary>
        public BigInteger? GetNumericValue()
        {
            return Value switch
            {
                BigInteger bi => bi,
                long l => l,
                ulong ul => ul,
                int i => i,
                uint ui => ui,
                short s => s,
                ushort us => us,
                byte b => b,
                sbyte sb => sb,
                _ => null
            };
        }

        public bool Equals(UniqueIdentifier? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (HashValue != other.HashValue) return false;

            return Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as UniqueIdentifier);
        }

        public int CompareTo(UniqueIdentifier? other)
        {
            if (other is null) return 1;

            // First compare by hash
            var hashComparison = HashValue.CompareTo(other.HashValue);
            if (hashComparison != 0) return hashComparison;

            // If hashes are equal, compare by string representation
            return string.Compare(ToString(), other.ToString(), StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return (int)HashValue;
        }

        public static bool operator ==(UniqueIdentifier? left, UniqueIdentifier? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UniqueIdentifier? left, UniqueIdentifier? right)
        {
            return !Equals(left, right);
        }

        public static bool operator <(UniqueIdentifier? left, UniqueIdentifier? right)
        {
            return left?.CompareTo(right) < 0;
        }

        public static bool operator >(UniqueIdentifier? left, UniqueIdentifier? right)
        {
            return left?.CompareTo(right) > 0;
        }

        public static bool operator <=(UniqueIdentifier? left, UniqueIdentifier? right)
        {
            return left?.CompareTo(right) <= 0;
        }

        public static bool operator >=(UniqueIdentifier? left, UniqueIdentifier? right)
        {
            return left?.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Implicit conversion from string.
        /// </summary>
        public static implicit operator UniqueIdentifier(string name)
        {
            return new UniqueIdentifier(name);
        }

        /// <summary>
        /// Implicit conversion to string.
        /// </summary>
        public static implicit operator string(UniqueIdentifier identifier)
        {
            return identifier.ToString();
        }

        /// <summary>
        /// Implicit conversion from BigInteger.
        /// </summary>
        public static implicit operator UniqueIdentifier(BigInteger value)
        {
            return new UniqueIdentifier(value);
        }

        /// <summary>
        /// Implicit conversion from long.
        /// </summary>
        public static implicit operator UniqueIdentifier(long value)
        {
            return new UniqueIdentifier(value);
        }

        /// <summary>
        /// Implicit conversion from ulong.
        /// </summary>
        public static implicit operator UniqueIdentifier(ulong value)
        {
            return new UniqueIdentifier(value);
        }
    }
}
