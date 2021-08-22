using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions
{
    public static class DateTimeExtensions
    {
        #region DateTime
        /// <summary>
        /// Gets the difference in time between <paramref name="startDate"/> and <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="startDate">Date to compare with <see cref="DateTime.Now"/></param>
        /// <returns>Time difference between <paramref name="startDate"/> and <see cref="DateTime.Now"/></returns>
        public static TimeSpan GetDifference(this DateTime startDate)
        {
            return startDate.GetDifference(DateTime.Now);
        }
        /// <summary>
        /// Gets the difference in time between <paramref name="startDate"/> and <paramref name="endDate"/>.
        /// </summary>
        /// <param name="startDate">Date to compare with <paramref name="endDate"/></param>
        /// <param name="endDate">Date to compare with <paramref name="startDate"/></param>
        /// <returns>Time difference between <paramref name="startDate"/> and <paramref name="endDate"/></returns>
        public static TimeSpan GetDifference(this DateTime startDate, DateTime endDate)
        {
            return startDate - endDate;
        }

        /// <summary>
        /// Gets the difference in minutes between <paramref name="startDate"/> and <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="startDate">Date to compare with <see cref="DateTime.Now"/></param>
        /// <returns>Total minute difference between <paramref name="startDate"/> and <see cref="DateTime.Now"/></returns>
        public static int GetMinuteDifference(this DateTime startDate)
        {
            return startDate.GetMinuteDifference(DateTime.Now);
        }
        /// <summary>
        /// Gets the difference in minutes between <paramref name="startDate"/> and <paramref name="endDate"/>.
        /// </summary>
        /// <param name="startDate">Date to compare with <paramref name="endDate"/></param>
        /// <param name="endDate">Date to compare with <paramref name="startDate"/></param>
        /// <returns>Total minute difference between <paramref name="startDate"/> and <paramref name="endDate"/></returns>
        public static int GetMinuteDifference(this DateTime startDate, DateTime endDate)
        {
            return Math.Abs((startDate - endDate).TotalMinutes.ConvertTo<int>());
        }

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
