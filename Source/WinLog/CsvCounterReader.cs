// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    ///     Reads OS recording of performance counters from CSV file
    ///     To start the OS counter recording, one can use system tools such as PerfMon or logman.exe
    /// </summary>
    public class CsvCounterReader
    {
        // The OS counter recording puts this prefix as the first token in the csv file
        // The code here only owrks if the pre
        private const string Prefix = "\"(PDH-CSV 4.0)";

        /// <summary>
        ///     Reads the content of one csv file
        /// </summary>
        /// <param name="csvFilePath">Tpath to the csv file</param>
        /// <returns>Collection of counter samples</returns>
        public static IEnumerable<CounterSample> ReadCountersFile(string csvFilePath)
        {
            string content = File.ReadAllText(csvFilePath);
            return ReadCountersContent(content);
        }

        /// <summary>
        ///     Reads the content of one csv, passed as string
        /// </summary>
        /// <param name="csvFileContent">The entire content of a single csv file</param>
        /// <returns>Collection of counter samples</returns>
        public static IEnumerable<CounterSample> ReadCountersContent(string csvFileContent)
        {
            if (!csvFileContent.StartsWith(Prefix))
            {
                throw new Exception("The expected OS prefix is missing from the recording. It should start with '" + Prefix + "'");
            }

            string[] lines = csvFileContent.Split(new char[]
            {
                '\n',
                '\r'
            }, StringSplitOptions.RemoveEmptyEntries);
            ColumnInfo[] columns = ParseColumns(lines[0]);

            for (int row = 1; row < lines.Length; row++)
            {
                string[] values = lines[row].Split(',');
                DateTime timeStamp = DateTime.Parse(values[0].Trim('\"'));
                string lastInstance = null;

                CounterSample sample = null;

                for (int col = 0; col < columns.Length; col++)
                {
                    long value = long.Parse(values[col + 1].Trim('\"'));
                    var info = columns[col];

                    // This code relies on the OS behavior to trace all the counters for given instance, 
                    // and then move to the next instance
                    if (lastInstance != info.Instance)
                    {
                        if (sample != null)
                        {
                            yield return PrepareJson(sample);
                        }

                        lastInstance = info.Instance;

                        sample = new CounterSample
                        {
                            Timestamp = timeStamp,
                            Machine = info.Machine,
                            Instance = info.Instance,
                            Counters = new ExpandoObject()
                        };
                    }

                    ((IDictionary<string, object>) sample.Counters).Add(info.CounterName, value);
                }

                if (sample != null)
                {
                    yield return PrepareJson(sample);
                }
            }
        }

        /// <summary>
        ///     To cause Kusto to parse data into "dynamic", we have to pass the JSON
        ///     into property of C# type "dynamic".
        ///     This method replaces a dictionary we have build so far with JSON
        /// </summary>
        /// <param name="sample">The smaple to "fix"</param>
        /// <returns>the fixed sample</returns>
        private static CounterSample PrepareJson(CounterSample sample)
        {
            string json = JsonConvert.SerializeObject(sample.Counters, Formatting.Indented);
            sample.Counters = json;
            return sample;
        }

        /// <summary>
        ///     Parses the column information
        /// </summary>
        /// <param name="firstLine">The entire first line from the csv content</param>
        /// <returns>Array on info structures for the columns</returns>
        private static ColumnInfo[] ParseColumns(string firstLine)
        {
            string[] tokens = firstLine.Split(',');

            var headers = new List<ColumnInfo>();
            for (int col = 1; col < tokens.Length; col++)
            {
                var info = ParseHeader(tokens[col]);
                headers.Add(info);
            }

            ColumnInfo[] columns = headers.ToArray();
            return columns;
        }

        /// <summary>
        ///     Parses one column header such as
        ///     \\GEORGIS1\PMU Counters(3)\Instructions Retired
        /// </summary>
        /// <param name="header">the header string</param>
        /// <returns>Info structure for this column</returns>
        private static ColumnInfo ParseHeader(string header)
        {
            string[] fragments = header.Split(new char[]
            {
                '\\'
            }, StringSplitOptions.RemoveEmptyEntries);
            string[] byParenthesis = fragments[2].Split('(', ')');
            string instance = byParenthesis[1];

            return new ColumnInfo
            {
                Machine = fragments[1].Trim('\"'),
                Instance = instance,
                CounterName = fragments[3].Trim('\"').Replace(" ", string.Empty)
            };
        }

        /// <summary>
        ///     Helper class to keep single column info
        /// </summary>
        public class ColumnInfo
        {
            public string Machine;

            public string Instance;

            public string CounterName;
        }
    }
}