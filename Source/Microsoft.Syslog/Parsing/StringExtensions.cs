// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using System;
    using System.Linq;

    public static class StringExtensions
    {
        public static bool IsIpV4Char(this char ch)
        {
            return ch == SyslogChars.Dot || char.IsDigit(ch);
        }

        public static bool IsHexDigit(this char ch)
        {
            return char.IsDigit(ch) || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');
        }

        public static bool IsIpV6Char(this char ch)
        {
            return ch == SyslogChars.Colon || IsHexDigit(ch);
        }

        public static int SkipUntil(this string message, int start, Func<char, bool> func)
        {
            var p = start;
            while (p < message.Length && !func(message[p]))
                p++;
            return p; 
        }

        public static int Skip(this string message, int start, params char[] chars)
        {
            var p = start;
            while (p < message.Length && chars.Contains(message[p]))
                p++;
            return p;
        }
        public static int SkipUntil(this string message, int start, params char[] chars)
        {
            var p = start;
            while (p < message.Length && !chars.Contains(message[p]))
                p++;
            return p;
        }
    }
}
