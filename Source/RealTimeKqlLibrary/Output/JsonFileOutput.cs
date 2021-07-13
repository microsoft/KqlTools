using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Kql.CustomTypes;
using System.Threading;

namespace RealTimeKqlLibrary
{
    public class JsonFileOutput: IOutput
    {
        public AutoResetEvent Completed { get; private set; }
        private StreamWriter _outputWriter;
        private bool _firstEntry = true;
        private bool _running = false;
        private bool _error = false;
        private int numEntries = 0;
        private readonly BaseLogger _logger;

        public JsonFileOutput(BaseLogger logger, string fileName)
        {
            _logger = logger;
            Completed = new AutoResetEvent(false);
            _outputWriter = new StreamWriter(fileName);
            _outputWriter.Write($"[");
            _running = true;
        }

        public void KqlOutputAction(KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            if (_error || !_running) return;

            try
            {
                string content;
                if (_firstEntry)
                {
                    _firstEntry = false;
                    content = $"{JsonConvert.SerializeObject(obj)}";
                }
                else
                {
                    content = $",{JsonConvert.SerializeObject(obj)}";
                }

                _outputWriter.Write(content);
                _outputWriter.Flush();
                PrettyPrintEntryCount();
            }
            catch(Exception ex)
            {
                OutputError(ex);
            }
        }

        public void OutputError(Exception ex)
        {
            _error = true;
            _running = false;
            _logger.Log(LogLevel.ERROR, ex);
        }

        public void OutputCompleted()
        {
            _running = false;
            _logger.Log(LogLevel.INFORMATION, "\nStopping RealTimeKql...");

            if (!_error)
            {
                _outputWriter.Write("]");
                _outputWriter.Dispose();
                _outputWriter = null;
            }

            Completed.Set();
        }

        public void Stop()
        {
            Completed.WaitOne();
            _logger.Log(LogLevel.INFORMATION, $"\nCompleted!\nThank you for using RealTimeKql!");
        }

        private void PrettyPrintEntryCount()
        {
            numEntries++;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Wrote entry # {numEntries}");
        }
    }
}
