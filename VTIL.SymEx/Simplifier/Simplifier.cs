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
using System.Collections.Generic;
using System.Numerics;
using VTIL.Common.Math;

namespace VTIL.SymEx
{
    /// <summary>
    /// Simplifies symbolic expressions using various rules and transformations.
    /// </summary>
    public sealed class Simplifier
    {
        private readonly SimplifierState _state = new();

        /// <summary>
        /// Creates a new simplifier with default rules.
        /// </summary>
        public Simplifier()
        {
        }

        /// <summary>
        /// Simplifies an expression.
        /// </summary>
        public Expression Simplify(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            // Use the complete simplifier implementation
            var previousState = SimplifierComplete.SwapState(_state);
            try
            {
                return SimplifierComplete.SimplifyExpression(expression, false, true) ?? expression;
            }
            finally
            {
                SimplifierComplete.SwapState(previousState);
            }
        }

        /// <summary>
        /// Simplifies an expression with options.
        /// </summary>
        public Expression? SimplifyExpression(Expression expression, bool pretty = false, bool unpack = true)
        {
            if (expression == null)
                return null;

            var previousState = SimplifierComplete.SwapState(_state);
            try
            {
                return SimplifierComplete.SimplifyExpression(expression, pretty, unpack);
            }
            finally
            {
                SimplifierComplete.SwapState(previousState);
            }
        }

        /// <summary>
        /// Clears the simplifier cache.
        /// </summary>
        public void ClearCache()
        {
            _state.Clear();
        }

        /// <summary>
        /// Applies all simplification rules to an expression (legacy method).
        /// </summary>
        private Expression ApplyRulesLegacy(Expression expression)
        {
            var current = expression;
            var changed = true;
            var iterations = 0;
            const int maxIterations = 100; // Prevent infinite loops

            while (changed && iterations < maxIterations)
            {
                changed = false;
                iterations++;

                foreach (var rule in _rules)
                {
                    if (rule.TryApply(current, out var newExpression))
                    {
                        current = newExpression;
                        changed = true;
                        break; // Start over with the new expression
                    }
                }
            }

            return current;
        }

        /// <summary>
        /// Initializes default simplification rules.
        /// </summary>
        private void InitializeDefaultRules()
        {
            // Arithmetic rules
            AddRule(new ConstantFoldingRule());
            AddRule(new IdentityRule());
            AddRule(new ZeroRule());
            AddRule(new OneRule());

            // Bitwise rules
            AddRule(new BitwiseIdentityRule());
            AddRule(new BitwiseZeroRule());
            AddRule(new BitwiseAllOnesRule());

            // Logical rules
            AddRule(new LogicalIdentityRule());
            AddRule(new LogicalZeroRule());
            AddRule(new LogicalOneRule());

            // Comparison rules
            AddRule(new ComparisonRule());

            // Algebraic rules
            AddRule(new AssociativeRule());
            AddRule(new CommutativeRule());
            AddRule(new DistributiveRule());
        }

        /// <summary>
        /// Adds a simplification rule.
        /// </summary>
        public void AddRule(SimplificationRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            _rules.Add(rule);
        }

        /// <summary>
        /// Clears the simplification cache.
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Gets the number of cached expressions.
        /// </summary>
        public int CacheSize => _cache.Count;
    }

    /// <summary>
    /// Base class for simplification rules.
    /// </summary>
    public abstract class SimplificationRule
    {
        /// <summary>
        /// Tries to apply this rule to an expression.
        /// </summary>
        public abstract bool TryApply(Expression expression, out Expression result);
    }

