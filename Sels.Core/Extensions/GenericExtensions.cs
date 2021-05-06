using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CSharp;

namespace Sels.Core.Extensions
{
    public static class GenericExtensions
    {
        #region HasValue
        /// <summary>
        /// Calls HasValue using dynamic. Checks if the object contains information worth processing. Returns false when objects have default types, are empty collections, are empty or whitespace strings, ...
        /// </summary>
        public static bool CheckUnknownHasValue(this object value)
        {
            if(value == null) { return false; }

            return ((dynamic)value).HasValue();
        }

        public static bool HasValue(this object value)
        {
            return value != null;
        }

        public static bool HasValue(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        #region Collection
        public static bool HasValue<T>(this IEnumerable<T> value)
        {
            if (value != null)
            {
                return value.Any();
            }

            return false;
        }

        public static bool HasValue<T>(this IEnumerable<T> value, Func<T, bool> condition)
        {
            if (value != null)
            {
                return value.Any(condition);
            }

            return false;
        }

        public static bool HasValue<T>(this T[] value)
        {
            if (value != null)
            {
                return value.Length > 0;
            }

            return false;
        }

        public static bool HasValue<T>(this ICollection<T> value)
        {
            if (value != null)
            {
                return value.Count > 0;
            }

            return false;
        }
        #endregion

        #region Numeric
        public static bool HasValue(this double value)
        {
            return value > 0;
        }
        public static bool HasValue(this decimal value)
        {
            return value > 0;
        }
        #endregion

        #region Class Types
        public static bool HasValue<TKey, TValue>(this KeyValuePair<TKey, TValue> pair)
        {
            return !pair.Key.IsDefault() || !pair.Value.IsDefault();
        }

        public static bool HasValue(this FileSystemInfo info)
        {
            return info != null && info.Exists;
        }

        public static bool HasValue(this ILogger logger, LogLevel logLevel)
        {
            if (logger.HasValue() && logger.IsEnabled(logLevel))
            {
                return true;
            }

            return false;
        }

        public static bool HasValue(this IEnumerable<ILogger> loggers, LogLevel logLevel)
        {
            if (loggers.HasValue())
            {
                foreach (var logger in loggers)
                {
                    if (logger.HasValue(logLevel))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
        #endregion        
    }
}
