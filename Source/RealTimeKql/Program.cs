// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace RealTimeKql
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reactive.Kql;
    using System.Reactive.Kql.EventTypes;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Kusto.Data;
    using Kusto.Language.Syntax;
    using Microsoft.Extensions.CommandLineUtils;
    using Microsoft.Syslog;
    using Microsoft.Syslog.Model;
    using Microsoft.Syslog.Parsing;
    using Newtonsoft.Json;
    using EventLevel = System.Diagnostics.Tracing.EventLevel;

    partial class Program
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



#if BUILT_FOR_WINDOWS
            app.Command("WinLog", InvokeWinLog);
            app.Command("Etw", InvokeEtw);
            app.Command("Csv", InvokeCsv);
#endif
            app.Command("Csv", InvokeCsv);
            app.Command("Syslog", InvokeSyslog);

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

        public static Tuple<KustoConnectionStringBuilder, KustoConnectionStringBuilder> GetKustoConnectionStrings(
            string authority,
            string clusterAddress,
            string database,
            string appClientId,
            string appKey)
        {
            KustoConnectionStringBuilder kscbAdmin = null;
            KustoConnectionStringBuilder kscbIngest = null;

            if (!string.IsNullOrEmpty(authority))
            {
                if (!string.IsNullOrEmpty(appClientId) && !string.IsNullOrEmpty(appKey))
                {
                    kscbIngest = new KustoConnectionStringBuilder($"https://ingest-{clusterAddress}", database).WithAadApplicationKeyAuthentication(appClientId, appKey, authority);
                    kscbAdmin = new KustoConnectionStringBuilder($"https://{clusterAddress}", database).WithAadApplicationKeyAuthentication(appClientId, appKey, authority);
                }
#if NET462
                else
                {
                    kscbIngest = new KustoConnectionStringBuilder($"https://ingest-{clusterAddress}", database).WithAadUserPromptAuthentication(authority);
                    kscbAdmin = new KustoConnectionStringBuilder($"https://{clusterAddress}", database).WithAadUserPromptAuthentication(authority);
                }
#endif
            }

            return new Tuple<KustoConnectionStringBuilder, KustoConnectionStringBuilder>(kscbIngest, kscbAdmin);
        }

        public static void InvokeSyslog(CommandLineApplication command)
        {
            command.Description = "Realtime processing of Syslog Events";
            command.ExtendedHelpText = Environment.NewLine + "Use this option to listen to Syslog Events." + Environment.NewLine
                + Environment.NewLine + "Real-time SysLog Events"
                + Environment.NewLine + "\tRealtimeKql syslog --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EvtxOutput --adxquickingest --adxreset" + Environment.NewLine;

            command.HelpOption("-?|-h|--help");

            // input
            var adapterNameOption = command.Option(
                "-n|--networkAdapter <value>",
                "Network Adapter Name. Optional, when not specified, listner listens on all adapters. Used along with UDP Port.",
                CommandOptionType.SingleValue);

            var listnerUdpPortOption = command.Option(
                "-p|--udpport <value>",
                "Listen to a UDP port for syslog messages. eg, --udpport=514.",
                CommandOptionType.SingleValue);

            var logFileOption = command.Option(
                "-l|--logfile <value>",
                "Retrieve syslog messages from local log specified. eg, --logfile=/var/log/syslog.",
                CommandOptionType.SingleValue);

            // do we need to give the option to specify this?
            //var priorityOption = command.Option(
            //    "-pr|--priority <value>",
            //    "Priority value to use for local logs. Optional, when not specified, Facility=Local and Severity=Informational is used. eg, --priority=134.",
            //    CommandOptionType.SingleValue);

            // query for real-time view or pre-processing
            var kqlQueryOption = command.Option("-q|--query <value>",
                "Optional: KQL filter query file that describes what processing to apply to the events on the stream. It uses a subset of Kusto Query Language, https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/",
                CommandOptionType.SingleValue);

            // output
            var consoleLogOption = command.Option("-oc|--outputconsole",
                "Log the output to console.",
                CommandOptionType.NoValue);

            var outputFileOption = command.Option("-oj|--outputjson <value>",
                "Write output to JSON file. eg, --outputjson=FilterOutput.json",
                CommandOptionType.SingleValue);

            var blobStorageConnectionStringOption = command.Option("-bscs|--blobstorageconnectionstring <value>", 
                "Azure Blob Storage Connection string. Optional when want to upload as JSON to blob storage.", 
                CommandOptionType.SingleValue);

            var blobStorageContainerOption = command.Option("-bsc|--blobstoragecontainer <value>",
                "Azure Blob Storage container name. Optional when want to upload as JSON to blob storage.",
                CommandOptionType.SingleValue);

            var adAuthority = command.Option("-ad|--adxauthority <value>",
                "Azure Data Explorer (ADX) authority. Optional when not specified microsoft.com is used. eg, --adxauthority=microsoft.com",
                CommandOptionType.SingleValue);

            var adClientAppId = command.Option("-aclid|--adxclientid <value>",
                "Azure Data Explorer (ADX) ClientId. Optional ClientId that has permissions to access Azure Data Explorer.",
                CommandOptionType.SingleValue);

            var adKey = command.Option("-akey|--adxkey <value>",
                "Azure Data Explorer (ADX) Access Key. Used along with ClientApp Id",
                CommandOptionType.SingleValue);

            var clusterAddressOption = command.Option("-ac|--adxcluster <value>",
                "Azure Data Explorer (ADX) cluster address. eg, --adxcluster=CDOC.kusto.windows.net",
                CommandOptionType.SingleValue);

            var databaseOption = command.Option("-ad|--adxdatabase <value>",
                "Azure Data Explorer (ADX) database name. eg, --adxdatabase=TestDb",
                CommandOptionType.SingleValue);

            var tableOption = command.Option("-at|--adxtable <value>",
                "Azure Data Explorer (ADX) table name. eg, --adxtable=OutputTable",
                CommandOptionType.SingleValue);

            var resetTableOption = command.Option("-ar|--adxreset",
                "The existing data in the destination table is dropped before new data is logged.",
                CommandOptionType.NoValue);

            var directIngestOption = command.Option("-ad|--adxdirect",
                "Default upload to ADX is using queued ingest. Use this option to do a direct ingest to ADX.",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                KustoConnectionStringBuilder kscbIngest = null;
                KustoConnectionStringBuilder kscbAdmin = null;

                if(logFileOption.HasValue() && !File.Exists(logFileOption.Value()))
                {
                    Console.WriteLine("Log file specified doesn't exist: {0}", logFileOption.Value());
                    return -1;
                }

                if (kqlQueryOption.HasValue() && !File.Exists(kqlQueryOption.Value()))
                {
                    Console.WriteLine("KqlQuery file doesnt exist: {0}", kqlQueryOption.Value());
                    return -1;
                }

                if (!outputFileOption.HasValue() && !consoleLogOption.HasValue())
                {
                    if (blobStorageConnectionStringOption.HasValue()) //Blob Storage Upload
                    {
                        if (!blobStorageContainerOption.HasValue())
                        {
                            Console.WriteLine("Missing Blob Storage Container Name");
                            return -1;
                        }
                    }
                    else //Kusto Upload
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

                        string authority = "microsoft.com";
                        if (adAuthority.HasValue())
                        {
                            authority = adAuthority.Value();
                        }

                        if (clusterAddressOption.HasValue() && databaseOption.HasValue())
                        {
                            var connectionStrings = GetKustoConnectionStrings(
                                authority,
                                clusterAddressOption.Value(),
                                databaseOption.Value(),
                                adClientAppId.Value(),
                                adKey.Value());

                            kscbIngest = connectionStrings.Item1;
                            kscbAdmin = connectionStrings.Item2;
                        }
                    }
                }

                int udpPort = 514;
                if (listnerUdpPortOption.HasValue())
                {
                    int.TryParse(listnerUdpPortOption.Value(), out udpPort);
                }

                string adapterName;
                if (adapterNameOption.HasValue())
                {
                    adapterName = adapterNameOption.Value();
                }

                try
                {
                    ProcessSyslogRealTime(
                    logFileOption.Value(),
                    adapterNameOption.Value(),
                    udpPort,
                    kqlQueryOption.Value(),
                    outputFileOption.Value(),
                    blobStorageConnectionStringOption.Value(),
                    blobStorageContainerOption.Value(),
                    kscbAdmin,
                    kscbIngest,
                    directIngestOption.HasValue(),
                    tableOption.Value(),
                    resetTableOption.HasValue());
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

        static void ProcessSyslogRealTime(
            string logFileName,
            string listenerAdapterName,
            int listenerUdpPort,
            string queryFile,
            string outputFileName,
            string blobConnectionString,
            string blobContainerName,
            KustoConnectionStringBuilder kscbAdmin,
            KustoConnectionStringBuilder kscbIngest,
            bool directIngest,
            string tableName,
            bool resetTable)
        {
            SyslogListener listener = null;
            SyslogFileListener fileListener = null;
            bool fileReadMode = false;

            BlockingKustoUploader ku = null;
            FileOutput fileOutput = null;
            ConsoleOutput consoleOutput = null;

            var parser = CreateSIEMfxSyslogParser();
            var _converter = new SyslogEntryToRecordConverter();

            // input
            if (string.IsNullOrEmpty(logFileName))
            {
                // reading from local port
                IPAddress localIp = null;
                if (!string.IsNullOrEmpty(listenerAdapterName))
                {
                    localIp = GetLocalIp(listenerAdapterName);
                }

                localIp ??= IPAddress.IPv6Any;
                var endPoint = new IPEndPoint(localIp, listenerUdpPort);
                var PortListener = new UdpClient(AddressFamily.InterNetworkV6);
                PortListener.Client.DualMode = true;
                PortListener.Client.Bind(endPoint);
                PortListener.Client.ReceiveBufferSize = 10 * 1024 * 1024;

                listener = new SyslogListener(parser, PortListener);

                var filter = new SyslogFilter();
                if (filter != null)
                {
                    listener.Filter = filter.Allow;
                }

                listener.Error += Listener_Error;
                listener.EntryReceived += Listener_EntryReceived;
                listener.Subscribe(_converter);
                listener.Start();
            }
            else
            {
                // reading from local file
                var fileStream = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fileListener = new SyslogFileListener(parser, fileStream);
                fileReadMode = true;

                var filter = new SyslogFilter();
                if (filter != null)
                {
                    fileListener.Filter = filter.Allow;
                }

                fileListener.Error += FileListener_Error;
                fileListener.EntryReceived += FileListener_EntryReceived;
                fileListener.Subscribe(_converter);
                fileListener.Start();
            }

            Console.WriteLine();
            Console.WriteLine("Listening to Syslog events. Press any key to terminate");

            // output
            if (kscbAdmin != null)
            {
                // output to kusto
                ku = CreateUploader(UploadTimespan, blobConnectionString, blobContainerName, kscbAdmin, kscbIngest, directIngest, tableName, resetTable);
                Task task = Task.Factory.StartNew(() =>
                {
                    RunUploader(ku, _converter, queryFile);
                });
            }
            else if (!string.IsNullOrEmpty(outputFileName))
            {
                // output to file
                fileOutput = new FileOutput(outputFileName);
                RunFileOutput(fileOutput, _converter, queryFile);
            }
            else
            {
                // output to console
                consoleOutput = new ConsoleOutput();
                RunConsoleOutput(consoleOutput, _converter, queryFile);
            }

            string readline = Console.ReadLine();

            // clean up
            if (!fileReadMode)
            {
                listener.Stop();
                listener.Dispose();
                listener = null;
            }
            else
            {
                fileListener.Stop();
                fileListener.Dispose();
                fileListener = null;
            }

            if (kscbAdmin != null)
            {
                ku.OnCompleted();
            }
            else if (!string.IsNullOrEmpty(outputFileName))
            {
                fileOutput.OnCompleted();
            }
            else
            {
                consoleOutput.OnCompleted();
            }

        }

        /// <summary>Creates syslog parser for SIEMfx. Adds specific keyword and pattern-based extractors to default parser. </summary>
        /// <returns></returns>
        public static SyslogParser CreateSIEMfxSyslogParser()
        {
            var parser = SyslogParser.CreateDefault();
            parser.AddValueExtractors(new KeywordValuesExtractor(), new PatternBasedValuesExtractor());
            return parser;
        }

        /// <summary>
        /// Returns the IPv4 address associated with the local adapter name provided.
        /// </summary>
        /// <param name="adapterName">The name of the local adapter to reference.</param>
        /// <returns>IP address for the local adapter provided.</returns>
        internal static IPAddress GetLocalIp(string adapterName)
        {
            // return IPAddress.Parse("127.0.0.1");
            UnicastIPAddressInformation unicastIPAddressInformation = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(i => i.Name == adapterName)
                .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                .FirstOrDefault(i =>
                    //i.PrefixOrigin != PrefixOrigin.WellKnown
                    //&& 
                    i.Address.AddressFamily.Equals(AddressFamily.InterNetwork)
                    && !IPAddress.IsLoopback(i.Address)
                    && i.Address != IPAddress.None);

            IPAddress localAddr = null;
            if (unicastIPAddressInformation != null)
            {
                localAddr = unicastIPAddressInformation.Address;
            }

            if (localAddr == null)
            {
                throw new Exception($"Unable to find local address for adapter {adapterName}.");
            }

            return localAddr;
        }

        private static void Listener_Error(object sender, SyslogErrorEventArgs e)
        {
            Console.WriteLine(e.Error.ToString());
        }

        private static void FileListener_Error(object sender, Exception e)
        {
            Console.WriteLine(e.Message.ToString());
        }

        private static void Listener_EntryReceived(object sender, SyslogEntryEventArgs e)
        {
            var parseErrors = e.ServerEntry.ParseErrorMessages;
            if (parseErrors != null && parseErrors.Count > 0)
            {
                var strErrors = "Parser errors encountered: " + string.Join(Environment.NewLine, parseErrors);
                Console.WriteLine(strErrors);
            }
        }

        private static void FileListener_EntryReceived(object sender, ServerSyslogEntry e)
        {
            var parseErrors = e.ParseErrorMessages;
            if(parseErrors != null && parseErrors.Count > 0)
            {
                var strErrors = "Parser errors encountered " + string.Join(Environment.NewLine, parseErrors);
                Console.WriteLine(strErrors);
            }
        }

        static void SyslogDataSender()
        {
            //var localIp = GetLocalIp(_listenerAdapterName);
            var localIp = IPAddress.Parse("127.0.0.1");
            var _sender = new SyslogClient(localIp.ToString());

            foreach (var message in SyslogMessageGenerator.CreateTestSyslogStream(500))
            {
                _sender.Send(message);
            }
        }

        private static BlockingKustoUploader CreateUploader(
            TimeSpan flushDuration,
            string blobConnectionString,
            string blobContainerName,
            KustoConnectionStringBuilder kscbAdmin,
            KustoConnectionStringBuilder kscbIngest,
            bool _demoMode,
            string _tableName,
            bool _resetTable)
        {
            var ku = new BlockingKustoUploader(
                 blobConnectionString, blobContainerName, kscbAdmin, kscbIngest, _demoMode, _tableName, 10000, flushDuration, _resetTable);

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

        private static void RunFileOutput(FileOutput fileOutput, IObservable<IDictionary<string, object>> records, string queryFile)
        {
            if (queryFile == null)
            {
                records.Subscribe(fileOutput);
            }
            else
            {
                KqlNode preProcessor = new KqlNode();
                preProcessor.KqlKqlQueryFailed += PreProcessor_KqlKqlQueryFailed;
                preProcessor.AddCslFile(queryFile);

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
                    processed.Subscribe(fileOutput);
                    records.Subscribe(preProcessor);
                }
                else
                {
                    Console.WriteLine("No Queries are running. Press Enter to terminate");
                }
            }
        }

        private static void RunConsoleOutput(ConsoleOutput consoleOutput, IObservable<IDictionary<string, object>> records, string queryFile)
        {
            if(queryFile == null)
            {
                records.Subscribe(consoleOutput);
            }
            else
            {
                KqlNode preProcessor = new KqlNode();
                preProcessor.KqlKqlQueryFailed += PreProcessor_KqlKqlQueryFailed;
                preProcessor.AddCslFile(queryFile);

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
                    processed.Subscribe(consoleOutput);
                    records.Subscribe(preProcessor);
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
