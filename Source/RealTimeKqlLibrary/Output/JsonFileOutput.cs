using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Kql.CustomTypes;

namespace RealTimeKqlLibrary
{
    public class JsonFileOutput: IOutput
    {
        private StreamWriter _outputWriter;
        private bool _firstEntry = true;
        private bool _running = false;
        private bool _error = false;
        private int numEntries = 0;

        public JsonFileOutput(string fileName)
        {
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

            string content;
            if(_firstEntry)
            {
                _firstEntry = false;
                content = $"{JsonConvert.SerializeObject(obj)}";
            }
            else
            {
                content = $",{JsonConvert.SerializeObject(obj)}";
            }

            try
            {
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
            Console.WriteLine(ex.Message);
        }

        public void OutputCompleted()
        {
            if (!_running) return;
            _running = false;

            if (!_error)
            {
                _outputWriter.Write("]");
                _outputWriter.Dispose();
                _outputWriter = null;
            }

            Console.WriteLine("\nCompleted!");
            Console.WriteLine("Thank you for using RealTimeKql!");
        }

        public void Stop()
        {
            System.Environment.Exit(0);
        }

        private void PrettyPrintEntryCount()
        {
            numEntries++;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Wrote entry # {numEntries}");
        }
    }
}
