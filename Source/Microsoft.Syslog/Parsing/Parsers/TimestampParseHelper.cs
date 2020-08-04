// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Syslog.Model;

    public static class TimestampParseHelper
    {
        static readonly string[] _months = "Jan,Feb,Mar,Apr,May,Jun,Jul,Aug,Sep,Oct,Nov,Dec".ToUpperInvariant().Split(',');
        static readonly string[] _timezones = "UTC,GMT,PDT,PST,IST".Split(',');

        public static bool TryParseTimestamp(ParserContext ctx)
        {
            // sometimes there's starting space
            if (ctx.Current == SyslogChars.Space)
                ctx.SkipSpaces();

            // some messages start with some numeric Id: 
            // <139>36177473: Mar  4 17:21:18 UTC: 
            if (char.IsDigit(ctx.Current))
            {
                // make sure it's not a year and at least 5 chars
                var fiveDigits = ctx.Text.Substring(ctx.Position, 5).All(ch => char.IsDigit(ch));
                if (fiveDigits)
                {
                    var savePos = ctx.Position; 
                    var digits = ctx.ReadDigits(20);
                    if (ctx.Match(": "))
                    {
                        // we swallowed this numeric prefix and ': ' after that, nothing to do
                    } else
                    {
                        ctx.Position = savePos; // rollback, timestamp evaluation will go from string start
                    }
                }
            }
            try
            {
                // quick guess - if it contains current month name
                if (TryParseIfStartsWithYear(ctx) || TryParseTimestampWithMonthName(ctx) ||
                    TryParseIfStartsWithColonThenYear(ctx) || TryParseIfStartsWithSpace(ctx))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                ctx.ErrorMessages.Add(ex.ToString());
            }
            return false; 
                   
        }

        // match entry like  
        //  <141>2020-03-04T17:20:54.412705+00:00 MWH....
        public static bool TryParseIfStartsWithYear(ParserContext ctx)
        {
            var year = DateTime.UtcNow.Year;
            var savePos = ctx.Position;
            if (ctx.Match($"{year}-") || ctx.Match($"{year - 1}-")) //(also check prior year)
            {
                ctx.Position = savePos;
                var spPos = ctx.Text.IndexOf(" ", ctx.Position);// 
                if (spPos > 0)
                {
                    var dtStr = ctx.Text.Substring(ctx.Position, spPos - ctx.Position);
                    ctx.Position = spPos;
                    if (DateTime.TryParse(dtStr, out var dt))
                    {
                        ctx.Entry.Header.Timestamp = dt.ToUniversalTime();
                        return true;
                    }
                }
            }
            ctx.Position = savePos;
            return false;
        }

        public static bool TryParseTimestampWithMonthName(ParserContext ctx)
        {
            if (ctx.Text.Length < ctx.Position + 20)
                return false; 
            var tsStr = ctx.Text.Substring(ctx.Position, 35);
            if (tsStr.StartsWith(": "))
                tsStr = tsStr.Substring(2);
            if (tsStr.Contains("/"))
                return false; 
            var words = tsStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 4)
                return false; 
            string year = null;
            string month = null;
            string day = null;
            string time = null;
            string tz = null;
            var y = DateTime.UtcNow.Year; 
            var thisYear = y.ToString();
            var prevYear = (y - 1).ToString();
            string lastWord = null; 
            // Take first 5 words and try to understand what it is
            for(int i = 0; i < 5; i++)
            {
                if (i >= words.Length)
                    break; 
                var word = words[i];
                if (word == thisYear || word == prevYear)
                    year = word;
                else if (word.Length == 3 && _months.Contains(word.ToUpperInvariant()))
                    month = word;
                else if (word.Length < 3 && int.TryParse(word, out var iDay))
                {
                    day = word;
                }
                else if (word.Contains(":") && word.Split(':').Length == 3)
                {
                    time = word;
                    lastWord = word; 
                }
                else if (word.Length == 4 && word.EndsWith(":"))
                {
                    var w3 = word.Substring(0, 3).ToUpperInvariant();
                    if (_timezones.Contains(w3))
                    {
                        tz = w3;
                        lastWord = word; 
                        break; //for loop
                    }
                }                     
            }
            // check if we have enough - at least month, day and time
            if (month == null || day == null || time == null)
                return false; 
            if (year == null)
                year = GuessYear(month).ToString();
            ctx.Entry.Header.Timestamp = ConstructDateTime(year, month, day, time, tz);
            // advance position
             ctx.Position = ctx.Text.IndexOf(lastWord, ctx.Position) + lastWord.Length; 
            return true; 
        }

        private static DateTime ConstructDateTime(string year, string month, string day, string time, string tz)
        {
            var y = int.Parse(year);
            var m = Array.IndexOf(_months, month.ToUpperInvariant()) + 1;
            var d = int.Parse(day);
            var t = TimeSpan.Parse(time);
            var dt = new DateTime(y, m, d, t.Hours, t.Minutes, t.Seconds, DateTimeKind.Utc);
            if (tz != null)
            {
                var offs = GetTimezoneOffset(tz); 
                dt = dt.Subtract(offs);
            }
            return dt; 
        }

        private static TimeSpan GetTimezoneOffset(string value)
        {
            TimeSpan ts;
            switch (value)
            {
                case "PDT":
                    ts = TimeSpan.FromHours(-7);
                    break;
                case "PST":
                    ts = TimeSpan.FromHours(-8);
                    break;
                default:
                    ts = TimeSpan.Zero;
                    break;
            }
            return ts;
        }


        // guess year for a month, most likely current year, but handle new year 
        private static int GuessYear(string month)
        {
            var now = DateTime.UtcNow;
            var year = now.Year;
            // If it is Jan, but entry's month is Dec, then it was previous year
            if (month == "Dec" && now.Month == 0)
            {
                year--;
            }
            return year;
        }

        // match entry like  
        //  <186>: 2020 Mar  4 17:20:54.183 UTC:
        public static bool TryParseIfStartsWithColonThenYear(ParserContext ctx)
        {
            var year = DateTime.UtcNow.Year;
            var savePos = ctx.Position;
            if (ctx.Match(": ") && (ctx.Match($"{year}") || ctx.Match($"{year - 1}"))) //check prior year
            {
                // we already matched beyond year; move back: 
                ctx.Position = savePos + ": ".Length;
                var UTC = "PST:"; // "UTC:";
                var utcPos = ctx.Text.IndexOf(UTC, ctx.Position);
                if (utcPos > 0)
                {
                    var dtStr = ctx.Text.Substring(ctx.Position, utcPos - ctx.Position);
                    ctx.Position = utcPos + UTC.Length; 
                    if (DateTime.TryParse(dtStr, out var dt))
                    {
                        ctx.Entry.Header.Timestamp = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                        return true;
                    }
                }
            }
            ctx.Position = savePos; 
            return false; 
        }

        // match entry like  
        //  <134> 03/04/2020:17:20:58 GMT ams07....
        public static bool TryParseIfStartsWithSpace(ParserContext ctx)
        {
            ctx.Reset();
            if (ctx.Current != SyslogChars.Space)
                return false; 
            var prefix = ctx.Text.Substring(ctx.Position, 25);
            var year = DateTime.UtcNow.Year;
            if (prefix.Contains($"/{year}:") || prefix.Contains($"/{year - 1}:")) //(also check prior year)
            {
                ctx.Position++;
                var spPos = ctx.Text.IndexOf(" ", ctx.Position); 
                if (spPos > 0)
                {
                    var dtStr = ctx.Text.Substring(ctx.Position, spPos - ctx.Position);
                    ctx.Position = spPos + 1;
                    ctx.Match("GMT"); //skip also GMT
                    if (DateTime.TryParse(dtStr, out var dt) || 
                        DateTime.TryParseExact(dtStr, "MM/dd/yyyy:HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dt))
                    {
                        ctx.Entry.Header.Timestamp = dt.ToUniversalTime();
                    }
                }
                return true;
            }
            return false;
        }

        public static DateTime? ParseStandardTimestamp(this ParserContext ctx)
        {
            var ts = ctx.ReadWord();
            if (ts == null)
            {
                return null;
            }

            if (DateTime.TryParse(ts, out var dt))
            {
                // by default TryParse produces local time
                return dt.ToUniversalTime();
            }
            ctx.AddError($"Invalid timestamp '{ts}'.");
            return DateTime.MinValue;
        }


    }
}
