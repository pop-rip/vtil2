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
using System.Linq;
using System.Numerics;
using VTIL.Common.Math;

namespace VTIL.SymEx
{
    /// <summary>
    /// Internal representation of the Variable -> Expression mapping.
    /// </summary>
    public class SymbolTable
    {
        private readonly Expression?[] _lookupTable;

        /// <summary>
        /// Creates a new symbol table with the specified size.
        /// </summary>
        public SymbolTable(int numberOfLookupIndices = 128)
        {
            _lookupTable = new Expression?[numberOfLookupIndices];
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public SymbolTable(SymbolTable other)
        {
            _lookupTable = (Expression?[])other._lookupTable.Clone();
        }

        /// <summary>
        /// Adds the mapping of a variable to an expression.
        /// </summary>
        public bool Add(DirectiveInstance directive, Expression expression)
        {
            // If it's the first time this variable is being used:
            if (_lookupTable[directive.LookupIndex] == null)
            {
                // Check if the matching condition is met.
                switch (directive.MatchType)
                {
                    case MatchingType.MatchAny:
                        break;
                    case MatchingType.MatchVariable:
                        if (!expression.IsVariable)
                            return false;
                        break;
                    case MatchingType.MatchConstant:
                        if (!expression.IsConstant)
                            return false;
                        break;
                    case MatchingType.MatchExpression:
                        if (!expression.IsOperation)
                            return false;
                        break;
                    case MatchingType.MatchNonConstant:
                        if (expression.UnknownMask == 0)
                            return false;
                        break;
                    case MatchingType.MatchNonExpression:
                        if (expression.IsOperation)
                            return false;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown matching type: {directive.MatchType}");
                }

                // Save the mapping of this symbol and return success.
                _lookupTable[directive.LookupIndex] = expression;
                return true;
            }
            else
            {
                // Check if saved expression is equivalent, if not fail.
                return _lookupTable[directive.LookupIndex]!.IsIdentical(expression);
            }
        }

        /// <summary>
        /// Translates a variable to the matching expression.
        /// </summary>
        public Expression Translate(DirectiveInstance directive)
        {
            // Assert the looked up type is variable.
            if (directive.Operator != OperatorId.Invalid || directive.IsConstant)
                throw new InvalidOperationException("Cannot translate a non-variable directive");

            // Translate using the lookup table.
            return _lookupTable[directive.LookupIndex]!;
        }

        /// <summary>
        /// Clones this symbol table.
        /// </summary>
        public SymbolTable Clone()
        {
            return new SymbolTable(this);
        }
    }

    /// <summary>
    /// Fast matcher for directives - matches expressions against patterns.
    /// </summary>
    public static class FastMatcher
    {
        /// <summary>
        /// Tries to match the given expression with the directive and fills the
        /// given container of SymbolTables with the list of possible matches.
        /// </summary>
        public static int FastMatch(List<SymbolTable> results, DirectiveInstance directive, Expression expression, int index = 0)
        {
            // Initialize the result list if not done already.
            int size0 = results.Count;
            if (size0 == 0)
            {
                results.Add(new SymbolTable());
                size0++;
            }

            // If directive is a constant or a variable:
            if (directive.Operator == OperatorId.Invalid)
            {
                if (index >= results.Count)
                    return 0;

                // If directive is a variable:
                if (directive.Id != null)
                {
                    // If we could not add to the table / match the existing entry, erase the iterator off the results.
                    if (!results[index].Add(directive, expression))
                    {
                        results.RemoveAt(index);
                    }
                }
                // If directive is a constant:
                else
                {
                    // If the constants do not match, erase the iterator off the results.
                    var mask = BigInteger.One << expression.SizeInBits;
                    mask -= 1;

                    if (!expression.IsConstant || 
                        (expression.KnownOne & mask) != (directive.Value.KnownOne & mask))
                    {
                        results.RemoveAt(index);
                    }
                }
            }
            // If directive is an expression and the operators are the same
            else if (expression.Operator == directive.Operator)
            {
                // Resolve operator descriptor, if unary, redirect to the matching of RHS.
                var desc = Operators.GetDescriptor(expression.Operator);
                if (desc.OperandCount == 1)
                {
                    return FastMatch(results, directive.Rhs!, expression.Rhs!, index);
                }

                // If operator is commutative:
                if (desc.IsCommutative)
                {
                    // Save the current table on stack.
                    var tmp = results[index].Clone();

                    // Try matching the directive's RHS with expression's RHS.
                    int n = FastMatch(results, directive.Rhs!, expression.Rhs!, index);
                    if (n > 0)
                    {
                        // For each result produced, try matching the directive's LHS with expression's LHS.
                        while (n > 0)
                        {
                            n--;
                            FastMatch(results, directive.Lhs!, expression.Lhs!, index + n);
                        }
                    }

                    // Push the saved table into the results and update the iterator.
                    results.Add(tmp);
                    index = results.Count - 1;

                    // Try matching the directive's LHS with expression's RHS.
                    n = FastMatch(results, directive.Lhs!, expression.Rhs!, index);
                    if (n > 0)
                    {
                        // For each result produced, try matching the directive's RHS with expression's LHS.
                        while (n > 0)
                        {
                            n--;
                            FastMatch(results, directive.Rhs!, expression.Lhs!, index + n);
                        }
                    }
                }
                // If operator is not commutative:
                else
                {
                    // Try matching the directive's RHS with expression's RHS.
                    int n = FastMatch(results, directive.Rhs!, expression.Rhs!, index);
                    if (n > 0)
                    {
                        // For each result produced, try matching the directive's LHS with expression's LHS.
                        while (n > 0)
                        {
                            n--;
                            FastMatch(results, directive.Lhs!, expression.Lhs!, index + n);
                        }
                    }
                }
            }
            // If operators do not match, erase the iterator off the results.
            else
            {
                if (index < results.Count)
                {
                    results.RemoveAt(index);
                }
            }

            // Calculate and return the number of results.
            return (results.Count + 1) - size0;
        }
    }
}
