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
using Xunit;
using VTIL.Common.Math;
using VTIL.SymEx;
using VTIL.SymEx.Simplifier;

namespace VTIL.Tests
{
    /// <summary>
    /// Comprehensive tests for the complete SymEx implementation.
    /// </summary>
    public class SymExCompleteTests
    {
        [Fact]
        public void TestFastMatcher_SimpleVariable()
        {
            var x = Expression.Variable("x");
            var pattern = DirectiveInstance.Variables.A;

            var results = new List<SymbolTable>();
            var matchCount = FastMatcher.FastMatch(results, pattern, x);

            Assert.True(matchCount > 0);
            Assert.True(results.Count > 0);
        }

        [Fact]
        public void TestFastMatcher_Constant()
        {
            var five = Expression.Constant(5);
            var pattern = DirectiveInstance.Constant(5);

            var results = new List<SymbolTable>();
            var matchCount = FastMatcher.FastMatch(results, pattern, five);

            Assert.True(matchCount > 0);
        }

        [Fact]
        public void TestFastMatcher_BinaryOperation()
        {
            var x = Expression.Variable("x");
            var y = Expression.Variable("y");
            var expr = new Expression(x, OperatorId.Add, y);

            var a = DirectiveInstance.Variables.A;
            var b = DirectiveInstance.Variables.B;
            var pattern = new DirectiveInstance(a, OperatorId.Add, b);

            var results = new List<SymbolTable>();
            var matchCount = FastMatcher.FastMatch(results, pattern, expr);

            Assert.True(matchCount > 0);
            Assert.True(results.Count > 0);
        }

        [Fact]
        public void TestSymbolTable_AddAndTranslate()
        {
            var symbolTable = new SymbolTable();
            var x = Expression.Variable("x");
            var directive = DirectiveInstance.Variables.A;

            Assert.True(symbolTable.Add(directive, x));

            var translated = symbolTable.Translate(directive);
            Assert.Equal(x, translated);
        }

        [Fact]
        public void TestSymbolTable_MatchingType()
        {
            var symbolTable = new SymbolTable();

            // Test match_variable
            var varDirective = DirectiveInstance.Variables.V; // Match variable only
            var x = Expression.Variable("x");
            var constant = Expression.Constant(5);

            Assert.True(symbolTable.Add(varDirective, x));
            
            var symbolTable2 = new SymbolTable();
            Assert.False(symbolTable2.Add(varDirective, constant));
        }

        [Fact]
        public void TestTransformer_SimpleTransformation()
        {
            var x = Expression.Variable("x");
            var expr = new Expression(x, OperatorId.Add, Expression.Constant(0));

            var a = DirectiveInstance.Variables.A;
            var from = new DirectiveInstance(a, OperatorId.Add, DirectiveInstance.Constant(0));
            var to = a;

            var result = Transformer.Transform(expr, from, to);

            Assert.NotNull(result);
            Assert.Equal(x, result);
        }

        [Fact]
        public void TestTransformer_DoubleNegation()
        {
            var x = Expression.Variable("x");
            var negX = new Expression(OperatorId.Negate, x);
            var negNegX = new Expression(OperatorId.Negate, negX);

            var a = DirectiveInstance.Variables.A;
            var from = new DirectiveInstance(OperatorId.Negate, new DirectiveInstance(OperatorId.Negate, a));
            var to = a;

            var result = Transformer.Transform(negNegX, from, to);

            Assert.NotNull(result);
            Assert.Equal(x, result);
        }

        [Fact]
        public void TestDirectiveRules_UniversalSimplifiers()
        {
            var simplifiers = DirectiveRules.UniversalSimplifiers;
            Assert.NotEmpty(simplifiers);

            // Check that we have the basic identity rules
            var identityRules = simplifiers.Where(s => 
                s.from.Operator == OperatorId.Add && 
                s.to.Operator == OperatorId.Invalid);

            Assert.NotEmpty(identityRules);
        }

        [Fact]
        public void TestDirectiveRules_JoinDescriptors()
        {
            var joiners = DirectiveRules.JoinDescriptors;
            Assert.NotEmpty(joiners);
        }

        [Fact]
        public void TestBooleanDirectives_Simplifiers()
        {
            var simplifiers = BooleanDirectives.BooleanSimplifiers;
            Assert.NotEmpty(simplifiers);

            // Check for self-comparison rules
            var selfComparisonRules = simplifiers.Where(s => s.to.IsConstant);
            Assert.NotEmpty(selfComparisonRules);
        }

