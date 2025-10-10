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

namespace VTIL.Architecture
{
    /// <summary>
    /// Describes a calling convention for a routine.
    /// </summary>
    public sealed class CallConvention : IEquatable<CallConvention>
    {
        /// <summary>
        /// Whether the callee purges the stack.
        /// </summary>
        public bool PurgeStack { get; set; }

        /// <summary>
        /// The number of bytes to purge from the stack.
        /// </summary>
        public int StackPurgeBytes { get; set; }

        /// <summary>
        /// The registers used for parameter passing.
        /// </summary>
        public IReadOnlyList<RegisterDescriptor> ParameterRegisters { get; set; }

        /// <summary>
        /// The register used for return value.
        /// </summary>
        public RegisterDescriptor? ReturnRegister { get; set; }

        /// <summary>
        /// The registers that are preserved across calls.
        /// </summary>
        public IReadOnlyList<RegisterDescriptor> PreservedRegisters { get; set; }

        /// <summary>
        /// The registers that may be clobbered by calls.
        /// </summary>
        public IReadOnlyList<RegisterDescriptor> VolatileRegisters { get; set; }

        /// <summary>
        /// The stack alignment requirement.
        /// </summary>
        public int StackAlignment { get; set; }

        /// <summary>
        /// The size of the shadow space on the stack.
        /// </summary>
        public int ShadowSpace { get; set; }

        /// <summary>
        /// Whether floating point parameters are passed in registers.
        /// </summary>
        public bool UseFloatingPointRegisters { get; set; }

        /// <summary>
        /// The maximum number of parameters that can be passed in registers.
        /// </summary>
        public int MaxRegisterParameters { get; set; }

        /// <summary>
        /// Creates a new call convention.
        /// </summary>
        public CallConvention()
        {
            ParameterRegisters = Array.Empty<RegisterDescriptor>();
            PreservedRegisters = Array.Empty<RegisterDescriptor>();
            VolatileRegisters = Array.Empty<RegisterDescriptor>();
            StackAlignment = 8; // Default 8-byte alignment
            ShadowSpace = 0;
            MaxRegisterParameters = 4;
        }

        /// <summary>
        /// Creates a call convention with the specified properties.
        /// </summary>
        public CallConvention(
            bool purgeStack = false,
            int stackPurgeBytes = 0,
            RegisterDescriptor[]? parameterRegisters = null,
            RegisterDescriptor? returnRegister = null,
            RegisterDescriptor[]? preservedRegisters = null,
            RegisterDescriptor[]? volatileRegisters = null,
            int stackAlignment = 8,
            int shadowSpace = 0,
            bool useFloatingPointRegisters = false,
            int maxRegisterParameters = 4)
        {
            PurgeStack = purgeStack;
            StackPurgeBytes = stackPurgeBytes;
            ParameterRegisters = parameterRegisters ?? Array.Empty<RegisterDescriptor>();
            ReturnRegister = returnRegister;
            PreservedRegisters = preservedRegisters ?? Array.Empty<RegisterDescriptor>();
            VolatileRegisters = volatileRegisters ?? Array.Empty<RegisterDescriptor>();
            StackAlignment = stackAlignment;
            ShadowSpace = shadowSpace;
            UseFloatingPointRegisters = useFloatingPointRegisters;
            MaxRegisterParameters = maxRegisterParameters;
        }

        /// <summary>
        /// Gets the number of parameter registers.
        /// </summary>
        public int ParameterRegisterCount => ParameterRegisters.Count;

        /// <summary>
        /// Gets the number of preserved registers.
        /// </summary>
        public int PreservedRegisterCount => PreservedRegisters.Count;

        /// <summary>
        /// Gets the number of volatile registers.
        /// </summary>
        public int VolatileRegisterCount => VolatileRegisters.Count;

        /// <summary>
        /// Checks if a register is used for parameter passing.
        /// </summary>
        public bool IsParameterRegister(RegisterDescriptor register)
        {
            return ParameterRegisters.Contains(register);
        }

