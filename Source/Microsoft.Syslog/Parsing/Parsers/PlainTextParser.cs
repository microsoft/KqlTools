// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using System;

    public class PlainTextParser : ISyslogMessageParser
    {
        public bool TryParse(ParserContext ctx)
        {
            ctx.SkipSpaces();
            ctx.Entry.PayloadType = Model.PayloadType.PlainText; 
            ctx.Entry.Message = ctx.Text.Substring(ctx.Position);
            ctx.Entry.Header.Timestamp = DateTime.UtcNow; 
            return true; 
        }
    }
}
