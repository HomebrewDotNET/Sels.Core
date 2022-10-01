using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions
{
    /// <summary>
    /// Contains extension methods for working with <see cref="DateTime"/>.
    /// </summary>
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
            return Convert.ChangeType(Math.Abs((startDate - endDate).TotalMinutes), typeof(int)).Cast<int>();
        }
        /// <summary>
        /// Gets the difference in days between <paramref name="startDate"/> and <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="startDate">Date to compare with <see cref="DateTime.Now"/></param>
        /// <returns>Total day difference between <paramref name="startDate"/> and <see cref="DateTime.Now"/></returns>
        public static int GetDayDifference(this DateTime startDate)
        {
            return startDate.GetDayDifference(DateTime.Now);
        }
        /// <summary>
        /// Gets the difference in days between <paramref name="startDate"/> and <paramref name="endDate"/>.
        /// </summary>
        /// <param name="startDate">Date to compare with <paramref name="endDate"/></param>
        /// <param name="endDate">Date to compare with <paramref name="startDate"/></param>
        /// <returns>Total day difference between <paramref name="startDate"/> and <paramref name="endDate"/></returns>
        public static int GetDayDifference(this DateTime startDate, DateTime endDate)
        {
            return Math.Abs((startDate - endDate).Days);
        }
        /// <summary>
        /// Gets the difference in years between <paramref name="startDate"/> and <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="startDate">Date to compare with <see cref="DateTime.Now"/></param>
        /// <returns>Total year difference between <paramref name="startDate"/> and <see cref="DateTime.Now"/></returns>
        public static int GetYearDifference(this DateTime startDate)
        {
            return startDate.GetYearDifference(DateTime.Now);
        }
        /// <summary>
        /// Gets the difference in years between <paramref name="startDate"/> and <paramref name="endDate"/>.
        /// </summary>
        /// <param name="startDate">Date to compare with <paramref name="endDate"/></param>
        /// <param name="endDate">Date to compare with <paramref name="startDate"/></param>
        /// <returns>Total year difference between <paramref name="startDate"/> and <paramref name="endDate"/></returns>
        public static int GetYearDifference(this DateTime startDate, DateTime endDate)
        {
            int years = (int.Parse(startDate.ToString("yyyyMMdd")) - int.Parse(endDate.ToString("yyyyMMdd"))) / 10000;

            return Math.Abs(years);
        }
        /// <summary>
        /// Checks if <paramref name="date"/> is on the same day as <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <returns>True if <paramref name="date"/> is today, otherwise false</returns>
        public static bool IsToday(this DateTime date)
        {
            return date.IsSameDay(DateTime.Now);
        }
        /// <summary>
        /// Checks if <paramref name="date"/> and <paramref name="otherDate"/> are on the same day and year.
        /// </summary>
        /// <param name="date">The date to compare to <paramref name="otherDate"/></param>
        /// <param name="otherDate">The date to compare to <paramref name="date"/></param>
        /// <returns>True if <paramref name="date"/> and <paramref name="otherDate"/> are on the same day</returns>
        public static bool IsSameDay(this DateTime date, DateTime otherDate)
        {
            return (date.DayOfYear == otherDate.DayOfYear) && (date.Year == otherDate.Year);
        }
        /// <summary>
        /// Checks if <paramref name="date"/> is in the past.
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <returns>True if <paramref name="date"/> is smaller than <see cref="DateTime.Now"/>, otherwise false</returns>
        public static bool IsInPast(this DateTime date)
        {
            return date < DateTime.Now;
        }
        /// <summary>
        /// Checks if <paramref name="date"/> is in the future.
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <returns>True if <paramref name="date"/> is larger than <see cref="DateTime.Now"/>, otherwise false</returns>
        public static bool IsInFuture(this DateTime date)
        {
            return date > DateTime.Now;
        }
        #endregion

        #region Timespan
        private const string MsFormat = "ms";
        /// <summary>
        /// Returns a formatted string with the total milliseconds of <paramref name="timeSpan"/>.
        /// </summary>
        /// <param name="timeSpan">The timespan to get the formatted string from</param>
        /// <returns>The formatted string with the total milliseconds of <paramref name="timeSpan"/> </returns>
        public static string PrintTotalMs(this TimeSpan timeSpan)
        {
            return $"{timeSpan.TotalMilliseconds}{MsFormat}";
        }
        #endregion
    }
}
