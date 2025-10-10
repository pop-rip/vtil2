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
using System.Collections.Generic;
using System.Linq;
using VTIL.Common.Math;

namespace VTIL.Architecture
{
    /// <summary>
    /// VTIL instruction set definitions.
    /// </summary>
    public static class InstructionSet
    {
        /// <summary>
        /// NOP instruction - No operation.
        /// </summary>
        public static readonly InstructionDescriptor Nop = InstructionDescriptor.Create(
            "nop",
            Array.Empty<OperandType>(),
            isVolatile: true
        );

        /// <summary>
        /// MOV instruction - Move value.
        /// </summary>
        public static readonly InstructionDescriptor Mov = InstructionDescriptor.Create(
            "mov",
            new[] { OperandType.Write, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.Cast
        );

        /// <summary>
        /// ADD instruction - Add values.
        /// </summary>
        public static readonly InstructionDescriptor Add = InstructionDescriptor.Create(
            "add",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.Add
        );

        /// <summary>
        /// SUB instruction - Subtract values.
        /// </summary>
        public static readonly InstructionDescriptor Sub = InstructionDescriptor.Create(
            "sub",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.Subtract
        );

        /// <summary>
        /// MUL instruction - Multiply values.
        /// </summary>
        public static readonly InstructionDescriptor Mul = InstructionDescriptor.Create(
            "mul",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.Multiply
        );

        /// <summary>
        /// DIV instruction - Divide values.
        /// </summary>
        public static readonly InstructionDescriptor Div = InstructionDescriptor.Create(
            "div",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.Divide
        );

        /// <summary>
        /// REM instruction - Remainder operation.
        /// </summary>
        public static readonly InstructionDescriptor Rem = InstructionDescriptor.Create(
            "rem",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.Remainder
        );

        /// <summary>
        /// AND instruction - Bitwise AND.
        /// </summary>
        public static readonly InstructionDescriptor And = InstructionDescriptor.Create(
            "and",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.BitwiseAnd
        );

        /// <summary>
        /// OR instruction - Bitwise OR.
        /// </summary>
        public static readonly InstructionDescriptor Or = InstructionDescriptor.Create(
            "or",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.BitwiseOr
        );

        /// <summary>
        /// XOR instruction - Bitwise XOR.
        /// </summary>
        public static readonly InstructionDescriptor Xor = InstructionDescriptor.Create(
            "xor",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.BitwiseXor
        );

        /// <summary>
        /// NOT instruction - Bitwise NOT.
        /// </summary>
        public static readonly InstructionDescriptor Not = InstructionDescriptor.Create(
            "not",
            new[] { OperandType.ReadWrite },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.BitwiseNot
        );

        /// <summary>
        /// SHL instruction - Shift left.
        /// </summary>
        public static readonly InstructionDescriptor Shl = InstructionDescriptor.Create(
            "shl",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.ShiftLeft
        );

        /// <summary>
        /// SHR instruction - Shift right.
        /// </summary>
        public static readonly InstructionDescriptor Shr = InstructionDescriptor.Create(
            "shr",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.ShiftRight
        );

        /// <summary>
        /// ROL instruction - Rotate left.
        /// </summary>
        public static readonly InstructionDescriptor Rol = InstructionDescriptor.Create(
            "rol",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.RotateLeft
        );

        /// <summary>
        /// ROR instruction - Rotate right.
        /// </summary>
        public static readonly InstructionDescriptor Ror = InstructionDescriptor.Create(
            "ror",
            new[] { OperandType.ReadWrite, OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.RotateRight
        );

        /// <summary>
        /// NEG instruction - Negate.
        /// </summary>
        public static readonly InstructionDescriptor Neg = InstructionDescriptor.Create(
            "neg",
            new[] { OperandType.ReadWrite },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.Negate
        );

        /// <summary>
        /// PUSH instruction - Push to stack.
        /// </summary>
        public static readonly InstructionDescriptor Push = InstructionDescriptor.Create(
            "push",
            new[] { OperandType.ReadAny },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.Push
        );

        /// <summary>
        /// POP instruction - Pop from stack.
        /// </summary>
        public static readonly InstructionDescriptor Pop = InstructionDescriptor.Create(
            "pop",
            new[] { OperandType.Write },
            accessSizeIndex: 1,
            symbolicOperator: OperatorId.Pop
        );

        /// <summary>
        /// READ instruction - Read from memory.
        /// </summary>
        public static readonly InstructionDescriptor Read = InstructionDescriptor.Create(
            "read",
            new[] { OperandType.Write, OperandType.ReadReg, OperandType.ReadImm },
            accessSizeIndex: 1,
            memoryOperandIndex: 1,
            memoryWrite: false,
            symbolicOperator: OperatorId.Read
        );

        /// <summary>
        /// WRITE instruction - Write to memory.
        /// </summary>
        public static readonly InstructionDescriptor Write = InstructionDescriptor.Create(
            "write",
            new[] { OperandType.ReadReg, OperandType.ReadImm, OperandType.ReadAny },
            accessSizeIndex: 2,
            memoryOperandIndex: 0,
            memoryWrite: true,
            symbolicOperator: OperatorId.Write
        );

        /// <summary>
        /// JMP instruction - Jump to address.
        /// </summary>
        public static readonly InstructionDescriptor Jmp = InstructionDescriptor.Create(
            "jmp",
            new[] { OperandType.ReadAny },
            branchOperandsVip: new[] { 0 },
            symbolicOperator: OperatorId.Jump
        );

        /// <summary>
        /// CALL instruction - Call function.
        /// </summary>
        public static readonly InstructionDescriptor Call = InstructionDescriptor.Create(
            "call",
            new[] { OperandType.ReadAny },
            branchOperandsVip: new[] { 0 },
            symbolicOperator: OperatorId.Call
        );

        /// <summary>
        /// RET instruction - Return from function.
        /// </summary>
        public static readonly InstructionDescriptor Ret = InstructionDescriptor.Create(
            "ret",
            Array.Empty<OperandType>(),
            isVolatile: true,
            symbolicOperator: OperatorId.Return
        );

        /// <summary>
        /// JE instruction - Jump if equal.
        /// </summary>
        public static readonly InstructionDescriptor Je = InstructionDescriptor.Create(
            "je",
            new[] { OperandType.ReadAny },
            branchOperandsVip: new[] { 0 }
        );

        /// <summary>
        /// JNE instruction - Jump if not equal.
        /// </summary>
        public static readonly InstructionDescriptor Jne = InstructionDescriptor.Create(
            "jne",
            new[] { OperandType.ReadAny },
            branchOperandsVip: new[] { 0 }
        );

        /// <summary>
        /// JL instruction - Jump if less than.
        /// </summary>
        public static readonly InstructionDescriptor Jl = InstructionDescriptor.Create(
            "jl",
            new[] { OperandType.ReadAny },
            branchOperandsVip: new[] { 0 }
        );

        /// <summary>
        /// JLE instruction - Jump if less than or equal.
        /// </summary>
        public static readonly InstructionDescriptor Jle = InstructionDescriptor.Create(
            "jle",
            new[] { OperandType.ReadAny },
            branchOperandsVip: new[] { 0 }
        );

        /// <summary>
        /// JG instruction - Jump if greater than.
        /// </summary>
        public static readonly InstructionDescriptor Jg = InstructionDescriptor.Create(
            "jg",
            new[] { OperandType.ReadAny },
            branchOperandsVip: new[] { 0 }
        );

        /// <summary>
        /// JGE instruction - Jump if greater than or equal.
        /// </summary>
        public static readonly InstructionDescriptor Jge = InstructionDescriptor.Create(
            "jge",
            new[] { OperandType.ReadAny },
            branchOperandsVip: new[] { 0 }
        );

        /// <summary>
        /// CMP instruction - Compare values.
        /// </summary>
        public static readonly InstructionDescriptor Cmp = InstructionDescriptor.Create(
            "cmp",
            new[] { OperandType.ReadAny, OperandType.ReadAny },
            accessSizeIndex: 1
        );

        /// <summary>
        /// TEST instruction - Test values.
        /// </summary>
        public static readonly InstructionDescriptor Test = InstructionDescriptor.Create(
            "test",
            new[] { OperandType.ReadAny, OperandType.ReadAny },
            accessSizeIndex: 1
        );

        /// <summary>
        /// SET instruction - Set flags.
        /// </summary>
        public static readonly InstructionDescriptor Set = InstructionDescriptor.Create(
            "set",
            new[] { OperandType.Write },
            accessSizeIndex: 1
        );

        /// <summary>
        /// SYSCALL instruction - System call.
        /// </summary>
        public static readonly InstructionDescriptor Syscall = InstructionDescriptor.Create(
            "syscall",
            Array.Empty<OperandType>(),
            isVolatile: true,
            symbolicOperator: OperatorId.Syscall
        );

        /// <summary>
        /// INTRINSIC instruction - Intrinsic operation.
        /// </summary>
        public static readonly InstructionDescriptor Intrinsic = InstructionDescriptor.Create(
            "intrinsic",
            new[] { OperandType.ReadImm },
            isVolatile: true,
            symbolicOperator: OperatorId.Intrinsic
        );

        /// <summary>
        /// VM_ENTER instruction - Virtual machine enter.
        /// </summary>
        public static readonly InstructionDescriptor VmEnter = InstructionDescriptor.Create(
            "vm_enter",
            new[] { OperandType.ReadAny },
            isVolatile: true,
            symbolicOperator: OperatorId.VmEnter
        );

        /// <summary>
        /// VM_EXIT instruction - Virtual machine exit.
        /// </summary>
        public static readonly InstructionDescriptor VmExit = InstructionDescriptor.Create(
            "vm_exit",
            Array.Empty<OperandType>(),
            isVolatile: true,
            symbolicOperator: OperatorId.VmExit
        );

        /// <summary>
        /// VM_CALL instruction - Virtual machine call.
        /// </summary>
        public static readonly InstructionDescriptor VmCall = InstructionDescriptor.Create(
            "vm_call",
            new[] { OperandType.ReadAny },
            isVolatile: true,
            symbolicOperator: OperatorId.VmCall
        );

        /// <summary>
        /// Gets all instruction descriptors.
        /// </summary>
        public static IReadOnlyList<InstructionDescriptor> GetAllInstructions()
        {
            return new[]
            {
                Nop, Mov, Add, Sub, Mul, Div, Rem,
                And, Or, Xor, Not, Shl, Shr, Rol, Ror, Neg,
                Push, Pop, Read, Write,
                Jmp, Call, Ret, Je, Jne, Jl, Jle, Jg, Jge,
                Cmp, Test, Set, Syscall, Intrinsic,
                VmEnter, VmExit, VmCall
            };
        }

        /// <summary>
        /// Gets instruction descriptor by name.
        /// </summary>
        public static InstructionDescriptor? GetInstruction(string name)
        {
            return GetAllInstructions().FirstOrDefault(inst => inst.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if an instruction name exists.
        /// </summary>
        public static bool HasInstruction(string name)
        {
            return GetInstruction(name) != null;
        }
    }
}
