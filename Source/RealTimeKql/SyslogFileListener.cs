using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Syslog;
using Microsoft.Syslog.Internals;
using Microsoft.Syslog.Model;
using Microsoft.Syslog.Parsing;

namespace RealTimeKql
{
    class SyslogFileListener: Observable<ServerSyslogEntry>, IDisposable
    {
        public event EventHandler<ServerSyslogEntry> EntryReceived;
        public int QueuedMessagesCount => _logFileBuffer.Count;
        public int ActiveParserLoopCount => _activeParserLoopCount;
        public int MessageBatchSize = 100; // changed this down from 500, better be lower to let threads switch 
        public event EventHandler<Exception> Error;
        public long LogfileEntryCount => _logFileEntryCount;
        public long SyslogMessageCount => _syslogMessageCount;
        public long IgnoredMessageCount => _skippedMessageCount;
        public Func<ServerSyslogEntry, bool> Filter;

        private readonly LogFileListener _logFileListener;
        private readonly BatchingQueue<string> _logFileBuffer;
        private readonly int _parseProcessCount;
        private readonly SyslogParser _parser;
        private bool _running;
        private long _logFileEntryCount;
        private long _syslogMessageCount;
        private long _skippedMessageCount;
        private int _activeParserLoopCount;
        private object _eventLock = new object();

        // constructor for reading syslog from local file
        public SyslogFileListener(SyslogParser parser, FileStream fileStream, int parseProcessCount = 4)
        {
            _parser = parser;
            _parseProcessCount = parseProcessCount;
            _logFileListener = new LogFileListener(fileStream);
            _logFileListener.Error += LogfileListener_Error;
            _logFileBuffer = new BatchingQueue<string>();
            // hook buffer to LogfileListener output
            _logFileListener.Subscribe(_logFileBuffer);
        }

        public void Start()
        {
            if (_running)
            {
                return;
            }
            // start n parallel parser threads; note - we need real threads here, not pool threads
            _running = true;
            for (int i = 0; i < _parseProcessCount; i++)
            {
                var thread = new Thread(RunParserLoop);
                thread.Start();
            }
            _logFileListener.Start();
        }

        public void Stop()
        {
            _running = false;
            _logFileListener.Stop();
        }

        public bool IsIdle => QueuedMessagesCount == 0 && ActiveParserLoopCount == 0;

        /* Note about encoding.
        The RFC-5424 states that most of the syslog message should be encoded as plain ASCII string
        - except values of parameters in StructuredData section; these are allowed to be in Unicode/UTF-8. 
        Since 
           any valid ASCII text is valid UTF-8 text  
        ... we use UTF-8 for reading the payload, so we can read correctly the entire mix. 
        So in this case the recommendation in the RFC doc is primarily for writers (log producers) 
        to stay with ASCII most of the time, with occasional values in UTF-8. 
        We on the other hand, as reader, are 'forgiving', reading the entire text as UTF-8 message. 
        
        About BOM: the RFC doc states that [Message] portion of the payload (the tail part) can start with  
        the BOM - byte order mark - to indicate Unicode content. 
        We strip the BOM off when we parse the payload, as it brings troubles if left in the string
        - just from past experience, it is invisible, debugger does not show it, but it can break some string
        operations.
         */

        // log file version of RunParserLoop
        private void RunParserLoop(object data)
        {
            while (_running)
            {
                if (_logFileBuffer.Count == 0)
                {
                    Thread.Sleep(20);
                }
                try
                {
                    Interlocked.Increment(ref _activeParserLoopCount);
                    var logfileEntries = _logFileBuffer.DequeueMany(MessageBatchSize);
                    foreach (var entry in logfileEntries)
                    {
                        Interlocked.Increment(ref _logFileEntryCount);
                        var text = TransformToRFC(entry);
                        var ctx = new ParserContext(text);

                        // If tryParse returns false, it means it is not syslog at all
                        if (!(entry == "") && _parser.TryParse(ctx))
                        {
                            Interlocked.Increment(ref _syslogMessageCount);
                            var serverEntry = new ServerSyslogEntry()
                            {
                                Payload = entry,
                                Entry = ctx.Entry,
                                ParseErrorMessages = ctx.ErrorMessages
                            };
                            OnEntryReceived(serverEntry);
                            // run filter
                            if (Filter != null && !Filter(serverEntry))
                            {
                                Interlocked.Increment(ref _skippedMessageCount);
                                serverEntry.Ignore = true;
                            }
                            else
                            {
                                Broadcast(serverEntry);
                            }
                        }
                        if (_syslogMessageCount % 10 == 0)
                        {
                            Thread.Yield(); // play nice - yield CPU regularly; note - this is very CPU-heavy loop, no IO
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!_running)
                        return;
                    OnError(ex);
                }
                finally
                {
                    Interlocked.Decrement(ref _activeParserLoopCount);
                }
            }
        } //method

        private string TransformToRFC(string entry)
        {
            // hard-coded priority and version for now
            string pri = "<99>";
            string res = $"{pri} {entry}";
            return res;
        }

        // We use _eventLock to serialize firing events, to help prevent thread-safety errors in listeners
        private void OnEntryReceived(ServerSyslogEntry serverEntry)
        {
            if (EntryReceived != null)
                lock (_eventLock)
                    EntryReceived.Invoke(this, serverEntry);
        }

        private void OnError(Exception error)
        {
            if (Error != null)
                lock (_eventLock)
                    Error.Invoke(this, error);
        }

        private void LogfileListener_Error(object sender, Exception e)
        {
            if (Error != null)
                lock (_eventLock)
                    Error.Invoke(this, e);
        }

        public void Dispose()
        {
            _logFileListener.Dispose();
        }
    }
}
