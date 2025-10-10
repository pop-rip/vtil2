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
using System.Threading;
using VTIL.Common.Util;

namespace VTIL.Architecture
{
    /// <summary>
    /// Describes any routine that is being translated.
    /// </summary>
    public sealed class Routine : IDisposable
    {
        private readonly object _mutex = new object();
        private readonly Dictionary<VipT, BasicBlock> _exploredBlocks = new();
        private readonly Dictionary<VipT, CallConvention> _specializedCallConventions = new();
        private long _lastInternalId = 0;
        private long _localOptCount = 0;
        private ulong _epoch;
        private ulong _cfgEpoch;

        /// <summary>
        /// Physical architecture routine is bound to.
        /// </summary>
        public ArchitectureIdentifier ArchitectureId { get; }

        /// <summary>
        /// Cache of explored blocks, mapping virtual instruction pointer to the basic block structure.
        /// </summary>
        public IReadOnlyDictionary<VipT, BasicBlock> ExploredBlocks => _exploredBlocks;

        /// <summary>
        /// Reference to the first block, entry point.
        /// </summary>
        public BasicBlock? EntryPoint { get; private set; }

        /// <summary>
        /// Last local identifier used for an internal register.
        /// </summary>
        public long LastInternalId => Interlocked.Read(ref _lastInternalId);

        /// <summary>
        /// Calling convention of the routine.
        /// </summary>
        public CallConvention RoutineConvention { get; set; }

        /// <summary>
        /// Calling convention of a non-specialized VXCALL.
        /// </summary>
        public CallConvention SubroutineConvention { get; set; }

        /// <summary>
        /// Convention of specialized calls, maps the vip of the VXCALL instruction onto the convention used.
        /// </summary>
        public IReadOnlyDictionary<VipT, CallConvention> SpecializedCallConventions => _specializedCallConventions;

        /// <summary>
        /// Misc. stats.
        /// </summary>
        public long LocalOptCount => Interlocked.Read(ref _localOptCount);

        /// <summary>
        /// Epoch provided to allow external entities determine if the routine is modified or not 
        /// since their last read from it in an easy and fast way.
        /// </summary>
        public ulong Epoch => Interlocked.Read(ref _epoch);

        /// <summary>
        /// Control flow graph epoch.
        /// </summary>
        public ulong CfgEpoch => Interlocked.Read(ref _cfgEpoch);

        /// <summary>
        /// Creates a new routine for the specified architecture.
        /// </summary>
        public Routine(ArchitectureIdentifier architectureId)
        {
            ArchitectureId = architectureId;
            _epoch = (ulong)Random.Shared.NextInt64();
            _cfgEpoch = (ulong)Random.Shared.NextInt64();

            // Set default calling conventions based on architecture
            SetDefaultCallingConventions();
        }

        /// <summary>
        /// Sets default calling conventions based on architecture.
        /// </summary>
        private void SetDefaultCallingConventions()
        {
            var defaultConvention = new CallConvention();

            switch (ArchitectureId)
            {
                case ArchitectureIdentifier.Amd64:
                    // x64 fastcall
                    defaultConvention = new CallConvention(
                        purgeStack: false,
                        stackAlignment: 16,
                        shadowSpace: 32,
                        maxRegisterParameters: 4
                    );
                    break;

                case ArchitectureIdentifier.X86:
                    // x86 cdecl
                    defaultConvention = new CallConvention(
                        purgeStack: true,
                        stackAlignment: 4,
                        maxRegisterParameters: 0
                    );
                    break;

                case ArchitectureIdentifier.Arm64:
                    // AAPCS
                    defaultConvention = new CallConvention(
                        purgeStack: false,
                        stackAlignment: 16,
                        maxRegisterParameters: 8
                    );
                    break;

                case ArchitectureIdentifier.Virtual:
                    defaultConvention = new CallConvention(purgeStack: true);
                    break;
            }

            RoutineConvention = defaultConvention;
            SubroutineConvention = defaultConvention;
        }

