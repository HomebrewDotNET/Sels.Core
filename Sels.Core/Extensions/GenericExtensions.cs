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

namespace Sels.Core.Extensions
{
    public static class GenericExtensions
    {
        #region Transforming and casting
        public static T Cast<T>(object value) where T : class
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        #endregion

        #region Hashing
        public static string GenerateHash<THash>(object sourceObject) where THash : HashAlgorithm
        {
            sourceObject.ValidateVariable(nameof(sourceObject));
            var hashType = typeof(THash);
            hashType.ValidateVariable(x => !x.Equals(typeof(HashAlgorithm)), () => $"Please use an implementation of {typeof(HashAlgorithm)}");

            using (var hash = HashAlgorithm.Create(hashType.Name))
            {
                var hashedBytes = hash.ComputeHash(sourceObject.GetBytes());

                return hashedBytes.Select(x => x.ToString("x2")).JoinString(string.Empty);
            }
        }
        #endregion

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
