﻿using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sels.Core.Extensions.Reflection
{
    /// <summary>
    /// Contains extension methods for working with <see cref="MethodInfo"/>.
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Checks if <paramref name="method"/> is the same method as <paramref name="methodToCompare"/>.
        /// </summary>
        /// <param name="method">The first method to compare</param>
        /// <param name="methodToCompare">The second method to compare</param>
        /// <returns>Whether or not <paramref name="method"/> is equal to <paramref name="methodToCompare"/></returns>
        public static bool AreEqual(this MethodInfo method, MethodInfo methodToCompare)
        {
            method.ValidateArgument(nameof(method));
            methodToCompare.ValidateArgument(nameof(methodToCompare));

            return method.DeclaringType.Equals(methodToCompare.DeclaringType) && method.AreSameDefinition(methodToCompare);
        }
        /// <summary>
        /// Checks if 2 methods have the same definition (Name, returns value, parameters, ...).
        /// </summary>
        /// <param name="method">The first method to compare</param>
        /// <param name="methodToCompare">The second method to compare</param>
        /// <returns>Whether or not <paramref name="method"/> has the same definition as <paramref name="methodToCompare"/></returns>
        public static bool AreSameDefinition(this MethodInfo method, MethodInfo methodToCompare)
        {
            method.ValidateArgument(nameof(method));
            methodToCompare.ValidateArgument(nameof(methodToCompare));

            // Check name
            if (!method.Name.Equals(methodToCompare.Name)) return false;

            // Check return type
            if (!method.ReturnType.Equals(methodToCompare.ReturnType)) return false;

            // Check parameters
            var methodParameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
            var methodToCompareParameterTypes = methodToCompare.GetParameters().Select(x => x.ParameterType).ToArray();

            if (methodParameterTypes.Length != methodToCompareParameterTypes.Length) return false;

            for (int i = 0; i < methodParameterTypes.Length; i++)
            {
                if (!methodParameterTypes[i].Equals(methodToCompareParameterTypes[i])) return false;
            }

            // Check if both are generic
            if (!method.IsGenericMethod.Equals(methodToCompare.IsGenericMethod)) return false;

            var genericMethodParameters = method.IsGenericMethod ? method.GetGenericArguments() : Array.Empty<Type>();
            var genericMethodToCompareParameters = method.IsGenericMethod ? method.GetGenericArguments() : Array.Empty<Type>();

            if (genericMethodParameters.Length != genericMethodToCompareParameters.Length) return false;

            for (int i = 0; i < genericMethodParameters.Length; i++)
            {
                if (!genericMethodParameters[i].Equals(genericMethodToCompareParameters[i])) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if <paramref name="method"/> is an override of a method on a base class.
        /// </summary>
        /// <param name="method">The method to check</param>
        /// <returns>True if method is overriden in a derived class, otherwise false</returns>
        public static bool IsOverriden(this MethodInfo method)
        {
            method.ValidateArgument(nameof(method));

            return !method.GetBaseDefinition().DeclaringType.Equals(method.DeclaringType);
        }

        /// <summary>
        /// Returns a display name where in case of a generic method the types are fully filled out.
        /// </summary>
        /// <param name="method">Method to get the display name for</param>
        /// <param name="includeNamespace">If the namespace needs to be included</param>
        /// <param name="arguments">Optional list of argument for the method</param>
        /// <returns>The display name for <paramref name="method"/></returns>
        public static string GetDisplayName(this MethodInfo method, bool includeNamespace = true, params string[] arguments)
        {
            method.ValidateArgument(nameof(method));

            StringBuilder builder = new StringBuilder();

            // Add namespace + method name
            builder.GetDisplayName(method.ReflectedType, includeNamespace).Append('.').Append(method.Name);

            // Add generic type arguments if present
            if (method.IsGenericMethod)
            {
                builder.Append('<');
                var genericArguments = method.GetGenericArguments();
                genericArguments.Execute((i, x) =>
                {
                    builder.GetDisplayName(x, includeNamespace);
                    if (i < genericArguments.Length - 1) builder.Append(',');
                });
                builder.Append('>');
            }

            // Add method parameters
            builder.Append('(');

            if (arguments.HasValue())
            {
                arguments.Execute((i, x) =>
                {
                    builder.Append(x);
                    if (i < arguments.Length - 1) builder.Append(',');
                });
            }

            builder.Append(')');

            return builder.ToString();
        }
    }
}
