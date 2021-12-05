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

        public static string JoinString<T>(this IEnumerable<T> values, string joinValue)
        {
            return values.HasValue() ? string.Join(joinValue, values) : string.Empty;
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

        public static string TrySplitFirstOrDefault(this string source, object splitValue, out string splitResult, StringSplitOptions splitOption = StringSplitOptions.None)
        {
            splitValue.ValidateVariable(nameof(splitValue));

            splitResult = null;

            if (source.HasValue())
            {
                var split = source.Split(splitValue.ToString(), splitOption);

                if(split.Length > 1)
                {
                    splitResult = split.Skip(1).JoinString(splitValue.ToString());
                    return split[0];
                }
            }

            return source;
        }

        public static string[] SplitStringOnNewLine(this string source)
        {
            return source.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
        #endregion

        #region Builder
        public static StringBuilder AppendSpace(this StringBuilder builder)
        {
            builder.ValidateVariable(nameof(builder));

            builder.Append(Constants.Strings.Space);

            return builder;
        }

        public static StringBuilder AppendTab(this StringBuilder builder)
        {
            builder.ValidateVariable(nameof(builder));

            builder.Append(Constants.Strings.Tab);

            return builder;
        }
        #endregion
    }
}
