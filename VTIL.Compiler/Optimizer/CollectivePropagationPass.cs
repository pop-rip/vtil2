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
    /// Collective propagation pass that combines multiple optimization passes.
    /// </summary>
    public class CollectivePropagationPass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets the execution order for this pass (serial).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Serial;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "Collective Propagation";

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return ApplyCollectivePropagation(block, crossBlock);
            }
        }

        /// <summary>
        /// Applies collective propagation to a basic block.
        /// </summary>
        private int ApplyCollectivePropagation(BasicBlock block, bool crossBlock)
        {
            var totalOptimizations = 0;
            var passes = new IOptimizationPass[]
            {
                new StackPropagationPass(),
                new MovPropagationPass(),
                new DeadCodeEliminationPass(),
                new RegisterRenamingPass(),
                new DeadCodeEliminationPass()
            };

            foreach (var pass in passes)
            {
                var optimizations = pass.Pass(block, crossBlock);
                totalOptimizations += optimizations;

                // If we made changes, continue optimizing
                if (optimizations > 0)
                {
                    // Reset the pass to allow re-optimization
                    continue;
                }
            }

            return totalOptimizations;
        }
    }
}
