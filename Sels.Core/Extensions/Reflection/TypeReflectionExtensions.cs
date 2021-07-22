using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
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
        /// Returns source type of it's not null, otherwise return null
        /// </summary>
        public static Type GetTypeOrDefault(this object source)
        {
            return source != null ? source.GetType() : null;
        }

        /// <summary>
        /// Checks if type is a collection of some sorts.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean indicating if type is an item container</returns>
        public static bool IsItemContainer(this Type type)
        {
            return !type.IsValueType && type.IsArray || type.IsEnumerable() || type.IsTypedEnumerable();
        }

        public static bool IsEnumerable(this Type type)
        {
            return !type.IsValueType && type.IsArray || typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsTypedEnumerable(this Type type)
        {
            return !type.IsValueType && type.IsArray || type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)); ;
        }

        public static bool IsDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Dictionary<,>));
        }

        /// <summary>
        /// Check if we can assign null to the type
        /// </summary>
        public static bool IsNullable(this Type type)
        {
            return !type.IsValueType;
        }

        public static bool IsString(this Type type)
        {
            return type.Equals(typeof(string));
        }

        public static bool IsNumeric(this Type type)
        {
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
            return source != null && type.IsAssignableFrom(source.GetType());
        }

        /// <summary>
        /// Checks if an instance of <paramref name="sourceType"/> can be assigned to a variable of type <paramref name="type"/>.
        /// </summary>
        /// <param name="sourceType">Source type to check</param>
        /// <param name="type">Type to check against</param>
        /// <returns>Boolean indicating if an instance of <paramref name="sourceType"/> can be assigned to <paramref name="type"/></returns>
        public static bool IsAssignableTo(this Type sourceType, Type type)
        {
            return sourceType != null && type.IsAssignableFrom(sourceType);
        }

        public static Type GetItemTypeFromContainer(this Type containerType)
        {
            containerType.ValidateVariable(x => x.IsItemContainer(), () => $"{nameof(containerType)} must be an item container like IEnumerable, Array, Dictionary");

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

            throw new NotSupportedException($"Could not extract underlying item type from type {containerType}");
        }

        /// <summary>
        /// Returns the default value for <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to return the default value from</param>
        /// <returns>Default value for <paramref name="type"/></returns>
        public static object GetDefaultValue(this Type type)
        {
            type.ValidateVariable(nameof(type));

            if (type.IsValueType)
            {
                return type.Construct();
            }

            return null;
        }

        #region Property Finder
        public static PropertyInfo FindProperty(this Type type, string propertyName)
        {
            return type.GetProperties().Where(x => x.Name.Equals(propertyName)).FirstOrDefault();
        }

        public static bool TryFindProperty(this Type type, string propertyName, out PropertyInfo property)
        {
            property = type.GetProperties().Where(x => x.Name.Equals(propertyName)).FirstOrDefault();

            return property != null;
        }

        public static PropertyInfo FindProperty(this object source, string propertyName)
        {
            if (source.HasValue())
            {
                return source.GetType().FindProperty(propertyName);
            }

            return null;
        }

        public static bool TryFindProperty(this object source, string propertyName, out PropertyInfo property)
        {
            property = null;
            if (source.HasValue())
            {
                return source.GetType().TryFindProperty(propertyName, out property);
            }

            return false;
        }

        public static Type[] GetDelegateTypes(this MethodInfo method)
        {
            return (from parameter in method.GetParameters() select parameter.ParameterType)
            .Concat(new[] { method.ReturnType })
            .ToArray();
        }

        public static Type GetAsDelegateType(this MethodInfo method)
        {
            return Expression.GetDelegateType(method.GetDelegateTypes());
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

            return Activator.CreateInstance(type).As<T>();
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

            return Activator.CreateInstance(type, parameters).As<T>();
        }

        /// <summary>
        /// Creates an instance of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to create object from</param>
        /// <returns>Constructed object</returns>
        public static object Construct(this Type type)
        {
            type.ValidateArgument(nameof(type));

            var instance = Activator.CreateInstance(type);

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
    }
}
