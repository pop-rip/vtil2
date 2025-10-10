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
using VTIL.Common.Math;

namespace VTIL.SymEx
{
    /// <summary>
    /// This class allows O(1) approximation of tree-matching by storing a compressed signature.
    /// </summary>
    public readonly struct ExpressionSignature : IEquatable<ExpressionSignature>
    {
        /// <summary>
        /// Signature itself - three 64-bit integers for fast comparison.
        /// </summary>
        private readonly ulong _signature0;
        private readonly ulong _signature1;
        private readonly ulong _signature2;

        /// <summary>
        /// Signature hash.
        /// </summary>
        public ulong HashValue { get; }

        /// <summary>
        /// Creates an empty signature.
        /// </summary>
        public ExpressionSignature()
        {
            _signature0 = 0;
            _signature1 = 0;
            _signature2 = 0;
            HashValue = 0;
        }

        /// <summary>
        /// Creates a signature from a bit vector value.
        /// </summary>
        public ExpressionSignature(BigInteger value)
        {
            var bytes = value.ToByteArray();
            
            // Initialize signatures
            _signature0 = 0;
            _signature1 = 0;
            _signature2 = 0;

            // Fill signatures with the value
            for (int i = 0; i < Math.Min(bytes.Length, 24); i++)
            {
                var byteValue = (ulong)bytes[i];
                if (i < 8)
                    _signature0 |= byteValue << (i * 8);
                else if (i < 16)
                    _signature1 |= byteValue << ((i - 8) * 8);
                else
                    _signature2 |= byteValue << ((i - 16) * 8);
            }

            // Calculate hash
            HashValue = CalculateHash(_signature0, _signature1, _signature2);
        }

        /// <summary>
        /// Creates a signature for a unary operation.
        /// </summary>
        public ExpressionSignature(OperatorId op, ExpressionSignature rhs)
        {
            _signature0 = _signature0 | ((ulong)(int)op << 56);
            _signature1 = rhs._signature0;
            _signature2 = rhs._signature1;

            HashValue = CalculateHash(_signature0, _signature1, _signature2);
        }

        /// <summary>
        /// Creates a signature for a binary operation.
        /// </summary>
        public ExpressionSignature(ExpressionSignature lhs, OperatorId op, ExpressionSignature rhs)
        {
            _signature0 = lhs._signature0 | ((ulong)(int)op << 48);
            _signature1 = lhs._signature1 | (rhs._signature0 << 32);
            _signature2 = lhs._signature2 | (rhs._signature1 >> 32);

            HashValue = CalculateHash(_signature0, _signature1, _signature2);
        }

        /// <summary>
        /// Creates a signature with explicit values.
        /// </summary>
        private ExpressionSignature(ulong sig0, ulong sig1, ulong sig2, ulong hash)
        {
            _signature0 = sig0;
            _signature1 = sig1;
            _signature2 = sig2;
            HashValue = hash;
        }

        /// <summary>
        /// Shrinks the signature to a single 64-bit integer.
        /// </summary>
        public ulong Shrink()
        {
            return _signature0 ^ _signature1 ^ _signature2;
        }

        /// <summary>
        /// Checks if RHS can match into LHS.
        /// </summary>
        public bool CanMatch(ExpressionSignature other)
        {
            return (_signature0 & other._signature0) == other._signature0 &&
                   (_signature1 & other._signature1) == other._signature1 &&
                   (_signature2 & other._signature2) == other._signature2;
        }

        /// <summary>
        /// Calculates a hash from the signature components.
        /// </summary>
        private static ulong CalculateHash(ulong sig0, ulong sig1, ulong sig2)
        {
            // Simple hash combination
            return sig0 ^ RotateLeft(sig1, 21) ^ RotateLeft(sig2, 42);
        }

        /// <summary>
        /// Rotates a value left by the specified number of bits.
        /// </summary>
        private static ulong RotateLeft(ulong value, int rotation)
        {
            return (value << rotation) | (value >> (64 - rotation));
        }

        /// <summary>
        /// Gets the operator ID from the signature (if it represents an operation).
        /// </summary>
        public OperatorId? GetOperator()
        {
            var opBits = (_signature0 >> 48) & 0xFF;
            if (opBits == 0) return null;
            return (OperatorId)opBits;
        }

        /// <summary>
        /// Checks if this signature represents a constant value.
        /// </summary>
        public bool IsConstant()
        {
            return (_signature0 >> 56) == 0; // No operator in the high bits
        }

        /// <summary>
        /// Checks if this signature represents a unary operation.
        /// </summary>
        public bool IsUnaryOperation()
        {
            var opBits = (_signature0 >> 56) & 0xFF;
            return opBits != 0 && (_signature0 >> 48) == 0; // Operator in high bits, no operator in mid bits
        }

        /// <summary>
        /// Checks if this signature represents a binary operation.
        /// </summary>
        public bool IsBinaryOperation()
        {
            var opBits = (_signature0 >> 48) & 0xFF;
            return opBits != 0; // Operator in mid bits
        }

        /// <summary>
        /// Gets the depth of the expression represented by this signature.
        /// </summary>
        public int GetDepth()
        {
            if (IsConstant()) return 0;
            if (IsUnaryOperation()) return 1;
            if (IsBinaryOperation()) return 2;
            return 0;
        }

        /// <summary>
        /// Combines two signatures with an operator.
        /// </summary>
        public static ExpressionSignature Combine(ExpressionSignature lhs, OperatorId op, ExpressionSignature rhs)
        {
            return new ExpressionSignature(lhs, op, rhs);
        }

        /// <summary>
        /// Applies an operator to a signature.
        /// </summary>
        public static ExpressionSignature Apply(OperatorId op, ExpressionSignature operand)
        {
            return new ExpressionSignature(op, operand);
        }

        public bool Equals(ExpressionSignature other)
        {
            return _signature0 == other._signature0 &&
                   _signature1 == other._signature1 &&
                   _signature2 == other._signature2;
        }

        public override bool Equals(object? obj)
        {
            return obj is ExpressionSignature other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)HashValue;
        }

        public static bool operator ==(ExpressionSignature left, ExpressionSignature right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ExpressionSignature left, ExpressionSignature right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            if (IsConstant())
                return $"const(0x{_signature0:X})";

            var op = GetOperator();
            if (op.HasValue)
            {
                if (IsUnaryOperation())
                    return $"{op.Value.GetName()}(...)";
                else
                    return $"(... {op.Value.GetName()} ...)";
            }

            return $"sig(0x{_signature0:X16}, 0x{_signature1:X16}, 0x{_signature2:X16})";
        }
    }
}
