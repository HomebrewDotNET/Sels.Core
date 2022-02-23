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
        /// Checks if <paramref name="value"/> has the default value for type <typeparamref name="T"/>.
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
        /// <summary>
        /// Creates a delegate for the method with name <paramref name="methodName"/> on the type of <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The object to get the type from</param>
        /// <param name="methodName">The name of the method to create the delegate for</param>
        /// <returns>A delegate pointing to method <paramref name="methodName"/> on <paramref name="source"/></returns>
        /// <exception cref="ArgumentException">Thrown when no method with name <paramref name="methodName"/> could be found on the type of <paramref name="source"/></exception>
        public static Delegate CreateDelegateForMethod(this object source, string methodName)
        {
            methodName.ValidateArgumentNotNullOrWhitespace(nameof(methodName));
            source.ValidateArgument(nameof(source));

            var objectType = source.GetType();

            var method = objectType.GetTypeInfo().GetDeclaredMethod(methodName);

            if (method.HasValue())
            {
                return method.CreateDelegate(method.GetAsDelegateType(), source);
            }

            throw new ArgumentException($"No method with name {methodName} found on type {objectType}");
        }
        /// <summary>
        /// Creates a delegate for the method with name <paramref name="methodName"/> on the type of <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The type of the delegate to return</typeparam>
        /// <param name="source">The object to get the type from</param>
        /// <param name="methodName">The name of the method to create the delegate for</param>
        /// <returns>A delegate pointing to method <paramref name="methodName"/> on <paramref name="source"/></returns>
        /// <exception cref="ArgumentException">Thrown when no method with name <paramref name="methodName"/> could be found on the type of <paramref name="source"/></exception>
        public static T CreateDelegateForMethod<T>(this object source, string methodName) where T : Delegate
        {
            methodName.ValidateArgumentNotNullOrWhitespace(nameof(methodName));
            source.ValidateArgument(nameof(source));

            var objectType = source.GetType();

            var method = objectType.GetTypeInfo().GetDeclaredMethod(methodName);

            if (method.HasValue())
            {
                return (T)method.CreateDelegate(typeof(T), source);
            }

            throw new ArgumentException($"No method with name {methodName} found on type {objectType}");
        }
        /// <summary>
        /// Invokes <paramref name="delegateFunction"/> with optional arguments <paramref name="arguments"/> and casts the return value to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The return type of <paramref name="delegateFunction"/></typeparam>
        /// <param name="delegateFunction">The delegate to invoke</param>
        /// <param name="arguments">Optional arguments to invoke <paramref name="delegateFunction"/> with</param>
        /// <returns>The value returned from invoking <paramref name="delegateFunction"/></returns>
        public static T Invoke<T>(this Delegate delegateFunction, params object[] arguments)
        {
            delegateFunction.ValidateArgument(nameof(delegateFunction));

            return (T)delegateFunction.DynamicInvoke(arguments);
        }
        #endregion
    }
}
