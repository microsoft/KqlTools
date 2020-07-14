// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog.Helpers
{
    using System;

    /// <summary>
    ///     Common functions for use within library
    /// </summary>
    public static class CommonFunctions
    {
        /// <summary>
        ///     Gets the date from UNIX time
        /// </summary>
        /// <param name="intDate">the number representing the date in UNIX format</param>
        /// <returns>the valid date time</returns>
        public static DateTime GetDateTimeFromUnix(long intDate)
        {
            var timeInTicks = intDate * TimeSpan.TicksPerMillisecond;
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddTicks(timeInTicks);
        }

        /// <summary>
        ///     Returns UNIX Time from the DateTime provided.
        /// </summary>
        /// <param name="dateTime">the datetime desired to be convered to UNIX</param>
        /// <returns>the long UNIX time</returns>
        public static string GetUnixTime(this DateTime dateTime)
        {
            var date = dateTime.ToUniversalTime();
            var ticks = date.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks;
            var tickRatio = ticks / TimeSpan.TicksPerMillisecond;
            return tickRatio.ToString();
        }

        /// <summary>
        ///     Returns UNIX Time from the DateTimeOffset provided.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>the long UNIX time</returns>
        public static string GetUnixTime(this DateTimeOffset dateTime)
        {
            var date = dateTime.ToUniversalTime();
            var ticks = date.Ticks - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks;
            var tickRatio = ticks / TimeSpan.TicksPerMillisecond;
            return tickRatio.ToString();
        }
    }
}