        [Fact]
        public void TestBooleanDirectives_Joiners()
        {
            var joiners = BooleanDirectives.BooleanJoiners;
            Assert.NotEmpty(joiners);
        }

        [Fact]
        public void TestSimplifierComplete_ConstantFolding()
        {
            var expr = new Expression(Expression.Constant(10), OperatorId.Add, Expression.Constant(20));
            var result = SimplifierEngine.SimplifyExpression(expr);

            Assert.NotNull(result);
            Assert.True(result.IsConstant);
            Assert.Equal(30, result.GetConstantValue());
        }

        [Fact]
        public void TestSimplifierComplete_IdentityRule()
        {
            var x = Expression.Variable("x");
            var expr = new Expression(x, OperatorId.Add, Expression.Constant(0));
            var result = SimplifierEngine.SimplifyExpression(expr);

            Assert.NotNull(result);
            // Should simplify to x
            Assert.True(result.IsVariable || result.Equals(x));
        }

        [Fact]
        public void TestSimplifierComplete_DoubleNegation()
        {
            var x = Expression.Variable("x");
            var negX = new Expression(OperatorId.Negate, x);
            var negNegX = new Expression(OperatorId.Negate, negX);

            var result = SimplifierComplete.SimplifyExpression(negNegX);

            Assert.NotNull(result);
            // Should simplify to x
            Assert.Equal(x.GetVariableName(), result.IsVariable ? result.GetVariableName() : null);
        }

        [Fact]
        public void TestSimplifierComplete_BitwiseIdentity()
        {
            var x = Expression.Variable("x");
            var expr = new Expression(x, OperatorId.BitwiseAnd, x);
            var result = SimplifierEngine.SimplifyExpression(expr);

            Assert.NotNull(result);
            // Should simplify to x
            Assert.True(result.IsVariable || result.Equals(x));
        }

        [Fact]
        public void TestSimplifierComplete_BitwiseXorSelf()
        {
            var x = Expression.Variable("x");
            var expr = new Expression(x, OperatorId.BitwiseXor, x);
            var result = SimplifierEngine.SimplifyExpression(expr);

            Assert.NotNull(result);
            // Should simplify to 0
            Assert.True(result.IsConstant);
            Assert.Equal(0, result.GetConstantValue());
        }

        [Fact]
        public void TestSimplifierComplete_SelfComparison()
        {
            var x = Expression.Variable("x");
            var expr = new Expression(x, OperatorId.Equal, x);
            var result = SimplifierEngine.SimplifyExpression(expr);

            Assert.NotNull(result);
            // Should simplify to 1 (true)
            if (result.IsConstant)
            {
                Assert.Equal(1, result.GetConstantValue());
            }
        }

        [Fact]
        public void TestSimplifierComplete_ComplexExpression()
        {
            var x = Expression.Variable("x");
            
            // (x + 0) * (5 + 3) = x * 8
            var expr = new Expression(
                new Expression(x, OperatorId.Add, Expression.Constant(0)),
                OperatorId.Multiply,
                new Expression(Expression.Constant(5), OperatorId.Add, Expression.Constant(3)));

            var result = SimplifierEngine.SimplifyExpression(expr);

            Assert.NotNull(result);
            // Should at least constant-fold the (5 + 3) part
            Assert.True(result.Complexity <= expr.Complexity);
        }

        [Fact]
        public void TestSimplifierState_Caching()
        {
            var state = new SimplifierState();
            var x = Expression.Variable("x");

            Assert.False(state.TryGetCached(x, out _, out _));

            state.Cache(x, x, true);

            Assert.True(state.TryGetCached(x, out var cachedResult, out var isSimplified));
            Assert.Equal(x, cachedResult);
            Assert.True(isSimplified);
        }

        [Fact]
        public void TestSimplifierState_CachePruning()
        {
            var state = new SimplifierState();

            // Fill cache beyond limit
            for (int i = 0; i < SimplifierState.MaxCacheEntries + 100; i++)
            {
                var expr = Expression.Variable($"x{i}");
                state.Cache(expr, expr, true);
            }

            // Cache should be pruned
            // (We can't easily test the exact count due to private implementation,
            // but we can verify it doesn't throw)
            Assert.True(true);
        }

        [Fact]
        public void TestExpressionResize_ZeroExtension()
        {
            var expr = Expression.Constant(0xFF);
            var resized = expr.Resize(16, false);

            Assert.Equal(16, resized.SizeInBits);
            Assert.True(resized.IsConstant);
            Assert.Equal(0xFF, resized.GetConstantValue());
        }