    /// <summary>
    /// Constant folding rule - evaluates constant expressions.
    /// </summary>
    public class ConstantFoldingRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsOperation)
            {
                var evaluated = expression.Evaluate();
                if (evaluated.HasValue)
                {
                    result = Expression.Constant(evaluated.Value);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Identity rule - x + 0 = x, x * 1 = x, etc.
    /// </summary>
    public class IdentityRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x + 0 = x
                if (op == OperatorId.Add && right.IsConstant && right.GetConstantValue() == 0)
                {
                    result = left;
                    return true;
                }

                // 0 + x = x
                if (op == OperatorId.Add && left.IsConstant && left.GetConstantValue() == 0)
                {
                    result = right;
                    return true;
                }

                // x * 1 = x
                if (op == OperatorId.Multiply && right.IsConstant && right.GetConstantValue() == 1)
                {
                    result = left;
                    return true;
                }

                // 1 * x = x
                if (op == OperatorId.Multiply && left.IsConstant && left.GetConstantValue() == 1)
                {
                    result = right;
                    return true;
                }

                // x - 0 = x
                if (op == OperatorId.Subtract && right.IsConstant && right.GetConstantValue() == 0)
                {
                    result = left;
                    return true;
                }

                // x / 1 = x
                if (op == OperatorId.Divide && right.IsConstant && right.GetConstantValue() == 1)
                {
                    result = left;
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Zero rule - x * 0 = 0, 0 * x = 0, etc.
    /// </summary>
    public class ZeroRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x * 0 = 0
                if (op == OperatorId.Multiply && 
                    ((left.IsConstant && left.GetConstantValue() == 0) ||
                     (right.IsConstant && right.GetConstantValue() == 0)))
                {
                    result = Expression.Constant(0);
                    return true;
                }

                // 0 / x = 0 (when x != 0)
                if (op == OperatorId.Divide && left.IsConstant && left.GetConstantValue() == 0)
                {
                    result = Expression.Constant(0);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// One rule - x ^ 0 = 1, x ^ 1 = x, etc.
    /// </summary>
    public class OneRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x ^ 0 = 1
                if (op == OperatorId.BitwiseXor && right.IsConstant && right.GetConstantValue() == 0)
                {
                    result = left;
                    return true;
                }

                // 0 ^ x = x
                if (op == OperatorId.BitwiseXor && left.IsConstant && left.GetConstantValue() == 0)
                {
                    result = right;
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Bitwise identity rule - x & x = x, x | x = x, etc.
    /// </summary>
    public class BitwiseIdentityRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x & x = x
                if (op == OperatorId.BitwiseAnd && left.Equals(right))
                {
                    result = left;
                    return true;
                }

                // x | x = x
                if (op == OperatorId.BitwiseOr && left.Equals(right))
                {
                    result = left;
                    return true;
                }

                // x ^ x = 0
                if (op == OperatorId.BitwiseXor && left.Equals(right))
                {
                    result = Expression.Constant(0);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Bitwise zero rule - x & 0 = 0, x | 0 = x, etc.
    /// </summary>
    public class BitwiseZeroRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x & 0 = 0
                if (op == OperatorId.BitwiseAnd && 
                    ((left.IsConstant && left.GetConstantValue() == 0) ||
                     (right.IsConstant && right.GetConstantValue() == 0)))
                {
                    result = Expression.Constant(0);
                    return true;
                }

                // x | 0 = x
                if (op == OperatorId.BitwiseOr && right.IsConstant && right.GetConstantValue() == 0)
                {
                    result = left;
                    return true;
                }

                // 0 | x = x
                if (op == OperatorId.BitwiseOr && left.IsConstant && left.GetConstantValue() == 0)
                {
                    result = right;
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Bitwise all ones rule - x & ~0 = x, x | ~0 = ~0, etc.
    /// </summary>
    public class BitwiseAllOnesRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x | ~0 = ~0
                if (op == OperatorId.BitwiseOr && 
                    ((left.IsConstant && left.GetConstantValue() == -1) ||
                     (right.IsConstant && right.GetConstantValue() == -1)))
                {
                    result = Expression.Constant(-1);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Logical identity rule - x && x = x, x || x = x, etc.
    /// </summary>
    public class LogicalIdentityRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x && x = x
                if (op == OperatorId.LogicalAnd && left.Equals(right))
                {
                    result = left;
                    return true;
                }

                // x || x = x
                if (op == OperatorId.LogicalOr && left.Equals(right))
                {
                    result = left;
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Logical zero rule - x && 0 = 0, x || 0 = x, etc.
    /// </summary>
    public class LogicalZeroRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x && 0 = 0
                if (op == OperatorId.LogicalAnd && 
                    ((left.IsConstant && left.GetConstantValue() == 0) ||
                     (right.IsConstant && right.GetConstantValue() == 0)))
                {
                    result = Expression.Constant(0);
                    return true;
                }

                // x || 0 = x
                if (op == OperatorId.LogicalOr && right.IsConstant && right.GetConstantValue() == 0)
                {
                    result = left;
                    return true;
                }

                // 0 || x = x
                if (op == OperatorId.LogicalOr && left.IsConstant && left.GetConstantValue() == 0)
                {
                    result = right;
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Logical one rule - x && 1 = x, x || 1 = 1, etc.
    /// </summary>
    public class LogicalOneRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x && 1 = x
                if (op == OperatorId.LogicalAnd && right.IsConstant && right.GetConstantValue() == 1)
                {
                    result = left;
                    return true;
                }

                // 1 && x = x
                if (op == OperatorId.LogicalAnd && left.IsConstant && left.GetConstantValue() == 1)
                {
                    result = right;
                    return true;
                }

                // x || 1 = 1
                if (op == OperatorId.LogicalOr && 
                    ((left.IsConstant && left.GetConstantValue() == 1) ||
                     (right.IsConstant && right.GetConstantValue() == 1)))
                {
                    result = Expression.Constant(1);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Comparison rule - x == x = 1, x != x = 0, etc.
    /// </summary>
    public class ComparisonRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // x == x = 1
                if (op == OperatorId.Equal && left.Equals(right))
                {
                    result = Expression.Constant(1);
                    return true;
                }

                // x != x = 0
                if (op == OperatorId.NotEqual && left.Equals(right))
                {
                    result = Expression.Constant(0);
                    return true;
                }

                // x < x = 0
                if (op == OperatorId.LessThan && left.Equals(right))
                {
                    result = Expression.Constant(0);
                    return true;
                }

                // x <= x = 1
                if (op == OperatorId.LessEqual && left.Equals(right))
                {
                    result = Expression.Constant(1);
                    return true;
                }

                // x > x = 0
                if (op == OperatorId.GreaterThan && left.Equals(right))
                {
                    result = Expression.Constant(0);
                    return true;
                }

                // x >= x = 1
                if (op == OperatorId.GreaterEqual && left.Equals(right))
                {
                    result = Expression.Constant(1);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Associative rule - (a + b) + c = a + (b + c), etc.
    /// </summary>
    public class AssociativeRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // Check if left operand is the same operation
                if (left.IsBinaryOperation && left.Operator == op)
                {
                    // (a + b) + c = a + (b + c)
                    result = new Expression(left.Lhs!, op, new Expression(left.Rhs!, op, right));
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Commutative rule - a + b = b + a, a * b = b * a, etc.
    /// </summary>
    public class CommutativeRule : SimplificationRule
    {
        private static readonly HashSet<OperatorId> CommutativeOperators = new()
        {
            OperatorId.Add,
            OperatorId.Multiply,
            OperatorId.BitwiseAnd,
            OperatorId.BitwiseOr,
            OperatorId.BitwiseXor,
            OperatorId.LogicalAnd,
            OperatorId.LogicalOr,
            OperatorId.Equal,
            OperatorId.NotEqual
        };

        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation && CommutativeOperators.Contains(expression.Operator))
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // Reorder to put constants on the right
                if (left.IsConstant && !right.IsConstant)
                {
                    result = new Expression(right, op, left);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Distributive rule - a * (b + c) = a * b + a * c, etc.
    /// </summary>
    public class DistributiveRule : SimplificationRule
    {
        public override bool TryApply(Expression expression, out Expression result)
        {
            result = expression;

            if (expression.IsBinaryOperation)
            {
                var left = expression.Lhs!;
                var right = expression.Rhs!;
                var op = expression.Operator;

                // a * (b + c) = a * b + a * c
                if (op == OperatorId.Multiply && right.IsBinaryOperation && right.Operator == OperatorId.Add)
                {
                    result = new Expression(
                        new Expression(left, OperatorId.Multiply, right.Lhs!),
                        OperatorId.Add,
                        new Expression(left, OperatorId.Multiply, right.Rhs!));
                    return true;
                }

                // (a + b) * c = a * c + b * c
                if (op == OperatorId.Multiply && left.IsBinaryOperation && left.Operator == OperatorId.Add)
                {
                    result = new Expression(
                        new Expression(left.Lhs!, OperatorId.Multiply, right),
                        OperatorId.Add,
                        new Expression(left.Rhs!, OperatorId.Multiply, right));
                    return true;
                }
            }

            return false;
        }
    }
}
