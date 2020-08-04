// // /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Syslog.Model;

    /// <summary>Extracts selected keywords and following values from a text message.</summary>
    public abstract class KeywordValuesExtractorBase : IValuesExtractor
    {
        public abstract IList<NameValuePair> ExtractValues(ParserContext ctx);

        protected bool TryExtractAndAdd(string message, string keyword, IList<NameValuePair> prms, bool grabAll = false)
        {
            try
            {
                if (TryExtractValue(message, keyword, out var value, grabAll))
                {
                    prms.Add(new NameValuePair() { Name = keyword, Value = value });
                    return true; 
                }
            }
            catch (Exception ex)
            {
                var err = $"Error while extracting keyword {keyword} from message: {ex.Message}";
                prms.Add( new NameValuePair() {Name = "ExtractorError", Value = err });
            }
            return false; 
        }

        protected bool TryExtractValue(string message, string keyword, out string value, bool grabAll = false)
        {
            value = null; 
            var pos = message.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (pos < 0)
                return false;
            // if preceeded by letter or digit - this is partial match, ignore it
            if (pos > 0 && char.IsLetterOrDigit(message[pos-1]))
                return false;
            pos += keyword.Length;
            if (pos >= message.Length - 2)
                return false; 
            // if followed by letter or digit - this is partial match, ignore it
            if (char.IsLetterOrDigit(message[pos]))
                return false; 
            // skip special symbols like =
            var valueStart = pos = message.Skip(pos, ' ', ':', '=');
            int valueEnd = -1;
            var startCh = message[valueStart];
            if (startCh == '"' || startCh == '\'')
            {
                valueStart++; 
                valueEnd = message.SkipUntil(valueStart, startCh);
            }
            else
            {
                valueEnd = grabAll? message.Length :  message.SkipUntil(valueStart, ' ', ',', ';', ']', ')');
            }

            if (valueEnd <= valueStart)
            {
                return false;
            }
            value = message.Substring(valueStart, valueEnd - valueStart);
            return true;
        }
    }
}
