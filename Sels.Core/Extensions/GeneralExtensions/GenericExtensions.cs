using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.General.Math;
using Sels.Core.Extensions.General.Validation;
using Sels.Core.Extensions.Object.Number;
using Sels.Core.Extensions.Object.String;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Sels.Core.Extensions.General.Generic
{
    public static class GenericExtensions
    {
        public static bool IsDefault<T>(this T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default);
        }



        #region HasValue
        public static bool HasValue(this object value)
        {
            return !value.IsDefault();
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

        #region AlphaNumeric
        public static string ToAlphaNumericString(this int value, int width, int offset = 0)
        {
            return value.ToAlphaNumericString(width, offset);
        }
        public static string ToAlphaNumericString(this uint value, int width, int offset = 0)
        {
            var builder = new StringBuilder();
            var convertedWidth = width.ToUInt32();

            while (value > 0)
            {
                char referenceChar;
                if (value <= width)
                {
                    referenceChar = (char) ('A' + value- offset);

                    value = 0;
                }
                else
                {
                    referenceChar = (char)('A' + (value % width)- offset);

                    value /= convertedWidth;
                }

                builder.Insert(0, referenceChar);
            }

            return builder.ToString();
        }

        public static uint ToStringFromAlphaNumeric(this string value, int width, int offset = 0)
        {
            var chars = value.ToCharArray().Where(x => char.IsLetter(x)).ToArray();
            int cellIndex = 0;

            for(int i = chars.Length; i > 0; i--)
            {
                // Add offset because 1 = A
                var actualCharValue = chars[i-1] - 'A' + offset;

                var multiplier = chars.Length-i;

                cellIndex += actualCharValue.CalculateSquared(width, multiplier);
            }

            return cellIndex.ToUInt32();
        }

        #endregion
    }
}
