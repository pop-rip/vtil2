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

namespace VTIL.Common.Math
{
    /// <summary>
    /// Mathematical operator identifiers for symbolic operations.
    /// </summary>
    public enum OperatorId : byte
    {
        Invalid = 0,

        // ------------------ Bitwise Operators ------------------ //

        // Bitwise modifiers:
        BitwiseNot,    // ~RHS

        // Basic bitwise operations:
        BitwiseAnd,    // LHS&(RHS&...)
        BitwiseOr,     // LHS|(RHS|...)
        BitwiseXor,    // LHS^(RHS^...)

        // Distributing bitwise operations:
        ShiftRight,    // LHS>>(RHS+...)
        ShiftLeft,     // LHS<<(RHS+...)
        RotateRight,   // LHS>](RHS+...)
        RotateLeft,    // LHS[<(RHS+...)

        // ---------------- Arithmetic Operators ----------------- //

        // Arithmetic modifiers:
        Negate,         // -RHS

        // Basic arithmetic operations:
        Add,            // LHS+(RHS+...)
        Subtract,       // LHS-(RHS+...)

        // Distributing arithmetic operations:
        MultiplyHigh,  // HI(LHS*RHS)
        Multiply,       // LHS*(RHS*...)
        Divide,         // LHS/(RHS*...)
        Remainder,      // LHS%RHS

        UMultiplyHigh, // < Unsigned variants of above >
        UMultiply,      // 
        UDivide,        // 
        URemainder,     // 

        // ----------------- Special Operators ----------------- //
        UCast,          // uintRHS_t(LHS)
        Cast,           // intRHS_t(LHS)
        PopCnt,         // POPCNT(RHS)
        BitscanFwd,    // BitScanForward(RHS)
        BitscanRev,    // BitScanReverse(RHS)
        BitTest,       // [LHS>>RHS]&1
        Mask,           // RHS.mask()
        BitCount,      // RHS.bitcount()
        ValueIf,       // LHS&1 ? RHS : 0

        MaxValue,      // LHS>=RHS ? LHS : RHS
        MinValue,      // LHS<=RHS ? LHS : RHS

        UMaxValue,     // < Unsigned variants of above >
        UMinValue,     //

        // ------------------ Logical Operators ----------------- //

        // Logical modifiers:
        LogicalNot,    // !RHS

        // Basic logical operations:
        LogicalAnd,    // LHS&&(RHS&&...)
        LogicalOr,     // LHS||(RHS||...)

        // ------------------ Comparison Operators -------------- //

        // Basic comparisons:
        Equal,          // LHS==RHS
        NotEqual,       // LHS!=RHS
        LessThan,       // LHS<RHS
        LessEqual,      // LHS<=RHS
        GreaterThan,    // LHS>RHS
        GreaterEqual,   // LHS>=RHS

        // Unsigned comparisons:
        ULessThan,      // < Unsigned variants of above >
        ULessEqual,     //
        UGreaterThan,   //
        UGreaterEqual,  //

        // ------------------ Memory Operators ----------------- //

        // Memory access:
        Read,           // [LHS]
        Write,          // [LHS]=RHS

        // ------------------ Stack Operators ------------------ //

        // Stack manipulation:
        Push,           // push(RHS)
        Pop,            // pop()

        // ------------------ Control Flow Operators ----------- //

        // Conditional execution:
        If,             // if(LHS) RHS
        IfElse,         // if(LHS) RHS1 else RHS2

        // ------------------ System Operators ----------------- //

        // System calls:
        Syscall,        // syscall(LHS, RHS, ...)

        // ------------------ Virtual Machine Operators -------- //

        // Virtual machine operations:
        VmEnter,        // vm_enter(LHS)
        VmExit,         // vm_exit()
        VmCall,         // vm_call(LHS)

        // ------------------ Special Purpose Operators -------- //

        // Custom operations:
        Custom,         // custom(LHS, RHS, ...)

        // ------------------ Terminating Operators ------------ //

        // Control flow termination:
        Return,         // return RHS
        Jump,           // jump RHS
        Call,           // call RHS
        Intrinsic,      // intrinsic(LHS, RHS, ...)
    }

