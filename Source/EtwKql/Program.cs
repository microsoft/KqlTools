// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace EtwKql
{
    using Kusto.Data;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.IO;
    using System.Linq;
    using System.Reactive.Kql;
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
        static string _sessionName;
        static string _queryFile;
        static bool _demoMode;
        static bool _resetTable;
        static bool _logToConsole;
        static readonly TimeSpan UploadTimespan = TimeSpan.FromMilliseconds(5);

        static KustoConnectionStringBuilder kscbIngest;
        static KustoConnectionStringBuilder kscbAdmin;

        static void Main(string[] args)
        {
            ScalarFunctionFactory.AddFunctions(typeof(CustomScalarFunctions));

            //bool firstRetVal = EtwSessionManager.Stop("JoseMorris");

            //EtwSessionManager etwSessionManager = new EtwSessionManager(
            //    "JoseMorris",
            //    EventTraceMode.RealTimeMode,
            //    new EtwBufferConfig { MaximumBuffers = 10240, MinimumBuffers = 1024, FlushTimer = 10000, Size= 10 });

            //etwSessionManager.StartSession();
            //etwSessionManager.EnableProvider(new Guid("7dd42a49-5329-4832-8dfd-43d979153a88"));

            //bool retVal = EtwSessionManager.Stop("JoseMorris");

            ConsoleEventListener eventListener = new ConsoleEventListener();
            eventListener.EnableEvents(RxKqlEventSource.Log, EventLevel.Verbose);

            _resetTable = false;
            _demoMode = false;
            _logToConsole = false;

            ParseArgs(args);

            try
            {
                if (_filePattern != null)
                {
                    UploadFiles();
                }
                else if (_sessionName != null)
                {
                    UploadRealTime();
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
                // Get directory and file parts of complete relative pattern
                string pattern = Path.GetFileName(_filePattern);
                string relDir = _filePattern.Substring(0, _filePattern.Length - pattern.Length);

                // Get absolute path (root+relative)
                string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string absPath = Path.GetFullPath(Path.Combine(rootDir, relDir));

                // Search files mathing the pattern
                files = Directory.GetFiles(absPath, pattern, SearchOption.TopDirectoryOnly);
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
                var etw = EtwTdhObservable.FromFiles(files);
                var ku = CreateUploader(etw, UploadTimespan);
                RunUploader(ku, etw);
            }
        }

        static void UploadRealTime()
        {
            var etw = EtwTdhObservable.FromSession(_sessionName);
            Console.WriteLine();
            Console.WriteLine("Listening to real-time session '{0}'. Press Enter to termintate", _sessionName);

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

                    using (etw.Subscribe(preProcessor))
                    {
                        using (processed.Subscribe(ku))
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

                    case "session":
                        _sessionName = value;
                        break;

                    case "query":
                        _queryFile = value;
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

                    case "logtoconsole":
                        bool logToConsole;
                        if (bool.TryParse(value, out logToConsole))
                        {
                            _logToConsole = logToConsole;
                        }
                        break;

                    default:
                        ExitWithInvalidArgument(a);
                        break;
                }
            }

            if (_filePattern != null && _sessionName != null)
            {
                Console.WriteLine("Uploading from both file and session together is not supported.");
                Console.WriteLine();
                PrintHelpAndExit();
            }

            // Both output to file and Kusto destination details should not be empty
            if (_logToConsole == false && string.IsNullOrEmpty(_outputFileName))
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
            Console.WriteLine(
@"EtwKql is tool for uploading raw ETW events into Kusto. Usage examples:

1) Real-time session

    EtwKql clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:EtwTcp session:tcp demomode:true resettable:true query:QueryFile.csl

To use real-time mode:
- the tool must be run with administrative permissions 
- the session has to be created ahead of time with system tools like logman.exe or Perfmon
- demomode uses Kusto direct ingest instead of queued ingest
- resettable clears the table before adding new data
- query contains the optional Query file that filters the events

demomode and resettable are optional parameters and when not present, the tool runs in regular mode and the table is not reset.

Example is: 

logman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets

When done, stopping the trace session is using command,
logman.exe stop tcp -ets

tcp is the name of the session we created using create trace

2) Previously recorded Event Trace Log (.etl files)

    EtwKql clusteraddress:CDOC.kusto.windows.net database:GeorgiTest table:EtwTcp file:*.etl query:QueryFile.csl

Note:

If you are getting the error Unexpected TDH status 1168, this indicates ERROR_NOT_FOUND. 

Only users with administrative privileges, users in the Performance Log Users group, and applications running as LocalSystem, LocalService, NetworkService could do that. It could mean that 
you are running the application as someone who doesnt belong to the above mentioned user groups. To grant a restricted user the ability to consume events in real time, add them to the Performance
Log Users group.

Note: Save data to text file instead of Kusto
When Kusto is not accessible, we can log the data to a text file.

    EtwKql outputfile:OutputLog.json file:*.etl query:QueryFile.csl

Note: Use commandline argument logtoconsole:true to log to console. Logging to console negates other outputs the results to console only.
    EtwKql logtoconsole:true file:*.etl query:QueryFile.csl
");
            Environment.Exit(1);
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