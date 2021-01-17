using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sels.Core.Extensions.Object.String
{
    public static class StringExtensions
    {
        private const string StringTab = "\t";
        private const string StringSpace = " ";
        private const string NoStringSpace = "";

        public static string FormatString(this string value, params object[] parameters)
        {
            return string.Format(value, parameters);
        }

        public static string GetWithoutDigits(this string value)
        {
            return Regex.Replace(value, @"\d", "");
        }

        #region Contains
        public static bool Contains(this string value, params char[] chars)
        {
            value.ValidateVariable(nameof(value));
            chars.ValidateVariable(nameof(chars));

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

        public static bool ContainsAll(this string value, params char[] chars)
        {
            value.ValidateVariable(nameof(value));
            chars.ValidateVariable(nameof(chars));

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

        public static bool Contains(this string value, params string[] strings)
        {
            value.ValidateVariable(nameof(value));
            strings.ValidateVariable(nameof(strings));

            foreach (var stringValue in strings)
            {
                if (value.Contains(stringValue))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsAll(this string value, params string[] strings)
        {
            value.ValidateVariable(nameof(value));
            strings.ValidateVariable(nameof(strings));

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


        #region FileSystem
        private const char _defaultFileSystemReplaceChar = '_';

        public static string ToValidFileSystemName(this string value, char replaceValue)
        {
            return value.ToValidFileName(replaceValue).ToValidPath(replaceValue);
        }

        public static string ToValidFileName(this string value, char replaceValue)
        {
            var invalidChars = Path.GetInvalidFileNameChars();

            var builder = new StringBuilder(value);

            foreach(var invalidChar in invalidChars){
                builder.Replace(invalidChar, replaceValue);
            }

            return builder.ToString();
        }

        public static string ToValidPath(this string value, char replaceValue)
        {
            var invalidChars = Path.GetInvalidPathChars();

            var builder = new StringBuilder(value);

            foreach (var invalidChar in invalidChars)
            {
                builder.Replace(invalidChar, replaceValue);
            }

            return builder.ToString();
        }

        public static string ToValidFileSystemName(this string value)
        {
            return value.ToValidFileSystemName(_defaultFileSystemReplaceChar);
        }

        public static string ToValidFileName(this string value)
        {
            return value.ToValidFileName(_defaultFileSystemReplaceChar);
        }

        public static string ToValidPath(this string value)
        {
            return value.ToValidPath(_defaultFileSystemReplaceChar);
        }
        #endregion

        #region Join
        private const char DefaultJoinValue = ',';

        public static string JoinString<T>(this IEnumerable<T> values, string joinValue)
        {
            return string.Join(joinValue, values);
        }

        public static string JoinString<T>(this IEnumerable<T> values)
        {
            return values.JoinString(DefaultJoinValue.ToString());
        }

        public static string JoinStringNewLine<T>(this IEnumerable<T> values)
        {
            return values.JoinString(Environment.NewLine);
        }

        public static string JoinStringTab<T>(this IEnumerable<T> values)
        {
            return values.JoinString(StringTab);
        }

        public static string JoinStringSpace<T>(this IEnumerable<T> values)
        {
            return values.JoinString(StringSpace);
        }

        public static string JoinStringNoSpace<T>(this IEnumerable<T> values)
        {
            return values.JoinString(NoStringSpace);
        }

        public static string JoinStrings(params object[] values)
        {
            values.ValidateVariable(nameof(values));

            return values.JoinString();
        }

        public static string JoinStrings(string joinValue, params string[] strings)
        {
            strings.ValidateVariable(nameof(strings));

            return strings.JoinString(joinValue);
        }

        public static string JoinStringsNewLine(params string[] strings)
        {
            strings.ValidateVariable(nameof(strings));

            return strings.JoinStringNewLine();
        }

        public static string JoinStringsTab(params string[] strings)
        {
            strings.ValidateVariable(nameof(strings));

            return strings.JoinStringTab();
        }

        #endregion
    }
}
