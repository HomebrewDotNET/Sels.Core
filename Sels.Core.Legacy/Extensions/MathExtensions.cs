using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions
{
    /// <summary>
    /// Contains static extension methods utilizing math.
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Rounds <paramref name="value"/> to <paramref name="digits"/> digits.
        /// </summary>
        /// <param name="value">The value to round</param>
        /// <param name="digits">To how many digits to round</param>
        /// <param name="rounding">The rounding strategy to use</param>
        /// <returns><paramref name="value"/> rounded to <paramref name="digits"/> digits</returns>
        public static double RoundTo(this double value, int digits, MidpointRounding rounding = MidpointRounding.ToEven)
        {
            return Math.Round(value, digits, rounding);
        }

        /// <summary>
        /// Rounds <paramref name="value"/> to <paramref name="digits"/> digits.
        /// </summary>
        /// <param name="value">The value to round</param>
        /// <param name="digits">To how many digits to round</param>
        /// <param name="rounding">The rounding strategy to use</param>
        /// <returns><paramref name="value"/> rounded to <paramref name="digits"/> digits</returns>
        public static decimal RoundTo(this decimal value, int digits, MidpointRounding rounding = MidpointRounding.ToEven)
        {
            return Math.Round(value, digits, rounding);
        }
    }
}
