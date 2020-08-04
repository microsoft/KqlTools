// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using System.Text;

    public static class SyslogChars
    {
        public const string Nil = "-";
        public const char NilChar = '-';
        public const char Space = ' ';
        public const char LT = '<';
        public const char GT = '>';
        public const char Lbr = '[';
        public const char Rbr = ']';
        public const char Escape = '\\';
        public const char DQuote = '"';
        public const char Dot = '.';
        public const char Colon = ':';
        public const char EQ = '=';
        public static readonly char[] QuoteOrEscape = new char[] { DQuote, Escape };
        public static readonly char[] WordSeparators = new char[] { Space, Lbr, Rbr, EQ };

        // BOM - byte-order-mark, special byte sequence often used as string prefix to indicate unicode. 
        //   we generally strip it out whenever we find it, it breaks string operations if left in string values
        public static readonly string BOM = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        public static readonly string SyslogStart = LT.ToString();
        public static readonly string SyslogStartBom = BOM + LT; 
    }
}
