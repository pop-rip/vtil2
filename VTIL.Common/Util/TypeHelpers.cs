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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace VTIL.Common.Util
{
    /// <summary>
    /// Type helper utilities for VTIL.
    /// </summary>
    public static class TypeHelpers
    {
        /// <summary>
        /// Gets the size of a type in bytes.
        /// </summary>
        public static int SizeOf<T>() where T : unmanaged
        {
            return Unsafe.SizeOf<T>();
        }

        /// <summary>
        /// Gets the alignment of a type.
        /// </summary>
        public static int AlignOf<T>() where T : unmanaged
        {
            return Unsafe.SizeOf<T>();
        }

        /// <summary>
        /// Checks if a type is an enum.
        /// </summary>
        public static bool IsEnum<T>()
        {
            return typeof(T).IsEnum;
        }

        /// <summary>
        /// Checks if a type is a primitive type.
        /// </summary>
        public static bool IsPrimitive<T>()
        {
            return typeof(T).IsPrimitive;
        }

        /// <summary>
        /// Checks if a type is a value type.
        /// </summary>
        public static bool IsValueType<T>()
        {
            return typeof(T).IsValueType;
        }

        /// <summary>
        /// Checks if a type is a reference type.
        /// </summary>
        public static bool IsReferenceType<T>()
        {
            return !typeof(T).IsValueType;
        }

        /// <summary>
        /// Gets the underlying type of an enum.
        /// </summary>
        public static Type GetUnderlyingType<T>() where T : Enum
        {
            return Enum.GetUnderlyingType(typeof(T));
        }

        /// <summary>
        /// Gets all enum values of a type.
        /// </summary>
        public static T[] GetEnumValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        /// <summary>
        /// Gets all enum names of a type.
        /// </summary>
        public static string[] GetEnumNames<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T));
        }

        /// <summary>
        /// Parses an enum value from a string.
        /// </summary>
        public static T ParseEnum<T>(string value) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Tries to parse an enum value from a string.
        /// </summary>
        public static bool TryParseEnum<T>(string value, out T result) where T : struct, Enum
        {
            return Enum.TryParse(value, out result);
        }

        /// <summary>
        /// Gets the default value for a type.
        /// </summary>
        public static T GetDefault<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Creates an instance of a type using the default constructor.
        /// </summary>
        public static T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Creates an instance of a type using the default constructor.
        /// </summary>
        public static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Gets all fields of a type.
        /// </summary>
        public static FieldInfo[] GetFields<T>()
        {
            return typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }

        /// <summary>
        /// Gets all properties of a type.
        /// </summary>
        public static PropertyInfo[] GetProperties<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }

        /// <summary>
        /// Gets all methods of a type.
        /// </summary>
        public static MethodInfo[] GetMethods<T>()
        {
            return typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }

        /// <summary>
        /// Checks if a type implements an interface.
        /// </summary>
        public static bool ImplementsInterface<T, TInterface>()
        {
            return typeof(TInterface).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Checks if a type is assignable to another type.
        /// </summary>
        public static bool IsAssignableTo<T, TTarget>()
        {
            return typeof(TTarget).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Gets the type name without generic parameters.
        /// </summary>
        public static string GetTypeName<T>()
        {
            return typeof(T).Name;
        }

        /// <summary>
        /// Gets the full type name with namespace.
        /// </summary>
        public static string GetFullTypeName<T>()
        {
            return typeof(T).FullName ?? typeof(T).Name;
        }

        /// <summary>
        /// Gets the assembly qualified name of a type.
        /// </summary>
        public static string GetAssemblyQualifiedName<T>()
        {
            return typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? typeof(T).Name;
        }

        /// <summary>
        /// Checks if a type has a default constructor.
        /// </summary>
        public static bool HasDefaultConstructor<T>()
        {
            return typeof(T).GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// Gets the hash code of a type.
        /// </summary>
        public static int GetTypeHashCode<T>()
        {
            return typeof(T).GetHashCode();
        }

        /// <summary>
        /// Compares two types for equality.
        /// </summary>
        public static bool TypeEquals<T1, T2>()
        {
            return typeof(T1) == typeof(T2);
        }

        /// <summary>
        /// Gets the type of a generic type parameter.
        /// </summary>
        public static Type GetGenericTypeDefinition<T>()
        {
            return typeof(T).GetGenericTypeDefinition();
        }

        /// <summary>
        /// Gets the generic type arguments of a type.
        /// </summary>
        public static Type[] GetGenericTypeArguments<T>()
        {
            return typeof(T).GetGenericArguments();
        }

        /// <summary>
        /// Checks if a type is generic.
        /// </summary>
        public static bool IsGenericType<T>()
        {
            return typeof(T).IsGenericType;
        }

        /// <summary>
        /// Checks if a type is a generic type definition.
        /// </summary>
        public static bool IsGenericTypeDefinition<T>()
        {
            return typeof(T).IsGenericTypeDefinition;
        }

        /// <summary>
        /// Gets the base type of a type.
        /// </summary>
        public static Type? GetBaseType<T>()
        {
            return typeof(T).BaseType;
        }

        /// <summary>
        /// Gets all interfaces implemented by a type.
        /// </summary>
        public static Type[] GetInterfaces<T>()
        {
            return typeof(T).GetInterfaces();
        }

        /// <summary>
        /// Checks if a type is abstract.
        /// </summary>
        public static bool IsAbstract<T>()
        {
            return typeof(T).IsAbstract;
        }

        /// <summary>
        /// Checks if a type is sealed.
        /// </summary>
        public static bool IsSealed<T>()
        {
            return typeof(T).IsSealed;
        }

        /// <summary>
        /// Checks if a type is a class.
        /// </summary>
        public static bool IsClass<T>()
        {
            return typeof(T).IsClass;
        }

        /// <summary>
        /// Checks if a type is an interface.
        /// </summary>
        public static bool IsInterface<T>()
        {
            return typeof(T).IsInterface;
        }
    }
}
