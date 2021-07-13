using System;
using System.Collections.Generic;
using System.Reactive.Kql.CustomTypes;

namespace RealTimeKqlLibrary
{
    public class ConsoleTableOutput : IOutput
    {
        private bool _running;
        private bool _firstEntry;
        private readonly IDictionary<string, int> _columnWidths;
        private int _totalWidth;
        private int _counter;
        private readonly BaseLogger _logger;
        
        public ConsoleTableOutput(BaseLogger logger)
        {
            _logger = logger;
            _running = true;
            _firstEntry = true;
            _columnWidths = new Dictionary<string, int>();
            _totalWidth = 0;
            _counter = 0;
        }

        public void KqlOutputAction(KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            if(_running)
            {
                try
                {
                    if (_firstEntry)
                    {
                        _firstEntry = false;
                        CalculateColumnWidths(obj);
                    }

                    if (_counter % 10 == 0)
                    {
                        _counter = 0;
                        PrintHeaders(obj);
                    }

                    PrintValues(obj);
                    _counter++;
                }
                catch(Exception ex)
                {
                    OutputError(ex);
                }
            }
        }

        public void OutputError(Exception ex)
        {
            _running = false;
            _logger.Log(LogLevel.ERROR, ex);
        }

        public void OutputCompleted()
        {
            _running = false;
            _logger.Log(LogLevel.INFORMATION, "Stopping RealTimeKql...");
        }

        public void Stop()
        {
            _logger.Log(LogLevel.INFORMATION, $"\nCompleted!\nThank you for using RealTimeKql!");
        }

        private void CalculateColumnWidths(IDictionary<string, object> obj)
        {
            foreach (var pair in obj)
            {
                var key = pair.Key;
                var valueAsString = pair.Value == null ? "null" : pair.Value.ToString();
                var biggerLen = key.Length > valueAsString.Length ? key.Length : valueAsString.Length;
                _columnWidths[key] = biggerLen + biggerLen/2;
                _totalWidth += biggerLen + biggerLen/2;
            }
        }

        private void PrintHeaders(IDictionary<string, object> obj)
        {
            Console.WriteLine($"{"".PadRight(_totalWidth, '-')}");
            foreach (var key in obj.Keys)
            {
                var len = GetLength(key);
                Console.Write($"{key.PadRight(len)}");
            }
            Console.WriteLine();
            Console.WriteLine($"{"".PadRight(_totalWidth, '-')}");
        }

        private void PrintValues(IDictionary<string, object> obj)
        {
            foreach (var pair in obj)
            {
                var valueAsString = pair.Value == null ? "null" : pair.Value.ToString();
                if(pair.Value?.GetType() == typeof(Dictionary<string, object>))
                {
                    valueAsString = "Dictionary";
                }
                var len = GetLength(pair.Key);
                Console.Write($"{valueAsString.Truncate(len).PadRight(len)}");
            }
            Console.WriteLine();
        }

        private int GetLength(string key)
        {
            int len;
            if (!_columnWidths.TryGetValue(key, out len))
            {
                len = 10;
            }
            return len;
        }
    }

    public static class StringExtensions
    {
        public static string Truncate(this string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length <= maxLen) return s;
            return s.Substring(0, maxLen);
        }
    }
}

