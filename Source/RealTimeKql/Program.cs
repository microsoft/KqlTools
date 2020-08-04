// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace RealTimeKql
{
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
    using Kusto.Data;
    using Microsoft.EvtxEventXmlScrubber;
    using Microsoft.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using Tx.Windows;
    using EventLevel = System.Diagnostics.Tracing.EventLevel;

    class Program
    {
        static readonly TimeSpan UploadTimespan = TimeSpan.FromMilliseconds(5);

        static void Main(string[] args)
        {
            ConsoleEventListener eventListener = new ConsoleEventListener();
            eventListener.EnableEvents(RxKqlEventSource.Log, EventLevel.Verbose);

            // Instantiate the command line app
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#command-line-configuration-provider
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = AppDomain.CurrentDomain.FriendlyName,

                Description = "The Real-Time KQL tools allow the user to explore the events by directly viewing and querying real-time streams.",

                ExtendedHelpText = Environment.NewLine + $"{AppDomain.CurrentDomain.FriendlyName} allows user to filter the stream and show only the events of interest."
                + Environment.NewLine + "Kusto Query language is used for defining the queries. "
                + Environment.NewLine + "Learn more about the query syntax at, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/ "
                + Environment.NewLine
                + Environment.NewLine + "All values must follow the parameter with an equals sign (=), or the key must have a prefix (-- or /) when the value follows a space. " +
                "The value isn't required if an equals sign is used (for example, CommandLineKey=)."
            };

            // Set the arguments to display the description and help text
            app.HelpOption("-?|-h|--help");

            // The default help text is "Show version Information"
            app.VersionOption("-v|--version", () => {
                return string.Format("Version {0}", Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            });

            // When no commands are specified, this block will execute.
            // This is the main "command"
            app.OnExecute(() =>
            {
                // ShowHint() will display: "Specify --help for a list of available options and commands."
                app.ShowHint();
                return 0;
            });

            app.Command("WinLog", InvokeWinLog);
            app.Command("Etw", InvokeEtw);

            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
            }
        }

        public static void InvokeWinLog(CommandLineApplication command)
        {
            command.Description = "Realtime filter of Winlog Events";

            command.ExtendedHelpText = Environment.NewLine + "Use this option to filter OS or application log you see in EventVwr. This option can also be used with log file(s) on disk. Example is file(s) copied from another machine." + Environment.NewLine
                + Environment.NewLine + "Real-time session using WecFilter xml"
                + Environment.NewLine + "\tRealtimeKql winlog --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=EvtxOutput --wecFile=WecFilter.xml --readexisting --quickingest --resettable --kqlquery=QueryFile.csl" + Environment.NewLine
                + Environment.NewLine + "Real-time session using Log"
                + Environment.NewLine + "\tRealtimeKql winlog --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=AzInfoProtectOutput --logname=\"Azure Information Protection\" --readexisting --quickingest --resettable --kqlquery=QueryFile.csl" + Environment.NewLine
                + Environment.NewLine + "Note: To use real-time mode, the tool must be run with winlog reader permissions" + Environment.NewLine
                + Environment.NewLine + "Previously recorded Evtx Trace Log (.evtx files)"
                + Environment.NewLine + "\tRealtimeKql winlog --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=SecurityEvtx --filter=*.evtx --kqlquery=ProcessCreation.csl" + Environment.NewLine
                + Environment.NewLine + "When Kusto is not accessible, we can log the data to a text file."
                + Environment.NewLine + "\tRealtimeKql winlog --outputfile=AzInfoProtectionLog.json --logname=\"Azure Information Protection\" --readexisting --quickingest --resettable --kqlquery=QueryFile.csl";

            command.HelpOption("-?|-h|--help");

            var outputFileOption = command.Option("-o|--outputfile <value>",
                "Write output to file. eg, --outputfile=FilterOutput.json",
                CommandOptionType.SingleValue);

            var clusterAddressOption = command.Option("-c|--clusteraddress <value>",
                "Azure Data Explorer (Kusto) cluster address. eg, --clusteraddress=CDOC.kusto.windows.net",
                CommandOptionType.SingleValue);

            var databaseOption = command.Option("-d|--database <value>",
                "Azure Data Explorer (Kusto) database name. eg, --database=TestDb",
                CommandOptionType.SingleValue);

            var tableOption = command.Option("-t|--table <value>",
                "Azure Data Explorer (Kusto) table name. eg, --table=OutputTable",
                CommandOptionType.SingleValue);

            var filterPatternOption = command.Option("-f|--filter <value>",
                "File extension pattern to filter files by. eg, --filter=*.evtx",
                CommandOptionType.SingleValue);

            var lognameOption = command.Option("-l|--logname <value>",
                "logName can be one of the windows logs Application, Security, Setup, System, Forwarded Events or any of the Applications and Services Logs. eg, --logname=Security",
                CommandOptionType.SingleValue);

            var wecFileOption = command.Option("-w|--wecfile <value>",
                "Optional: Query file that contains the windows event log filtering using structured xml query format. Refer, https://docs.microsoft.com/en-us/windows/win32/wes/consuming-events",
                CommandOptionType.SingleValue);

            var kqlQueryOption = command.Option("-q|--kqlquery <value>",
                "Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/",
                CommandOptionType.SingleValue);

            var quickIngestOption = command.Option("-qi|--quickingest",
                "Default upload to Kusto is using queued ingest. Use this option to do a direct ingest to Kusto.",
                CommandOptionType.NoValue);

            var resetTableOption = command.Option("-r|--resettable",
                "The existing data in the destination table is dropped before new data is logged.",
                CommandOptionType.NoValue);

            var readExistingOption = command.Option("-e|--readexisting",
                "By default, only the future log entries are read. Use this option to start reading the events from the beginning of the log.",
                CommandOptionType.NoValue);

            var consoleLogOption = command.Option("-lc|--logtoconsole",
                "Log the output to console.",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                KustoConnectionStringBuilder kscbIngest = null;
                KustoConnectionStringBuilder kscbAdmin = null;

                if (wecFileOption.HasValue() && !File.Exists(wecFileOption.Value()))
                {
                    Console.WriteLine("Wec file doesnt exist: {0}", wecFileOption.Value());
                    return -1;
                }

                if (kqlQueryOption.HasValue() && !File.Exists(kqlQueryOption.Value()))
                {
                    Console.WriteLine("KqlQuery file doesnt exist: {0}", kqlQueryOption.Value());
                    return -1;
                }

                if (!outputFileOption.HasValue() && !consoleLogOption.HasValue())
                {
                    if (!clusterAddressOption.HasValue())
                    {
                        Console.WriteLine("Missing Cluster Address");
                        return -1;
                    }

                    if (!databaseOption.HasValue())
                    {
                        Console.WriteLine("Missing Database Name");
                        return -1;
                    }

                    if (!tableOption.HasValue())
                    {
                        Console.WriteLine("Missing Table Name");
                        return -1;
                    }

                    if (clusterAddressOption.HasValue() && databaseOption.HasValue())
                    {
                        kscbIngest = new KustoConnectionStringBuilder($"https://ingest-{clusterAddressOption.Value()}", databaseOption.Value())
                        {
                            FederatedSecurity = true,
                        };

                        kscbAdmin = new KustoConnectionStringBuilder($"https://{clusterAddressOption.Value()}", databaseOption.Value())
                        {
                            FederatedSecurity = true,
                        };
                    }
                }

                try
                {
                    if (filterPatternOption.HasValue())
                    {
                        UploadFiles(
                            filterPatternOption.Value(), 
                            kqlQueryOption.Value(), 
                            outputFileOption.Value(), 
                            kscbAdmin, 
                            kscbIngest, 
                            quickIngestOption.HasValue(), 
                            tableOption.Value(), 
                            resetTableOption.HasValue());
                    }
                    else if (wecFileOption.HasValue() || lognameOption.HasValue())
                    {
                        UploadUsingWecFile(
                            wecFileOption.Value(), 
                            lognameOption.Value(), 
                            kqlQueryOption.Value(), 
                            readExistingOption.HasValue(), 
                            outputFileOption.Value(), 
                            kscbAdmin, 
                            kscbIngest,
                            quickIngestOption.HasValue(),
                            tableOption.Value(),
                            resetTableOption.HasValue());
                    }
                    else
                    {
                        Console.WriteLine("Missing required options");
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }

                return 0;
            });
        }

        public static void InvokeEtw(CommandLineApplication command)
        {
            //description and help text of the command.
            command.Description = "Realtime filter of ETW Events";

            command.ExtendedHelpText = Environment.NewLine + "Use this option to filter ETW events that are logged to the trace session. This option can also be used with ETL log file(s) on disk. Example is file(s) copied from another machine or previous ETW sessions." + Environment.NewLine
                + Environment.NewLine + "Real-time session"
                + Environment.NewLine + "\tRealtimeKql etw --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=EtwTcp --session=tcp --quickingest --resettable --kqlquery=QueryFile.csl" + Environment.NewLine
                + Environment.NewLine + "Note: To use real-time mode, the tool must be run with ETW reader permissions" + Environment.NewLine
                + Environment.NewLine + "Previously recorded ETL Trace Log (.etl files)"
                + Environment.NewLine + "\tRealtimeKql etw --clusteraddress=CDOC.kusto.windows.net --database=GeorgiTest --table=EtwTcp --filter=*.etl --kqlquery=QueryFile.csl" + Environment.NewLine
                + Environment.NewLine + "When Kusto is not accessible, we can log the data to a text file."
                + Environment.NewLine + "\tRealtimeKql etw --outputfile=Tcp.json --session=tcp --kqlquery=QueryFile.csl" + Environment.NewLine
                + Environment.NewLine + "Note: Logman can be used to start a ETW trace. In this example we are creating a trace session named tcp with Tcp Provider guid." + Environment.NewLine
                + Environment.NewLine + "\tlogman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets" + Environment.NewLine
                + Environment.NewLine + "When done, stopping the trace session is using command,"
                + Environment.NewLine + "\tlogman.exe stop tcp -ets" + Environment.NewLine
                + Environment.NewLine + "If you are getting the error Unexpected TDH status 1168, this indicates ERROR_NOT_FOUND."
                + Environment.NewLine + "Only users with administrative privileges, users in the Performance Log Users group, and applications running as LocalSystem, LocalService, NetworkService could do that. It could mean that you are running the application as someone who doesnt belong to the above mentioned user groups.To grant a restricted user the ability to consume events in real time, add them to the Performance Log Users group.";

            command.HelpOption("-?|-h|--help");

            var outputFileOption = command.Option("-o|--outputfile <value>",
                "Write output to file. eg, --outputfile=FilterOutput.json",
                CommandOptionType.SingleValue);

            var clusterAddressOption = command.Option("-c|--clusteraddress <value>",
                "Azure Data Explorer (Kusto) cluster address. eg, --clusteraddress=CDOC.kusto.windows.net",
                CommandOptionType.SingleValue);

            var databaseOption = command.Option("-d|--database <value>",
                "Azure Data Explorer (Kusto) database name. eg, --database=TestDb",
                CommandOptionType.SingleValue);

            var tableOption = command.Option("-t|--table <value>",
                "Azure Data Explorer (Kusto) table name. eg, --table=OutputTable",
                CommandOptionType.SingleValue);

            var sessionOption = command.Option("-s|--session <value>",
                "Name of the ETW Session to attach to. eg, --session=tcp. tcp is the name of the session started using logman or such tools.",
                CommandOptionType.SingleValue);

            var filterPatternOption = command.Option("-f|--filter <value>",
                "File extension pattern to filter files by. eg, --filter=*.etl",
                CommandOptionType.SingleValue);

            var kqlQueryOption = command.Option("-q|--kqlquery <value>",
                "Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/",
                CommandOptionType.SingleValue);

            var quickIngestOption = command.Option("-qi|--quickingest",
                "Default upload to Kusto is using queued ingest. Use this option to do a direct ingest to Kusto.",
                CommandOptionType.NoValue);

            var resetTableOption = command.Option("-r|--resettable",
                "The existing data in the destination table is dropped before new data is logged.",
                CommandOptionType.NoValue);

            var consoleLogOption = command.Option("-lc|--logtoconsole",
                "Log the output to console.",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                KustoConnectionStringBuilder kscbIngest = null;
                KustoConnectionStringBuilder kscbAdmin = null;

                if (kqlQueryOption.HasValue() && !File.Exists(kqlQueryOption.Value()))
                {
                    Console.WriteLine("KqlQuery file doesnt exist: {0}", kqlQueryOption.Value());
                    return -1;
                }

                if (!outputFileOption.HasValue() && !consoleLogOption.HasValue())
                {
                    if (!clusterAddressOption.HasValue())
                    {
                        Console.WriteLine("Missing Cluster Address");
                        return -1;
                    }

                    if (!databaseOption.HasValue())
                    {
                        Console.WriteLine("Missing Database Name");
                        return -1;
                    }

                    if (!tableOption.HasValue())
                    {
                        Console.WriteLine("Missing Table Name");
                        return -1;
                    }

                    if (clusterAddressOption.HasValue() && databaseOption.HasValue())
                    {
                        kscbIngest = new KustoConnectionStringBuilder($"https://ingest-{clusterAddressOption.Value()}", databaseOption.Value())
                        {
                            FederatedSecurity = true,
                        };

                        kscbAdmin = new KustoConnectionStringBuilder($"https://{clusterAddressOption.Value()}", databaseOption.Value())
                        {
                            FederatedSecurity = true,
                        };
                    }
                }

                try
                {
                    if (filterPatternOption.HasValue())
                    {
                        UploadEtlFiles(
                            filterPatternOption.Value(), 
                            kqlQueryOption.Value(), 
                            outputFileOption.Value(), 
                            kscbAdmin,
                            kscbIngest, 
                            quickIngestOption.HasValue(),
                            tableOption.Value(),
                            resetTableOption.HasValue());
                    }
                    else if (sessionOption.HasValue())
                    {
                        UploadRealTime(
                            sessionOption.Value(),
                            kqlQueryOption.Value(),
                            outputFileOption.Value(),
                            kscbAdmin,
                            kscbIngest,
                            quickIngestOption.HasValue(),
                            tableOption.Value(),
                            resetTableOption.HasValue());
                    }
                    else
                    {
                        Console.WriteLine("Missing required options");
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }

                return 0; //return 0 on a successful execution
            });
        }

        static void UploadRealTime(
            string _sessionName,
            string _queryFile,
            string _outputFileName,
            KustoConnectionStringBuilder kscbAdmin,
            KustoConnectionStringBuilder kscbIngest,
            bool _demoMode,
            string _tableName,
            bool _resetTable)
        {
            var etw = EtwTdhObservable.FromSession(_sessionName);
            Console.WriteLine();
            Console.WriteLine("Listening to real-time session '{0}'. Press Enter to terminate", _sessionName);

            var ku = CreateUploader(UploadTimespan, _outputFileName, kscbAdmin, kscbIngest, _demoMode, _tableName, _resetTable);
            Task task = Task.Factory.StartNew(() =>
            {
                RunUploader(ku, etw, _queryFile);
            });

            string readline = Console.ReadLine();
            ku.OnCompleted();
        }

        static void UploadEtlFiles(
            string _filePattern,
            string _queryFile,
            string _outputFileName,
            KustoConnectionStringBuilder kscbAdmin,
            KustoConnectionStringBuilder kscbIngest,
            bool _demoMode,
            string _tableName,
            bool _resetTable)
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
                var ku = CreateUploader(UploadTimespan, _outputFileName, kscbAdmin, kscbIngest, _demoMode, _tableName, _resetTable);
                RunUploader(ku, etw, _queryFile);
            }
        }

        private static void UploadFiles(
            string _filePattern, 
            string _queryFile, 
            string _outputFileName, 
            KustoConnectionStringBuilder kscbAdmin, 
            KustoConnectionStringBuilder kscbIngest,
            bool _demoMode,
            string _tableName,
            bool _resetTable)
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
                var ku = CreateUploader(UploadTimespan, _outputFileName, kscbAdmin, kscbIngest, _demoMode, _tableName, _resetTable);
                RunUploader(ku, etw, _queryFile);
            }
        }

        private static void UploadUsingWecFile(
            string _wecFile, 
            string _logName, 
            string _queryFile, 
            bool _readExisting, 
            string _outputFileName, 
            KustoConnectionStringBuilder kscbAdmin, 
            KustoConnectionStringBuilder kscbIngest, 
            bool _demoMode, 
            string _tableName, 
            bool _resetTable)
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
                Console.WriteLine("Listening using WecFile '{0}'. Press Enter to terminate", _wecFile);
            }
            else
            {
                etw = EvtxObservable.FromLog(_logName, null, _readExisting).Select(x => x.Deserialize());

                Console.WriteLine();
                Console.WriteLine("Listening using Log Name '{0}'. Press Enter to terminate", _logName);
            }

            var ku = CreateUploader(UploadTimespan, _outputFileName, kscbAdmin, kscbIngest, _demoMode, _tableName, _resetTable);
            Task task = Task.Factory.StartNew(() =>
            {
                RunUploader(ku, etw, _queryFile);
            });
            string readline = Console.ReadLine();
            ku.OnCompleted();
        }

        private static BlockingKustoUploader CreateUploader(
            TimeSpan flushDuration, 
            string _outputFileName, 
            KustoConnectionStringBuilder kscbAdmin, 
            KustoConnectionStringBuilder kscbIngest, 
            bool _demoMode, 
            string _tableName, 
            bool _resetTable)
        {
            var ku = new BlockingKustoUploader(
                 _outputFileName, kscbAdmin, kscbIngest, _demoMode, _tableName, 10000, flushDuration, _resetTable);

            return ku;
        }

        private static void RunUploader(BlockingKustoUploader ku, IObservable<IDictionary<string, object>> etw, string _queryFile)
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
                preProcessor.KqlKqlQueryFailed += PreProcessor_KqlKqlQueryFailed; ;
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
