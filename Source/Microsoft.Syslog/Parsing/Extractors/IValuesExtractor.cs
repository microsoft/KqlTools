// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using Microsoft.Syslog.Model;
    using System.Collections.Generic;

    /// <summary>
    ///     A value extractor is a component for extracting values from free-form message part of syslog, for ex: IP addresses. 
    ///     The extracted values are placed into the StructuredData dictionary of the syslog entry. 
    /// </summary>
    public interface IValuesExtractor
    {
        IList<NameValuePair> ExtractValues(ParserContext context);
    }
}
