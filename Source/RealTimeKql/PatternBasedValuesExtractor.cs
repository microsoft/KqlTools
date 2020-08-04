// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using Microsoft.Syslog.Model;
    using System.Collections.Generic;

    // Extracts specific patterns: 
    //    Category, ex: "%L2-MKA-5-SESSION_SECURED:"
    //    Proc, ex: abcd[1234]

    public class PatternBasedValuesExtractor : IValuesExtractor
    {
        public IList<NameValuePair> ExtractValues(ParserContext ctx)
        {
            var entry = ctx.Entry;
            var result = new List<NameValuePair>(); 
            var category = CheckCategoryInHostName(entry) ?? ExtractCategory(entry.Message);
            if (!string.IsNullOrEmpty(category))
            {
                result.Add("Category", category);
            }

            var proc = ExtractProc(entry.Message);
            var procId = entry.Header.ProcId;
            if (proc == null && procId != null && LooksLikeProc(procId))
                proc = procId; //it might end there already            
            if (!string.IsNullOrEmpty(proc))
            {
                result.Add("Proc", proc);
            }

            return result; 
        }

        private bool LooksLikeProc(string s)
        {
            return !string.IsNullOrEmpty(s) && s.EndsWith("]") && char.IsLetter(s[0]);
        }

        // hack: category might be confused for HostName, it might end there
        private string CheckCategoryInHostName(SyslogEntry entry)
        {
            var hostName = entry.Header.HostName;
            if (hostName != null && hostName.StartsWith("%"))
                return hostName;
            return null; 
        }

        // pattern: % followed by letter
        private string ExtractCategory(string message)
        {
            var percPos = message.StartsWith("%")? 0 : message.IndexOf(" %");
            if (percPos < 0 || percPos >= message.Length - 4 || !char.IsLetter(message[percPos + 2]))
                return null;
            if (message[percPos] == ' ')
                percPos++; //skip space
            var colonPos = message.IndexOf(':', percPos);
            if (colonPos < 0)
                colonPos = message.Length;
            var cat = message.Substring(percPos, colonPos - percPos).Trim(' ', ':');
            return cat; 
        }

        private string ExtractProc(string message)
        {
            var start = 0;
            while(start < message.Length - 5)
            {
                var brPos = message.IndexOf("[", start);
                if (brPos < 2)
                    return null; 
                // must be preceeded by letter and followed by digit
                if (brPos >= message.Length - 4 ||
                      !IsProcNameChar(message[brPos - 1]) || !char.IsDigit(message[brPos + 1]))
                {
                    start = brPos + 1;
                    continue; // look for next one
                }
                var endPos = message.IndexOf(']', brPos);
                if (endPos < 0)
                    return null;
                // find start pos, walkback until space
                var startPos = brPos;
                while (startPos > 0 && IsProcNameChar(message[startPos - 1]))
                    startPos--;
                var proc = message.Substring(startPos, endPos - startPos + 1).Trim(' ', ':');
                return proc;
            }
            return null;
        }

        private bool IsProcNameChar(char ch)
        {
            return char.IsLetterOrDigit(ch) || ch == '_' || ch == '.';
        }
    }
}
