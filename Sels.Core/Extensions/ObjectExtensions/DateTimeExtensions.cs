using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions.Object.Time
{
    public static class DateTimeExtensions
    {
        #region DateTime
        public static int GetDayDifference(this DateTime startDate)
        {
            return startDate.GetDayDifference(DateTime.Now);
        }

        public static int GetDayDifference(this DateTime startDate, DateTime endDate)
        {
            return Math.Abs((startDate - endDate).Days);
        }

        public static int GetYearDifference(this DateTime startDate)
        {
            return startDate.GetYearDifference(DateTime.Now);
        }

        public static int GetYearDifference(this DateTime startDate, DateTime endDate)
        {
            int years = (int.Parse(startDate.ToString("yyyyMMdd")) - int.Parse(endDate.ToString("yyyyMMdd"))) / 10000;

            return Math.Abs(years);
        }
        #endregion

        #region Timespan
        private const string MsFormat = "ms";
        public static string PrintTotalMs(this TimeSpan timeSpan)
        {
            return $"{timeSpan.TotalMilliseconds}{MsFormat}";
        }
        #endregion

    }
}
