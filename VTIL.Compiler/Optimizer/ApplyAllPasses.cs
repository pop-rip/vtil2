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
using VTIL.Architecture;

namespace VTIL.Compiler.Optimizer
{
    /// <summary>
    /// Applies all optimization passes to a routine in the correct order.
    /// </summary>
    public static class ApplyAllPasses
    {
        /// <summary>
        /// Applies all optimization passes to a routine.
        /// </summary>
        public static int ApplyAll(Routine routine)
        {
            return ApplyCollectivePass(routine);
        }

        /// <summary>
        /// Applies all optimization passes to a routine with profiling.
        /// </summary>
        public static int ApplyAllProfiled(Routine routine)
        {
            var totalOptimizations = 0;
            var passTimes = new Dictionary<string, TimeSpan>();

            foreach (var pass in GetOptimizationPasses())
            {
                var startTime = DateTime.Now;
                var optimizations = pass.CrossPass(routine);
                var endTime = DateTime.Now;

                passTimes[pass.Name] = endTime - startTime;
                totalOptimizations += optimizations;

                Console.WriteLine($"{pass.Name}: {optimizations} optimizations in {passTimes[pass.Name].TotalMilliseconds:F2}ms");
            }

            Console.WriteLine($"Total optimizations: {totalOptimizations}");
            return totalOptimizations;
        }

        /// <summary>
        /// Applies the collective optimization pass.
        /// </summary>
        public static int ApplyCollectivePass(Routine routine)
        {
            var totalOptimizations = 0;

            // Apply collective cross pass
            totalOptimizations += ApplyCollectiveCrossPass(routine);

            return totalOptimizations;
        }

        /// <summary>
        /// Applies the collective cross pass.
        /// </summary>
        private static int ApplyCollectiveCrossPass(Routine routine)
        {
            var totalOptimizations = 0;

            // Phase 1: Stack and IStack optimization
            totalOptimizations += ApplyPass(new StackPinningPass(), routine);
            totalOptimizations += ApplyPass(new IStackRefSubstitutionPass(), routine);

            // Phase 2: Basic block extension
            totalOptimizations += ApplyPass(new BasicBlockExtensionPass(), routine);

            // Phase 3: Core local propagation
            totalOptimizations += ApplyCoreLocalPropagation(routine);

            // Phase 4: Dead code elimination
            totalOptimizations += ApplyPass(new DeadCodeEliminationPass(), routine);

            // Phase 5: Symbolic rewrite
            totalOptimizations += ApplyPass(new SymbolicRewritePass(true), routine);

            // Phase 6: Branch correction
            totalOptimizations += ApplyPass(new BranchCorrectionPass(), routine);

            // Phase 7: Collective propagation
            totalOptimizations += ApplyPass(new CollectivePropagationPass(), routine);

            // Phase 8: Symbolic rewrite (second pass)
            totalOptimizations += ApplyPass(new SymbolicRewritePass(true), routine);

            // Phase 9: Core local propagation (second pass)
            totalOptimizations += ApplyCoreLocalPropagation(routine);

            // Phase 10: Collective propagation (second pass)
            totalOptimizations += ApplyPass(new CollectivePropagationPass(), routine);

            // Phase 11: Exhaustive branch and block optimization
            totalOptimizations += ApplyExhaustiveBranchBlockOptimization(routine);

            // Phase 12: Exhaustive symbolic optimization
            totalOptimizations += ApplyExhaustiveSymbolicOptimization(routine);

            // Phase 13: Final stack pinning
            totalOptimizations += ApplyPass(new StackPinningPass(), routine);

            return totalOptimizations;
        }

