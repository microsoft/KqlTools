using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RealTimeKqlLibrary
{
    public class CsvFileReader : EventComponent
    {
        private readonly string _fileName;
        private Observable<IDictionary<string, object>> _eventStream;

        private Thread _thread;
        private bool _running = false;

        private List<string> _headers;
        private IDictionary<string, object> _temp;
        private int _cellNum;
        private int _rowPos;

        public CsvFileReader(string fileName, IOutput output, params string[] queries)
            : base(output, queries)
        {
            _fileName = fileName;
            _eventStream = new Observable<IDictionary<string, object>>();
        }

        public override bool Start()
        {
            if (!File.Exists(_fileName))
            {
                Console.WriteLine($"ERROR! {_fileName} does not seem to exist.");
                return false;
            }

            // Setting up rest of pipeline
            var eventStreamName = _fileName.Split('.');
            if (!Start(_eventStream, eventStreamName[0], true)) return false;

            // Starting reader loop
            _running = true;
            _thread = new Thread(RunReaderLoop)
            {
                Priority = ThreadPriority.Highest
            };
            _thread.Start();

            return true;
        }

        private void RunReaderLoop()
        {
            StreamReader csvReader = new StreamReader(
                new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            // Pulling headers from first row of file
            _headers = (csvReader.ReadLine()).Split(',').ToList();

            // Reading file line by line
            string row;
            while (_running && (row = csvReader.ReadLine()) != null)
            {
                ParseRow(row);
            }
        }

        private void ParseRow(string row)
        {
            _temp = new Dictionary<string, object>();
            _cellNum = 0;
            _rowPos = 0;

            while (_rowPos < row.Length)
            {
                if (row[_rowPos] != '"')
                {
                    ParseSimpleCell(row);
                }
                else
                {
                    ParseComplexCell(row);
                }

                _cellNum++;
            }

            _eventStream.Broadcast(_temp);
        }

        private void ParseSimpleCell(string row)
        {
            StringBuilder cell = new StringBuilder();
            while ((_rowPos < row.Length) && (row[_rowPos] != ','))
            {
                cell.Append(row[_rowPos]);
                _rowPos++;
            }
            _rowPos++;
            ParseType(cell.ToString());
        }

        private void ParseComplexCell(string row)
        {
            StringBuilder cell = new StringBuilder();
            int numQuotes = 0;
            int consecutiveQuotes = 0;
            string header = _headers[_cellNum];

            while (_rowPos < row.Length)
            {
                if ((consecutiveQuotes > 1) && (consecutiveQuotes % 2 == 0))
                {
                    cell.Append('"');
                }

                switch (row[_rowPos])
                {
                    case '"':
                        numQuotes++;
                        consecutiveQuotes++;
                        break;
                    case ',':
                        consecutiveQuotes = 0; // reset consecutive quotes
                        if (numQuotes % 2 == 0)
                        {
                            // reached end of cell
                            _temp.Add(header, cell.ToString());
                            _rowPos++;
                            return;
                        }
                        else
                        {
                            cell.Append(row[_rowPos]);
                        }
                        break;
                    default:
                        consecutiveQuotes = 0;
                        cell.Append(row[_rowPos]);
                        break;

                }

                _rowPos++;
            }

            _temp.Add(header, cell.ToString());
        }

        private void ParseType(string cell)
        {
            string header = _headers[_cellNum];

            if (DateTime.TryParse(cell, out var tempDate))
            {
                _temp.Add(header, tempDate);
            }
            else if (int.TryParse(cell, out var tempInt))
            {
                _temp.Add(header, tempInt);
            }
            else
            {
                _temp.Add(header, cell);
            }
        }
    }
}
