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
using VTIL.Common.Util;

namespace VTIL.Architecture
{
    /// <summary>
    /// Represents a basic block in a VTIL routine.
    /// </summary>
    public sealed class BasicBlock
    {
        private readonly List<Instruction> _instructions = new();
        private readonly List<BasicBlock> _predecessors = new();
        private readonly List<BasicBlock> _successors = new();

        /// <summary>
        /// The routine this block belongs to.
        /// </summary>
        public Routine? Routine { get; internal set; }

        /// <summary>
        /// The virtual instruction pointer for this block.
        /// </summary>
        public VipT Vip { get; }

        /// <summary>
        /// The instructions in this block.
        /// </summary>
        public IReadOnlyList<Instruction> Instructions => _instructions;

        /// <summary>
        /// The predecessor blocks.
        /// </summary>
        public IReadOnlyList<BasicBlock> Predecessors => _predecessors;

        /// <summary>
        /// The successor blocks.
        /// </summary>
        public IReadOnlyList<BasicBlock> Successors => _successors;

        /// <summary>
        /// Whether this block has been explored.
        /// </summary>
        public bool IsExplored { get; internal set; }

        /// <summary>
        /// Whether this block is an exit block.
        /// </summary>
        public bool IsExit => _successors.Count == 0;

        /// <summary>
        /// Whether this block is an entry block.
        /// </summary>
        public bool IsEntry => _predecessors.Count == 0;

        /// <summary>
        /// Creates a new basic block.
        /// </summary>
        public BasicBlock(VipT vip)
        {
            Vip = vip;
        }

        /// <summary>
        /// Creates a new basic block with a routine.
        /// </summary>
        internal BasicBlock(Routine routine, VipT vip)
        {
            Routine = routine;
            Vip = vip;
        }

        /// <summary>
        /// Gets the number of instructions in this block.
        /// </summary>
        public int InstructionCount => _instructions.Count;

