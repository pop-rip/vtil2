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
using VTIL.Common.Math;

namespace VTIL.SymEx
{
    /// <summary>
    /// Directive variables with matching constraints.
    /// </summary>
    public enum MatchingType
    {
        /// <summary>
        /// None - matches anything.
        /// </summary>
        MatchAny,

        /// <summary>
        /// Must be a variable.
        /// </summary>
        MatchVariable,

        /// <summary>
        /// Must be a constant.
        /// </summary>
        MatchConstant,

        /// <summary>
        /// Must be a full-expression.
        /// </summary>
        MatchExpression,

        /// <summary>
        /// Must be anything but a full-expression.
        /// </summary>
        MatchNonExpression,

        /// <summary>
        /// Must be anything but a constant.
        /// </summary>
        MatchNonConstant
    }

    /// <summary>
    /// Directive operator identifiers.
    /// </summary>
    public enum DirectiveOperatorId : byte
    {
        Invalid = 0,
        TrySimplify = 1,
        Simplify = 2,
        If = 3,
        OrAlso = 4,
        Unreachable = 5,
        MaskUnknown = 6,
        MaskOne = 7,
        MaskZero = 8
    }

    /// <summary>
    /// Represents a directive instance for pattern matching.
    /// </summary>
    public sealed class DirectiveInstance : IEquatable<DirectiveInstance>
    {
        /// <summary>
        /// If symbolic variable, the identifier of the variable and type of expressions it can match.
        /// </summary>
        public string? Id { get; }
        public MatchingType MatchingType { get; }
        public int LookupIndex { get; }

        /// <summary>
        /// Priority hint for transformer.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// The operation we're matching and the operands.
        /// </summary>
        public OperatorId Operator { get; }
        public DirectiveInstance? Lhs { get; }
        public DirectiveInstance? Rhs { get; }

        /// <summary>
        /// Number of nodes, cumulatively calculated.
        /// </summary>
        public int NodeCount { get; }

        /// <summary>
        /// Creates a directive instance for a variable.
        /// </summary>
        public DirectiveInstance(string id, int lookupIndex, MatchingType matchingType = MatchingType.MatchAny)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            LookupIndex = lookupIndex;
            MatchingType = matchingType;
            Priority = 0;
            Operator = OperatorId.Invalid;
            Lhs = null;
            Rhs = null;
            NodeCount = 1;
        }

        /// <summary>
        /// Creates a directive instance for an operation.
        /// </summary>
        public DirectiveInstance(OperatorId op, DirectiveInstance? lhs = null, DirectiveInstance? rhs = null)
        {
            Id = null;
            LookupIndex = 0;
            MatchingType = MatchingType.MatchAny;
            Priority = 0;
            Operator = op;
            Lhs = lhs;
            Rhs = rhs;
            NodeCount = 1 + (lhs?.NodeCount ?? 0) + (rhs?.NodeCount ?? 0);
        }

        /// <summary>
        /// Creates a directive instance with explicit parameters.
        /// </summary>
        private DirectiveInstance(
            string? id,
            int lookupIndex,
            MatchingType matchingType,
            int priority,
            OperatorId op,
            DirectiveInstance? lhs,
            DirectiveInstance? rhs)
        {
            Id = id;
            LookupIndex = lookupIndex;
            MatchingType = matchingType;
            Priority = priority;
            Operator = op;
            Lhs = lhs;
            Rhs = rhs;
            NodeCount = 1 + (lhs?.NodeCount ?? 0) + (rhs?.NodeCount ?? 0);
        }

        /// <summary>
        /// Checks if this is a variable directive.
        /// </summary>
        public bool IsVariable => Id != null;

        /// <summary>
        /// Checks if this is an operation directive.
        /// </summary>
        public bool IsOperation => Operator != OperatorId.Invalid;

        /// <summary>
        /// Checks if this is a unary operation directive.
        /// </summary>
        public bool IsUnaryOperation => IsOperation && Lhs == null && Rhs != null;

        /// <summary>
        /// Checks if this is a binary operation directive.
        /// </summary>
        public bool IsBinaryOperation => IsOperation && Lhs != null && Rhs != null;

        /// <summary>
        /// Gets all variables referenced in this directive.
        /// </summary>
        public IEnumerable<DirectiveInstance> GetVariables()
        {
            if (IsVariable)
            {
                yield return this;
            }

            if (Lhs != null)
            {
                foreach (var variable in Lhs.GetVariables())
                    yield return variable;
            }

            if (Rhs != null)
            {
                foreach (var variable in Rhs.GetVariables())
                    yield return variable;
            }
        }

        /// <summary>
        /// Checks if this directive matches the given expression.
        /// </summary>
        public bool Matches(Expression expression, Dictionary<string, Expression>? bindings = null)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (IsVariable)
            {
                if (bindings != null && bindings.TryGetValue(Id!, out var boundExpression))
                {
                    return Equals(boundExpression, expression);
                }

                // Check matching constraints
                return MatchingType switch
                {
                    MatchingType.MatchAny => true,
                    MatchingType.MatchVariable => expression.IsVariable,
                    MatchingType.MatchConstant => expression.IsConstant,
                    MatchingType.MatchExpression => expression.IsOperation,
                    MatchingType.MatchNonExpression => !expression.IsOperation,
                    MatchingType.MatchNonConstant => !expression.IsConstant,
                    _ => false
                };
            }

