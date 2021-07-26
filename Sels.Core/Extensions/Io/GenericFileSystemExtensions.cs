using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Extensions.Io
{
    public static class GenericFileSystemExtensions
    {
        #region ToValid
        private const char _defaultFileSystemReplaceChar = '_';

        public static string ToValidFileSystemName(this string value, char replaceValue)
        {
            return value.ToValidFileName(replaceValue).ToValidPath(replaceValue);
        }

        public static string ToValidFileName(this string value, char replaceValue)
        {
            var invalidChars = Path.GetInvalidFileNameChars();

            var builder = new StringBuilder(value);

            foreach (var invalidChar in invalidChars)
            {
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
    }
}
