using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Extensions
{
    public static class ArgumentValidationExtensions
    {
        public static string ValidateVariable(this string value, string parameterName)
        {
            return value.ValidateVariable(x => x.HasValue(), () => $"{parameterName} cannot be null, empty or whitespace");
        }

        #region Numeric Validation
        public static double ValidateVariable(this double value, string parameterName)
        {
            return value.ValidateVariable((x) => x > 0, () => $"{parameterName} must be higher than 0");
        }

        public static decimal ValidateVariable(this decimal value, string parameterName)
        {
            return value.ValidateVariable((x) => x > 0, () => $"{parameterName} must be higher than 0");
        }
        #endregion

        #region Typed Validation
        public static T ValidateVariable<T>(this T value, string parameterName)
        {
            if (!value.HasValue())
            {
                var type = typeof(T);

                if (type.IsNullable())
                {
                    throw new ArgumentNullException(parameterName);
                }
                else
                {
                    throw new ArgumentException($"{parameterName} cannot be default value");
                }           
            }

            return value;
        }

        public static T ValidateVariable<T>(this T value, Predicate<T> requiredValueCase, Func<string> failMessageFunc)
        {
            if (!requiredValueCase(value))
            {
                throw new ArgumentException(failMessageFunc());
            }

            return value;
        }

        public static T ValidateVariable<T>(this T value, Predicate<T> requiredValueCase, Func<T, string> failMessageFunc)
        {
            if (!requiredValueCase(value))
            {
                throw new ArgumentException(failMessageFunc(value));
            }

            return value;
        }

        public static T ValidateVariable<T>(this T value, Predicate<T> requiredValueCase, Func<Exception> exceptionFunc)
        {
            if (!requiredValueCase(value))
            {
                throw exceptionFunc();
            }

            return value;
        }

        public static T ValidateVariable<T>(this T value, Predicate<T> requiredValueCase, Func<T, Exception> exceptionFunc)
        {
            if (!requiredValueCase(value))
            {
                throw exceptionFunc(value);
            }

            return value;
        }
        #endregion

        #region FileSystem Validation
        public static FileSystemInfo ValidateIfExists(this FileSystemInfo value, string parameterName)
        {
            if (!value.HasValue())
            {
                throw new ArgumentException($"{parameterName} cannot be null and needs to exist on the filesystem. Path<{value?.FullName}>");
            }

            return value;
        }
        #endregion

        #region Reflection Validation
        public static object ValidateIfType<TType>(this object value, string parameterName)
        {
            return value.ValidateVariable(x => x != null && value.GetType() == typeof(TType), () => $"{parameterName} must be of type {typeof(TType)}");
        }
        #endregion
    }
}