        /// <summary>
        /// Gets an instruction by index.
        /// </summary>
        public Instruction GetInstruction(int index)
        {
            if (index < 0 || index >= _instructions.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _instructions[index];
        }

        /// <summary>
        /// Gets the first instruction in this block.
        /// </summary>
        public Instruction? FirstInstruction => _instructions.Count > 0 ? _instructions[0] : null;

        /// <summary>
        /// Gets the last instruction in this block.
        /// </summary>
        public Instruction? LastInstruction => _instructions.Count > 0 ? _instructions[_instructions.Count - 1] : null;

        /// <summary>
        /// Adds an instruction to the end of this block.
        /// </summary>
        public void AddInstruction(Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException(nameof(instruction));

            _instructions.Add(instruction);
            Routine?.SignalModification();
        }

        /// <summary>
        /// Inserts an instruction at the specified index.
        /// </summary>
        public void InsertInstruction(int index, Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException(nameof(instruction));
            if (index < 0 || index > _instructions.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _instructions.Insert(index, instruction);
            Routine?.SignalModification();
        }

        /// <summary>
        /// Inserts an instruction at the specified index.
        /// </summary>
        public void ReplaceInstruction(int index, Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException(nameof(instruction));
            if (index < 0 || index > _instructions.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _instructions[index] = instruction;
            Routine?.SignalModification();
        }

        /// <summary>
        /// Removes an instruction at the specified index.
        /// </summary>
        public Instruction RemoveInstruction(int index)
        {
            if (index < 0 || index >= _instructions.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var instruction = _instructions[index];
            _instructions.RemoveAt(index);
            Routine?.SignalModification();
            return instruction;
        }

        /// <summary>
        /// Removes the specified instruction.
        /// </summary>
        public bool RemoveInstruction(Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException(nameof(instruction));

            var removed = _instructions.Remove(instruction);
            if (removed)
                Routine?.SignalModification();
            return removed;
        }

        /// <summary>
        /// Clears all instructions from this block.
        /// </summary>
        public void ClearInstructions()
        {
            _instructions.Clear();
            Routine?.SignalModification();
        }

        /// <summary>
        /// Adds a predecessor block.
        /// </summary>
        public void AddPredecessor(BasicBlock predecessor)
        {
            if (predecessor == null)
                throw new ArgumentNullException(nameof(predecessor));
            if (!_predecessors.Contains(predecessor))
            {
                _predecessors.Add(predecessor);
                Routine?.SignalCfgModification();
            }
        }

        /// <summary>
        /// Removes a predecessor block.
        /// </summary>
        public void RemovePredecessor(BasicBlock predecessor)
        {
            if (predecessor == null)
                throw new ArgumentNullException(nameof(predecessor));
            if (_predecessors.Remove(predecessor))
            {
                Routine?.SignalCfgModification();
            }
        }

        /// <summary>
        /// Adds a successor block.
        /// </summary>
        public void AddSuccessor(BasicBlock successor)
        {
            if (successor == null)
                throw new ArgumentNullException(nameof(successor));
            if (!_successors.Contains(successor))
            {
                _successors.Add(successor);
                successor.AddPredecessor(this);
                Routine?.SignalCfgModification();
            }
        }

        /// <summary>
        /// Removes a successor block.
        /// </summary>
        public void RemoveSuccessor(BasicBlock successor)
        {
            if (successor == null)
                throw new ArgumentNullException(nameof(successor));
            if (_successors.Remove(successor))
            {
                successor.RemovePredecessor(this);
                Routine?.SignalCfgModification();
            }
        }

        /// <summary>
        /// Clears all predecessors and successors.
        /// </summary>
        public void ClearConnections()
        {
            foreach (var predecessor in _predecessors.ToArray())
            {
                predecessor.RemoveSuccessor(this);
            }
            foreach (var successor in _successors.ToArray())
            {
                RemoveSuccessor(successor);
            }
        }

        /// <summary>
        /// Checks if this block has the specified predecessor.
        /// </summary>
        public bool HasPredecessor(BasicBlock predecessor)
        {
            return _predecessors.Contains(predecessor);
        }

        /// <summary>
        /// Checks if this block has the specified successor.
        /// </summary>
        public bool HasSuccessor(BasicBlock successor)
        {
            return _successors.Contains(successor);
        }

        /// <summary>
        /// Checks if this block is reachable from the entry point.
        /// </summary>
        public bool IsReachable()
        {
            if (IsEntry) return true;

            var visited = new HashSet<BasicBlock>();
            var queue = new Queue<BasicBlock>();

            // Start from all entry blocks
            if (Routine != null)
            {
                foreach (var block in Routine.GetEntryBlocks())
                {
                    queue.Enqueue(block);
                    visited.Add(block);
                }
            }

            // BFS to find reachability
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == this)
                    return true;

                foreach (var successor in current.Successors)
                {
                    if (!visited.Contains(successor))
                    {
                        visited.Add(successor);
                        queue.Enqueue(successor);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all blocks reachable from this block.
        /// </summary>
        public IReadOnlyList<BasicBlock> GetReachableBlocks()
        {
            var result = new List<BasicBlock>();
            var visited = new HashSet<BasicBlock>();
            var queue = new Queue<BasicBlock>();

            queue.Enqueue(this);
            visited.Add(this);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                foreach (var successor in current.Successors)
                {
                    if (!visited.Contains(successor))
                    {
                        visited.Add(successor);
                        queue.Enqueue(successor);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if this block is in a loop.
        /// </summary>
        public bool IsInLoop()
        {
            var visited = new HashSet<BasicBlock>();
            return IsInLoopRecursive(this, visited);
        }

        /// <summary>
        /// Recursive helper for loop detection.
        /// </summary>
        private static bool IsInLoopRecursive(BasicBlock block, HashSet<BasicBlock> visited)
        {
            if (visited.Contains(block))
                return true;

            visited.Add(block);

            foreach (var successor in block.Successors)
            {
                if (IsInLoopRecursive(successor, visited))
                    return true;
            }

            visited.Remove(block);
            return false;
        }

        /// <summary>
        /// Gets the depth of this block in the control flow graph.
        /// </summary>
        public int GetDepth()
        {
            var visited = new HashSet<BasicBlock>();
            return GetDepthRecursive(this, visited);
        }

        /// <summary>
        /// Recursive helper for depth calculation.
        /// </summary>
        private static int GetDepthRecursive(BasicBlock block, HashSet<BasicBlock> visited)
        {
            if (visited.Contains(block))
                return 0;

            visited.Add(block);

            int maxDepth = 0;
            foreach (var predecessor in block.Predecessors)
            {
                maxDepth = Math.Max(maxDepth, GetDepthRecursive(predecessor, visited));
            }

            visited.Remove(block);
            return maxDepth + 1;
        }

        /// <summary>
        /// Clones this basic block.
        /// </summary>
        public BasicBlock Clone()
        {
            var clone = new BasicBlock(Vip);
            
            foreach (var instruction in _instructions)
            {
                // Note: Instructions are immutable, so we can just copy references
                clone._instructions.Add(instruction);
            }

            return clone;
        }

        public override string ToString()
        {
            return $"Block(0x{Vip:X})[{InstructionCount} instructions]";
        }
    }
}
