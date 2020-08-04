// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog
{
    using Microsoft.Syslog.Model;
    using Microsoft.Syslog.Parsing;
    using System.Collections.Generic;
    using System.Text;

    public static class SyslogSerializer
    {
        private const char Space = ' ';
        private const char NilChar = '-';
        private const char Lbr = '[';
        private const char Rbr = ']';
        private const char Escape = '\\';
        private const char DQuote = '"';

        static char[] _charsToEscape = new char[] { DQuote, Escape, Rbr }; //according to RFC-5424 these must be escaped

        public static string TimestampFormat = "yyyy'-'MM'-'ddTHH:mm:ss.fffZ";

        public static string Serialize(SyslogEntry entry)
        {
            var writer = new StringBuilder();
            writer.WriteHeader(entry);
            writer.WriteStructuredData(entry.StructuredData);
            if (!string.IsNullOrEmpty(entry.Message))
            {
                writer.Append(SyslogChars.Space);
                writer.Append(entry.Message);
            }
            return writer.ToString(); 
        }

        private static void WriteHeader(this StringBuilder writer, SyslogEntry entry)
        {
            writer.Append('<');
            var pri = GetPriority(entry);
            writer.Append(pri);
            writer.Append('>');
            writer.Append('1');
            writer.Append(Space);
            var header = entry.Header; 
            var tsStr = header.Timestamp == null ? string.Empty : header.Timestamp.Value.ToString(TimestampFormat);
            writer.AppendWordOrNil(tsStr);
            writer.AppendWordOrNil(header.HostName);
            writer.AppendWordOrNil(header.AppName);
            writer.AppendWordOrNil(header.ProcId);
            writer.AppendWordOrNil(header.MsgId);
        }

        private static void AppendWordOrNil(this StringBuilder writer, string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                writer.Append(NilChar);
            }
            else
            {
                writer.Append(word);
            }
            writer.Append(Space);
        }

        private static int GetPriority(this SyslogEntry entry)
        {
            return ((int)entry.Facility) * 8 + (int)entry.Severity;
        }

        private static void WriteStructuredData(this StringBuilder writer, IDictionary<string, IList<NameValuePair>> structuredData)
        {
            if (structuredData == null || structuredData.Count == 0)
            {
                writer.Append(NilChar);
                writer.Append(Space);
                return; 
            }

            foreach(var de in structuredData)
            {
                var elemName = de.Key;
                var paramList = de.Value; 
                writer.Append(Lbr);
                writer.Append(elemName);
                foreach(var prm in paramList)
                {
                    writer.Append(Space);
                    writer.Append(prm.Name);
                    writer.Append('=');
                    var prmValue = EscapeParamValue(prm.Value);
                    writer.Append(DQuote);
                    writer.Append(prmValue);
                    writer.Append(DQuote);
                }
                writer.Append(Rbr);
            }
        }

        private static string EscapeParamValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty; 
            if(value.IndexOfAny(_charsToEscape) < 0)
                return value;
            // replace with escaped
            var escaped = value.Replace(@"\", @"\\").Replace(@"""", @"\""").Replace(@"]", @"\]");
            return escaped; 
        }
    }
}
