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
            return (T)delegateFunction.DynamicInvoke(parameters);
        }

        public static void Invoke(this Delegate delegateFunction, params object[] parameters)
        {
            delegateFunction.DynamicInvoke(parameters);
        }
        #endregion

        #region Properties
        public static PropertyInfo[] GetProperties(this object value)
        {
            return value.GetType().GetProperties();
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
        #endregion
    }
}
