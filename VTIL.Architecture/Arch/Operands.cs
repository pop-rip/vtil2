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
    /// Describes the type of operand in relation to the instruction.
    /// </summary>
    public enum OperandType : byte
    {
        /// <summary>
        /// Invalid operand type.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Read immediate value.
        /// </summary>
        ReadImm = 1,

        /// <summary>
        /// Read register.
        /// </summary>
        ReadReg = 2,

        /// <summary>
        /// Read any operand (immediate or register).
        /// </summary>
        ReadAny = 3,

        /// <summary>
        /// Alias for ReadAny.
        /// </summary>
        Read = ReadAny,

        /// <summary>
        /// Write to register (implicit "_reg" as we cannot write into an immediate).
        /// </summary>
        Write = 4,

        /// <summary>
        /// Read and write to register.
        /// </summary>
        ReadWrite = 5
    }

    /// <summary>
    /// Extension methods for operand types.
    /// </summary>
    public static class OperandTypeExtensions
    {
        /// <summary>
        /// Checks if the operand type is a read operation.
        /// </summary>
        public static bool IsRead(this OperandType type)
        {
            return type is OperandType.ReadImm or OperandType.ReadReg or OperandType.ReadAny or OperandType.ReadWrite;
        }

        /// <summary>
        /// Checks if the operand type is a write operation.
        /// </summary>
        public static bool IsWrite(this OperandType type)
        {
            return type is OperandType.Write or OperandType.ReadWrite;
        }

        /// <summary>
        /// Checks if the operand type is a read-write operation.
        /// </summary>
        public static bool IsReadWrite(this OperandType type)
        {
            return type == OperandType.ReadWrite;
        }

        /// <summary>
        /// Checks if the operand type is an immediate read.
        /// </summary>
        public static bool IsImmediateRead(this OperandType type)
        {
            return type == OperandType.ReadImm;
        }

        /// <summary>
        /// Checks if the operand type is a register read.
        /// </summary>
        public static bool IsRegisterRead(this OperandType type)
        {
            return type is OperandType.ReadReg or OperandType.ReadWrite;
        }

        /// <summary>
        /// Checks if the operand type is a register write.
        /// </summary>
        public static bool IsRegisterWrite(this OperandType type)
        {
            return type is OperandType.Write or OperandType.ReadWrite;
        }

        /// <summary>
        /// Gets the operand type name as a string.
        /// </summary>
        public static string GetName(this OperandType type)
        {
            return type switch
            {
                OperandType.Invalid => "invalid",
                OperandType.ReadImm => "read_imm",
                OperandType.ReadReg => "read_reg",
                OperandType.ReadAny => "read_any",
                OperandType.Write => "write",
                OperandType.ReadWrite => "readwrite",
                _ => $"<unknown:{type}>"
            };
        }
    }

    /// <summary>
    /// Represents an operand in a VTIL instruction.
    /// </summary>
    public struct Operand : IEquatable<Operand>
    {
        private RegisterDescriptor register;

        /// <summary>
        /// The type of operand.
        /// </summary>
        public OperandType Type { get; }

        /// <summary>
        /// The immediate value (if applicable).
        /// </summary>
        public BigInteger Immediate { get; }

        public BigInteger GetImmediate()
        {
            return Immediate;
        }

        /// <summary>
        /// The register descriptor (if applicable).
        /// </summary>
        public RegisterDescriptor Register => register;

        public bool IsMemory => !IsRegister && !IsImmediate;

        public RegisterDescriptor GetRegister()
        {
            return Register;
        }

        public void SetRegister(RegisterDescriptor reg)
        {
            register = reg;
        }

        /// <summary>
        /// The access size in bits.
        /// </summary>
        public BitCntT AccessSize { get; }

        /// <summary>
        /// Creates an immediate operand.
        /// </summary>
        public static Operand CreateImmediate(BigInteger value, BitCntT accessSize)
        {
            return new Operand(OperandType.ReadImm, value, null, accessSize);
        }

        /// <summary>
        /// Creates a register operand.
        /// </summary>
        public static Operand CreateRegister(RegisterDescriptor register, OperandType type, BitCntT accessSize)
        {
            return new Operand(type, 0, register, accessSize);
        }

        /// <summary>
        /// Creates a read-only register operand.
        /// </summary>
        public static Operand CreateReadRegister(RegisterDescriptor register, BitCntT accessSize)
        {
            return new Operand(OperandType.ReadReg, 0, register, accessSize);
        }

        /// <summary>
        /// Creates a write-only register operand.
        /// </summary>
        public static Operand CreateWriteRegister(RegisterDescriptor register, BitCntT accessSize)
        {
            return new Operand(OperandType.Write, 0, register, accessSize);
        }

        /// <summary>
        /// Creates a read-write register operand.
        /// </summary>
        public static Operand CreateReadWriteRegister(RegisterDescriptor register, BitCntT accessSize)
        {
            return new Operand(OperandType.ReadWrite, 0, register, accessSize);
        }

        private Operand(OperandType type, BigInteger immediate, RegisterDescriptor? register, BitCntT accessSize)
        {
            Type = type;
            Immediate = immediate;
            this.register = register.Value;
            AccessSize = accessSize;
        }

        /// <summary>
        /// Checks if this is an immediate operand.
        /// </summary>
        public bool IsImmediate => Type == OperandType.ReadImm;

        /// <summary>
        /// Checks if this is a register operand.
        /// </summary>
        public bool IsRegister => Register != null;

        /// <summary>
        /// Checks if this operand is readable.
        /// </summary>
        public bool IsReadable => Type.IsRead();

        /// <summary>
        /// Checks if this operand is writable.
        /// </summary>
        public bool IsWritable => Type.IsWrite();

        /// <summary>
        /// Checks if this operand is read-write.
        /// </summary>
        public bool IsReadWrite => Type.IsReadWrite();

        /// <summary>
        /// Gets the size of the operand in bytes.
        /// </summary>
        public int SizeInBytes => AccessSize.Value / 8;

        /// <summary>
        /// Gets the size of the operand in bits.
        /// </summary>
        public int SizeInBits => AccessSize.Value;

        public bool Equals(Operand other)
        {
            return Type == other.Type && 
                   Immediate.Equals(other.Immediate) && 
                   Equals(Register, other.Register) && 
                   AccessSize.Equals(other.AccessSize);
        }

        public override bool Equals(object? obj)
        {
            return obj is Operand other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, Immediate, Register, AccessSize);
        }

        public static bool operator ==(Operand left, Operand right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Operand left, Operand right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return Type switch
            {
                OperandType.ReadImm => $"imm({Immediate})",
                OperandType.ReadReg => $"read({Register})",
                OperandType.Write => $"write({Register})",
                OperandType.ReadWrite => $"readwrite({Register})",
                _ => $"invalid({Type})"
            };
        }
    }
}
