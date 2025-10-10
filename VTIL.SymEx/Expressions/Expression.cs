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
using VTIL.Common.Math;

namespace VTIL.SymEx
{
    /// <summary>
    /// Represents a symbolic expression in VTIL.
    /// </summary>
    public sealed class Expression : IEquatable<Expression>
    {
        /// <summary>
        /// If symbolic variable, the unique identifier that it maps to.
        /// </summary>
        public UniqueIdentifier? Uid { get; }

        /// <summary>
        /// If operation, identifier of the operator and the sub-expressions for the operands.
        /// </summary>
        public OperatorId Operator { get; }
        public Expression? Lhs { get; }
        public Expression? Rhs { get; }

        /// <summary>
        /// An arbitrarily defined complexity value that is used as an inverse reward function in simplification.
        /// </summary>
        public double Complexity { get; set; }

        /// <summary>
        /// Depth of the current expression.
        /// - If constant or symbolic variable, = 0
        /// - Otherwise = max(operands...) + 1
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Hash of the expression used by the simplifier cache.
        /// </summary>
        public ulong HashValue { get; }

        /// <summary>
        /// Signature of the expression.
        /// </summary>
        public ExpressionSignature Signature { get; }

        /// <summary>
        /// Whether expression passed the simplifier already or not.
        /// </summary>
        public bool SimplifyHint { get; set; }

        /// <summary>
        /// Disables implicit auto-simplification for the expression if set.
        /// </summary>
        public bool IsLazy { get; set; }

        /// <summary>
        /// Creates a constant expression.
        /// </summary>
        public Expression(BigInteger value)
        {
            Uid = new UniqueIdentifier(value);
            Operator = OperatorId.Invalid;
            Lhs = null;
            Rhs = null;
            Depth = 0;
            Signature = new ExpressionSignature(value);
            HashValue = (ulong)value.GetHashCode();
            Complexity = 0;
        }

        /// <summary>
        /// Creates a symbolic variable expression.
        /// </summary>
        public Expression(UniqueIdentifier uid)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
            Operator = OperatorId.Invalid;
            Lhs = null;
            Rhs = null;
            Depth = 0;
            Signature = new ExpressionSignature();
            HashValue = uid.HashValue;
            Complexity = 1;
        }

        /// <summary>
        /// Creates a unary operation expression.
        /// </summary>
        public Expression(OperatorId op, Expression operand)
        {
            if (operand == null)
                throw new ArgumentNullException(nameof(operand));

            Operator = op;
            Lhs = null;
            Rhs = operand;
            Depth = operand.Depth + 1;
            Signature = new ExpressionSignature(op, operand.Signature);
            
            // Calculate hash
            var hash = new HashCode();
            hash.Add((int)op);
            hash.Add(operand.HashValue);
            HashValue = (ulong)hash.ToHashCode();

            Complexity = operand.Complexity + 1;
        }

