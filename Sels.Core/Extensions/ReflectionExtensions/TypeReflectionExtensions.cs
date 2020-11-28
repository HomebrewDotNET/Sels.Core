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
        public static bool IsItemContainer(this Type type)
        {
            return type.IsArray || type.IsEnumerable() || type.IsTypedEnumerable();
        }

        public static bool IsEnumerable(this Type type)
        {
            return type.IsArray || typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsTypedEnumerable(this Type type)
        {
            return type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
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
    }
}
