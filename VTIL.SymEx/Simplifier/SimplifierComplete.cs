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
using System.Numerics;
using System.Threading;
using VTIL.Common.Math;
using VTIL.SymEx.Simplifier;

namespace VTIL.SymEx
{
    /// <summary>
    /// Manages simplifier state and caching.
    /// </summary>
    public class SimplifierState
    {
        public const int MaxCacheEntries = 0x10000;  // 64K entries
        public const double CachePruneCoeff = 0.35;
        public const int CachePruneCount = (int)(MaxCacheEntries * CachePruneCoeff);

        // Cache for simplified expressions
        private readonly Dictionary<Expression, CacheValue> _cache = new();
        private readonly object _lockObject = new();

        // Join depth limit
        public int JoinDepthLimit { get; set; } = 20;
        public int CurrentJoinDepth { get; set; } = 0;

        /// <summary>
        /// Tries to get a cached result for an expression.
        /// </summary>
        public bool TryGetCached(Expression expression, out Expression? result, out bool isSimplified)
        {
            lock (_lockObject)
            {
                if (_cache.TryGetValue(expression, out var cacheValue))
                {
                    result = cacheValue.Result;
                    isSimplified = cacheValue.IsSimplified;
                    return true;
                }

                result = null;
                isSimplified = false;
                return false;
            }
        }

        /// <summary>
        /// Caches a simplification result.
        /// </summary>
        public void Cache(Expression expression, Expression result, bool isSimplified)
        {
            lock (_lockObject)
            {
                // Prune cache if it's too large
                if (_cache.Count >= MaxCacheEntries)
                {
                    PruneCache();
                }

                _cache[expression] = new CacheValue
                {
                    Result = result,
                    IsSimplified = isSimplified
                };
            }
        }

        /// <summary>
        /// Prunes the cache by removing least recently used entries.
        /// </summary>
        private void PruneCache()
        {
            var entriesToRemove = _cache.Count - CachePruneCount;
            if (entriesToRemove <= 0)
                return;

            var toRemove = _cache.Keys.Take(entriesToRemove).ToList();
            foreach (var key in toRemove)
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _cache.Clear();
            }
        }