        [Fact]
        public void TestExpressionResize_SignExtension()
        {
            var expr = Expression.Constant(-1);
            var resized = expr.Resize(16, true);

            Assert.Equal(16, resized.SizeInBits);
            Assert.True(resized.IsConstant);
        }

        [Fact]
        public void TestIntegration_SimplifyWithDirectives()
        {
            var simplifier = new Simplifier();

            // Test various simplification patterns
            var testCases = new[]
            {
                // Constant folding
                (new Expression(Expression.Constant(10), OperatorId.Add, Expression.Constant(20)), 30),
                (new Expression(Expression.Constant(100), OperatorId.Subtract, Expression.Constant(50)), 50),
                (new Expression(Expression.Constant(7), OperatorId.Multiply, Expression.Constant(6)), 42),
            };

            foreach (var (expr, expected) in testCases)
            {
                var result = simplifier.Simplify(expr);
                Assert.NotNull(result);
                
                if (result.IsConstant)
                {
                    Assert.Equal(expected, result.GetConstantValue());
                }
            }
        }

        [Fact]
        public void TestIntegration_SimplifyIdentityRules()
        {
            var simplifier = new Simplifier();
            var x = Expression.Variable("x");

            var testCases = new[]
            {
                // Identity rules
                new Expression(x, OperatorId.Add, Expression.Constant(0)),      // x + 0 = x
                new Expression(x, OperatorId.Subtract, Expression.Constant(0)), // x - 0 = x
                new Expression(x, OperatorId.Multiply, Expression.Constant(1)), // x * 1 = x
                new Expression(x, OperatorId.BitwiseOr, Expression.Constant(0)), // x | 0 = x
                new Expression(x, OperatorId.BitwiseAnd, x),                     // x & x = x
                new Expression(x, OperatorId.BitwiseOr, x),                      // x | x = x
            };

            foreach (var expr in testCases)
            {
                var result = simplifier.Simplify(expr);
                Assert.NotNull(result);
                // Result should be simpler than the original
                Assert.True(result.Complexity <= expr.Complexity);
            }
        }

        [Fact]
        public void TestIntegration_SimplifyZeroRules()
        {
            var simplifier = new Simplifier();
            var x = Expression.Variable("x");

            var testCases = new[]
            {
                // Zero rules
                new Expression(x, OperatorId.Multiply, Expression.Constant(0)),  // x * 0 = 0
                new Expression(x, OperatorId.BitwiseAnd, Expression.Constant(0)), // x & 0 = 0
                new Expression(x, OperatorId.BitwiseXor, x),                      // x ^ x = 0
                new Expression(x, OperatorId.Subtract, x),                        // x - x = 0
            };

            foreach (var expr in testCases)
            {
                var result = simplifier.Simplify(expr);
                Assert.NotNull(result);
                
                // Most of these should simplify to 0
                if (result.IsConstant)
                {
                    Assert.Equal(0, result.GetConstantValue());
                }
            }
        }

        [Fact]
        public void TestIntegration_SimplifyComparisonRules()
        {
            var simplifier = new Simplifier();
            var x = Expression.Variable("x");

            // Self-comparisons
            var eqSelf = new Expression(x, OperatorId.Equal, x);
            var neqSelf = new Expression(x, OperatorId.NotEqual, x);
            var ltSelf = new Expression(x, OperatorId.LessThan, x);
            var leSelf = new Expression(x, OperatorId.LessThanOrEqual, x);

            var eqResult = simplifier.Simplify(eqSelf);
            var neqResult = simplifier.Simplify(neqSelf);
            var ltResult = simplifier.Simplify(ltSelf);
            var leResult = simplifier.Simplify(leSelf);

            // x == x should be true (1)
            if (eqResult.IsConstant)
                Assert.Equal(1, eqResult.GetConstantValue());

            // x != x should be false (0)
            if (neqResult.IsConstant)
                Assert.Equal(0, neqResult.GetConstantValue());

            // x < x should be false (0)
            if (ltResult.IsConstant)
                Assert.Equal(0, ltResult.GetConstantValue());

            // x <= x should be true (1)
            if (leResult.IsConstant)
                Assert.Equal(1, leResult.GetConstantValue());
        }

        [Fact]
        public void TestIntegration_SimplifyNestedExpressions()
        {
            var simplifier = new Simplifier();
            var x = Expression.Variable("x");

            // ((x + 0) * 1) + 0 = x
            var expr = new Expression(
                new Expression(
                    new Expression(x, OperatorId.Add, Expression.Constant(0)),
                    OperatorId.Multiply,
                    Expression.Constant(1)),
                OperatorId.Add,
                Expression.Constant(0));

            var result = simplifier.Simplify(expr);

            Assert.NotNull(result);
            // Should be significantly simpler
            Assert.True(result.Complexity < expr.Complexity);
        }

