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
using VTIL.Architecture;

namespace VTIL.Compiler
{
    /// <summary>
    /// Interface for optimization passes that operate on basic blocks.
    /// </summary>
    public interface IOptimizationPass
    {
        /// <summary>
        /// Gets the execution order for this pass.
        /// </summary>
        ExecutionOrder ExecutionOrder { get; }

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        /// <param name="block">The basic block to optimize</param>
        /// <param name="crossBlock">Whether cross-block exploration is allowed</param>
        /// <returns>The number of optimizations applied</returns>
        int Pass(BasicBlock block, bool crossBlock = false);

        /// <summary>
        /// Passes every block through the optimizer with block references freely explorable.
        /// </summary>
        /// <param name="routine">The routine to optimize</param>
        /// <returns>The number of optimizations applied</returns>
        int CrossPass(Routine routine);
    }

    /// <summary>
    /// Base class for optimization passes with common functionality.
    /// </summary>
    public abstract class OptimizationPassBase : IOptimizationPass
    {
        /// <summary>
        /// Gets the execution order for this pass.
        /// </summary>
        public abstract ExecutionOrder ExecutionOrder { get; }

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public virtual string Name => GetType().Name;

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public abstract int Pass(BasicBlock block, bool crossBlock = false);

        /// <summary>
        /// Passes every block through the optimizer.
        /// </summary>
        public virtual int CrossPass(Routine routine)
        {
            return ApplyPass(routine, this);
        }

        /// <summary>
        /// Applies a pass to a routine based on its execution order.
        /// </summary>
        protected static int ApplyPass(Routine routine, IOptimizationPass pass)
        {
            var totalOptimizations = 0;

            switch (pass.ExecutionOrder)
            {
                case ExecutionOrder.Serial:
                    routine.ForEachBlock(block => totalOptimizations += pass.Pass(block, true));
                    break;

                case ExecutionOrder.SerialBreadthFirst:
                case ExecutionOrder.SerialDepthFirst:
                    totalOptimizations += ApplySerialTraversal(routine, pass, 
                        pass.ExecutionOrder == ExecutionOrder.SerialDepthFirst);
                    break;

                case ExecutionOrder.Parallel:
                    totalOptimizations += ApplyParallel(routine, pass);
                    break;

                case ExecutionOrder.ParallelBreadthFirst:
                case ExecutionOrder.ParallelDepthFirst:
                    totalOptimizations += ApplyParallelTraversal(routine, pass,
                        pass.ExecutionOrder == ExecutionOrder.ParallelBreadthFirst);
                    break;

                case ExecutionOrder.Custom:
                    throw new InvalidOperationException("Custom execution order must be implemented by the pass");

                default:
                    throw new ArgumentException($"Unknown execution order: {pass.ExecutionOrder}");
            }

            return totalOptimizations;
        }

        /// <summary>
        /// Applies serial traversal (breadth-first or depth-first).
        /// </summary>
        private static int ApplySerialTraversal(Routine routine, IOptimizationPass pass, bool depthFirst)
        {
            var totalOptimizations = 0;
            var visited = new HashSet<BasicBlock>();

            Action<BasicBlock> traverse = null;
            traverse = (block) =>
            {
                if (!visited.Add(block))
                    return;

                if (depthFirst)
                {
                    // Depth-first: process successors first
                    foreach (var successor in block.Successors)
                        traverse(successor);
                }
                else
                {
                    // Breadth-first: process predecessors first
                    foreach (var predecessor in block.Predecessors)
                        traverse(predecessor);
                }

                totalOptimizations += pass.Pass(block, true);
            };

            if (depthFirst)
            {
                if (routine.EntryPoint != null)
                    traverse(routine.EntryPoint);
            }
            else
            {
                var exits = routine.GetExitBlocks();
                foreach (var exit in exits)
                    traverse(exit);
            }

            return totalOptimizations;
        }

        /// <summary>
        /// Applies parallel execution to all blocks.
        /// </summary>
        private static int ApplyParallel(Routine routine, IOptimizationPass pass)
        {
            var totalOptimizations = 0;
            var lockObject = new object();

            Parallel.ForEach(routine.ExploredBlocks.Values, block =>
            {
                var optimizations = pass.Pass(block, true);
                if (optimizations > 0)
                {
                    lock (lockObject)
                    {
                        totalOptimizations += optimizations;
                    }
                }
            });

            return totalOptimizations;
        }

        /// <summary>
        /// Applies parallel traversal with depth ordering.
        /// </summary>
        private static int ApplyParallelTraversal(Routine routine, IOptimizationPass pass, bool breadthFirst)
        {
            var totalOptimizations = 0;
            var lockObject = new object();

            // Get depth-ordered blocks
            var depthOrdered = GetDepthOrderedBlocks(routine, breadthFirst);

            // Process blocks in parallel by depth level
            foreach (var levelBlocks in depthOrdered)
            {
                Parallel.ForEach(levelBlocks, block =>
                {
                    var optimizations = pass.Pass(block, true);
                    if (optimizations > 0)
                    {
                        lock (lockObject)
                        {
                            totalOptimizations += optimizations;
                        }
                    }
                });
            }

            return totalOptimizations;
        }

        /// <summary>
        /// Gets blocks ordered by depth for traversal.
        /// </summary>
        private static IEnumerable<List<BasicBlock>> GetDepthOrderedBlocks(Routine routine, bool breadthFirst)
        {
            var depthMap = new Dictionary<int, List<BasicBlock>>();
            var visited = new HashSet<BasicBlock>();

            void CalculateDepth(BasicBlock block, int depth)
            {
                if (!visited.Add(block))
                    return;

                if (!depthMap.ContainsKey(depth))
                    depthMap[depth] = new List<BasicBlock>();
                depthMap[depth].Add(block);

                if (breadthFirst)
                {
                    foreach (var successor in block.Successors)
                        CalculateDepth(successor, depth + 1);
                }
                else
                {
                    foreach (var predecessor in block.Predecessors)
                        CalculateDepth(predecessor, depth + 1);
                }
            }

            if (breadthFirst && routine.EntryPoint != null)
            {
                CalculateDepth(routine.EntryPoint, 0);
            }
            else
            {
                var exits = routine.GetExitBlocks();
                foreach (var exit in exits)
                    CalculateDepth(exit, 0);
            }

            return depthMap.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value);
        }
    }
}
