using Microsoft.Syslog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Syslog.Internals;

namespace RealTimeKql
{
    class LogFileListener: Observable<string>, IDisposable
    {
        public FileStream LogFileReader { get; private set; }
        private bool DisposeClientOnDispose;
        public event EventHandler<Exception> Error;

        private Thread _thread;
        private bool _running;

        public LogFileListener(FileStream logFileReader)
        {
            LogFileReader = logFileReader;
            DisposeClientOnDispose = false;
        }

        public void Start()
        {
            if(_running)
            {
                // already started
                return;
            }
            _running = true;
            _thread = new Thread(RunListenerLoop);
            _thread.Priority = ThreadPriority.Highest;
            _thread.Start();
        }

        public void Stop()
        {
            if(_running)
            {
                // TODO: add clean-up here
            }
            _running = false;
            Thread.Sleep(10);
        }

        private void RunListenerLoop()
        {
            try
            {
                StreamReader syslogReader = new StreamReader(LogFileReader);
                long lastLength = syslogReader.BaseStream.Length;

                while (true)
                {
                    Thread.Sleep(20);

                    // if file size has not changed, idle
                    if (syslogReader.BaseStream.Length == lastLength)
                        continue;

                    // seek to last length
                    syslogReader.BaseStream.Seek(lastLength, SeekOrigin.Begin);

                    // broadcast new entries
                    string entry = "";
                    while ((entry = syslogReader.ReadLine()) != null)
                    {
                        if(entry != "")
                        {
                            Broadcast(entry);
                        }
                    }
                        
                    //update last length
                    lastLength = syslogReader.BaseStream.Position;
                }
            }
            catch (Exception ex)
            {
                if (!_running)
                    return;
                OnError(ex);
            }
        }

        private void OnError(Exception error)
        {
            Error?.Invoke(this, error);
        }

        public void Dispose()
        {
            if (DisposeClientOnDispose && LogFileReader != null)
            {
                LogFileReader.Dispose();
            }
        }
    }
}
