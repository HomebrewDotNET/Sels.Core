using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sels.Core.Extensions.Calculation
{
    public static class CalculationExtensions
    {
        public static double GetMinimum(this IEnumerable<double> list)
        {
            double? minimum = null;

            if (list.HasValue())
            {
                foreach(var item in list)
                {
                    if(minimum == null)
                    {
                        minimum = item;
                    }
                    else if(item < minimum)
                    {
                        minimum = item;
                    }
                }
            }

            return minimum ?? 0;
        }

        public static double GetMaximum(this IEnumerable<double> list)
        {
            double? maximum = null;

            if (list.HasValue())
            {
                foreach (var item in list)
                {
                    if (maximum == null)
                    {
                        maximum = item;
                    }
                    else if (item > maximum)
                    {
                        maximum = item;
                    }
                }
            }

            return maximum ?? 0;
        }

        public static double GetAverage(this IEnumerable<double> list)
        {
            double? average = 0;

            if (list.HasValue())
            {
                var sum = list.GetSum();

                average = sum / list.Count();
            }

            return average ?? 0;
        }

        public static double GetSum(this IEnumerable<double> list)
        {
            double? sum = null;

            if (list.HasValue())
            {
                foreach (var item in list)
                {
                    if (sum == null)
                    {
                        sum = item;
                    }
                    else
                    {
                        sum += item;
                    }
                }
            }

            return sum ?? 0;
        }

        [Obsolete]
        public static int CalculateSquared(this int value, int squareValue, int square)
        {
            square.ValidateArgumentLargerOrEqual(nameof(square), 1);

            while(square > 0)
            {
                value *= squareValue;

                square--;
            }

            return value;
        }

        #region Multiply
        /// <summary>
        /// Multiplies <paramref name="value"/> by <paramref name="multiplyValue"/> <paramref name="times"/> times.
        /// </summary>
        /// <param name="value">Value to multiply</param>
        /// <param name="multiplyValue">Value to multiply <paramref name="value"/> with</param>
        /// <param name="times">How many times to multiply <paramref name="value"/> with <paramref name="multiplyValue"/></param>
        /// <returns>Calculated value</returns>
        public static decimal MultiplyBy(this decimal value, decimal multiplyValue, int times = 1)
        {
            times.ValidateArgumentLargerOrEqual(nameof(times), 1);

            while (times > 0)
            {
                value *= multiplyValue;

                times--;
            }

            return value;
        }

        /// <summary>
        /// Multiplies <paramref name="value"/> by <paramref name="multiplyValue"/> <paramref name="times"/> times.
        /// </summary>
        /// <param name="value">Value to multiply</param>
        /// <param name="multiplyValue">Value to multiply <paramref name="value"/> with</param>
        /// <param name="times">How many times to multiply <paramref name="value"/> with <paramref name="multiplyValue"/></param>
        /// <returns>Calculated value</returns>
        public static decimal MultiplyBy(this long value, long multiplyValue, int times = 1)
        {
            times.ValidateArgumentLargerOrEqual(nameof(times), 1);

            while (times > 0)
            {
                value *= multiplyValue;

                times--;
            }

            return value;
        }
        #endregion

        #region DivideBy
        /// <summary>
        /// Divides <paramref name="value"/> by <paramref name="divideValue"/> <paramref name="times"/> times.
        /// </summary>
        /// <param name="value">Value to divide</param>
        /// <param name="divideValue">Value to divide <paramref name="value"/> with</param>
        /// <param name="times">How many times to divide <paramref name="value"/> with <paramref name="divideValue"/></param>
        /// <returns>Calculated value</returns>
        public static decimal DivideBy(this decimal value, decimal divideValue, int times = 1)
        {
            times.ValidateArgumentLargerOrEqual(nameof(times), 1);

            while (times > 0)
            {
                value /= divideValue;

                times--;
            }

            return value;
        }

        /// <summary>
        /// Divides <paramref name="value"/> by <paramref name="divideValue"/> <paramref name="times"/> times.
        /// </summary>
        /// <param name="value">Value to divide</param>
        /// <param name="divideValue">Value to divide <paramref name="value"/> with</param>
        /// <param name="times">How many times to divide <paramref name="value"/> with <paramref name="divideValue"/></param>
        /// <returns>Calculated value</returns>
        public static decimal DivideBy(this long value, long divideValue, int times = 1)
        {
            times.ValidateArgumentLargerOrEqual(nameof(times), 1);

            decimal newValue = value;

            while (times > 0)
            {
                newValue /= divideValue;

                times--;
            }

            return newValue;
        }
        #endregion


    }
}
