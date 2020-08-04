// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Syslog.Model;

    public class Rfc5424SyslogParser: ISyslogMessageParser
    {
        public bool TryParse(ParserContext ctx)
        {
            if (!ctx.Reset())
                return false;
            if(!ctx.Match("1 "))
            {
                return false; 
            }

            // It is RFC-5424 entry
            var entry = ctx.Entry; 
            entry.PayloadType = PayloadType.Rfc5424; 
            try
            {
                entry.Header = this.ParseHeader(ctx);
                this.ParseStructuredData(ctx);
                entry.Message = this.ParseMessage(ctx);
                return true; 
            }
            catch (Exception ex)
            {
                ctx.AddError(ex.Message);
                return false; 
            }
        }

        private  SyslogHeader ParseHeader(ParserContext ctx)
        {
            var header = ctx.Entry.Header = new SyslogHeader(); 
            header.Timestamp = ctx.ParseStandardTimestamp();
            header.HostName = ctx.ReadWordOrNil();
            header.AppName = ctx.ReadWordOrNil();
            header.ProcId = ctx.ReadWordOrNil();
            header.MsgId = ctx.ReadWordOrNil();
            return header; 
        }

        private void ParseStructuredData(ParserContext ctx)
        {
            ctx.SkipSpaces();

            if (ctx.Current == SyslogChars.NilChar)
            {
                ctx.Position++;
                return; 
            }

            var data = ctx.Entry.StructuredData;
            try
            {
                if (ctx.Current != SyslogChars.Lbr)
                {
                    // do not report it as an error, some messages out there are a bit malformed
                    // ctx.AddError("Expected [ for structured data.");
                    return; 
                }
                // start parsing elements
                while(!ctx.Eof())
                {
                    var elem = ParseElement(ctx);
                    if (elem == null)
                    {
                        return;
                    }
                    data[elem.Item1] = elem.Item2; 
                }

            } catch (Exception ex)
            {
                ctx.AddError(ex.Message);
            }
        }

        private  Tuple<string, List<NameValuePair>> ParseElement(ParserContext ctx)
        {
            if (ctx.Current != SyslogChars.Lbr)
            {
                return null; 
            }
            ctx.Position++;
            var elemName = ctx.ReadWord();
            ctx.SkipSpaces();
            var paramList = new List<NameValuePair>();
            var elem = new Tuple<string, List<NameValuePair>>(elemName, paramList);
            while (ctx.Current != SyslogChars.Rbr)
            {
                var paramName = ctx.ReadWord();
                ctx.ReadSymbol('=');
                var paramValue = ctx.ReadQuotedString();
                var prm = new NameValuePair() { Name = paramName, Value = paramValue };
                paramList.Add(prm);
                ctx.SkipSpaces();
            }

            ctx.ReadSymbol(SyslogChars.Rbr);
            return elem; 
        }

        private string ParseMessage(ParserContext ctx)
        {
            if (ctx.Eof())
            {
                return null;
            }
            var msg = ctx.Text.Substring(ctx.Position);
            msg = msg.TrimStart(SyslogChars.Space);
            // RFC 5424 allows BOM (byte order mark, 3 byte sequence) to precede the actual message. 
            // it will be read into the message OK, now 'msg' can contain this prefix - it is invisible
            // and will bring a lot of trouble when working with the string (ex: string comparisons are broken)
            // So we remove it explicitly.
            return msg.CutOffBOM();
        }


    } //class

}
