// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    /// <summary>General interface for a parser for specific variant/version of syslog. </summary>
    /// <remarks>
    ///     The top SyslogParser calls all registered variant parsers asking to parse a message. 
    ///     If a variant parser recognizes its version and can parse it, it should do it and return true.
    /// </remarks>
    public interface ISyslogMessageParser
    {
        bool TryParse(ParserContext context);
    }
}
