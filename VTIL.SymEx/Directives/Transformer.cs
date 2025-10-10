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
using VTIL.Common.Math;

namespace VTIL.SymEx
{
    /// <summary>
    /// Transforms expressions using directive-based pattern matching and replacement.
    /// </summary>
    public static class Transformer
    {
        /// <summary>
        /// Translates the given directive into an expression (of size given) using the symbol table.
        /// </summary>
        public static Expression? Translate(SymbolTable symbolTable, DirectiveInstance directive, int bitCount)
        {
            // If expression operator:
            if (directive.Operator < OperatorId.Max)
            {
                // If directive is a variable or a constant, translate to expression equivalent.
                if (directive.Operator == OperatorId.Invalid)
                {
                    if (directive.Id == null)
                    {
                        return new Expression(directive.Value.Value, bitCount > 0 ? bitCount : 64);
                    }
                    else
                    {
                        return symbolTable.Translate(directive);
                    }
                }
                // If it is an expression:
                else
                {
                    // Handle casts as a redirect to resize.
                    if (directive.Operator == OperatorId.Cast || directive.Operator == OperatorId.UnsignedCast)
                    {
                        var lhs = Translate(symbolTable, directive.Lhs!, 0);
                        if (lhs == null)
                            return null;

                        var rhs = Translate(symbolTable, directive.Rhs!, bitCount);
                        if (rhs == null)
                            return null;

                        if (rhs.IsConstant)
                        {
                            var sz = (int)rhs.GetConstantValue();
                            return lhs.Resize(sz, directive.Operator == OperatorId.Cast);
                        }
                        
                        throw new InvalidOperationException("Cast size must be constant");
                    }
                    // If operation is binary:
                    else if (directive.Lhs != null)
                    {
                        Expression? lhs = null, rhs = null;

                        // Translate operands in priority order
                        var txPairs = new List<(Expression? output, DirectiveInstance dir, int priority)>
                        {
                            (lhs, directive.Lhs, directive.Lhs.Priority),
                            (rhs, directive.Rhs!, directive.Rhs!.Priority)
                        };

                        // Sort by priority (higher priority first)
                        txPairs = txPairs.OrderByDescending(x => x.priority).ToList();

                        foreach (var (output, dir, _) in txPairs)
                        {
                            var translated = Translate(symbolTable, dir, bitCount);
                            if (translated == null)
                                return null;

                            if (dir == directive.Lhs)
                                lhs = translated;
                            else
                                rhs = translated;
                        }

                        return new Expression(lhs!, directive.Operator, rhs!);
                    }
                    // If operation is unary:
                    else
                    {
                        var rhsExpr = Translate(symbolTable, directive.Rhs!, bitCount);
                        if (rhsExpr == null)
                            return null;

                        return new Expression(directive.Operator, rhsExpr);
                    }
                }
            }

            // If directive operator:
            var directiveOp = new DirectiveOpDesc(directive.Operator);
            
            switch (directiveOp.Value)
            {
                case DirectiveOpType.Simplify:
                {
                    // If expression translates successfully:
                    var e1 = Translate(symbolTable, directive.Rhs!, bitCount);
                    if (e1 != null)
                    {
                        // Return only if it was successful.
                        if (!e1.SimplifyHint)
                        {
                            var simplifier = new Simplifier();
                            var simplified = simplifier.SimplifyExpression(e1, false, false);
                            if (simplified != null)
                                return simplified;
                        }
                    }
                    break;
                }

                case DirectiveOpType.TrySimplify:
                {
                    // Translate right hand side.
                    var e1 = Translate(symbolTable, directive.Rhs!, bitCount);
                    if (e1 != null)
                    {
                        // Simplify the expression.
                        var simplifier = new Simplifier();
                        var simplified = simplifier.SimplifyExpression(e1, false, false);
                        return simplified ?? e1;
                    }
                    break;
                }

                case DirectiveOpType.OrAlso:
                {
                    // Unpack first expression, if translated successfully, return it as is.
                    var e1 = Translate(symbolTable, directive.Lhs!, bitCount);
                    if (e1 != null)
                        return e1;

                    // Unpack second expression, if translated successfully, return it as is.
                    var e2 = Translate(symbolTable, directive.Rhs!, bitCount);
                    if (e2 != null)
                        return e2;

                    break;
                }

                case DirectiveOpType.Iff:
                {
                    // Translate left hand side, if failed to do so or is not equal to [true], fail.
                    var conditionStatus = Translate(symbolTable, directive.Lhs!, 0);
                    if (conditionStatus == null)
                        return null;

                    // Check if condition evaluates to true
                    var simplified = new Simplifier().Simplify(conditionStatus);
                    if (!simplified.IsConstant || simplified.GetConstantValue() != 1)
                        return null;

                    // Continue the translation from the right hand side.
                    return Translate(symbolTable, directive.Rhs!, bitCount);
                }

                case DirectiveOpType.MaskUnknown:
                {
                    // Translate right hand side.
                    var exp = Translate(symbolTable, directive.Rhs!, bitCount);
                    if (exp != null)
                    {
                        // Return the unknown mask.
                        return new Expression(exp.UnknownMask, exp.SizeInBits);
                    }
                    break;
                }

                case DirectiveOpType.MaskOne:
                {
                    // Translate right hand side.
                    var exp = Translate(symbolTable, directive.Rhs!, bitCount);
                    if (exp != null)
                    {
                        // Return the known one mask.
                        return new Expression(exp.KnownOne, exp.SizeInBits);
                    }
                    break;
                }

                case DirectiveOpType.MaskZero:
                {
                    // Translate right hand side.
                    var exp = Translate(symbolTable, directive.Rhs!, bitCount);
                    if (exp != null)
                    {
                        // Return the known zero mask.
                        return new Expression(exp.KnownZero, exp.SizeInBits);
                    }
                    break;
                }

                case DirectiveOpType.Unreachable:
                {
                    throw new InvalidOperationException("Directive-time assertion failure!");
                }

                case DirectiveOpType.Warning:
                {
                    // Print a warning.
                    Console.WriteLine("WARNING: Directive-time warning!");

                    // Continue the translation from the right hand side.
                    return Translate(symbolTable, directive.Rhs!, bitCount);
                }

                default:
                    throw new InvalidOperationException($"Unknown directive operator: {directiveOp.Value}");
            }

            // Failed translating the directive.
            return null;
        }

