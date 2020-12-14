using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace RealTimeKql
{
    class FileOutput: IObserver<IDictionary<string, object>>
    {
        public string OutputFileName { get; private set; }
        private StreamWriter outputFile;
        private bool firstEntry = true;
        private bool running = false;
        private bool error = false;
        private int numEntries = 0;

        public FileOutput(string outputFileName)
        {
            numEntries = 0;
            running = true;
            OutputFileName = outputFileName;
            outputFile = new StreamWriter(this.OutputFileName);
            outputFile.Write($"[{Environment.NewLine}");
        }

        public void OnNext(IDictionary<string, object> value)
        {
            if(!running)
            {
                return;
            }

            string content;
            if (firstEntry)
            {
                firstEntry = false;
                content = $"{JsonConvert.SerializeObject(value, Formatting.Indented)}";
            }
            else
            {
                content = $",{Environment.NewLine}{JsonConvert.SerializeObject(value, Formatting.Indented)}";
            }

            try
            {
                outputFile.Write(content);
                outputFile.Flush();
                PrettyPrintEntryCount();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void OnError(Exception error)
        {
            this.error = true;
        }

        public void OnCompleted()
        {
            running = false;
            if (error != true)
            {
                outputFile.Write($"{Environment.NewLine}]{Environment.NewLine}");
                outputFile.Dispose();
                outputFile = null;

                Console.WriteLine("\nCompleted!");
            }
        }

        private void PrettyPrintEntryCount()
        {
            numEntries++;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Wrote entry # {numEntries}");
        }
    }
}
