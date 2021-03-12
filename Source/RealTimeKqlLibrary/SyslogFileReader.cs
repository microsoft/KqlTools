using Microsoft.Syslog.Model;
using Microsoft.Syslog.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace RealTimeKqlLibrary
{
    public class SyslogFileReader: EventComponent
    {
        private readonly string _fileName;
        private Observable<IDictionary<string, object>> _eventStream;

        // Syslog parsing
        private Thread _thread;
        private readonly SyslogParser _syslogParser;
        private int _syslogEntryCount;
        private readonly int _defaultPriority = 134;

        public SyslogFileReader(string fileName, IOutput output, params string[] queries) : base(output, queries)
        {
            _fileName = fileName;
            _eventStream = new Observable<IDictionary<string, object>>();

            // Setting up syslog parser
            _syslogParser = SyslogParser.CreateDefault();
            _syslogParser.AddValueExtractors(new SyslogKeywordValuesExtractor(), new SyslogPatternBasedValuesExtractor());
            _syslogEntryCount = 0;
        }

        public override bool Start()
        {
            if(!File.Exists(_fileName))
            {
                Console.WriteLine($"ERROR! {_fileName} does not seem to exist.");
                return false;
            }

            // Setting up rest of pipeline
            var eventStreamName = _fileName.Split('.');
            if (!Start(_eventStream, eventStreamName[0], true)) return false;

            // Starting file listener loop
            _thread = new Thread(RunListenerLoop)
            {
                Priority = ThreadPriority.Highest
            };
            _thread.Start();

            return true;
        }

        private void RunListenerLoop()
        {
            try
            {
                FileStream fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader syslogReader = new StreamReader(fs);
                long lastLength = syslogReader.BaseStream.Length;

                while (true)
                {
                    Thread.Sleep(20);

                    // if file size has not changed, idle
                    if (syslogReader.BaseStream.Length == lastLength)
                        continue;

                    // seek to last length
                    syslogReader.BaseStream.Seek(lastLength, SeekOrigin.Begin);

                    // retrieve new entries
                    string entry = "";
                    while ((entry = syslogReader.ReadLine()) != null)
                    {
                        if (entry != "")
                        {
                            Interlocked.Increment(ref _syslogEntryCount);
                            Parse(entry);
                        }
                    }

                    // update last length
                    lastLength = syslogReader.BaseStream.Position;
                }
            }
            catch (Exception ex)
            {
                _eventStream.BroadcastError(ex);
            }
        }

        private void Parse(string entry)
        {
            // giving CPU a break every so often
            if (_syslogEntryCount % 10 == 0)
            {
                _syslogEntryCount = 0;
                Thread.Yield();
            }

            // Transform to RFC
            var text = $"<{_defaultPriority}> {entry}";

            // Begin parsing using Microsoft.Syslog
            var ctx = new ParserContext(text);
            if (_syslogParser.TryParse(ctx))
            {
                var serverEntry = new ServerSyslogEntry()
                {
                    Payload = entry,
                    Entry = ctx.Entry,
                    ParseErrorMessages = ctx.ErrorMessages
                };

                var dict = SyslogEntryToDictionaryConverter.Convert(serverEntry);
                _eventStream.Broadcast(dict);
            }
        }
    }
}
