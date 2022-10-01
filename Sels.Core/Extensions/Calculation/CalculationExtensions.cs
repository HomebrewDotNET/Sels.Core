using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sels.Core.Extensions.Calculation
{
    /// <summary>
    /// Contains extension for doing math related actions.
    /// </summary>
    public static class CalculationExtensions
    {
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
