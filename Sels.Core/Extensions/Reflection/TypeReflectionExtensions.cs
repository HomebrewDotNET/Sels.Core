using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sels.Core.Extensions.Reflection
{
    /// <summary>
    /// Contains extension methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeReflectionExtensions
    {
        private static readonly HashSet<Type> _numericTypes = new HashSet<Type>
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float)
        };

        /// <summary>
        ///  Returns the type of <paramref name="source"/> if it's not null, otherwise return null.
        /// </summary>
        /// <param name="source">The object to get the type from</param>
        /// <returns>The type of <paramref name="source"/> if it is not null, otherwise false</returns>
        public static Type GetTypeOrDefault(this object source)
        {
            return source != null ? source.GetType() : null;
        }

        /// <summary>
        /// Checks if type is a collection of some sorts.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean indicating if type is an item container</returns>
        public static bool IsContainer(this Type type)
        {
            type.ValidateArgument(nameof(type));

            return !type.IsValueType && type.IsArray || type.IsEnumerable() || type.IsTypedEnumerable();
        }

        /// <summary>
        /// Checks if <paramref name="type"/> is assignable to <see cref="IEnumerable"/>.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>If <paramref name="type"/> is assignable to <see cref="IEnumerable"/></returns>
        public static bool IsEnumerable(this Type type)
        {
            type.ValidateArgument(nameof(type));

            return !type.IsValueType && type.IsArray || typeof(IEnumerable).IsAssignableFrom(type);
        }
        /// <summary>
        /// Checks if <paramref name="type"/> is assignable to <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>If <paramref name="type"/> is assignable to <see cref="IEnumerable{T}"/></returns>
        public static bool IsTypedEnumerable(this Type type)
        {
            type.ValidateArgument(nameof(type));

            return !type.IsValueType && type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        /// <summary>
        /// Check if we can assign null to the type
        /// </summary>
        public static bool IsNullable(this Type type)
        {
            return !type.IsValueType;
        }
        /// <summary>
        /// Checks if <paramref name="type"/> is <see cref="string"/>.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>If <paramref name="type"/> is <see cref="string"/></returns>
        public static bool IsString(this Type type)
        {
            type.ValidateArgument(nameof(type));

            return type.Is<string>();
        }
        /// <summary>
        /// Checks if <paramref name="type"/> is any of the numeric types.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>If <paramref name="type"/> is any of the numeric types</returns>
        public static bool IsNumeric(this Type type)
        {
            type.ValidateArgument(nameof(type));

            return _numericTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);
        }

        /// <summary>
        /// Checks if an instance of <typeparamref name="TType"/> can be assigned to a variable with the same type as <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TType">Type to check</typeparam>
        /// <param name="source">Object to check against</param>
        /// <returns>Boolean indicating if an instance of <typeparamref name="TType"/> can be assigned to a variable with the same type as <paramref name="source"/></returns>
        public static bool IsAssignableFrom<TType>(this object source)
        {
            return source.IsAssignableFrom(typeof(TType));
        }

        /// <summary>
        /// Checks if an instance of <paramref name="type"/> can be assigned to a variable with the same type as <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Object to check against</param>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean indicating if an instance of <paramref name="type"/> can be assigned to a variable with the same type as <paramref name="source"/></returns>
        public static bool IsAssignableFrom(this object source, Type type)
        {           
            return source != null && source.GetType().IsAssignableFrom(type);
        }

        /// <summary>
        /// Checks if <paramref name="source"/> can be assigned to a variable of type <typeparamref name="TType"/>.
        /// </summary>
        /// <typeparam name="TType">Type to check against</typeparam>
        /// <param name="source">Source object to check</param>
        /// <returns>Boolean indicating if <paramref name="source"/> can be assigned to <typeparamref name="TType"/></returns>
        public static bool IsAssignableTo<TType>(this object source)
        {
            return source.IsAssignableTo(typeof(TType));
        }

        /// <summary>
        /// Checks if an instance of <paramref name="source"/> can be assigned to a variable of type <typeparamref name="TType"/>.
        /// </summary>
        /// <typeparam name="TType">Type to check against</typeparam>
        /// <param name="source">Source type to check</param>
        /// <returns>Boolean indicating if an instance of <paramref name="source"/> can be assigned to <typeparamref name="TType"/></returns>
        public static bool IsAssignableTo<TType>(this Type source)
        {
            return source.IsAssignableTo(typeof(TType));
        }

        /// <summary>
        /// Checks if <paramref name="source"/> can be assigned to a variable of type <paramref name="type"/>.
        /// </summary>
        /// <param name="source">Source object to check</param>
        /// <param name="type">Type to check against</param>
        /// <returns>Boolean indicating if <paramref name="source"/> can be assigned to <paramref name="type"/></returns>
        public static bool IsAssignableTo(this object source, Type type)
        {
            return source != null && type != null && type.IsAssignableFrom(source.GetType());
        }

        /// <summary>
        /// Checks if an instance of <paramref name="sourceType"/> can be assigned to a variable of type <paramref name="type"/>.
        /// </summary>
        /// <param name="sourceType">Source type to check</param>
        /// <param name="type">Type to check against</param>
        /// <returns>Boolean indicating if an instance of <paramref name="sourceType"/> can be assigned to <paramref name="type"/></returns>
        public static bool IsAssignableTo(this Type sourceType, Type type)
        {
            return sourceType != null && type != null && type.IsAssignableFrom(sourceType);
        }

        /// <summary>
        /// Tries to get the element type from <paramref name="containerType"/>.
        /// </summary>
        /// <param name="containerType">Type to get the element type from</param>
        /// <returns>The element type for <paramref name="containerType"/></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static Type GetElementTypeFromCollection(this Type containerType)
        {
            containerType.ValidateArgument(x => x.IsContainer(), $"{nameof(containerType)} must be an item container like IEnumerable, Array, Dictionary, ...");

            if (containerType.IsArray)
            {
                return containerType.GetElementType();
            }
            else if (containerType.IsTypedEnumerable())
            {
                return containerType.GetGenericArguments()[0];
            }
            else
            {
                var elementType = containerType.GetInterfaces().Where(x => x.IsTypedEnumerable()).Select(x => x.GetGenericArguments()[0]).FirstOrDefault();

                if (!elementType.IsDefault()) return elementType;
            }

            throw new NotSupportedException($"Could not extract underlying element type from type {containerType}");
        }

        /// <summary>
        /// Tries to get the element type from <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Object to get the element type from</param>
        /// <returns>The element type for <paramref name="container"/></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static Type GetElementTypeFromCollection(this object container)
        {
            if(container != null)
            {
                return container.GetType().GetElementTypeFromCollection();
            }

            throw new NotSupportedException($"Could not extract underlying item type");
        }

        /// <summary>
        /// Returns the default value for <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to return the default value from</param>
        /// <returns>Default value for <paramref name="type"/></returns>
        public static object GetDefaultValue(this Type type)
        {
            type.ValidateArgument(nameof(type));

            if (type.IsValueType)
            {
                return type.Construct();
            }

            return null;
        }

        /// <summary>
        /// Gets all constant fields on <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to get the constant fields from</param>
        /// <returns>All constant fields on <paramref name="type"/></returns>
        public static IEnumerable<FieldInfo> GetConstants(this Type type)
        {
            type.ValidateArgument(nameof(type));

            return type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.IsLiteral && !x.IsInitOnly);
        }

        #region DisplayName
        /// <summary>
        /// Returns a display name where in case of a generic type the types are fully filled out.
        /// </summary>
        /// <param name="type">Type to get the display name for</param>
        /// <param name="includeNamespace">If the namespace needs to be included</param>
        /// <returns>The display name for <paramref name="type"/></returns>
        public static string GetDisplayName(this Type type, bool includeNamespace = true)
        {
            type.ValidateArgument(nameof(type));

            return new StringBuilder().GetDisplayName(type, includeNamespace).ToString();
        }
        /// <summary>
        /// Appends a display name where in case of a generic type the types are fully filled out to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="type">Type to get the display name for</param>
        /// <param name="includeNamespace">If the namespace needs to be included</param>
        /// <returns><paramref name="builder"/> for method chaining</returns>
        public static StringBuilder GetDisplayName(this StringBuilder builder, Type type, bool includeNamespace = true)
        {
            builder.ValidateArgument(nameof(builder));
            type.ValidateArgument(nameof(type));

            var baseName = type.GetBaseName(includeNamespace);
            if (type.IsArray)
            {
                builder.Append(GetDisplayName(type.GetElementType(), includeNamespace)).Append('[').Append(']');
            }
            else if (type.IsGenericTypeDefinition)
            {
                // Add generic type arguments in name
                builder.Append(baseName).Append('<');

                var argumentsCount = type.GetGenericArguments().Length;

                for (int i = 0; i < argumentsCount - 1; i++)
                {
                    builder.Append(',');
                }
                builder.Append('>');
            }
            else if (type.IsGenericType)
            {
                // Add generic type arguments in name
                builder.Append(baseName).Append('<');

                var arguments = type.GetGenericArguments();
                arguments.Execute((i, t) =>
                {
                    builder.GetDisplayName(t, includeNamespace);
                    if (i < arguments.Length - 1) builder.Append(',');
                });
                builder.Append('>');
            }
            else
            {
                builder.Append(baseName);
            }

            return builder;
        }
        /// <summary>
        /// Returns the type name of <paramref name="type"/> without any generic information.
        /// </summary>
        /// <param name="type">Type to get the name from</param>
        /// <param name="includeNamespace">If the namespace needs to be included</param>
        /// <returns>The type name for <paramref name="type"/></returns>
        public static string GetBaseName(this Type type, bool includeNamespace = true)
        {
            type.ValidateArgument(nameof(type));

            string name = includeNamespace ? type.FullName != null ? type.FullName : type.Name : type.Name;
            int index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }
        #endregion

        #region Delegate Types 
        /// <summary>
        /// Returns the return type and the parameter types in the right order so they can be used to create a delegate.
        /// </summary>
        /// <param name="method">The method to get the types from</param>
        /// <returns>The return type and the parameter types in the right order so they can be used to create a delegate</returns>
        public static Type[] GetDelegateTypes(this MethodInfo method)
        {
            method.ValidateArgument(nameof(method));

            return (from parameter in method.GetParameters() select parameter.ParameterType)
            .Concat(new[] { method.ReturnType })
            .ToArray();
        }
        /// <summary>
        /// Gets the generic <see cref="Action"/> or <see cref="Func{T, TResult}"/> type that is equal to the definition of <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The method to get the type for</param>
        /// <returns>The generic <see cref="Action"/> or <see cref="Func{T, TResult}"/> type that is equal to the definition of <paramref name="method"/></returns>
        public static Type GetAsDelegateType(this MethodInfo method)
        {
            method.ValidateArgument(nameof(method));

            return System.Linq.Expressions.Expression.GetDelegateType(method.GetDelegateTypes());
        }
        #endregion

        #region Construction
        #region Construct
        /// <summary>
        /// Creates an instance of <paramref name="type"/>.
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="type">Type to create object from</param>
        /// <returns>Constructed object</returns>
        public static T Construct<T>(this Type type)
        {
            type.ValidateArgument(nameof(type));

            return Activator.CreateInstance(type).Cast<T>();
        }

        /// <summary>
        /// Creates an instance of <paramref name="type"/> with <paramref name="parameters"/> as constructor arguments.
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="type">Type to create object from</param>
        /// <param name="parameters">Constructor arguments</param>
        /// <returns>Constructed object</returns>
        public static T Construct<T>(this Type type, params object[] parameters)
        {
            type.ValidateArgument(nameof(type));

            return Activator.CreateInstance(type, parameters).Cast<T>();
        }

        /// <summary>
        /// Creates an instance of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to create object from</param>
        /// <returns>Constructed object</returns>
        public static object Construct(this Type type)
        {
            type.ValidateArgument(nameof(type));

            object instance;
            // No arg constructor
            if (type.GetConstructors().Any(x => x.GetParameters().Length == 0))
            {
                instance = Activator.CreateInstance(type);
            }
            else
            {
                instance = Activator.CreateInstance(type, null);
            }

            return instance;
        }

        /// <summary>
        /// Creates an instance of <paramref name="type"/> with <paramref name="parameters"/> as constructor arguments.
        /// </summary>
        /// <param name="type">Type to create object from</param>
        /// <param name="parameters">Constructor arguments</param>
        /// <returns>Constructed object</returns>
        public static object Construct(this Type type, params object[] parameters)
        {
            type.ValidateArgument(nameof(type));

            var instance = Activator.CreateInstance(type, parameters);

            return instance;
        }
        #endregion

        #region CanConstructWith
        /// <summary>
        /// Checks if we can construct the type using the supplied argumentTypes
        /// </summary>
        public static bool CanConstructWith(this Type type, params Type[] argumentTypes)
        {
            type.ValidateArgument(nameof(type));

            foreach (var constructor in type.GetConstructors())
            {                
                var parameters = constructor.GetParameters();

                // Count how many parameters don't have a default value
                var nonDefaultValueParameterCount = parameters.Count(x => !x.HasDefaultValue);
                
                if(!argumentTypes.HasValue() && !parameters.HasValue())
                {
                    // No arg constructor
                    return true;
                }
                else if(parameters.Length > 0 && argumentTypes?.Length >= nonDefaultValueParameterCount)
                {
                    var canAssignAllTypes = true;

                    // Loop over all properties
                    for(int i = 0; i < argumentTypes.Length; i++)
                    {
                        var constructorArgumentType = parameters[i].ParameterType;
                        var argumentType = argumentTypes[i];

                        if(!(argumentType == null && constructorArgumentType.IsNullable()) && !constructorArgumentType.IsAssignableFrom(argumentType))
                        {
                            // A null type was supplied but the constructor type is not nullable OR we coudn't assign type to the constructor type
                            canAssignAllTypes = false;
                            break;
                        }
                    }

                    // We found a valid constructor
                    if (canAssignAllTypes) return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks if we can construct the type using the supplied arguments
        /// </summary>
        public static bool CanConstructWithArguments(this Type type, params object[] arguments)
        {
            type.ValidateArgument(nameof(type));

            return type.CanConstructWith(arguments.Select(x => x.GetTypeOrDefault()).ToArray());
        }

        /// <summary>
        /// Checks if type has a constructor that takes T.
        /// </summary>
        public static bool CanConstructWith<T>(this Type type)
        {
            return type.CanConstructWith(typeof(T));
        }
        #endregion
        #endregion

        #region Is
        /// <summary>
        /// Checks if <paramref name="type"/> is the same as <paramref name="typeToCheck"/>.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="typeToCheck">Type to compare to</param>
        /// <returns>Boolean indicating if <paramref name="type"/> and <paramref name="typeToCheck"/> are the same type</returns>
        public static bool Is(this Type type, Type typeToCheck)
        {
            type.ValidateArgument(nameof(type));
            typeToCheck.ValidateArgument(nameof(typeToCheck));

            return type.Equals(typeToCheck);
        }

        /// <summary>
        /// Checks if <paramref name="type"/> is the same as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to compare to</typeparam>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean indicating if <paramref name="type"/> and <typeparamref name="T"/> are the same type</returns>
        public static bool Is<T>(this Type type)
        {
            type.ValidateArgument(nameof(type));

            return type.Is(typeof(T));
        }
        #endregion
    }
}