        /// <summary>
        /// Applies the collective local pass.
        /// </summary>
        public static int ApplyCollectiveLocalPass(Routine routine)
        {
            var totalOptimizations = 0;

            // Phase 1: Stack optimization
            totalOptimizations += ApplyPass(new StackPinningPass(), routine);
            totalOptimizations += ApplyPass(new IStackRefSubstitutionPass(), routine);

            // Phase 2: Stack propagation
            totalOptimizations += ApplyPass(new StackPropagationPass(), routine);

            // Phase 3: Symbolic rewrite
            totalOptimizations += ApplyPass(new SymbolicRewritePass(true), routine);

            // Phase 4: Exhaustive local optimization
            totalOptimizations += ApplyExhaustiveLocalOptimization(routine);

            return totalOptimizations;
        }

        /// <summary>
        /// Applies core local propagation passes.
        /// </summary>
        private static int ApplyCoreLocalPropagation(Routine routine)
        {
            var totalOptimizations = 0;

            totalOptimizations += ApplyPass(new StackPropagationPass(), routine);
            totalOptimizations += ApplyPass(new DeadCodeEliminationPass(), routine);
            totalOptimizations += ApplyPass(new MovPropagationPass(), routine);
            totalOptimizations += ApplyPass(new RegisterRenamingPass(), routine);
            totalOptimizations += ApplyPass(new DeadCodeEliminationPass(), routine);

            return totalOptimizations;
        }

        /// <summary>
        /// Applies exhaustive branch and block optimization.
        /// </summary>
        private static int ApplyExhaustiveBranchBlockOptimization(Routine routine)
        {
            var totalOptimizations = 0;
            var passes = new IOptimizationPass[]
            {
                new BranchCorrectionPass(),
                new BasicBlockExtensionPass(),
                new BasicBlockThunkRemovalPass()
            };

            bool changed;
            do
            {
                changed = false;
                foreach (var pass in passes)
                {
                    var optimizations = ApplyPass(pass, routine);
                    if (optimizations > 0)
                    {
                        changed = true;
                        totalOptimizations += optimizations;
                    }
                }
            } while (changed);

            return totalOptimizations;
        }

        /// <summary>
        /// Applies exhaustive symbolic optimization.
        /// </summary>
        private static int ApplyExhaustiveSymbolicOptimization(Routine routine)
        {
            var totalOptimizations = 0;
            var passes = new IOptimizationPass[]
            {
                new SymbolicRewritePass(false),
                new CollectivePropagationPass()
            };

            bool changed;
            do
            {
                changed = false;
                foreach (var pass in passes)
                {
                    var optimizations = ApplyPass(pass, routine);
                    if (optimizations > 0)
                    {
                        changed = true;
                        totalOptimizations += optimizations;
                    }
                }
            } while (changed);

            return totalOptimizations;
        }

        /// <summary>
        /// Applies exhaustive local optimization.
        /// </summary>
        private static int ApplyExhaustiveLocalOptimization(Routine routine)
        {
            var totalOptimizations = 0;
            var passes = new IOptimizationPass[]
            {
                new MovPropagationPass(),
                new RegisterRenamingPass()
            };

            bool changed;
            do
            {
                changed = false;
                foreach (var pass in passes)
                {
                    var optimizations = ApplyPass(pass, routine);
                    if (optimizations > 0)
                    {
                        changed = true;
                        totalOptimizations += optimizations;
                    }
                }
            } while (changed);

            return totalOptimizations;
        }

        /// <summary>
        /// Applies a single optimization pass to a routine.
        /// </summary>
        private static int ApplyPass(IOptimizationPass pass, Routine routine)
        {
            return pass.CrossPass(routine);
        }

        /// <summary>
        /// Gets all optimization passes in the correct order.
        /// </summary>
        private static IEnumerable<IOptimizationPass> GetOptimizationPasses()
        {
            return new IOptimizationPass[]
            {
                new StackPinningPass(),
                new IStackRefSubstitutionPass(),
                new BasicBlockExtensionPass(),
                new StackPropagationPass(),
                new DeadCodeEliminationPass(),
                new MovPropagationPass(),
                new RegisterRenamingPass(),
                new SymbolicRewritePass(true),
                new BranchCorrectionPass(),
                new CollectivePropagationPass(),
                new BasicBlockThunkRemovalPass()
            };
        }
    }
}
