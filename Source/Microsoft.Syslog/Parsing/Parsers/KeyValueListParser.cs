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

    /// <summary>Parses list of key-value pairs; like Sophos firewal format: 
    /// https://docs.sophos.com/nsg/sophos-firewall/v16058/Help/en-us/webhelp/onlinehelp/index.html#page/onlinehelp/WAFLogs.html
    /// </summary>
    class KeyValueListParser : ISyslogMessageParser
    {
        public bool TryParse(ParserContext ctx)
        {
            if (ctx.Current == SyslogChars.Space)
                ctx.SkipSpaces(); 

            // typically entries start with 'device=' or 'date='
            var match = ctx.Match("device=") || ctx.Match("date=");
            if (!match)
            {
                match = TryMatchAnyKey(ctx); 
            }
            if (!match)
                return false; 

            // It is the format for this parser
            ctx.Reset(); // Match(...) moved the position, so return to the start
            ctx.Entry.PayloadType = PayloadType.KeyValuePairs;
            var kvList = ReadKeyValuePairs(ctx);
            ctx.Entry.ExtractedData.AddRange(kvList); 
            // try some known values and put them in the header
            var hdr = ctx.Entry.Header;
            hdr.HostName = kvList.GetValue("device_id");
            var date = kvList.GetValue("date");
            var time = kvList.GetValue("time");
            if (date != null)
            {
                var dateTimeStr = $"{date}T{time}";
                if (DateTime.TryParse(dateTimeStr, out var dt ))
                {
                    hdr.Timestamp = dt; 
                }
            }

            return true; 
        } //method

        private List<NameValuePair> ReadKeyValuePairs(ParserContext ctx)
        {
            var prmList = new List<NameValuePair>();
            NameValuePair lastPrm = null;
            /*
             2 troubles here: 
             */
            while (!ctx.Eof())
            {
                ctx.SkipSpaces();
                var name = ctx.ReadWord();
                if(!ctx.ReadSymbol('=', throwIfMismatch: false))
                {
                    // Some entries are malformed: double quoted strings  
                    // the result is that we do not find '=' after closing the quote. So we just add the rest to a separate param and exit
                    var text = ctx.Text.Substring(ctx.Position);
                    prmList.Add(new NameValuePair() { Name = "Message", Value = text });
                    return prmList;                     
                }
                ctx.SkipSpaces();
                string value;
                if (ctx.Current == SyslogChars.DQuote)
                {
                    // For double-quoted values, some values are malformed - they contain nested d-quoted strings that are not escaped.
                    value = ctx.ReadQuotedString();
                }
                else
                {
                    // Special case: non quoted empty values, ex: ' a= b=234 '; value of 'a' is Empty. We check the char after we read the value, 
                    //      and if it is '=', we back off, set value to empty. 
                    var saveP = ctx.Position; 
                    value = ctx.ReadWord();
                    if (ctx.Current == '=')
                    {
                        ctx.Position = saveP;
                        value = string.Empty; 
                    }
                }
                lastPrm = new NameValuePair() { Name = name, Value = value };
                prmList.Add(lastPrm);
            }
            return prmList; 
        }

        // let try to match any key, like <120> abc = def
        private bool TryMatchAnyKey(ParserContext ctx)
        {
            if (!char.IsLetter(ctx.Current))
                return false; 
            var savePos = ctx.Position;
            var word = ctx.ReadWord();
            ctx.SkipSpaces();
            var result = ctx.Match("=");
            ctx.Position = savePos;
            return result; 

        }


    }
}