        /// <summary>
        /// Attempts to transform the expression in form A to form B as indicated by the directives,
        /// and returns the first instance that matches query.
        /// </summary>
        public static Expression? Transform(Expression expression, DirectiveInstance from, DirectiveInstance to, params Func<Expression, bool>[] filters)
        {
            // Fast path: check if signature matches.
            if (expression.SizeInBits <= 0 || expression.SizeInBits > 64)
                return null;

            var fromSignature = from.Signatures[expression.SizeInBits - 1];
            if (!expression.Signature.CanMatch(fromSignature))
                return null;

            // Match the expression.
            var results = new List<SymbolTable>();
            if (FastMatcher.FastMatch(results, from, expression) == 0)
                return null;

            // For each possible match:
            foreach (var match in results)
            {
                // If we could translate the directive:
                var expNew = Translate(match, to, expression.SizeInBits);
                if (expNew != null)
                {
                    // If it passes through the filter:
                    bool passesFilter = true;
                    foreach (var filter in filters)
                    {
                        if (!filter(expNew))
                        {
                            passesFilter = false;
                            break;
                        }
                    }

                    if (passesFilter)
                    {
                        // Make sure the size matches.
                        if (expNew.SizeInBits != expression.SizeInBits)
                        {
                            // Auto fix if constant:
                            if (expNew.IsConstant)
                            {
                                expNew = new Expression(expNew.GetConstantValue(), expression.SizeInBits);
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Directive '{from}' => '{to}' left the simplifier unbalanced. " +
                                    $"Input ({expression.SizeInBits} bits): {expression}, " +
                                    $"Output ({expNew.SizeInBits} bits): {expNew}");
                            }
                        }

                        return expNew;
                    }
                }
            }

            // Indicate failure with null reference.
            return null;
        }

        /// <summary>
        /// Attempts to transform the expression using a directive pair.
        /// </summary>
        public static Expression? Transform(Expression expression, (DirectiveInstance from, DirectiveInstance to) directive, params Func<Expression, bool>[] filters)
        {
            return Transform(expression, directive.from, directive.to, filters);
        }
    }
}
