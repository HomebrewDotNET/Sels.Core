using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.RegularExpressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// Contains extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Formats <paramref name="value"/> using <paramref name="parameters"/>.
        /// </summary>
        /// <param name="value">The string to format</param>
        /// <param name="parameters">The parameters to format <paramref name="value"/> with</param>
        /// <returns>The formatted string</returns>
        public static string FormatString(this string value, params object[] parameters)
        {
            value.ValidateArgumentNotNullOrWhitespace(nameof(value));
            parameters.ValidateArgumentNotNullOrEmpty(nameof(parameters));

            return string.Format(value, parameters);
        }
        /// <summary>
        /// Returns <paramref name="value"/> where all digits are removed.
        /// </summary>
        /// <param name="value">The value to remove the digits from</param>
        /// <returns><paramref name="value"/> with all digits removed</returns>
        public static string GetWithoutDigits(this string value)
        {
            return Regex.Replace(value, @"\d", "");
        }
        /// <summary>
        /// Returns <paramref name="value"/> where all whitespace characters are removed.
        /// </summary>
        /// <param name="value">The value to remove the whitespace from</param>
        /// <returns><paramref name="value"/> with all whitespace removed</returns>
        public static string GetWithoutWhitespace(this string value)
        {
            value.ValidateArgument(nameof(value));

            return Regex.Replace(value, @"\s+", "");
        }
        /// <summary>
        /// Checks if <paramref name="value"/> is null or empty.
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <returns>True if <paramref name="value"/> is either null or an empty string, otherwise false</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
        /// <summary>
        /// Checks if <paramref name="value"/> contains any whitespace characters.
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <returns>True if <paramref name="value"/> contains whitespace characters, otherwise false</returns>
        public static bool ContainsWhitespace(this string value)
        {
            value.ValidateArgument(nameof(value));

            return value.Any(Char.IsWhiteSpace);
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
        /// <summary>
        /// Joins all <see cref="object.ToString"/> strings from <paramref name="values"/> using the <see cref="object.ToString"/> value from <paramref name="joinValue"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in <paramref name="values"/></typeparam>
        /// <param name="values">The values to join</param>
        /// <param name="joinValue">The object to get the string to join from</param>
        /// <returns>The joined string</returns>
        public static string JoinString<T>(this IEnumerable<T> values, object joinValue)
        {
            values.ValidateArgument(nameof(values));
            joinValue.ValidateArgument(nameof(joinValue));

            return string.Join(joinValue.ToString(), values);
        }
        /// <summary>
        /// Joins all <see cref="object.ToString"/> strings from <paramref name="values"/> using <see cref="Constants.Strings.Comma"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in <paramref name="values"/></typeparam>
        /// <param name="values">The values to join</param>
        /// <returns>The joined string</returns>
        public static string JoinString<T>(this IEnumerable<T> values)
        {
            values.ValidateArgument(nameof(values));

            return values.JoinString();
        }
        /// <summary>
        /// Joins all <see cref="object.ToString"/> strings from <paramref name="values"/> using the environment new line character.
        /// </summary>
        /// <typeparam name="T">Type of the elements in <paramref name="values"/></typeparam>
        /// <param name="values">The values to join</param>
        /// <returns>The joined string</returns>
        public static string JoinStringNewLine<T>(this IEnumerable<T> values)
        {
            values.ValidateArgument(nameof(values));

            return values.JoinString(Environment.NewLine);
        }
        /// <summary>
        /// Joins all <see cref="object.ToString"/> strings from <paramref name="values"/> using <see cref="Constants.Strings.Tab"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in <paramref name="values"/></typeparam>
        /// <param name="values">The values to join</param>
        /// <returns>The joined string</returns>
        public static string JoinStringTab<T>(this IEnumerable<T> values)
        {
            values.ValidateArgument(nameof(values));

            return values.JoinString(Constants.Strings.Tab);
        }
        /// <summary>
        /// Joins all <see cref="object.ToString"/> strings from <paramref name="values"/> using <see cref="Constants.Strings.Space"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in <paramref name="values"/></typeparam>
        /// <param name="values">The values to join</param>
        /// <returns>The joined string</returns>
        public static string JoinStringSpace<T>(this IEnumerable<T> values)
        {
            values.ValidateArgument(nameof(values));

            return values.JoinString(Constants.Strings.Space);
        }
        /// <summary>
        /// Joins all <see cref="object.ToString"/> strings from <paramref name="values"/> using <see cref="string.Empty"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in <paramref name="values"/></typeparam>
        /// <param name="values">The values to join</param>
        /// <returns>The joined string</returns>
        public static string JoinStringNoSpace<T>(this IEnumerable<T> values)
        {
            values.ValidateArgument(nameof(values));

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
            source.ValidateArgument(nameof(source));
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
        /// <returns>The first value after splitting</returns>
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
        /// <summary>
        /// Appends <see cref="Constants.Strings.Space"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <returns>Current builder for method chaining</returns>
        public static StringBuilder AppendSpace(this StringBuilder builder)
        {
            builder.ValidateArgument(nameof(builder));

            builder.Append(Constants.Strings.Space);

            return builder;
        }
        /// <summary>
        /// Appends <see cref="Constants.Strings.Tab"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <returns>Current builder for method chaining</returns>
        public static StringBuilder AppendTab(this StringBuilder builder)
        {
            builder.ValidateArgument(nameof(builder));

            builder.Append(Constants.Strings.Tab);

            return builder;
        }
        /// <summary>
        /// Joins the <see cref="object.ToString()"/> values in <paramref name="values"/> using the <see cref="object.ToString()"/> of <paramref name="joinValue"/> and adds it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="values">Enumerator returning the object to join the string values from</param>
        /// <param name="joinValue">Object containing the string value to join with</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static StringBuilder Join(this StringBuilder builder, IEnumerable<object> values, object joinValue)
        {
            builder.ValidateArgument(nameof(builder));
            joinValue.ValidateArgument(nameof(joinValue));
            if (values == null) return builder;
            var valueCount = values.GetCount();

            values.Execute((i, x) =>
            {
                builder.Append(x);
                if (i < valueCount-1) builder.Append(joinValue);
            });

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

        #region ToGrid
        /// <summary>
        /// Transforms <paramref name="source"/> into a 2 dimensional array by dividing <paramref name="source"/> into rows and columns.
        /// </summary>
        /// <param name="source">The string to transform</param>
        /// <param name="rowSplitter">The delegate that will split up <paramref name="source"/> into rows</param>
        /// <param name="columnSplitter">The delegate that will split up rows into columns</param>
        /// <param name="ignoreMissingColumns">If no exception should be thown when a row has less columns than the expected column length. When set to true the missing columns will be null in the grid</param>
        /// <param name="ignoreExtraColumns">If no exception should be thrown when a row has more columns than the expected column length. When set to true the extra columns will be missing from the grid</param>
        /// <param name="expectedColumns">The expected amount of rows. When set to null the first row will determine the column count</param>
        /// <returns>The grid created from the rows and columns in <paramref name="source"/></returns>
        /// <exception cref="InvalidOperationException">Thown when column lengths don't match up with the expected amount unless disabled</exception>
        public static string[,] ToGrid(this string source, Func<string, IEnumerable<string>> rowSplitter, Func<string, IEnumerable<string>> columnSplitter, bool ignoreMissingColumns = true, bool ignoreExtraColumns = true, int? expectedColumns = null)
        {
            source.ValidateArgumentNotNullOrWhitespace(nameof(source));
            rowSplitter.ValidateArgument(nameof(rowSplitter));
            columnSplitter.ValidateArgument(nameof(columnSplitter));
            if (expectedColumns.HasValue) expectedColumns.Value.ValidateArgumentLarger(nameof(expectedColumns), 0);

            int? columnLength = expectedColumns;
            var temporaryGrid = new List<string[]>();
            var rows = (rowSplitter(source) ?? throw new InvalidOperationException($"Row splitter returned null")).ToArray();

            if (rows.HasValue())
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    var row = rows[i];
                    var columns = (columnSplitter(row) ?? throw new InvalidOperationException($"Column splitter returned null")).ToArray();

                    if (!columnLength.HasValue) columnLength = columns.Length;

                    if (!ignoreExtraColumns && columns.Length > columnLength) throw new InvalidOperationException($"Row {i} has {columns.Length} columns which is larger than the first row of {columnLength} columns");
                    if (!ignoreMissingColumns && columns.Length < columnLength) throw new InvalidOperationException($"Row {i} has {columns.Length} columns which is smaller than the first row of {columnLength} columns");

                    var newColumns = new string[columnLength.Value];

                    for (int y = 0; y < columns.Length; y++)
                    {
                        if (y >= columnLength) break;
                        newColumns[y] = columns[y];
                    }

                    temporaryGrid.Add(newColumns);
                }

                var grid = new string[temporaryGrid.Count, columnLength.Value];

                for(int i = 0; i < rows.Length; i++)
                {
                    for (int y = 0; y < columnLength; y++)
                    {
                        grid[i, y] = temporaryGrid[i][y];
                    }
                }

                return grid;
            }

            return new string[0, 0];
        }
        #endregion

        #region ExtractFromFormat
        /// <summary>
        /// Extracts a value from <paramref name="value"/> where it is formatted according to <paramref name="format"/>.
        /// </summary>
        /// <param name="format">The format that <paramref name="value"/> is formatted in</param>
        /// <param name="parameter">The parameter to extract. Parameter is defined in <paramref name="format"/></param>
        /// <param name="value">The value to extract <paramref name="parameter"/> from</param>
        /// <param name="otherParameters">Optional parameters that are also included in the format. These are replaced so they match any characters</param>
        /// <returns>The extracted value or null if no value could be extracted</returns>
        public static string ExtractFromFormat(this string format, string parameter, string value, params string[] otherParameters)
        {
            value.ValidateArgument(nameof(value));
            parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter));
            format.ValidateArgumentNotNullOrWhitespace(nameof(format));
            parameter.ValidateArgument(x => format.Contains(x), $"{nameof(format)} does not contain {nameof(parameter)}");

            // Split format on the parameter to extract
            var lookBehind = format.SplitOnFirst(parameter, out var lookAhead);

            // Escape regex characters and remove any parameters
            if (!lookBehind.IsNullOrEmpty())
            {
                lookBehind = otherParameters.HasValue() ? lookBehind.Split(otherParameters, StringSplitOptions.None).Select(x => Regex.Escape(x)).JoinString(RegexBuilder.Expressions.AnyWord) : Regex.Escape(lookBehind);
            }

            if (!lookAhead.IsNullOrEmpty())
            {
                lookAhead = lookAhead.HasValue() ? lookAhead.Split(otherParameters, StringSplitOptions.None).Select(x => Regex.Escape(x)).JoinString(RegexBuilder.Expressions.AnyWord) : Regex.Escape(lookAhead);
            }

            // Build regex to match the value
            var builder = new RegexBuilder();
            if (!lookBehind.IsNullOrEmpty()) builder.LookBehind(lookBehind);
            builder.MatchAny();
            if (!lookAhead.IsNullOrEmpty()) builder.LookAhead(lookAhead);

            // Return the first match if present
            var match = builder.ToRegex().Match(value);
            return match.Success ? match.ToString() : null;
        }
        #endregion
    }
}
