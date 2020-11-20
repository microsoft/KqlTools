using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleCsvReader
{
    public class SimpleCsvParser : Observable<IDictionary<string, object>>, IObserver<IDictionary<string, object>>
    {
        public string CsvFileName { get; set; }
        private bool _running = false;

        public SimpleCsvParser(string csvFileName)
        {
            CsvFileName = csvFileName;
        }

        public void Start()
        {
            _running = true;
            Read();
        }

        public void Stop()
        {
            _running = false;
        }

        private void Read()
        {
            if (!_running) return;

            StreamReader csvReader = new StreamReader(new FileStream(CsvFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            string [] headers = (csvReader.ReadLine()).Split(',');
            CsvEntryParser simpleCsvParser = new CsvEntryParser(headers.ToList());
            simpleCsvParser.Subscribe(this);

            string row;
            while ((row = csvReader.ReadLine()) != null)
            {
                simpleCsvParser.ParseRow(row);
            }

            OnCompleted();
        }

        public void OnNext(IDictionary<string, object> value)
        {
            Broadcast(value);
        }

        public void OnError(Exception error)
        {
            Console.WriteLine(error.ToString());
        }

        public void OnCompleted()
        {
            _running = false;

            Console.WriteLine("Completed!");
        }

    }
}
