using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using Sels.Core.Extensions.Reflection.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Sels.Core.Extensions.Reflection.Object
{
    public static class ObjectReflectionExtensions
    {
        #region Attributes
        public static T GetAttribute<T>(this object source) where T : Attribute
        {
            source.ValidateVariable(nameof(source));

            var attribute = source.GetAttributeOrDefault<T>();

            if (!attribute.HasValue())
            {
                throw new InvalidOperationException($"Attribute {typeof(T)} was not present on object {source.GetType()}");
            }

            return attribute;
        }

        public static T GetAttributeOrDefault<T>(this object source) where T : Attribute
        {
            source.ValidateVariable(nameof(source));

            var sourceType = source.GetType();

            return sourceType.GetCustomAttribute<T>();
        }

        #endregion

        #region Delegates
        public static Delegate CreateDelegateForMethod(this object value, string methodName)
        {
            methodName.ValidateVariable(nameof(methodName));
            value.ValidateVariable(nameof(value));

            var objectType = value.GetType();

            var method = objectType.GetTypeInfo().GetDeclaredMethod(methodName);

            if (method.HasValue())
            {
                return method.CreateDelegate(method.GetAsDelegateType(), value);
            }

            throw new ArgumentException($"No method with name {methodName} found on type {objectType}");
        }

        public static T CreateDelegateForMethod<T>(this object value, string methodName) where T : Delegate
        {
            methodName.ValidateVariable(nameof(methodName));
            value.ValidateVariable(nameof(value));

            var objectType = value.GetType();

            var method = objectType.GetTypeInfo().GetDeclaredMethod(methodName);

            if (method.HasValue())
            {
                return (T)method.CreateDelegate(typeof(T), value);
            }

            throw new ArgumentException($"No method with name {methodName} found on type {objectType}");
        }

        public static T Invoke<T>(this Delegate delegateFunction, params object[] parameters)
        {
            delegateFunction.ValidateVariable(nameof(delegateFunction));

            return (T)delegateFunction.DynamicInvoke(parameters);
        }

        public static void Invoke(this Delegate delegateFunction, params object[] parameters)
        {
            delegateFunction.ValidateVariable(nameof(delegateFunction));

            delegateFunction.DynamicInvoke(parameters);
        }

        public static T InvokeOrDefault<T>(this Delegate delegateFunction, params object[] parameters)
        {
            if(delegateFunction != null)
            {
                return delegateFunction.Invoke<T>(parameters);
            }
            else
            {
                return default;
            }
        }

        public static void InvokeOrDefault(this Delegate delegateFunction, params object[] parameters)
        {
            if (delegateFunction != null)
            {
                delegateFunction.Invoke(parameters);
            }
        }

        public static IEnumerable<T> InvokeOrDefault<T>(this IEnumerable<Delegate> delegates, params object[] parameters)
        {
            if (delegates.HasValue())
            {
                foreach(var delegateFunc in delegates)
                {
                    yield return delegateFunc.InvokeOrDefault<T>(parameters);
                }
            }
        }

        public static void InvokeOrDefault(this IEnumerable<Delegate> delegates, params object[] parameters)
        {
            if (delegates.HasValue())
            {
                foreach (var delegateFunc in delegates)
                {
                    delegateFunc.InvokeOrDefault(parameters);
                }
            }
        }
        #endregion

        #region Properties
        public static PropertyInfo[] GetProperties(this object value)
        {
            return value.GetType().GetProperties();
        }

        public static PropertyInfo[] GetProperties(this object value, BindingFlags bindingFlags)
        {
            return value.GetType().GetProperties(bindingFlags);
        }

        public static PropertyInfo[] GetPublicProperties(this object value)
        {
            return value.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
        }

        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
        }

        public static bool AreEqual(this PropertyInfo property, PropertyInfo propertyToCompare)
        {
            property.ValidateVariable(nameof(property));
            propertyToCompare.ValidateVariable(nameof(propertyToCompare));

            if(property.Equals(propertyToCompare) || property.Name.Equals(propertyToCompare.Name))
            {
                return true;
            }

            return false;
        }

        public static T GetValue<T>(this PropertyInfo property, object sourceObject)
        {
            property.ValidateVariable(nameof(property));
            sourceObject.ValidateVariable(nameof(sourceObject));

            return (T)property.GetValue(sourceObject);
        }

        public static bool CanAssign<T>(this PropertyInfo property)
        {
            property.ValidateVariable(nameof(property));
            var typeToCheck = typeof(T);
            var propertyType = property.PropertyType;

            return propertyType.IsAssignableFrom(typeToCheck);
        }

        public static bool TryGetPropertyInfo(this object sourceObject, string propertyName, out PropertyInfo property)
        {
            sourceObject.ValidateVariable(nameof(sourceObject));
            propertyName.ValidateVariable(nameof(propertyName));

            property = sourceObject.GetType().GetProperty(propertyName);

            return property != null;
        }

        public static PropertyInfo GetPropertyInfo(this object sourceObject, string propertyName)
        {
            sourceObject.ValidateVariable(nameof(sourceObject));
            propertyName.ValidateVariable(nameof(propertyName));

            if(sourceObject.TryGetPropertyInfo(propertyName, out var property))
            {
                return property;
            }
            else
            {
                throw new InvalidOperationException($"Could not find property <{propertyName}> on object <{sourceObject.GetType()}>");
            }
        }

        public static void SetDefault(this PropertyInfo property, object sourceObject)
        {
            property.ValidateVariable(nameof(property));
            sourceObject.ValidateVariable(nameof(sourceObject));

            var propertyType = property.PropertyType;

            var defaultValue = propertyType.GetDefaultValue();

            property.SetValue(sourceObject, defaultValue);
        }
        #endregion
    }
}
