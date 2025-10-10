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
using VTIL.Common.Math;

namespace VTIL.SymEx.Simplifier
{
    /// <summary>
    /// Contains directive rules for expression simplification.
    /// </summary>
    public static class DirectiveRules
    {
        // Variable placeholders for patterns
        private static readonly DirectiveInstance A = DirectiveInstance.Variables.A;
        private static readonly DirectiveInstance B = DirectiveInstance.Variables.B;
        private static readonly DirectiveInstance C = DirectiveInstance.Variables.C;
        private static readonly DirectiveInstance U = DirectiveInstance.Variables.U; // Constant
        private static readonly DirectiveInstance V = DirectiveInstance.Variables.V; // Variable
        private static readonly DirectiveInstance X = DirectiveInstance.Variables.X; // Expression

        /// <summary>
        /// List of universal simplifiers - they have to reduce complexity or keep it equal
        /// at the very least to not cause an infinity loop.
        /// </summary>
        public static readonly (DirectiveInstance from, DirectiveInstance to)[] UniversalSimplifiers = new[]
        {
            // Double inverse
            (-(-A),                                              A),
            (~(~A),                                              A),
            (-(~A),                                              A+1),
            (~(-A),                                              A-1),

            // Identity constants
            (A+0,                                                A),
            (A-0,                                                A),
            (A|A,                                                A),
            (A|0,                                                A),
            (A&A,                                                A),
            (A^0,                                                A),
            (A&(-1),                                             A),
            (A*1,                                                A),
            (A/1,                                                A),

            // Constant result
            (A-A,                                                DirectiveInstance.Constant(0)),
            (A+(-A),                                             DirectiveInstance.Constant(0)),
            (A&0,                                                DirectiveInstance.Constant(0)),
            (A^A,                                                DirectiveInstance.Constant(0)),
            (A&(~A),                                             DirectiveInstance.Constant(0)),
            (A|(-1),                                             DirectiveInstance.Constant(-1)),
            (A+(~A),                                             DirectiveInstance.Constant(-1)),
            (A^(~A),                                             DirectiveInstance.Constant(-1)),
            (A|(~A),                                             DirectiveInstance.Constant(-1)),
            (A/A,                                                DirectiveInstance.Constant(1)),
            (A*0,                                                DirectiveInstance.Constant(0)),

            // SUB conversion
            (A+(-B),                                             A-B),
            (~((~A)+B),                                          A-B),
            (~(A-B),                                             (~A)+B),
            ((~A)+U,                                             (U-1)-A),

            // NEG conversion
            (~(A-1),                                             -A),
            (0-A,                                                -A),

            // MUL conversion
            (A+A,                                                A*2),
            (A*U-A,                                              A*(U-1)),
            (A*U+A,                                              A*(U+1)),

            // Invert comparison
            (~(A>B),                                             A<=B),
            (~(A>=B),                                            A<B),
            (~(A==B),                                            A!=B),
            (~(A!=B),                                            A==B),
            (~(A<=B),                                            A>B),
            (~(A<B),                                             A>=B),

            // NOT conversion
            (A^(-1),                                             ~A),

            // XOR conversion
            ((A|B)&(~(A&B)),                                     A^B),
            ((A|B)&((~A)|(~B)),                                  A^B),
            ((A&(~B))|((~A)&B),                                  A^B),
            ((~(A|B))|(A&B),                                     ~(A^B)),
            (((~A)&(~B))|(A&B),                                  ~(A^B)),

            // Simplify AND OR NOT XOR
            (A&(A|B),                                            A),
            (A|(A&B),                                            A),
            (A^(A&B),                                            A&(~B)),
            (A^(A|B),                                            B&(~A)),

            // Simplify AND/OR/NOT combinations
            ((~A)&(~B),                                          ~(A|B)),
            ((~A)|(~B),                                          ~(A&B)),
            ((A&B)|(A&C),                                        A&(B|C)),
            ((A|B)&(A|C),                                        A|(B&C)),
        };