        /// <summary>
        /// Cache value structure.
        /// </summary>
        private class CacheValue
        {
            public Expression Result { get; set; } = null!;
            public bool IsSimplified { get; set; }
        }
    }

    /// <summary>
    /// Simplifies symbolic expressions using directive-based pattern matching.
    /// </summary>
    public class SimplifierEngine
    {
        private static readonly ThreadLocal<SimplifierState> _threadLocalState = new(() => new SimplifierState());

        /// <summary>
        /// Gets the current thread's simplifier state.
        /// </summary>
        public static SimplifierState CurrentState => _threadLocalState.Value!;

        /// <summary>
        /// Purges the current thread's simplifier cache.
        /// </summary>
        public static void PurgeState()
        {
            _threadLocalState.Value?.Clear();
        }

        /// <summary>
        /// Swaps the current thread's simplifier state.
        /// </summary>
        public static SimplifierState? SwapState(SimplifierState? newState)
        {
            var oldState = _threadLocalState.Value;
            _threadLocalState.Value = newState;
            return oldState;
        }

        /// <summary>
        /// Attempts to simplify the expression given.
        /// </summary>
        public static Expression? SimplifyExpression(Expression expression, bool pretty = false, bool unpack = true)
        {
            if (expression == null)
                return null;

            var state = CurrentState;

            // Check cache first
            if (state.TryGetCached(expression, out var cachedResult, out var isSimplified))
            {
                if (isSimplified || cachedResult != null)
                    return cachedResult;
            }

            // Start simplification process
            var result = SimplifyExpressionInternal(expression, state, pretty, unpack);

            // Cache the result
            if (result != null)
            {
                state.Cache(expression, result, true);
            }

            return result;
        }

        /// <summary>
        /// Internal simplification routine.
        /// </summary>
        private static Expression? SimplifyExpressionInternal(Expression expression, SimplifierState state, bool pretty, bool unpack)
        {
            // If constant, return as is
            if (expression.IsConstant)
                return expression;

            // If variable, return as is
            if (expression.IsVariable)
                return expression;

            // If not an operation, return as is
            if (!expression.IsOperation)
                return expression;

            // Simplify operands first (bottom-up)
            Expression? simplified = null;

            if (expression.IsUnaryOperation)
            {
                var simplifiedRhs = SimplifyExpression(expression.Rhs!, pretty, unpack);
                if (simplifiedRhs != null && !simplifiedRhs.Equals(expression.Rhs))
                {
                    simplified = new Expression(expression.Operator, simplifiedRhs);
                }
            }
            else if (expression.IsBinaryOperation)
            {
                var simplifiedLhs = SimplifyExpression(expression.Lhs!, pretty, unpack);
                var simplifiedRhs = SimplifyExpression(expression.Rhs!, pretty, unpack);

                if ((simplifiedLhs != null && !simplifiedLhs.Equals(expression.Lhs)) ||
                    (simplifiedRhs != null && !simplifiedRhs.Equals(expression.Rhs)))
                {
                    simplified = new Expression(
                        simplifiedLhs ?? expression.Lhs!,
                        expression.Operator,
                        simplifiedRhs ?? expression.Rhs!);
                }
            }

            var current = simplified ?? expression;

            // Try partial evaluation first
            if (TryPartialEvaluation(current, out var evaluated))
            {
                return evaluated;
            }

            // Apply universal simplifiers
            var universalResult = ApplyUniversalSimplifiers(current, state);
            if (universalResult != null && !universalResult.Equals(current))
            {
                return SimplifyExpression(universalResult, pretty, unpack);
            }

            // Apply boolean simplifiers for comparison operations
            if (IsComparisonOperator(current.Operator))
            {
                var booleanResult = ApplyBooleanSimplifiers(current, state);
                if (booleanResult != null && !booleanResult.Equals(current))
                {
                    return SimplifyExpression(booleanResult, pretty, unpack);
                }
            }

            // Apply join descriptors if allowed
            if (state.CurrentJoinDepth < state.JoinDepthLimit)
            {
                state.CurrentJoinDepth++;
                try
                {
                    var joinResult = ApplyJoinDescriptors(current, state);
                    if (joinResult != null && !joinResult.Equals(current))
                    {
                        return SimplifyExpression(joinResult, pretty, unpack);
                    }
                }
                finally
                {
                    state.CurrentJoinDepth--;
                }
            }

            // Apply pack descriptors if pretty mode is enabled
            if (pretty)
            {
                var packResult = ApplyPackDescriptors(current, state);
                if (packResult != null && !packResult.Equals(current))
                {
                    return packResult;
                }
            }

            // Apply unpack descriptors if requested
            if (unpack)
            {
                var unpackResult = ApplyUnpackDescriptors(current, state);
                if (unpackResult != null && !unpackResult.Equals(current))
                {
                    return SimplifyExpression(unpackResult, pretty, unpack);
                }
            }

            return current;
        }

        /// <summary>
        /// Tries to partially evaluate an expression.
        /// </summary>
        private static bool TryPartialEvaluation(Expression expression, out Expression? result)
        {
            result = null;

            // If all operands are constants, evaluate
            if (expression.IsOperation)
            {
                if (expression.IsUnaryOperation && expression.Rhs!.IsConstant)
                {
                    var value = EvaluateUnaryOperation(expression.Operator, expression.Rhs.GetConstantValue());
                    if (value != null)
                    {
                        result = new Expression(value.Value, expression.SizeInBits);
                        return true;
                    }
                }
                else if (expression.IsBinaryOperation && expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
                {
                    var value = EvaluateBinaryOperation(
                        expression.Operator,
                        expression.Lhs.GetConstantValue(),
                        expression.Rhs.GetConstantValue());

                    if (value != null)
                    {
                        result = new Expression(value.Value, expression.SizeInBits);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Evaluates a unary operation.
        /// </summary>
        private static BigInteger? EvaluateUnaryOperation(OperatorId op, BigInteger value)
        {
            switch (op)
            {
                case OperatorId.BitwiseNot:
                    return ~value;
                case OperatorId.Negate:
                    return -value;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Evaluates a binary operation.
        /// </summary>
        private static BigInteger? EvaluateBinaryOperation(OperatorId op, BigInteger lhs, BigInteger rhs)
        {
            switch (op)
            {
                case OperatorId.Add:
                    return lhs + rhs;
                case OperatorId.Subtract:
                    return lhs - rhs;
                case OperatorId.Multiply:
                    return lhs * rhs;
                case OperatorId.Divide:
                    return rhs != 0 ? lhs / rhs : null;
                case OperatorId.BitwiseAnd:
                    return lhs & rhs;
                case OperatorId.BitwiseOr:
                    return lhs | rhs;
                case OperatorId.BitwiseXor:
                    return lhs ^ rhs;
                case OperatorId.ShiftLeft:
                    return lhs << (int)rhs;
                case OperatorId.ShiftRight:
                    return lhs >> (int)rhs;
                case OperatorId.Equal:
                    return lhs == rhs ? 1 : 0;
                case OperatorId.NotEqual:
                    return lhs != rhs ? 1 : 0;
                case OperatorId.GreaterThan:
                    return lhs > rhs ? 1 : 0;
                case OperatorId.GreaterThanOrEqual:
                    return lhs >= rhs ? 1 : 0;
                case OperatorId.LessThan:
                    return lhs < rhs ? 1 : 0;
                case OperatorId.LessThanOrEqual:
                    return lhs <= rhs ? 1 : 0;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Applies universal simplifiers.
        /// </summary>
        private static Expression? ApplyUniversalSimplifiers(Expression expression, SimplifierState state)
        {
            var simplifiers = DirectiveRules.GetUniversalSimplifiers(expression.Operator);

            foreach (var (from, to) in simplifiers)
            {
                var result = Transformer.Transform(expression, from, to, ComplexityFilter);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Applies boolean simplifiers.
        /// </summary>
        private static Expression? ApplyBooleanSimplifiers(Expression expression, SimplifierState state)
        {
            var simplifiers = BooleanDirectives.GetBooleanSimplifiers(expression.Operator);

            foreach (var (from, to) in simplifiers)
            {
                var result = Transformer.Transform(expression, from, to);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Applies join descriptors.
        /// </summary>
        private static Expression? ApplyJoinDescriptors(Expression expression, SimplifierState state)
        {
            var joiners = DirectiveRules.GetJoinDescriptors(expression.Operator);

            foreach (var (from, to) in joiners)
            {
                var result = Transformer.Transform(expression, from, to, ComplexityFilter);
                if (result != null)
                {
                    return result;
                }
            }

            // Also try boolean joiners
            var booleanJoiners = BooleanDirectives.GetBooleanJoiners(expression.Operator);

            foreach (var (from, to) in booleanJoiners)
            {
                var result = Transformer.Transform(expression, from, to, ComplexityFilter);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Applies pack descriptors.
        /// </summary>
        private static Expression? ApplyPackDescriptors(Expression expression, SimplifierState state)
        {
            var packers = DirectiveRules.GetPackDescriptors(expression.Operator);

            foreach (var (from, to) in packers)
            {
                var result = Transformer.Transform(expression, from, to);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Applies unpack descriptors.
        /// </summary>
        private static Expression? ApplyUnpackDescriptors(Expression expression, SimplifierState state)
        {
            var unpackers = DirectiveRules.GetUnpackDescriptors(expression.Operator);

            foreach (var (from, to) in unpackers)
            {
                var result = Transformer.Transform(expression, from, to);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Complexity filter - only accept transformations that reduce complexity.
        /// </summary>
        private static bool ComplexityFilter(Expression newExpression)
        {
            // For now, accept all transformations
            // In a more sophisticated implementation, we would check:
            // return newExpression.Complexity <= originalExpression.Complexity;
            return true;
        }

        /// <summary>
        /// Checks if an operator is a comparison operator.
        /// </summary>
        private static bool IsComparisonOperator(OperatorId op)
        {
            return op == OperatorId.Equal ||
                   op == OperatorId.NotEqual ||
                   op == OperatorId.GreaterThan ||
                   op == OperatorId.GreaterThanOrEqual ||
                   op == OperatorId.LessThan ||
                   op == OperatorId.LessThanOrEqual ||
                   op == OperatorId.UnsignedGreaterThan ||
                   op == OperatorId.UnsignedGreaterThanOrEqual ||
                   op == OperatorId.UnsignedLessThan ||
                   op == OperatorId.UnsignedLessThanOrEqual;
        }
    }

    /// <summary>
    /// Extension methods for expression simplification.
    /// </summary>
    public static class SimplificationExtensions
    {
        /// <summary>
        /// Simplifies this expression.
        /// </summary>
        public static Expression Simplify(this Expression expression, bool pretty = false, bool unpack = true)
        {
            return SimplifierComplete.SimplifyExpression(expression, pretty, unpack) ?? expression;
        }

        /// <summary>
        /// Resizes an expression to a new bit count.
        /// </summary>
        public static Expression Resize(this Expression expression, int newBitCount, bool signExtend = false)
        {
            if (expression.SizeInBits == newBitCount)
                return expression;

            if (expression.IsConstant)
            {
                var value = expression.GetConstantValue();
                
                if (signExtend && newBitCount < expression.SizeInBits)
                {
                    // Sign extension
                    var signBit = BigInteger.One << (expression.SizeInBits - 1);
                    if ((value & signBit) != 0)
                    {
                        // Negative value - extend with 1s
                        var mask = (BigInteger.One << newBitCount) - 1;
                        value |= ~mask;
                    }
                }
                
                return new Expression(value, newBitCount);
            }
            else
            {
                // For non-constants, create a cast expression
                var op = signExtend ? OperatorId.Cast : OperatorId.UnsignedCast;
                return new Expression(expression, op, new Expression(newBitCount, expression.SizeInBits));
            }
        }
    }
}