            if (IsOperation)
            {
                if (!expression.IsOperation || expression.Operator != Operator)
                    return false;

                if (IsUnaryOperation)
                {
                    return Rhs!.Matches(expression.Rhs!, bindings);
                }

                if (IsBinaryOperation)
                {
                    return Lhs!.Matches(expression.Lhs!, bindings) &&
                           Rhs!.Matches(expression.Rhs!, bindings);
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to match this directive against an expression and extract bindings.
        /// </summary>
        public bool TryMatch(Expression expression, Dictionary<string, Expression> bindings)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));

            if (IsVariable)
            {
                if (bindings.TryGetValue(Id!, out var boundExpression))
                {
                    return Equals(boundExpression, expression);
                }

                // Check matching constraints
                bool matches = MatchingType switch
                {
                    MatchingType.MatchAny => true,
                    MatchingType.MatchVariable => expression.IsVariable,
                    MatchingType.MatchConstant => expression.IsConstant,
                    MatchingType.MatchExpression => expression.IsOperation,
                    MatchingType.MatchNonExpression => !expression.IsOperation,
                    MatchingType.MatchNonConstant => !expression.IsConstant,
                    _ => false
                };

                if (matches)
                {
                    bindings[Id!] = expression;
                }

                return matches;
            }

            if (IsOperation)
            {
                if (!expression.IsOperation || expression.Operator != Operator)
                    return false;

                if (IsUnaryOperation)
                {
                    return Rhs!.TryMatch(expression.Rhs!, bindings);
                }

                if (IsBinaryOperation)
                {
                    return Lhs!.TryMatch(expression.Lhs!, bindings) &&
                           Rhs!.TryMatch(expression.Rhs!, bindings);
                }
            }

            return false;
        }

        public bool Equals(DirectiveInstance? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id == other.Id &&
                   MatchingType == other.MatchingType &&
                   LookupIndex == other.LookupIndex &&
                   Priority == other.Priority &&
                   Operator == other.Operator &&
                   Equals(Lhs, other.Lhs) &&
                   Equals(Rhs, other.Rhs);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DirectiveInstance);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Id);
            hashCode.Add((int)MatchingType);
            hashCode.Add(LookupIndex);
            hashCode.Add(Priority);
            hashCode.Add((int)Operator);
            hashCode.Add(Lhs);
            hashCode.Add(Rhs);
            return hashCode.ToHashCode();
        }

        public static bool operator ==(DirectiveInstance? left, DirectiveInstance? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DirectiveInstance? left, DirectiveInstance? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            if (IsVariable)
            {
                var suffix = MatchingType switch
                {
                    MatchingType.MatchVariable => ":var",
                    MatchingType.MatchConstant => ":const",
                    MatchingType.MatchExpression => ":expr",
                    MatchingType.MatchNonExpression => ":!expr",
                    MatchingType.MatchNonConstant => ":!const",
                    _ => ""
                };
                return $"{Id}{suffix}";
            }

            if (IsUnaryOperation)
                return $"{Operator.GetName()}({Rhs})";

            if (IsBinaryOperation)
                return $"({Lhs} {Operator.GetName()} {Rhs})";

            return "?";
        }

        /// <summary>
        /// Common directive variables.
        /// </summary>
        public static class Variables
        {
            public static readonly DirectiveInstance A = new("α", 0);
            public static readonly DirectiveInstance B = new("β", 1);
            public static readonly DirectiveInstance C = new("δ", 2);
            public static readonly DirectiveInstance D = new("ε", 3);
            public static readonly DirectiveInstance E = new("ζ", 4);
            public static readonly DirectiveInstance F = new("η", 5);
            public static readonly DirectiveInstance G = new("λ", 6);

            // Special variables, one per type:
            public static readonly DirectiveInstance V = new("Π", 7, MatchingType.MatchVariable);
            public static readonly DirectiveInstance U = new("Σ", 8, MatchingType.MatchConstant);
            public static readonly DirectiveInstance Q = new("Ω", 9, MatchingType.MatchExpression);
            public static readonly DirectiveInstance W = new("Ψ", 10, MatchingType.MatchNonConstant);
            public static readonly DirectiveInstance X = new("Θ", 11, MatchingType.MatchNonExpression);

            /// <summary>
            /// Number of lookup indices available.
            /// </summary>
            public const int NumberOfLookupIndices = 12;
        }

        /// <summary>
        /// Helper methods for creating directive instances.
        /// </summary>
        public static class Create
        {
            /// <summary>
            /// Creates a simplify directive.
            /// </summary>
            public static DirectiveInstance Simplify(DirectiveInstance operand)
            {
                return new DirectiveInstance(OperatorId.Invalid, null, operand);
            }

            /// <summary>
            /// Creates an if directive.
            /// </summary>
            public static DirectiveInstance If(DirectiveInstance condition, DirectiveInstance then)
            {
                return new DirectiveInstance(OperatorId.If, condition, then);
            }

            /// <summary>
            /// Creates an or directive.
            /// </summary>
            public static DirectiveInstance Or(DirectiveInstance left, DirectiveInstance right)
            {
                return new DirectiveInstance(OperatorId.LogicalOr, left, right);
            }

            /// <summary>
            /// Creates an unreachable directive.
            /// </summary>
            public static DirectiveInstance Unreachable()
            {
                return new DirectiveInstance(OperatorId.Invalid);
            }

            /// <summary>
            /// Changes the characteristics of the first variable to match the second.
            /// </summary>
            public static DirectiveInstance WithMatchingType(DirectiveInstance variable, MatchingType matchingType)
            {
                return new DirectiveInstance(variable.Id, variable.LookupIndex, matchingType, 
                    variable.Priority, variable.Operator, variable.Lhs, variable.Rhs);
            }
        }
    }
}