        /// <summary>
        /// Describes the way operands of two operators join each other.
        /// Has no obligation to produce simple output, should be checked.
        /// </summary>
        public static readonly (DirectiveInstance from, DirectiveInstance to)[] JoinDescriptors = new[]
        {
            // ADD
            (A+(B+C),                                            (A+B)+C),
            (A+(B-C),                                            (A+B)-C),
            (A+(B-C),                                            (A-C)+B),

            // SUB
            (A-(B+C),                                            (A-B)-C),
            (A-(B-C),                                            (A+C)-B),
            (A-(B-C),                                            (A-B)+C),
            ((B+C)-A,                                            (B-A)+C),
            ((B-C)-A,                                            B-(A+C)),
            ((B-C)-A,                                            (B-A)-C),

            // OR
            (A|(B|C),                                            (A|B)|(A|C)),
            (A|(B|C),                                            (A|B)|C),
            (A|(B&C),                                            (A|B)&(A|C)),
            (A|(B&C),                                            A|((A|B)&C)),
            (A|(B^C),                                            A|((B&(~A))^(C&(~A)))),
            (A|(~B),                                             ~(B&(~A))),

            // AND
            (A&(B|C),                                            (A&B)|(A&C)),
            (A&(B|C),                                            A&((A&B)|C)),
            (A&(B&C),                                            (A&B)&(A&C)),
            (A&(B&C),                                            (A&B)&C),
            (A&(B^C),                                            (A&B)^(A&C)),
            (A&(B^C),                                            A&((A&B)^C)),
            (A&(~B),                                             ~(B|(~A))),

            // XOR
            (A^(B&C),                                            (~(B&(A&C)))&(A|(B&C))),
            (A^(B|C),                                            ((A|C)|B)&(~(A&(B|C)))),
            (A^(B^C),                                            B^(A^C)),
            (A^(~B),                                             (~A)^B),

            // SHL
            ((A|B)<<C,                                           (A<<C)|(B<<C)),
            ((A&B)<<C,                                           (A<<C)&(B<<C)),
            ((A^B)<<C,                                           (A<<C)^(B<<C)),
            ((A<<B)<<C,                                          A<<(B+C)),
            ((~A)<<U,                                            (~(A<<U))&((-1)<<U)),

            // SHR
            ((A|B)>>C,                                           (A>>C)|(B>>C)),
            ((A&B)>>C,                                           (A>>C)&(B>>C)),
            ((A^B)>>C,                                           (A>>C)^(B>>C)),
            ((A>>B)>>C,                                          A>>(B+C)),
            ((~A)>>U,                                            (~(A>>U))&((-1)>>U)),

            // NOT
            (~(A|B),                                             (~A)&(~B)),
            (~(A&B),                                             (~A)|(~B)),
            (~(A^B),                                             (~A)^B),

            // MUL
            ((A+B)*C,                                            (A*C)+(B*C)),
            ((A+B)*C,                                            (A*C)+(B*C)),
            ((A-B)*C,                                            (A*C)-(B*C)),
            ((A-B)*C,                                            (A*C)-(B*C)),
            (A*(B*C),                                            (A*C)*B),
            (A*(-B),                                             ((-A)*B)),
            ((A*B)+(A*C),                                        A*(B+C)),
            ((A*B)-(A*C),                                        A*(B-C)),
        };

        /// <summary>
        /// Grouping of simple representations into more complex directives (packing).
        /// </summary>
        public static readonly (DirectiveInstance from, DirectiveInstance to)[] PackDescriptors = new[]
        {
            // Note: These would use directive functions like __bt, __min, __max, etc.
            // For now, providing simplified versions
        };

        /// <summary>
        /// Conversion from more complex directives into simple representations (unpacking).
        /// </summary>
        public static readonly (DirectiveInstance from, DirectiveInstance to)[] UnpackDescriptors = new[]
        {
            // Note: These would use directive functions like __bt, __min, __max, etc.
            // For now, providing simplified versions
        };

        /// <summary>
        /// Gets the appropriate directive table based on operator.
        /// </summary>
        public static IEnumerable<(DirectiveInstance from, DirectiveInstance to)> GetUniversalSimplifiers(OperatorId op)
        {
            return UniversalSimplifiers.Where(d => d.from.Operator == op);
        }

        /// <summary>
        /// Gets the join descriptors for an operator.
        /// </summary>
        public static IEnumerable<(DirectiveInstance from, DirectiveInstance to)> GetJoinDescriptors(OperatorId op)
        {
            return JoinDescriptors.Where(d => d.from.Operator == op);
        }

        /// <summary>
        /// Gets the pack descriptors for an operator.
        /// </summary>
        public static IEnumerable<(DirectiveInstance from, DirectiveInstance to)> GetPackDescriptors(OperatorId op)
        {
            return PackDescriptors.Where(d => d.from.Operator == op);
        }

        /// <summary>
        /// Gets the unpack descriptors for an operator.
        /// </summary>
        public static IEnumerable<(DirectiveInstance from, DirectiveInstance to)> GetUnpackDescriptors(OperatorId op)
        {
            return UnpackDescriptors.Where(d => d.from.Operator == op);
        }
    }
}