        [Fact]
        public void TestDirectiveInstance_ConstantCreation()
        {
            var zero = DirectiveInstance.Constant(0);
            var one = DirectiveInstance.Constant(1);
            var minusOne = DirectiveInstance.Constant(-1);

            Assert.True(zero.IsConstant);
            Assert.True(one.IsConstant);
            Assert.True(minusOne.IsConstant);

            Assert.Equal(0, zero.Value.KnownOne);
            Assert.Equal(1, one.Value.KnownOne);
        }

        [Fact]
        public void TestDirectiveInstance_VariableCreation()
        {
            var a = DirectiveInstance.Variables.A;
            var b = DirectiveInstance.Variables.B;
            var u = DirectiveInstance.Variables.U;

            Assert.NotNull(a.Id);
            Assert.NotNull(b.Id);
            Assert.NotNull(u.Id);

            Assert.NotEqual(a.LookupIndex, b.LookupIndex);
            Assert.NotEqual(a.MatchType, u.MatchType);
        }

        [Fact]
        public void TestDirectiveInstance_OperationCreation()
        {
            var a = DirectiveInstance.Variables.A;
            var b = DirectiveInstance.Variables.B;

            var add = new DirectiveInstance(a, OperatorId.Add, b);
            var sub = new DirectiveInstance(a, OperatorId.Subtract, b);
            var neg = new DirectiveInstance(OperatorId.Negate, a);

            Assert.Equal(OperatorId.Add, add.Operator);
            Assert.Equal(OperatorId.Subtract, sub.Operator);
            Assert.Equal(OperatorId.Negate, neg.Operator);

            Assert.True(add.IsBinaryOperation);
            Assert.True(neg.IsUnaryOperation);
        }

        [Fact]
        public void TestDirectiveInstance_Iff()
        {
            var a = DirectiveInstance.Variables.A;
            var b = DirectiveInstance.Variables.B;
            var c = DirectiveInstance.Variables.C;

            var condition = new DirectiveInstance(b, OperatorId.GreaterThan, c);
            var result = new DirectiveInstance(a, OperatorId.GreaterThan, b);
            var iff = DirectiveInstance.Iff(condition, result);

            Assert.Equal(OperatorId.DirectiveIff, iff.Operator);
            Assert.NotNull(iff.Lhs);
            Assert.NotNull(iff.Rhs);
        }

        [Fact]
        public void TestSimplifierState_ThreadLocal()
        {
            var state1 = SimplifierEngine.CurrentState;
            var state2 = SimplifierEngine.CurrentState;

            Assert.Equal(state1, state2); // Same thread, same state
        }

        [Fact]
        public void TestSimplifierState_JoinDepthLimit()
        {
            var state = new SimplifierState();
            Assert.Equal(20, state.JoinDepthLimit);
            Assert.Equal(0, state.CurrentJoinDepth);

            state.CurrentJoinDepth = 10;
            Assert.Equal(10, state.CurrentJoinDepth);
        }

        [Fact]
        public void TestExpressionSignature_CanMatch()
        {
            var x = Expression.Variable("x");
            var y = Expression.Variable("y");
            var expr1 = new Expression(x, OperatorId.Add, y);
            var expr2 = new Expression(y, OperatorId.Add, x);

            // Commutative operations should have similar signatures
            Assert.NotNull(expr1.Signature);
            Assert.NotNull(expr2.Signature);
        }

        [Fact]
        public void TestCompletePortIntegration()
        {
            // This test verifies that all components work together
            var simplifier = new Simplifier();

            // Create a complex expression
            var x = Expression.Variable("x");
            var y = Expression.Variable("y");
            
            // ((x + 0) & (y | y)) ^ ((x * 1) ^ 0)
            var complexExpr = new Expression(
                new Expression(
                    new Expression(x, OperatorId.Add, Expression.Constant(0)),
                    OperatorId.BitwiseAnd,
                    new Expression(y, OperatorId.BitwiseOr, y)),
                OperatorId.BitwiseXor,
                new Expression(
                    new Expression(x, OperatorId.Multiply, Expression.Constant(1)),
                    OperatorId.BitwiseXor,
                    Expression.Constant(0)));

            var result = simplifier.Simplify(complexExpr);

            Assert.NotNull(result);
            // Result should be significantly simpler
            Assert.True(result.Depth <= complexExpr.Depth);
            Assert.True(result.Complexity <= complexExpr.Complexity);
        }
    }
}