    /// <summary>
    /// Extension methods for operator IDs.
    /// </summary>
    public static class OperatorExtensions
    {
        /// <summary>
        /// Gets the operator name as a string.
        /// </summary>
        public static string GetName(this OperatorId op)
        {
            return op switch
            {
                OperatorId.Invalid => "<Invalid>",
                OperatorId.BitwiseNot => "~",
                OperatorId.BitwiseAnd => "&",
                OperatorId.BitwiseOr => "|",
                OperatorId.BitwiseXor => "^",
                OperatorId.ShiftRight => ">>",
                OperatorId.ShiftLeft => "<<",
                OperatorId.RotateRight => ">]",
                OperatorId.RotateLeft => "[<",
                OperatorId.Negate => "-",
                OperatorId.Add => "+",
                OperatorId.Subtract => "-",
                OperatorId.MultiplyHigh => "mulhi",
                OperatorId.Multiply => "*",
                OperatorId.Divide => "/",
                OperatorId.Remainder => "%",
                OperatorId.UMultiplyHigh => "umulhi",
                OperatorId.UMultiply => "umul",
                OperatorId.UDivide => "udiv",
                OperatorId.URemainder => "urem",
                OperatorId.UCast => "ucast",
                OperatorId.Cast => "cast",
                OperatorId.PopCnt => "popcnt",
                OperatorId.BitscanFwd => "bitscanfwd",
                OperatorId.BitscanRev => "bitscanrev",
                OperatorId.BitTest => "bittest",
                OperatorId.Mask => "mask",
                OperatorId.BitCount => "bitcount",
                OperatorId.ValueIf => "valueif",
                OperatorId.MaxValue => "max",
                OperatorId.MinValue => "min",
                OperatorId.UMaxValue => "umax",
                OperatorId.UMinValue => "umin",
                OperatorId.LogicalNot => "!",
                OperatorId.LogicalAnd => "&&",
                OperatorId.LogicalOr => "||",
                OperatorId.Equal => "==",
                OperatorId.NotEqual => "!=",
                OperatorId.LessThan => "<",
                OperatorId.LessEqual => "<=",
                OperatorId.GreaterThan => ">",
                OperatorId.GreaterEqual => ">=",
                OperatorId.ULessThan => "ult",
                OperatorId.ULessEqual => "ule",
                OperatorId.UGreaterThan => "ugt",
                OperatorId.UGreaterEqual => "uge",
                OperatorId.Read => "read",
                OperatorId.Write => "write",
                OperatorId.Push => "push",
                OperatorId.Pop => "pop",
                OperatorId.If => "if",
                OperatorId.IfElse => "ifelse",
                OperatorId.Syscall => "syscall",
                OperatorId.VmEnter => "vm_enter",
                OperatorId.VmExit => "vm_exit",
                OperatorId.VmCall => "vm_call",
                OperatorId.Custom => "custom",
                OperatorId.Return => "return",
                OperatorId.Jump => "jump",
                OperatorId.Call => "call",
                OperatorId.Intrinsic => "intrinsic",
                _ => $"<Unknown:{op}>"
            };
        }

        /// <summary>
        /// Checks if the operator is a binary operator.
        /// </summary>
        public static bool IsBinary(this OperatorId op)
        {
            return op switch
            {
                OperatorId.BitwiseAnd or OperatorId.BitwiseOr or OperatorId.BitwiseXor or
                OperatorId.ShiftRight or OperatorId.ShiftLeft or OperatorId.RotateRight or OperatorId.RotateLeft or
                OperatorId.Add or OperatorId.Subtract or OperatorId.MultiplyHigh or OperatorId.Multiply or
                OperatorId.Divide or OperatorId.Remainder or OperatorId.UMultiplyHigh or OperatorId.UMultiply or
                OperatorId.UDivide or OperatorId.URemainder or OperatorId.ValueIf or OperatorId.MaxValue or
                OperatorId.MinValue or OperatorId.UMaxValue or OperatorId.UMinValue or OperatorId.LogicalAnd or
                OperatorId.LogicalOr or OperatorId.Equal or OperatorId.NotEqual or OperatorId.LessThan or
                OperatorId.LessEqual or OperatorId.GreaterThan or OperatorId.GreaterEqual or OperatorId.ULessThan or
                OperatorId.ULessEqual or OperatorId.UGreaterThan or OperatorId.UGreaterEqual or OperatorId.Write or
                OperatorId.IfElse or OperatorId.Syscall or OperatorId.VmEnter or OperatorId.VmCall or
                OperatorId.Custom or OperatorId.Intrinsic => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the operator is a unary operator.
        /// </summary>
        public static bool IsUnary(this OperatorId op)
        {
            return op switch
            {
                OperatorId.BitwiseNot or OperatorId.Negate or OperatorId.UCast or OperatorId.Cast or
                OperatorId.PopCnt or OperatorId.BitscanFwd or OperatorId.BitscanRev or OperatorId.BitTest or
                OperatorId.Mask or OperatorId.BitCount or OperatorId.LogicalNot or OperatorId.Read or
                OperatorId.Pop or OperatorId.If or OperatorId.VmExit or OperatorId.Return or OperatorId.Jump or
                OperatorId.Call => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the operator is a comparison operator.
        /// </summary>
        public static bool IsComparison(this OperatorId op)
        {
            return op switch
            {
                OperatorId.Equal or OperatorId.NotEqual or OperatorId.LessThan or OperatorId.LessEqual or
                OperatorId.GreaterThan or OperatorId.GreaterEqual or OperatorId.ULessThan or OperatorId.ULessEqual or
                OperatorId.UGreaterThan or OperatorId.UGreaterEqual => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the operator is an arithmetic operator.
        /// </summary>
        public static bool IsArithmetic(this OperatorId op)
        {
            return op switch
            {
                OperatorId.Negate or OperatorId.Add or OperatorId.Subtract or OperatorId.MultiplyHigh or
                OperatorId.Multiply or OperatorId.Divide or OperatorId.Remainder or OperatorId.UMultiplyHigh or
                OperatorId.UMultiply or OperatorId.UDivide or OperatorId.URemainder => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the operator is a bitwise operator.
        /// </summary>
        public static bool IsBitwise(this OperatorId op)
        {
            return op switch
            {
                OperatorId.BitwiseNot or OperatorId.BitwiseAnd or OperatorId.BitwiseOr or OperatorId.BitwiseXor or
                OperatorId.ShiftRight or OperatorId.ShiftLeft or OperatorId.RotateRight or OperatorId.RotateLeft or
                OperatorId.PopCnt or OperatorId.BitscanFwd or OperatorId.BitscanRev or OperatorId.BitTest or
                OperatorId.Mask or OperatorId.BitCount => true,
                _ => false
            };
        }
    }
}
