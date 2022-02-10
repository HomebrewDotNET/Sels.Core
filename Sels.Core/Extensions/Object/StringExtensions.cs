using Sels.Core;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// Contains extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        public static string FormatString(this string value, params object[] parameters)
        {
            return string.Format(value, parameters);
        }

        public static string GetWithoutDigits(this string value)
        {
            return Regex.Replace(value, @"\d", "");
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Escapes all <paramref name="stringsToEscape"/> in <paramref name="source"/> by prefixing <paramref name="escapeString"/> to each <paramref name="stringsToEscape"/>.
        /// </summary>
        /// <param name="source">String to escape string in</param>
        /// <param name="escapeString">String to prefix in front of <paramref name="stringsToEscape"/></param>
        /// <param name="stringsToEscape">Array with strings to escape</param>
        /// <returns><paramref name="source"/> with string escaped</returns>
        public static string EscapeStrings(this string source, string escapeString, params string[] stringsToEscape)
        {
            escapeString.ValidateArgument(nameof(escapeString));
            stringsToEscape.ValidateArgumentNotNullOrEmpty(nameof(stringsToEscape));

            if (source.HasValue())
            {
                var builder = new StringBuilder(source);

                foreach(var stringToEscape in stringsToEscape)
                {
                    builder.Replace(stringToEscape, escapeString + stringToEscape);
                }

                return builder.ToString();
            }

            return source;
        }

        #region Contains
        /// <summary>
        /// Checks if <paramref name="value"/> contains one of the chars in <paramref name="chars"/>.
        /// </summary>
        /// <param name="value">String to check</param>
        /// <param name="chars">Chars to check that <paramref name="value"/> contains</param>
        /// <returns>If <paramref name="value"/> contains at one of the chars in <paramref name="chars"/></returns>
        public static bool Contains(this string value, params char[] chars)
        {
            value.ValidateArgument(nameof(value));
            chars.ValidateArgument(nameof(chars));

            var stringChars = value.ToCharArray();

            foreach (var stringChar in stringChars)
            {
                if (chars.Contains(stringChar))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Checks if <paramref name="value"/> contains all chars in <paramref name="chars"/>.
        /// </summary>
        /// <param name="value">String to check</param>
        /// <param name="chars">Chars to check that <paramref name="value"/> contains</param>
        /// <returns>If <paramref name="value"/> contains all chars in <paramref name="chars"/></returns>
        public static bool ContainsAll(this string value, params char[] chars)
        {
            value.ValidateArgument(nameof(value));
            chars.ValidateArgument(nameof(chars));

            var stringChars = value.ToCharArray();

            foreach (var stringChar in stringChars)
            {
                if (!chars.Contains(stringChar))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if <paramref name="value"/> contains at least one substring in <paramref name="strings"/>.
        /// </summary>
        /// <param name="value">String to check</param>
        /// <param name="strings">Substrings to check that <paramref name="value"/> contains</param>
        /// <returns>If <paramref name="value"/> contains at least one substring in <paramref name="strings"/></returns>
        public static bool Contains(this string value, params string[] strings)
        {
            value.ValidateArgument(nameof(value));
            strings.ValidateArgument(nameof(strings));

            foreach (var stringValue in strings)
            {
                if (value.Contains(stringValue))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Checks if <paramref name="value"/> contains all substrings in <paramref name="strings"/>
        /// </summary>
        /// <param name="value">String to check</param>
        /// <param name="strings">Substrings to check that <paramref name="value"/> contains</param>
        /// <returns>If <paramref name="value"/> contains all substrings in <paramref name="strings"/></returns>
        public static bool ContainsAll(this string value, params string[] strings)
        {
            value.ValidateArgument(nameof(value));
            strings.ValidateArgument(nameof(strings));

            foreach (var stringValue in strings)
            {
                if (!value.Contains(stringValue))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Join
        public static string JoinString<T>(this IEnumerable<T> values, object joinValue)
        {
            values.ValidateArgument(nameof(values));
            joinValue.ValidateArgument(nameof(joinValue));

            return string.Join(joinValue.ToString(), values);
        }

        public static string JoinString<T>(this IEnumerable<T> values)
        {
            return values.JoinString(Constants.Strings.Comma);
        }

        public static string JoinStringNewLine<T>(this IEnumerable<T> values)
        {
            return values.JoinString(Environment.NewLine);
        }

        public static string JoinStringTab<T>(this IEnumerable<T> values)
        {
            return values.JoinString(Constants.Strings.Tab);
        }

        public static string JoinStringSpace<T>(this IEnumerable<T> values)
        {
            return values.JoinString(Constants.Strings.Space);
        }

        public static string JoinStringNoSpace<T>(this IEnumerable<T> values)
        {
            return values.JoinString(string.Empty);
        }
        #endregion

        #region Split
        /// <summary>
        /// Splits <paramref name="source"/> on <paramref name="splitValue"/>.
        /// </summary>
        /// <param name="source">String to split</param>
        /// <param name="splitValue">What value to split <paramref name="source"/> on</param>
        /// <param name="options">Option to omit empty string values from the return value</param>
        /// <returns>Substrings after splitting <paramref name="source"/></returns>
        public static string[] Split(this string source, string splitValue, StringSplitOptions options = StringSplitOptions.None)
        {
            source.ValidateArgumentNotNullOrEmpty(nameof(source));
            splitValue.ValidateArgument(nameof(splitValue));

            return source.Split(new string[] { splitValue }, options);
        }

        /// <summary>
        /// Splits <paramref name="source"/> on the first occurance of <paramref name="splitValue"/>.
        /// </summary>
        /// <param name="source">String to split</param>
        /// <param name="splitValue">Value to split string with</param>
        /// <param name="other">The other values after splitting</param>
        /// <param name="options">Optional options for splitting the strings</param>
        /// <returns>The first value after splitting or the <paramref name="source"/> if the value could not be split</returns>
        public static string SplitOnFirstOrDefault(this string source, object splitValue, out string other, StringSplitOptions options = StringSplitOptions.None)
        {
            splitValue.ValidateArgument(nameof(splitValue));

            other = null;

            if (source.HasValue())
            {
                var split = source.Split(splitValue.ToString(), options);

                if(split.Length > 1)
                {
                    other = split.Skip(1).JoinString(splitValue.ToString());
                    return split[0];
                }
            }

            return source;
        }

        /// <summary>
        /// Splits <paramref name="source"/> on the first occurance of <paramref name="splitValue"/>. Throws <see cref="InvalidOperationException"/> if <paramref name="source"/> could not be split on <paramref name="splitValue"/>.
        /// </summary>
        /// <param name="source">String to split</param>
        /// <param name="splitValue">Value to split string with</param>
        /// <param name="other">The other values after splitting</param>
        /// <param name="options">Optional options for splitting the strings</param>
        /// <returns>The first value after splitting/returns>
        public static string SplitOnFirst(this string source, object splitValue, out string other, StringSplitOptions options = StringSplitOptions.None)
        {
            splitValue.ValidateArgument(nameof(splitValue));

            other = null;

            if (source.HasValue())
            {
                var split = source.Split(splitValue.ToString(), options);

                if (split.Length > 1)
                {
                    other = split.Skip(1).JoinString(splitValue.ToString());
                    return split[0];
                }
            }

            throw new InvalidOperationException($"Could not split <{source}> on <{splitValue}>");
        }

        /// <summary>
        /// Splits <paramref name="source"/> on the first occurance of <paramref name="splitValue"/>.
        /// </summary>
        /// <param name="source">String to split</param>
        /// <param name="splitValue">Value to split string with</param>
        /// <param name="first">The first value after splitting</param>
        /// <param name="other">The other values after splitting</param>
        /// <param name="options">Optional options for splitting the strings</param>
        /// <returns>If <paramref name="source"/> could be split on <paramref name="splitValue"/></returns>
        public static bool TrySplitOnFirst(this string source, object splitValue, out string first, out string other, StringSplitOptions options = StringSplitOptions.None)
        {
            splitValue.ValidateArgument(nameof(splitValue));

            first = null;
            other = null;

            if (source.HasValue())
            {
                var split = source.Split(splitValue.ToString(), StringSplitOptions.None);

                if (split.Length > 1)
                {
                    other = split.Skip(1).JoinString(splitValue.ToString());
                    first = split[0];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Splits <paramref name="source"/> using <see cref="Environment.NewLine"/>.
        /// </summary>
        /// <param name="source">String to split</param>
        /// <returns><paramref name="source"/> split up using <see cref="Environment.NewLine"/></returns>
        public static string[] SplitOnNewLine(this string source)
        {
            return source.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
        #endregion

        #region Builder
        public static StringBuilder AppendSpace(this StringBuilder builder)
        {
            builder.ValidateArgument(nameof(builder));

            builder.Append(Constants.Strings.Space);

            return builder;
        }

        public static StringBuilder AppendTab(this StringBuilder builder)
        {
            builder.ValidateArgument(nameof(builder));

            builder.Append(Constants.Strings.Tab);

            return builder;
        }
        #endregion

        #region Filter
        /// <summary>
        /// Modifies <paramref name="value"/> by calling <paramref name="filterFunction"/> for each filter in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of filter</typeparam>
        /// <param name="source">The filters to use</param>
        /// <param name="value">The value to modify</param>
        /// <param name="filterFunction">The function that will be called for each filter in <paramref name="source"/>. First arg is the filter, second arg is the current string value</param>
        /// <returns></returns>
        public static string Filter<T>(this IEnumerable<T> source, string value, Func<T, string, string> filterFunction)
        {
            value.ValidateArgument(nameof(value));
            filterFunction.ValidateArgument(nameof(filterFunction));

            if (source.HasValue())
            {
                foreach(var filter in source)
                {
                    value = filterFunction(filter, value);
                }
            }

            return value;
        }
        #endregion
    }
}