        /// <summary>
        /// Checks if a register is preserved across calls.
        /// </summary>
        public bool IsPreservedRegister(RegisterDescriptor register)
        {
            return PreservedRegisters.Contains(register);
        }

        /// <summary>
        /// Checks if a register is volatile (may be clobbered).
        /// </summary>
        public bool IsVolatileRegister(RegisterDescriptor register)
        {
            return VolatileRegisters.Contains(register);
        }

        /// <summary>
        /// Gets the parameter register for the specified parameter index.
        /// </summary>
        public RegisterDescriptor? GetParameterRegister(int parameterIndex)
        {
            if (parameterIndex < 0 || parameterIndex >= ParameterRegisters.Count)
                return null;
            return ParameterRegisters[parameterIndex];
        }

        /// <summary>
        /// Gets the stack offset for a parameter that's passed on the stack.
        /// </summary>
        public int GetStackParameterOffset(int parameterIndex)
        {
            // Calculate offset based on calling convention
            // This is a simplified implementation
            return ShadowSpace + (parameterIndex * 8); // Assume 8-byte parameters
        }

        /// <summary>
        /// Calculates the total stack space needed for parameters.
        /// </summary>
        public int CalculateStackSpace(int totalParameters)
        {
            var stackParameters = Math.Max(0, totalParameters - ParameterRegisterCount);
            return ShadowSpace + (stackParameters * 8); // Assume 8-byte parameters
        }

        /// <summary>
        /// Creates a copy of this call convention.
        /// </summary>
        public CallConvention Clone()
        {
            return new CallConvention(
                PurgeStack,
                StackPurgeBytes,
                ParameterRegisters.ToArray(),
                ReturnRegister,
                PreservedRegisters.ToArray(),
                VolatileRegisters.ToArray(),
                StackAlignment,
                ShadowSpace,
                UseFloatingPointRegisters,
                MaxRegisterParameters);
        }

        public bool Equals(CallConvention? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return PurgeStack == other.PurgeStack &&
                   StackPurgeBytes == other.StackPurgeBytes &&
                   ParameterRegisters.SequenceEqual(other.ParameterRegisters) &&
                   Equals(ReturnRegister, other.ReturnRegister) &&
                   PreservedRegisters.SequenceEqual(other.PreservedRegisters) &&
                   VolatileRegisters.SequenceEqual(other.VolatileRegisters) &&
                   StackAlignment == other.StackAlignment &&
                   ShadowSpace == other.ShadowSpace &&
                   UseFloatingPointRegisters == other.UseFloatingPointRegisters &&
                   MaxRegisterParameters == other.MaxRegisterParameters;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CallConvention);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(PurgeStack);
            hashCode.Add(StackPurgeBytes);
            hashCode.Add(ParameterRegisters);
            hashCode.Add(ReturnRegister);
            hashCode.Add(PreservedRegisters);
            hashCode.Add(VolatileRegisters);
            hashCode.Add(StackAlignment);
            hashCode.Add(ShadowSpace);
            hashCode.Add(UseFloatingPointRegisters);
            hashCode.Add(MaxRegisterParameters);
            return hashCode.ToHashCode();
        }

        public static bool operator ==(CallConvention? left, CallConvention? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CallConvention? left, CallConvention? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            var parts = new List<string>();

            if (PurgeStack)
                parts.Add($"purge_stack({StackPurgeBytes})");

            if (ParameterRegisterCount > 0)
                parts.Add($"params({string.Join(",", ParameterRegisters.Select(r => r.GetName()))})");

            if (ReturnRegister.HasValue)
                parts.Add($"return({ReturnRegister.Value.GetName()})");

            if (ShadowSpace > 0)
                parts.Add($"shadow({ShadowSpace})");

            if (StackAlignment != 8)
                parts.Add($"align({StackAlignment})");

            return parts.Count > 0 ? string.Join(", ", parts) : "default";
        }
    }
}
