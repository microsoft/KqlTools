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
        private readonly int _padding;
        private int _counter;

        public ConsoleTableOutput()
        {
            _running = true;
            _firstEntry = true;
            _columnWidths = new Dictionary<string, int>();
            _totalWidth = 0;
            _padding = 4;
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
        }

        public void OutputError(Exception ex)
        {
            _running = false;
            Console.WriteLine(ex);
        }

        public void OutputCompleted()
        {
            _running = false;
            Console.WriteLine("\nCompleted!");
            Console.WriteLine("Thank you for using RealTimeKql!");
        }

        public void Stop()
        {
            System.Environment.Exit(0);
        }

        private void CalculateColumnWidths(IDictionary<string, object> obj)
        {
            foreach (var pair in obj)
            {
                var key = pair.Key;
                var valueAsString = pair.Value == null ? "null" : pair.Value.ToString();
                var biggerLen = key.Length > valueAsString.Length ? key.Length : valueAsString.Length;
                _columnWidths[key] = biggerLen;
                _totalWidth += biggerLen + _padding;
            }
        }

        private void PrintHeaders(IDictionary<string, object> obj)
        {
            Console.WriteLine($"{"".PadRight(_totalWidth, '-')}");
            foreach (var key in obj.Keys)
            {
                var len = GetLength(key);
                Console.Write($"{key.PadRight(len + _padding)}");
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
                Console.Write($"{valueAsString.Truncate(len).PadRight(len + _padding)}");
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

