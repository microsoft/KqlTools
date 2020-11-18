using Kusto.Ingest.Exceptions;
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

        public FileOutput(string outputFileName)
        {
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
            if (error != true)
            {
                running = false;
                outputFile.Write($"{Environment.NewLine}]{Environment.NewLine}");
                outputFile.Dispose();
                outputFile = null;

                Console.WriteLine("Completed!");
            }
        }
    }
}
