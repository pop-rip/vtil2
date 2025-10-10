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

namespace VTIL.SymEx.Simplifier
{
    /// <summary>
    /// Contains boolean simplification directives for comparison operations.
    /// </summary>
    public static class BooleanDirectives
    {
        // Variable placeholders for patterns
        private static readonly DirectiveInstance A = DirectiveInstance.Variables.A;
        private static readonly DirectiveInstance B = DirectiveInstance.Variables.B;
        private static readonly DirectiveInstance C = DirectiveInstance.Variables.C;
        private static readonly DirectiveInstance U = DirectiveInstance.Variables.U; // Constant
        private static readonly DirectiveInstance W = DirectiveInstance.Variables.W; // Any

        private static List<(DirectiveInstance from, DirectiveInstance to)>? _booleanSimplifiers;

        /// <summary>
        /// Boolean simplifiers - lazily initialized.
        /// </summary>
        public static List<(DirectiveInstance from, DirectiveInstance to)> BooleanSimplifiers
        {
            get
            {
                if (_booleanSimplifiers != null)
                    return _booleanSimplifiers;

                _booleanSimplifiers = new List<(DirectiveInstance from, DirectiveInstance to)>
                {
                    // Self-comparisons
                    (A<=A,                                                1),
                    (A<A,                                                 0),
                    (A==A,                                                1),
                    (A!=A,                                                0),
                    (A>A,                                                 0),
                    (A>=A,                                                1),

                    // Trivial comparisons with inverse
                    (A==~A,                                               0),
                    (A!=~A,                                               1),
                    (A==(-A),                                             0),
                    (A!=(-A),                                             1),

                    // Basic boolean simplifications for AND combinations
                    ((A>B)&(A>C),                                         DirectiveInstance.Iff(B>C, A>B)),
                    ((A>B)&(A>C),                                         DirectiveInstance.Iff(B>=C, A>B)),
                    ((A>B)&(A>C),                                         DirectiveInstance.Iff(B==C, A>B)),
                    ((A>B)&(A>=C),                                        DirectiveInstance.Iff(B>C, A>B)),
                    ((A>B)&(A>=C),                                        DirectiveInstance.Iff(B>=C, A>B)),
                    ((A>B)&(A>=C),                                        DirectiveInstance.Iff(B==C, A>B)),
                    ((A>B)&(A==C),                                        DirectiveInstance.Iff(B>C, 0)),
                    ((A>B)&(A==C),                                        DirectiveInstance.Iff(B>=C, 0)),
                    ((A>B)&(A==C),                                        DirectiveInstance.Iff(B==C, 0)),
                    ((A>B)&(A!=C),                                        DirectiveInstance.Iff(B>C, A>B)),
                    ((A>B)&(A!=C),                                        DirectiveInstance.Iff(B>=C, A>B)),
                    ((A>B)&(A!=C),                                        DirectiveInstance.Iff(B==C, A>B)),
                    ((A>B)&(A<=C),                                        DirectiveInstance.Iff(B>C, 0)),
                    ((A>B)&(A<=C),                                        DirectiveInstance.Iff(B>=C, 0)),
                    ((A>B)&(A<=C),                                        DirectiveInstance.Iff(B==C, 0)),
                    ((A>B)&(A<C),                                         DirectiveInstance.Iff(B>C, 0)),
                    ((A>B)&(A<C),                                         DirectiveInstance.Iff(B>=C, 0)),
                    ((A>B)&(A<C),                                         DirectiveInstance.Iff(B==C, 0)),

                    // Basic boolean simplifications for OR combinations
                    ((A>B)|(A>C),                                         DirectiveInstance.Iff(B==C, A>B)),
                    ((A>B)|(A>C),                                         DirectiveInstance.Iff(B<=C, A>B)),
                    ((A>B)|(A>C),                                         DirectiveInstance.Iff(B<C, A>B)),
                    ((A>B)|(A>=C),                                        DirectiveInstance.Iff(B==C, A>=B)),
                    ((A>B)|(A>=C),                                        DirectiveInstance.Iff(B<C, A>B)),
                    ((A>B)|(A==C),                                        DirectiveInstance.Iff(B==C, A>=B)),
                    ((A>B)|(A==C),                                        DirectiveInstance.Iff(B<C, A>B)),
                    ((A>B)|(A!=C),                                        DirectiveInstance.Iff(B==C, A!=B)),
                    ((A>B)|(A!=C),                                        DirectiveInstance.Iff(B<C, 1)),
                    ((A>B)|(A<=C),                                        DirectiveInstance.Iff(B==C, 1)),
                    ((A>B)|(A<=C),                                        DirectiveInstance.Iff(B<=C, 1)),
                    ((A>B)|(A<=C),                                        DirectiveInstance.Iff(B<C, 1)),
                    ((A>B)|(A<C),                                         DirectiveInstance.Iff(B==C, A!=B)),
                    ((A>B)|(A<C),                                         DirectiveInstance.Iff(B<C, 1)),

                    // Equality simplifications
                    ((A>=B)&(A>=C),                                       DirectiveInstance.Iff(B>C, A>=B)),
                    ((A>=B)&(A>=C),                                       DirectiveInstance.Iff(B>=C, A>=B)),
                    ((A>=B)&(A>=C),                                       DirectiveInstance.Iff(B==C, A>=B)),
                    ((A>=B)&(A>C),                                        DirectiveInstance.Iff(B>C, A>=B)),
                    ((A>=B)&(A>C),                                        DirectiveInstance.Iff(B>=C, A>=B)),
                    ((A>=B)&(A>C),                                        DirectiveInstance.Iff(B==C, A>=B)),
                    ((A>=B)&(A==C),                                       DirectiveInstance.Iff(B>C, 0)),
                    ((A>=B)&(A==C),                                       DirectiveInstance.Iff(B>=C, 0)),
                    ((A>=B)&(A==C),                                       DirectiveInstance.Iff(B==C, A==B)),
                    ((A>=B)&(A!=C),                                       DirectiveInstance.Iff(B>C, A>=B)),
                    ((A>=B)&(A!=C),                                       DirectiveInstance.Iff(B>=C, A>=B)),
                    ((A>=B)&(A!=C),                                       DirectiveInstance.Iff(B==C, A>B)),
                    ((A>=B)&(A<=C),                                       DirectiveInstance.Iff(B>C, 0)),
                    ((A>=B)&(A<=C),                                       DirectiveInstance.Iff(B>=C, A==C)),
                    ((A>=B)&(A<=C),                                       DirectiveInstance.Iff(B==C, A==B)),
                    ((A>=B)&(A<C),                                        DirectiveInstance.Iff(B>C, 0)),
                    ((A>=B)&(A<C),                                        DirectiveInstance.Iff(B>=C, 0)),
                    ((A>=B)&(A<C),                                        DirectiveInstance.Iff(B==C, 0)),

                    // Comparison transformations
                    (A==B,                                                (A-B)==0),
                    (A==B,                                                (A^B)==0),
                    (A!=B,                                                (A-B)!=0),
                    (A!=B,                                                (A^B)!=0),
                    ((A+B)==C,                                            A==(C-B)),
                    ((A-B)==C,                                            A==(C+B)),
                    ((A+B)!=C,                                            A!=(C-B)),
                    ((A-B)!=C,                                            A!=(C+B)),
                };

                return _booleanSimplifiers;
            }
        }

