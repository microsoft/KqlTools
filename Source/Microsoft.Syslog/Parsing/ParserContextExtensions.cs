// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Syslog.Model;

    public static class ParserContextExtensions
    {
        public static bool ReadSymbol(this ParserContext ctx, char symbol, bool throwIfMismatch = true)
        {
            if (ctx.Current == SyslogChars.Space)
            {
                ctx.SkipSpaces();
            }
            if (ctx.Current == symbol)
            {
                ctx.Position++;
                return true; 
            }
            if (throwIfMismatch)
                throw new Exception($"Invalid input, expected '{symbol}' ");
            else
                return false; 
        }

        public static bool TryReadUntil(this ParserContext ctx, string symbol, out string text)
        {
            var spos = ctx.Text.IndexOf(symbol, ctx.Position); 
            if (spos > 0)
            {
                text = ctx.Text.Substring(ctx.Position, spos - ctx.Position);
                ctx.Position = spos;
                return true; 
            }
            text = null;
            return false; 
        }

        public static void SkipSpaces(this ParserContext ctx)
        {
            while (!ctx.Eof() && ctx.Current == SyslogChars.Space)
                ctx.Position++;
        }

        public static string ReadWordOrNil(this ParserContext ctx)
        {
            var word = ctx.ReadWord();
            return (word == SyslogChars.Nil) ? null : word;
        }


        public static string ReadWord(this ParserContext ctx)
        {
            if (ctx.Current == SyslogChars.Space)
            {
                ctx.SkipSpaces();
            }

            var separatorPos = ctx.Text.IndexOfAny(SyslogChars.WordSeparators, ctx.Position);
            if (separatorPos < 0)
            {
                separatorPos = ctx.Text.Length;
            }

            var word = ctx.Text.Substring(ctx.Position, separatorPos - ctx.Position);
            ctx.Position = separatorPos;
            return word;
        }

        public static string ReadQuotedString(this ParserContext ctx)
        {
            ctx.ReadSymbol(SyslogChars.DQuote);
            string result = string.Empty;
            int curr = ctx.Position;
            while (true)
            {
                var next = ctx.Text.IndexOfAny(SyslogChars.QuoteOrEscape, curr);
                var segment = ctx.Text.Substring(curr, next - curr);
                result += segment;
                switch (ctx.CharAt(next))
                {
                    case SyslogChars.DQuote:
                        // we are done
                        ctx.Position = next + 1; // after dquote
                        return result;

                    case SyslogChars.Escape:
                        // it is escape symbol, add next char to result, shift to next char and continue loop
                        result += ctx.CharAt(next + 1);
                        curr = next + 2;
                        break;
                }//switch
            } // loop
        }

        public static int ReadNumber(this ParserContext ctx, int maxDigits = 10)
        {
            var digits = ctx.ReadDigits(maxDigits);
            return int.Parse(digits); 

        }

        public static string ReadDigits(this ParserContext ctx, int maxDigits = 10)
        {
            var start = ctx.Position;
            for (int i = 0; i < maxDigits; i++) {
                if (!char.IsDigit(ctx.Current))
                    break;
                ctx.Position++;
                if (ctx.Position >= ctx.Text.Length)
                    break;
            }
            if (ctx.Position == start)
                return null;
            var res = ctx.Text.Substring(start, ctx.Position - start);
            return res; 
        }


        /// <summary>Parser standard (for all levels) prefix &lt;n&gt;. </summary>
        /// <param name="ctx">parser context.</param>
        /// <returns>True if prefix read correctly; otherwise, false.</returns>
        public static bool ReadSyslogPrefix(this ParserContext ctx)
        {
            try
            {
                ctx.Position = 0;
                ctx.ReadSymbol(SyslogChars.LT);
                var digits = ctx.ReadDigits(3);
                ctx.ReadSymbol(SyslogChars.GT);
                ctx.Prefix = ctx.Text.Substring(0, ctx.Position);
                return true; 
            }
            catch (Exception)
            {
                ctx.Position = 0; 
                return false; 
            }
        }

        public static void AssignFacilitySeverity(this ParserContext ctx)
        {
            
            var priStr = ctx.Prefix.Replace("<", string.Empty).Replace(">", string.Empty).Replace("?", string.Empty); 
            if (!int.TryParse(priStr, out var pri))
            {
                ctx.AddError($"Invalid priiority value '{priStr}', expected '<?>' where ? is int.");
                return;
            }

            // parse priority -> facility + severity
            var intFacility = pri / 8;
            var intSeverity = pri % 8;
            ctx.Entry.Facility = (Facility)intFacility;
            ctx.Entry.Severity = (Severity)intSeverity;
        }

        public static bool Reset(this ParserContext ctx)
        {
            if (ctx.Prefix == null && !ctx.ReadSyslogPrefix())
            {
                return false; 
            }
            ctx.Position = ctx.Prefix.Length;
            return true; 
        }

        public static bool Match(this ParserContext ctx, string token, 
              StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (ctx.Position + token.Length > ctx.Text.Length)
                return false; 
            var str = ctx.Text.Substring(ctx.Position, token.Length);
            if(str.Equals(token, comparison))
            {
                ctx.Position += token.Length;
                return true;
            }
            return false; 
        }

        public static string GetValue(this IList<NameValuePair> prms, string name)
        {
            return prms.FirstOrDefault(p => p.Name == name)?.Value;
        }

        public static string CutOffBOM(this string msg)
        {
            if (msg.StartsWith(SyslogChars.BOM, StringComparison.Ordinal))
                msg = msg.Substring(SyslogChars.BOM.Length);
            return msg; 
        }

    } //class
}
