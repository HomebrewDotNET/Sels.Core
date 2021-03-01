using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions
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

        public static bool IsToday(this DateTime date)
        {
            return date.IsSameDay(DateTime.Now);
        }

        public static bool IsSameDay(this DateTime date, DateTime otherDate)
        {
            return (date.DayOfYear == otherDate.DayOfYear) && (date.Year == otherDate.Year);
        }

        public static bool IsInPast(this DateTime date)
        {
            return date < DateTime.Now;
        }

        public static bool IsInFuture(this DateTime date)
        {
            return date > DateTime.Now;
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