        /// <summary>
        /// Boolean joiners - transformations that join comparisons.
        /// </summary>
        public static readonly (DirectiveInstance from, DirectiveInstance to)[] BooleanJoiners = new[]
        {
            // Comparison transformations
            ((W>B),                                              (~(~W)<(~B))),
            ((W>=B),                                             (~(~W)<=(~B))),
            ((W==B),                                             (~(~W)==(~B))),
            ((W!=B),                                             (~(~W)!=(~B))),
            ((W<=B),                                             (~(~W)>=(~B))),
            ((W<B),                                              (~(~W)>(~B))),

            // Negation transformations
            ((W>B),                                              ((-W)<(-B))),
            ((W>=B),                                             ((-W)<=(-B))),
            ((W==B),                                             ((-W)==(-B))),
            ((W!=B),                                             ((-W)!=(-B))),
            ((W<=B),                                             ((-W)>=(-B))),
            ((W<B),                                              ((-W)>(-B))),

            // Equality/inequality transformations
            ((A+B)==C,                                           A==(C-B)),
            ((A-B)==C,                                           A==(C+B)),
            ((A+B)!=C,                                           A!=(C-B)),
            ((A-B)!=C,                                           A!=(C+B)),
            (A==B,                                               (A-B)==0),
            (A==B,                                               (A^B)==0),
            (A!=B,                                               (A-B)!=0),
            (A!=B,                                               (A^B)!=0),
        };

        /// <summary>
        /// Gets boolean simplifiers for a specific operator.
        /// </summary>
        public static IEnumerable<(DirectiveInstance from, DirectiveInstance to)> GetBooleanSimplifiers(OperatorId op)
        {
            return BooleanSimplifiers.Where(d => d.from.Operator == op);
        }

        /// <summary>
        /// Gets boolean joiners for a specific operator.
        /// </summary>
        public static IEnumerable<(DirectiveInstance from, DirectiveInstance to)> GetBooleanJoiners(OperatorId op)
        {
            return BooleanJoiners.Where(d => d.from.Operator == op);
        }
    }
}
