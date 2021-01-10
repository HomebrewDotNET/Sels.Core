using Sels.Core.Extensions.General.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sels.Core.Extensions.General.Math
{
    public static class MathExtensions
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

        public static int CalculateSquared(this int value, int squareValue, int square)
        {
            while(square > 0)
            {
                value *= squareValue;

                square--;
            }

            return value;
        }
    }
}
