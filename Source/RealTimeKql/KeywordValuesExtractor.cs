// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using Microsoft.Syslog.Model;
    using System.Collections.Generic;

    /// <summary>Extracts selected keywords and following values from a text message.</summary>
    /// <remarks>Extracts the following keywords: Command, Port, User, Status, Error</remarks>
    public class KeywordValuesExtractor : KeywordValuesExtractorBase
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
