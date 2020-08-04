// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using Microsoft.Syslog.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///    Configurable syslog message parser. 
    /// </summary>
    /// <remarks>
    ///    Holds configurable lists of version parsers (for handling specific version/format) and 
    ///    list of value extractors for pattern-based extraction.   
    /// </remarks>
    public class SyslogParser
    {
        public IList<ISyslogMessageParser> VersionParsers => _versionParsers;
        public IList<IValuesExtractor> ValueExtractors => _valueExtractors;

        private readonly List<ISyslogMessageParser> _versionParsers = new List<ISyslogMessageParser>();
        private readonly List<IValuesExtractor> _valueExtractors = new List<IValuesExtractor>();

        /// <summary> Creates and configures a default parser, with support for all major syslog versions 
        ///     and IP addresses extractor. 
        /// </summary>
        /// <returns></returns>
        public static SyslogParser CreateDefault()
        {
            var parser = new SyslogParser();
            parser.AddVersionParsers(new Rfc5424SyslogParser(), new KeyValueListParser(), 
                                     new Rfc3164SyslogParser(), new PlainTextParser());
            parser.AddValueExtractors(new IpAddressesExtractor());
            return parser;
        }

        public void AddVersionParsers(params ISyslogMessageParser[] parsers)
        {
            _versionParsers.AddRange(parsers);
        }

        public void AddValueExtractors(params IValuesExtractor[] extractors)
        {
            _valueExtractors.AddRange(extractors); 
        }

        public SyslogEntry Parse(string text)
        {
            var ctx = new ParserContext(text);
            TryParse(ctx);
            ctx.Entry.BuildAllDataDictionary(); // put all parsed/extracted data into AllData dictionary
            return ctx.Entry; 
        }


        public bool TryParse(ParserContext context)
        {
            if (!context.ReadSyslogPrefix())
                return false;
            context.Entry = new SyslogEntry();
            context.AssignFacilitySeverity();

            foreach (var parser in _versionParsers)
            {
                context.Reset(); 
                try
                {
                    if (parser.TryParse(context))
                    {
                        ExtractDataFromMessage(context);
                        return context.ErrorMessages.Count == 0;
                    }
                }
                catch (Exception ex)
                {
                    context.ErrorMessages.Add(ex.ToString());
                    ex.Data["SyslogMessage"] = context.Text;
                    throw; 
                }
            }
            return false; 
        }

        private void ExtractDataFromMessage(ParserContext ctx)
        {
            var entry = ctx.Entry; 
            // For RFC-5424 and KeyValue payload types, everything is already structured and extracted
            // But we want to run IP values detector against all of them
            switch (entry.PayloadType)
            {
                case PayloadType.Rfc5424:
                    var allParams = entry.StructuredData.SelectMany(e => e.Value).ToList();
                    var Ips = IpAddressesExtractor.ExtractIpAddresses(allParams);
                    entry.ExtractedData.AddRange(Ips);
                    return; 

                case PayloadType.KeyValuePairs:
                    var Ips2 = IpAddressesExtractor.ExtractIpAddresses(entry.ExtractedData);
                    entry.ExtractedData.AddRange(Ips2); 
                    return; 
            }

            // otherwise run extractors from plain message
            if (entry == null || string.IsNullOrWhiteSpace(entry.Message) || entry.Message.Length < 10)
                return;

            var allValues = entry.ExtractedData; 
            foreach(var extr in _valueExtractors)
            {
                var values = extr.ExtractValues(ctx); 
                if (values != null && values.Count > 0)
                {
                    allValues.AddRange(values); 
                }
            }
        }
    }
}
