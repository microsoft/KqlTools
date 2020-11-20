using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCsvReader
{
    class CsvEntryParser : Observable<IDictionary<string, object>>
    {
        private List<String> _headers;
        private Dictionary<string, object> _temp;
        private int _cellNum;
        private int _rowPos;

        public CsvEntryParser(List<string> headers)
        {
            _headers = headers;
        }

        public void ParseRow(string row)
        {
            _temp = new Dictionary<string, object>();
            _cellNum = 0;
            _rowPos = 0;

            while(_rowPos < row.Length)
            {
                if(row[_rowPos] != '"')
                {
                    ParseSimpleCell(row);
                }
                else
                {
                    ParseComplexCell(row);
                }

                _cellNum++;
            }

            Broadcast(_temp);
        }

        private void ParseSimpleCell(string row)
        {
            StringBuilder cell = new StringBuilder();
            while(row[_rowPos] != ',')
            {
                cell.Append(row[_rowPos]);
                _rowPos++;
            }
            _rowPos++;
            ParseType(cell.ToString());
        }

        private void ParseType(string cell)
        {
            string header = _headers[_cellNum];
            DateTime tempDate;
            int tempInt;

            if (DateTime.TryParse(cell, out tempDate))
            {
                _temp.Add(header, tempDate);
            }
            else if(int.TryParse(cell, out tempInt))
            {
                _temp.Add(header, tempInt);
            }
            else
            {
                _temp.Add(header, cell);
            }
        }

        private void ParseComplexCell(string row)
        {
            StringBuilder cell = new StringBuilder();
            int numQuotes = 0;
            int consecutiveQuotes = 0;
            string header = _headers[_cellNum];

            while(_rowPos < row.Length)
            {
                if ((consecutiveQuotes > 1) && (consecutiveQuotes % 2 == 0))
                {
                    cell.Append('"');
                }

                switch(row[_rowPos])
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
    }
}
