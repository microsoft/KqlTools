// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLogKql
{
    using Kusto.Data;
    using Microsoft.EvtxEventXmlScrubber;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.IO;
    using System.Linq;
    using System.Reactive.Kql;
    using System.Reactive.Kql.EventTypes;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Tx.Windows;
    using EventLevel = System.Diagnostics.Tracing.EventLevel;

    class Program
    {
        static string _outputFileName;
        static string _cluster;
        static string _database;
        static string _tableName;
        static string _filePattern;
        static string _logName;
        static string _wecFile;
        static string _queryFile;
        static bool _demoMode;
        static bool _resetTable;
        static bool _readExisting;
        static readonly TimeSpan UploadTimespan = TimeSpan.FromMilliseconds(5);

        static KustoConnectionStringBuilder kscbIngest;
        static KustoConnectionStringBuilder kscbAdmin;

        static void Main(string[] args)
        {
            ScalarFunctionFactory.AddFunctions(typeof(CustomScalarFunctions));

            ConsoleEventListener eventListener = new ConsoleEventListener();
            eventListener.EnableEvents(RxKqlEventSource.Log, EventLevel.Verbose);

            _resetTable = false;
            _demoMode = false;
            _readExisting = true;

            ParseArgs(args);

            try
            {
                if (_filePattern != null)
                {
                    UploadFiles();
                }
                else if ((_wecFile != null) || (_logName != null))
                {
                    UploadUsingWecFile();
                }
                else
                {
                    ExitWithMissingArgument("file or session");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        static void UploadFiles()
        {
            string[] files;
            if (Path.IsPathRooted(_filePattern))
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(_filePattern));
                string pattern = Path.GetFileName(_filePattern);
                files = Directory.GetFiles(dir, pattern);
            }
            else
            {
                // input
                string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // Get directory and file parts of complete relative pattern
                string pattern = Path.GetFileName(_filePattern);
                string relDir = pattern.Substring(0, _filePattern.Length - pattern.Length);
                // Get absolute path (root+relative)
                string absPath = Path.GetFullPath(Path.Combine(rootDir, relDir));

                // Search files mathing the pattern
                files = Directory.GetFiles(absPath, pattern, SearchOption.TopDirectoryOnly);
            }

            if (files != null && files.Length > 0)
            {
                var etw = EvtxAsDictionaryObservable.FromFiles(files);
                var ku = CreateUploader(etw, UploadTimespan);
                RunUploader(ku, etw);
            }
        }

        static void UploadUsingWecFile()
        {
            IObservable<IDictionary<string, object>> etw;

            if (!string.IsNullOrEmpty(_wecFile))
            {
                if (!File.Exists(_wecFile))
                {
                    Console.WriteLine("Wec File doesnt exist!");
                    return;
                }

                string _wecFileContent = File.ReadAllText(_wecFile);
                etw = EvtxObservable.FromLog(_logName, _wecFileContent, _readExisting, null).Select(x => x.Deserialize());

                Console.WriteLine();
                Console.WriteLine("Listening using WecFile '{0}'. Press Enter to termintate", _wecFile);
            }
            else
            {
                etw = EvtxObservable.FromLog(_logName, null, _readExisting).Select(x => x.Deserialize());

                Console.WriteLine();
                Console.WriteLine("Listening using Log Name '{0}'. Press Enter to termintate", _logName);
            }

            var ku = CreateUploader(etw, UploadTimespan);
            Task task = Task.Factory.StartNew(() =>
            {
                RunUploader(ku, etw);
            });
            string readline = Console.ReadLine();
            ku.OnCompleted();
        }

        static BlockingKustoUploader CreateUploader(IObservable<IDictionary<string, object>> etw, TimeSpan flushDuration)
        {
            var ku = new BlockingKustoUploader(
                 _outputFileName, kscbAdmin, kscbIngest, _demoMode, _tableName, 10000, flushDuration, _resetTable);

            return ku;
        }

        static void RunUploader(BlockingKustoUploader ku, IObservable<IDictionary<string, object>> etw)
        {
            if (_queryFile == null)
            {
                using (etw.Subscribe(ku))
                {
                    ku.Completed.WaitOne();
                }
            }
            else
            {
                KqlNode preProcessor = new KqlNode();
                preProcessor.KqlKqlQueryFailed += PreProcessor_KqlKqlQueryFailed;
                preProcessor.AddCslFile(_queryFile);

                if (preProcessor.FailedKqlQueryList.Count > 0)
                {
                    foreach (var failedDetection in preProcessor.FailedKqlQueryList)
                    {
                        Console.WriteLine($"Message: {failedDetection.Message}");
                    }
                }

                // If we have atleast one valid detection there is a point in waiting otherwise exit
                if (preProcessor.KqlQueryList.Count > 0)
                {
                    var processed = preProcessor.Output.Select(e => e.Output);

                    using (processed.Subscribe(ku))
                    {
                        using (etw.Subscribe(preProcessor))
                        {
                            ku.Completed.WaitOne();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No Queries are running. Press Enter to terminate");
                }
            }
        }

        private static void PreProcessor_KqlKqlQueryFailed(object sender, KqlQueryFailedEventArgs kqlDetectionFailedEventArgs)
        {
            string detectionInfo = JsonConvert.SerializeObject(kqlDetectionFailedEventArgs.Comment);
            Console.WriteLine(detectionInfo);
        }

        static void ParseArgs(string[] args)
        {
            foreach (var a in args)
            {
                int index = a.IndexOf(":");
                if (index < 0)
                    ExitWithInvalidArgument(a);

                string name = a.Substring(0, index);
                string value = a.Substring(index + 1);

                switch (name.ToLowerInvariant())
                {
                    case "outputfile":
                        _outputFileName = value;
                        break;

                    case "clusteraddress":
                        _cluster = value;
                        break;

                    case "database":
                        _database = value;
                        break;

                    case "table":
                        _tableName = value;
                        break;

                    case "file":
                        _filePattern = value;
                        break;

                    case "logname":
                        _logName = value;
                        break;

                    case "wecfile":
                        _wecFile = value;
                        break;

                    case "query":
                        _queryFile = value;
                        if (!File.Exists(_queryFile))
                        {
                            ExitWithQueryFileMissing(_queryFile);
                        }
                        break;

                    case "demomode":
                        bool val;
                        if (bool.TryParse(value, out val))
                        {
                            _demoMode = val;
                        }
                        break;

                    case "resettable":
                        bool resetTable;
                        if (bool.TryParse(value, out resetTable))
                        {
                            _resetTable = resetTable;
                        }
                        break;

                    case "readexisting":
                        bool readExisting;
                        if (bool.TryParse(value, out readExisting))
                        {
                            _readExisting = readExisting;
                        }
                        break;

                    default:
                        ExitWithInvalidArgument(a);
                        break;
                }
            }

            if (_filePattern != null && ((_wecFile != null) || (_logName != null)))
            {
                Console.WriteLine("Uploading from both file and session together is not supported.");
                Console.WriteLine();
                PrintHelpAndExit();
            }

            // Both output to file and Kusto destination details should not be empty
            if (string.IsNullOrEmpty(_outputFileName))
            {
                if (_cluster == null)
                    ExitWithMissingArgument("clusteraddress");

                if (_database == null)
                    ExitWithMissingArgument("database");

                if (_tableName == null)
                    ExitWithMissingArgument("table");

                kscbIngest = new KustoConnectionStringBuilder($"https://ingest-{_cluster}", _database)
                {
                    FederatedSecurity = true,
                };

                kscbAdmin = new KustoConnectionStringBuilder($"https://{_cluster}", _database)
                {
                    FederatedSecurity = true,
                };
            }
        }

        static void ExitWithQueryFileMissing(string fileName)
        {
            Console.WriteLine("QueryFile not found: {0}", fileName);
            Console.WriteLine();
            PrintHelpAndExit();
        }

        static void ExitWithMissingArgument(string argument)
        {
            Console.WriteLine("Missing {0} argument", argument);
            Console.WriteLine();
            PrintHelpAndExit();
        }

        static void ExitWithInvalidArgument(string argument)
        {
            Console.WriteLine("Invalid argument {0}", argument);
            Console.WriteLine();
            PrintHelpAndExit();
        }

        static void PrintHelpAndExit()
        {

            // - password can be optionally pass as parameter or manually entered in the console
            Console.WriteLine(
@"WinLog2Kusto is tool for uploading WinLog events into Kusto. Usage examples:

1) Real-time session using WecFilter xml

    WinLog2Kusto clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:SecurityEvtx wecFile:WecFilter.xml readexisting:true demomode:true resettable:true query:QueryFile.csl

2) Real-time session using Log

    WinLog2Kusto clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:SecurityEvtx logName:""Azure Information Protection"" readexisting:true demomode:true resettable:true query:QueryFile.csl

To use real-time mode:
- the tool must be run with winlog reader permissions 
- demomode uses Kusto direct ingest instead of queued ingest
- resettable clears the table before adding new data
- readexisting when true starts reading the logs from the begining
- query contains the optional Query file that filters the events
- logName can be one of the windows logs Application, Security, Setup, System, Forwarded Events or any of the Applications and Services Logs.

demomode and resettable are optional parameters and when not present, the tool runs in regular mode and the table is not reset.
readexisting is an optional parameter and when not present, the tool reads existing events.

3) Previously recorded Evtx Trace Log (.evtx files)

    WinLog2Kusto clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:SecurityEvtx file:*.evtx query:QueryFile.csl

To replay recorded Evtx Trace:
- query contains the optional Query file that filters the events

Note: Save data to text file instead of Kusto
When Kusto is not accessible, we can log the data to a text file.

    WinLog2Kusto outputfile:AzInfoProtectionLog.json logName:""Azure Information Protection"" readexisting:true demomode:true resettable:true query:QueryFile.csl
");
            Environment.Exit(1);
        }

        static string GetPasswordForUser()
        {
            Console.Write("Enter password: ");
            string password = "";

            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);

            return password;
        }
    }

    public class ConsoleEventListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var message = string.Format(eventData.Message, eventData.Payload?.ToArray() ?? new object[0]);
            Console.WriteLine($"{eventData.EventId} {eventData.Channel} {eventData.Level} {message}");
        }
    }
}