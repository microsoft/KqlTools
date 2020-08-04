// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    /// <summary>Parses old-style, BSD syslog, RFC-3164. See https://tools.ietf.org/html/rfc3164 </summary>
    public class Rfc3164SyslogParser : ISyslogMessageParser
    {

        public bool TryParse(ParserContext ctx)
        {
            if (!TimestampParseHelper.TryParseTimestamp(ctx))
                return false;

            var entry = ctx.Entry;
            entry.PayloadType = Model.PayloadType.Rfc3164; 
            //Next - host name and proc name
            entry.Header.HostName = ctx.ReadWord();
            ctx.SkipSpaces();
            var procStart = ctx.Position; 
            var procEnd = ctx.Text.SkipUntil(procStart + 1, ' ', ':', ',');
            if (procEnd < ctx.Text.Length)
            {
                var proc = ctx.Text.Substring(procStart, procEnd - procStart);
                entry.Header.ProcId = proc;
                ctx.Position = procEnd + 1;
            }
            if (ctx.Position < ctx.Text.Length)
            {
                // the rest is message            
                ctx.SkipSpaces();
                entry.Message = ctx.Text.Substring(ctx.Position);
            }
            return true; 
        }

    }
}
