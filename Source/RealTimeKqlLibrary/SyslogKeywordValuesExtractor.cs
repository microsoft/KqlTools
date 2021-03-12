// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using Microsoft.Syslog.Model;
using Microsoft.Syslog.Parsing;
using System.Collections.Generic;

namespace RealTimeKqlLibrary
{
    /// <summary>Extracts selected keywords and following values from a text message.</summary>
    /// <remarks>Extracts the following keywords: Command, Port, User, Status, Error</remarks>
    public class SyslogKeywordValuesExtractor : KeywordValuesExtractorBase
    {
        public override IList<NameValuePair> ExtractValues(ParserContext ctx)
        {
            var prmList = new List<NameValuePair>();
            var msg = ctx.Entry.Message;
            if (string.IsNullOrWhiteSpace(msg))
                return prmList;
            TryExtractAndAdd(msg, "Command", prmList);
            TryExtractAndAdd(msg, "Status", prmList);
            TryExtractAndAdd(msg, "User", prmList);
            TryExtractAndAdd(msg, "Error", prmList, grabAll: true);
            TryExtractAndAdd(msg, "Port", prmList);
            return prmList;
        }
    }
}