using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Reflection.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Extensions.General.Validation
{
    public static class ArgumentValidationExtensions
    {
        public static void ValidateVariable(this string value, string parameterName)
        {
            if (!value.HasValue())
            {
                throw new ArgumentException($"{parameterName} cannot be null or whitespace");
            }
        }

        #region Numeric Validation
        public static void ValidateVariable(this double value, string parameterName)
        {
            ValidateVariable(value, (x) => x > 0, () => $"{parameterName} must be higher than 0");
        }

        public static void ValidateVariable(this decimal value, string parameterName)
        {
            ValidateVariable(value, (x) => x > 0, () => $"{parameterName} must be higher than 0");
        }
        #endregion

        #region Typed Validation
        public static void ValidateVariable<T>(this T value, string parameterName)
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
        }

        public static void ValidateVariable<T>(this T value, Predicate<T> requiredValueCase, Func<string> failMessageFunc)
        {
            if (!requiredValueCase(value))
            {
                throw new ArgumentException(failMessageFunc());
            }
        }

        public static void ValidateVariable<T>(this T value, Predicate<T> requiredValueCase, Func<T, string> failMessageFunc)
        {
            if (!requiredValueCase(value))
            {
                throw new ArgumentException(failMessageFunc(value));
            }
        }

        public static void ValidateVariable<T>(this T value, Predicate<T> requiredValueCase, Func<T, Exception> exceptionFunc)
        {
            if (!requiredValueCase(value))
            {
                throw exceptionFunc(value);
            }
        }
        #endregion

        #region FileSystem Validation
        public static void ValidateIfExists(this FileSystemInfo value, string parameterName)
        {
            if (!value.HasValue())
            {
                throw new ArgumentException($"{parameterName} cannot be null and needs to exist on the filesystem. Path<{value?.FullName}>");
            }
        }
        #endregion
    }
}
