// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Globalization;
    using System.Text.RegularExpressions;

    public static class DateTimeParser
    {
        private static readonly Regex TimespanRegex = new Regex(@"^([0-9]+?)(day|min|hour|sec|d|h|m|s)$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static bool TryParseAgoValue(string input, out TimeSpan result)
        {
            var match = TimespanRegex.Match(input);
            result = default(TimeSpan);

            if (match.Success)
            {
                var number = int.Parse(match.Groups[1].Value);

                switch (match.Groups[2].Value)
                {
                    case "d":
                    case "day":
                        result = TimeSpan.FromDays(number);
                        break;
                    case "h":
                    case "hour":
                        result = TimeSpan.FromHours(number);
                        break;
                    case "m":
                    case "min":
                        result = TimeSpan.FromMinutes(number);
                        break;
                    case "s":
                    case "sec":
                        result = TimeSpan.FromSeconds(number);
                        break;
                }
            }

            return match.Success;
        }

        public static bool TryParseDatetime(string input, out DateTimeOffset result)
        {
            return DateTimeOffset.TryParseExact(
                input,
                RxKqlCommonFunctions.SupportedDateTimeFormatStrings,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out result);
        }
    }
}