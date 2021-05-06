
using Sels.Core.Extensions.Calculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Extensions.Conversion
{
    public static class ConversionExtensions
    {
        public static T ConvertTo<T>(object value) where T : class
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

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
                    referenceChar = (char)('A' + value - offset);

                    value = 0;
                }
                else
                {
                    referenceChar = (char)('A' + (value % width) - offset);

                    value /= convertedWidth;
                }

                builder.Insert(0, referenceChar);
            }

            return builder.ToString();
        }

        public static uint ToStringFromAlphaNumeric(this string value, int width, int offset = 0)
        {
            var chars = Enumerable.ToArray(value.ToCharArray().Where(x => char.IsLetter(x)));
            int cellIndex = 0;

            for (int i = chars.Length; i > 0; i--)
            {
                // Add offset because 1 = A
                var actualCharValue = chars[i - 1] - 'A' + offset;

                var multiplier = chars.Length - i;

                cellIndex += actualCharValue.CalculateSquared(width, multiplier);
            }

            return cellIndex.ToUInt32();
        }

        #endregion

        #region ToArray
        public static T[] ToArray<T>(this T value)
        {
            value.ValidateVariable(nameof(value));

            return new T[] { value };
        }

        public static T[] ToArrayOrDefault<T>(this T value)
        {
            if (value.HasValue())
            {
                return new T[] { value };
            }

            return new T[0];
        }
        #endregion

        #region ToGrid
        public static T[,] ToGrid<T>(this List<List<T>> table)
        {
            var columnLength = table.GetColumnLength();
            var grid = new T[table.Count(), columnLength];

            for (int i = 0; i < table.Count(); i++)
            {
                for (int j = 0; j < columnLength; j++)
                {
                    grid[i, j] = table[i][j];
                }
            }

            return grid;
        }
        #endregion

        #region Numeric
        public static uint ToUInt32(this int number)
        {
            return Convert.ToUInt32(number);
        }

        public static int ToInt(this uint number)
        {
            return Convert.ToInt32(number);
        }
        #endregion
    }
}