        /// <summary>
        /// Gets the number of explored blocks.
        /// </summary>
        public int BlockCount => _exploredBlocks.Count;

        /// <summary>
        /// Gets the number of instructions across all blocks.
        /// </summary>
        public int InstructionCount => _exploredBlocks.Values.Sum(block => block.InstructionCount);

        /// <summary>
        /// Gets the number of branches across all blocks.
        /// </summary>
        public int BranchCount => _exploredBlocks.Values.Sum(block => block.Successors.Count);

        /// <summary>
        /// Helpers for the allocation of unique internal registers.
        /// </summary>
        public RegisterDescriptor AllocRegister(BitCntT size)
        {
            var id = Interlocked.Increment(ref _lastInternalId);
            return RegisterDescriptor.CreateInternal((ulong)id, size);
        }

        /// <summary>
        /// Allocates multiple internal registers.
        /// </summary>
        public RegisterDescriptor[] AllocRegisters(params BitCntT[] sizes)
        {
            return sizes.Select(AllocRegister).ToArray();
        }

        /// <summary>
        /// Invokes the enumerator passed for each basic block this routine contains.
        /// </summary>
        public void ForEachBlock(Action<BasicBlock> action)
        {
            lock (_mutex)
            {
                foreach (var block in _exploredBlocks.Values)
                {
                    action(block);
                }
            }
        }

        /// <summary>
        /// Gets the calling convention for the given VIP (that resolves into VXCALL).
        /// </summary>
        public CallConvention GetCallConvention(VipT vip)
        {
            lock (_mutex)
            {
                return _specializedCallConventions.TryGetValue(vip, out var convention) 
                    ? convention 
                    : SubroutineConvention;
            }
        }

        /// <summary>
        /// Sets the calling convention for the given VIP (that resolves into VXCALL).
        /// </summary>
        public void SetCallConvention(VipT vip, CallConvention convention)
        {
            lock (_mutex)
            {
                _specializedCallConventions[vip] = convention;
            }
        }

        /// <summary>
        /// Finds a block in the list.
        /// </summary>
        public BasicBlock? FindBlock(VipT vip)
        {
            lock (_mutex)
            {
                return _exploredBlocks.TryGetValue(vip, out var block) ? block : null;
            }
        }

        /// <summary>
        /// Gets a block in the list, throws if not found.
        /// </summary>
        public BasicBlock GetBlock(VipT vip)
        {
            return FindBlock(vip) ?? throw new KeyNotFoundException($"Block with VIP 0x{vip:X} not found");
        }

        /// <summary>
        /// Tries creating a new block bound to this routine.
        /// </summary>
        public (BasicBlock block, bool wasCreated) CreateBlock(VipT vip, BasicBlock? source = null)
        {
            lock (_mutex)
            {
                if (_exploredBlocks.TryGetValue(vip, out var existingBlock))
                {
                    return (existingBlock, false);
                }

                var newBlock = new BasicBlock(this, vip);
                _exploredBlocks[vip] = newBlock;

                if (EntryPoint == null)
                {
                    EntryPoint = newBlock;
                }

                if (source != null)
                {
                    source.AddSuccessor(newBlock);
                }

                SignalCfgModification();
                return (newBlock, true);
            }
        }

        /// <summary>
        /// Deletes a block, should have no links or links must be nullified (no back-links).
        /// </summary>
        public void DeleteBlock(BasicBlock block)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            lock (_mutex)
            {
                if (!_exploredBlocks.TryGetValue(block.Vip, out var foundBlock) || foundBlock != block)
                {
                    throw new ArgumentException("Block not found in this routine");
                }

                // Clear all connections
                block.ClearConnections();

                // Remove from explored blocks
                _exploredBlocks.Remove(block.Vip);

                // Update entry point if necessary
                if (EntryPoint == block)
                {
                    EntryPoint = _exploredBlocks.Values.FirstOrDefault();
                }

                SignalCfgModification();
            }
        }

