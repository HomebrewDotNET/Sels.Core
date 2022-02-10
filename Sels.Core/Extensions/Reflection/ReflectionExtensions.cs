using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Sels.Core.Extensions.Reflection
{
    /// <summary>
    /// Contains extra extension methods for helping with reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Returns the full type name for <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Object to get type name from</param>
        /// <returns>The full type name for <paramref name="value"/></returns>
        public static string GetTypeName(this object value)
        {
            value.ValidateArgument(nameof(value));
            return value.GetType().FullName;
        }

        /// <summary>
        /// If <paramref name="value"/> has the default value for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to check</typeparam>
        /// <param name="value">Object to check</param>
        /// <returns>If <paramref name="value"/> equals the default value for <typeparamref name="T"/></returns>
        public static bool IsDefault<T>(this T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default);
        }

        /// <summary>
        /// If <paramref name="value"/> has the default value for it's type.
        /// </summary>
        /// <param name="value">Object to check</param>
        /// <returns>If <paramref name="value"/> equals the default value for it's type</returns>
        public static bool IsDefault(this object value)
        {
            if (value == null) {
                return true;
            }

            return value.GetType().Construct() == value;
        }

        #region Delegates
        public static Delegate CreateDelegateForMethod(this object value, string methodName)
        {
            methodName.ValidateArgument(nameof(methodName));
            value.ValidateArgument(nameof(value));

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
            methodName.ValidateArgument(nameof(methodName));
            value.ValidateArgument(nameof(value));

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
            delegateFunction.ValidateArgument(nameof(delegateFunction));

            return (T)delegateFunction.DynamicInvoke(parameters);
        }

        public static void Invoke(this Delegate delegateFunction, params object[] parameters)
        {
            delegateFunction.ValidateArgument(nameof(delegateFunction));

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
    }
}