        /// <summary>
        /// Creates a binary operation expression.
        /// </summary>
        public Expression(Expression lhs, OperatorId op, Expression rhs)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));

            Operator = op;
            Lhs = lhs;
            Rhs = rhs;
            Depth = Math.Max(lhs.Depth, rhs.Depth) + 1;
            Signature = new ExpressionSignature(lhs.Signature, op, rhs.Signature);

            // Calculate hash
            var hash = new HashCode();
            hash.Add(lhs.HashValue);
            hash.Add((int)op);
            hash.Add(rhs.HashValue);
            HashValue = (ulong)hash.ToHashCode();

            Complexity = lhs.Complexity + rhs.Complexity + 1;
        }

        /// <summary>
        /// Checks if this is a constant expression.
        /// </summary>
        public bool IsConstant => Operator == OperatorId.Invalid && Uid != null && Uid.IsNumeric;

        /// <summary>
        /// Checks if this is a symbolic variable expression.
        /// </summary>
        public bool IsVariable => Operator == OperatorId.Invalid && Uid != null && !Uid.IsNumeric;

        /// <summary>
        /// Checks if this is an operation expression.
        /// </summary>
        public bool IsOperation => Operator != OperatorId.Invalid;

        /// <summary>
        /// Checks if this is a unary operation.
        /// </summary>
        public bool IsUnaryOperation => IsOperation && Lhs == null && Rhs != null;

        /// <summary>
        /// Checks if this is a binary operation.
        /// </summary>
        public bool IsBinaryOperation => IsOperation && Lhs != null && Rhs != null;

        /// <summary>
        /// Gets the constant value if this is a constant expression.
        /// </summary>
        public BigInteger GetConstantValue()
        {
            if (!IsConstant) return 0;
            return Uid!.GetNumericValue().Value;
        }

        /// <summary>
        /// Gets the variable name if this is a variable expression.
        /// </summary>
        public string? GetVariableName()
        {
            if (!IsVariable) return null;
            return Uid!.ToString();
        }

        /// <summary>
        /// Gets the number of operands.
        /// </summary>
        public int OperandCount
        {
            get
            {
                if (IsConstant || IsVariable) return 0;
                if (IsUnaryOperation) return 1;
                if (IsBinaryOperation) return 2;
                return 0;
            }
        }

        /// <summary>
        /// Gets all operands as an array.
        /// </summary>
        public Expression[] GetOperands()
        {
            if (IsConstant || IsVariable) return Array.Empty<Expression>();
            if (IsUnaryOperation) return new[] { Rhs! };
            if (IsBinaryOperation) return new[] { Lhs!, Rhs! };
            return Array.Empty<Expression>();
        }

        /// <summary>
        /// Gets the first operand.
        /// </summary>
        public Expression? GetOperand0() => Lhs;

        /// <summary>
        /// Gets the second operand.
        /// </summary>
        public Expression? GetOperand1() => Rhs;

        /// <summary>
        /// Checks if this expression contains the specified variable.
        /// </summary>
        public bool ContainsVariable(string variableName)
        {
            if (IsVariable)
                return GetVariableName() == variableName;

            if (IsOperation)
            {
                if (Lhs != null && Lhs.ContainsVariable(variableName))
                    return true;
                if (Rhs != null && Rhs.ContainsVariable(variableName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if this expression contains any variables.
        /// </summary>
        public bool ContainsVariables()
        {
            if (IsVariable) return true;

            if (IsOperation)
            {
                if (Lhs != null && Lhs.ContainsVariables())
                    return true;
                if (Rhs != null && Rhs.ContainsVariables())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Substitutes a variable with an expression.
        /// </summary>
        public Expression Substitute(string variableName, Expression replacement)
        {
            if (replacement == null)
                throw new ArgumentNullException(nameof(replacement));

            if (IsVariable && GetVariableName() == variableName)
                return replacement;

            if (IsConstant)
                return this;

            if (IsUnaryOperation)
                return new Expression(Operator, Rhs!.Substitute(variableName, replacement));

            if (IsBinaryOperation)
                return new Expression(
                    Lhs!.Substitute(variableName, replacement),
                    Operator,
                    Rhs!.Substitute(variableName, replacement));

            return this;
        }

        /// <summary>
        /// Evaluates the expression if it contains only constants.
        /// </summary>
        public BigInteger? Evaluate()
        {
            if (IsConstant)
                return GetConstantValue();

            if (IsVariable)
                return null; // Cannot evaluate variables

            if (IsUnaryOperation)
            {
                var operand = Rhs!.Evaluate();
                if (!operand.HasValue) return null;

                return EvaluateUnary(Operator, operand.Value);
            }

            if (IsBinaryOperation)
            {
                var left = Lhs!.Evaluate();
                var right = Rhs!.Evaluate();

                if (!left.HasValue || !right.HasValue) return null;

                return EvaluateBinary(Operator, left.Value, right.Value);
            }

            return null;
        }

        /// <summary>
        /// Evaluates a unary operation.
        /// </summary>
        private static BigInteger EvaluateUnary(OperatorId op, BigInteger value)
        {
            return op switch
            {
                OperatorId.BitwiseNot => ~value,
                OperatorId.Negate => -value,
                OperatorId.LogicalNot => value == 0 ? 1 : 0,
                OperatorId.PopCnt => BigInteger.PopCount(value),
                OperatorId.Cast => value,
                OperatorId.UCast => value,
                _ => throw new InvalidOperationException($"Cannot evaluate unary operator {op}")
            };
        }

        /// <summary>
        /// Evaluates a binary operation.
        /// </summary>
        private static BigInteger EvaluateBinary(OperatorId op, BigInteger left, BigInteger right)
        {
            return op switch
            {
                OperatorId.Add => left + right,
                OperatorId.Subtract => left - right,
                OperatorId.Multiply => left * right,
                OperatorId.Divide => right != 0 ? left / right : 0,
                OperatorId.Remainder => right != 0 ? left % right : 0,
                OperatorId.BitwiseAnd => left & right,
                OperatorId.BitwiseOr => left | right,
                OperatorId.BitwiseXor => left ^ right,
                OperatorId.ShiftLeft => left << (int)right,
                OperatorId.ShiftRight => left >> (int)right,
                OperatorId.Equal => left == right ? 1 : 0,
                OperatorId.NotEqual => left != right ? 1 : 0,
                OperatorId.LessThan => left < right ? 1 : 0,
                OperatorId.LessEqual => left <= right ? 1 : 0,
                OperatorId.GreaterThan => left > right ? 1 : 0,
                OperatorId.GreaterEqual => left >= right ? 1 : 0,
                _ => throw new InvalidOperationException($"Cannot evaluate binary operator {op}")
            };
        }

        public bool Equals(Expression? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Operator == other.Operator &&
                   Equals(Uid, other.Uid) &&
                   Equals(Lhs, other.Lhs) &&
                   Equals(Rhs, other.Rhs);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Expression);
        }

        public override int GetHashCode()
        {
            return (int)HashValue;
        }

        public static bool operator ==(Expression? left, Expression? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Expression? left, Expression? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            if (IsConstant)
                return GetConstantValue().ToString();

            if (IsVariable)
                return GetVariableName() ?? "?";

            if (IsUnaryOperation)
                return $"{Operator.GetName()}({Rhs})";

            if (IsBinaryOperation)
                return $"({Lhs} {Operator.GetName()} {Rhs})";

            return "?";
        }

        /// <summary>
        /// Creates a constant expression.
        /// </summary>
        public static Expression Constant(BigInteger value)
        {
            return new Expression(value);
        }

        /// <summary>
        /// Creates a constant expression.
        /// </summary>
        public static Expression Constant(long value)
        {
            return new Expression((BigInteger)value);
        }

        /// <summary>
        /// Creates a constant expression.
        /// </summary>
        public static Expression Constant(ulong value)
        {
            return new Expression((BigInteger)value);
        }

        /// <summary>
        /// Creates a variable expression.
        /// </summary>
        public static Expression Variable(string name)
        {
            return new Expression(new UniqueIdentifier(name));
        }

        /// <summary>
        /// Creates a variable expression.
        /// </summary>
        public static Expression Variable(UniqueIdentifier uid)
        {
            return new Expression(uid);
        }
    }
}
