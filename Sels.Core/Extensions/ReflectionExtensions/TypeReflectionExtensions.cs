using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sels.Core.Extensions.Reflection.Types
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
            return !type.IsValueType && type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) || type.GetInterfaces().Any(x => x.IsTypedEnumerable());
        }

        public static bool IsDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Dictionary<,>));
        }

        public static bool IsNullable(this Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsString(this Type type)
        {
            return type.Equals(typeof(string));
        }

        public static bool IsNumeric(this Type type)
        {
            return _numericTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);
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
        public static T Construct<T>(this Type type)
        {
            type.ValidateVariable(nameof(type));

            var instance = Activator.CreateInstance(type);

            return (T)instance;
        }

        public static T Construct<T>(this Type type, params object[] parameters)
        {
            type.ValidateVariable(nameof(type));

            var instance = Activator.CreateInstance(type, parameters);

            return (T)instance;
        }

        public static object Construct(this Type type)
        {
            type.ValidateVariable(nameof(type));

            var instance = Activator.CreateInstance(type);

            return instance;
        }

        public static object Construct(this Type type, params object[] parameters)
        {
            type.ValidateVariable(nameof(type));

            var instance = Activator.CreateInstance(type, parameters);

            return instance;
        }
        #endregion
    }
}