        /// <summary>
        /// Gets a list of exit blocks.
        /// </summary>
        public IReadOnlyList<BasicBlock> GetExitBlocks()
        {
            lock (_mutex)
            {
                return _exploredBlocks.Values.Where(block => block.IsExit).ToArray();
            }
        }

        /// <summary>
        /// Gets a list of entry blocks.
        /// </summary>
        public IReadOnlyList<BasicBlock> GetEntryBlocks()
        {
            lock (_mutex)
            {
                return _exploredBlocks.Values.Where(block => block.IsEntry).ToArray();
            }
        }

        /// <summary>
        /// Checks whether the block is in a loop.
        /// </summary>
        public bool IsBlockInLoop(BasicBlock block)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            return block.IsInLoop();
        }

        /// <summary>
        /// Checks if a path exists from source to destination block.
        /// </summary>
        public bool HasPath(BasicBlock source, BasicBlock destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            var visited = new HashSet<BasicBlock>();
            var queue = new Queue<BasicBlock>();

            queue.Enqueue(source);
            visited.Add(source);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == destination)
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
        /// Signals that the routine has been modified.
        /// </summary>
        public void SignalModification()
        {
            Interlocked.Increment(ref _epoch);
        }

        /// <summary>
        /// Signals that the control flow graph has been modified.
        /// </summary>
        public void SignalCfgModification()
        {
            Interlocked.Increment(ref _epoch);
            Interlocked.Increment(ref _cfgEpoch);
        }

        /// <summary>
        /// Increments the local optimization count.
        /// </summary>
        public void IncrementOptCount()
        {
            Interlocked.Increment(ref _localOptCount);
        }

        /// <summary>
        /// Clones the routine and all its blocks.
        /// </summary>
        public Routine Clone()
        {
            lock (_mutex)
            {
                var clone = new Routine(ArchitectureId);
                clone.RoutineConvention = RoutineConvention.Clone();
                clone.SubroutineConvention = SubroutineConvention.Clone();

                // Clone all blocks
                foreach (var kvp in _exploredBlocks)
                {
                    var clonedBlock = kvp.Value.Clone();
                    clone._exploredBlocks[kvp.Key] = clonedBlock;
                    clonedBlock.Routine = clone;
                }

                // Rebuild connections
                foreach (var kvp in _exploredBlocks)
                {
                    var originalBlock = kvp.Value;
                    var clonedBlock = clone._exploredBlocks[kvp.Key];

                    foreach (var successor in originalBlock.Successors)
                    {
                        if (clone._exploredBlocks.TryGetValue(successor.Vip, out var clonedSuccessor))
                        {
                            clonedBlock.AddSuccessor(clonedSuccessor);
                        }
                    }
                }

                // Set entry point
                if (EntryPoint != null && clone._exploredBlocks.TryGetValue(EntryPoint.Vip, out var clonedEntry))
                {
                    clone.EntryPoint = clonedEntry;
                }

                // Clone specialized call conventions
                foreach (var kvp in _specializedCallConventions)
                {
                    clone._specializedCallConventions[kvp.Key] = kvp.Value.Clone();
                }

                clone._lastInternalId = _lastInternalId;
                clone._localOptCount = _localOptCount;

                return clone;
            }
        }

        /// <summary>
        /// Clears all blocks from the routine.
        /// </summary>
        public void Clear()
        {
            lock (_mutex)
            {
                foreach (var block in _exploredBlocks.Values)
                {
                    block.ClearConnections();
                }

                _exploredBlocks.Clear();
                _specializedCallConventions.Clear();
                EntryPoint = null;
                _lastInternalId = 0;
                _localOptCount = 0;

                SignalCfgModification();
            }
        }

        /// <summary>
        /// Disposes of the routine and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        public override string ToString()
        {
            return $"Routine({ArchitectureId.GetName()})[{BlockCount} blocks, {InstructionCount} instructions]";
        }
    }
}
