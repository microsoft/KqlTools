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
    using Kusto.Language;
    using Microsoft.EvtxEventXmlScrubber;
    using Microsoft.Extensions.CommandLineUtils;
    using Microsoft.Syslog;
    using Microsoft.Syslog.Parsing;
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
                + Environment.NewLine + "\tRealtimeKql syslog --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EvtxOutput --adxdirect --adxreset" + Environment.NewLine;

            command.HelpOption("-?|-h|--help");

            // input
            var adapterNameOption = command.Option(
                "-n|--networkAdapter <value>",
                "Optional: Network Adapter Name. When not specified, listner listens on all adapters.",
                CommandOptionType.SingleValue);

            var listnerUdpPortOption = command.Option(
                "-p|--udpport <value>",
                "Optional: UDP Port to listen on. When not specified listner is listening on port 514.",
                CommandOptionType.SingleValue);

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
                    UploadSyslogRealTime(
                        adapterNameOption.Value(),
                        udpPort,
                        kqlQueryOption.Value(),
                        outputFileOption.Value(),
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

        static void UploadSyslogRealTime(
            string listenerAdapterName, 
            int listenerUdpPort,
            string queryFile,
            string outputFileName, 
            KustoConnectionStringBuilder kscbAdmin, 
            KustoConnectionStringBuilder kscbIngest, 
            bool directIngest, 
            string tableName, 
            bool resetTable)
        {
            var parser = CreateSIEMfxSyslogParser();

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

            using var listener = new SyslogListener(parser, PortListener);

            var filter = new SyslogFilter();
            if (filter != null)
            {
                listener.Filter = filter.Allow;
            }

            listener.Error += Listener_Error;
            listener.EntryReceived += Listener_EntryReceived;

            var _converter = new SyslogEntryToRecordConverter();
            listener.Subscribe(_converter);
            listener.Start();

            Console.WriteLine();
            Console.WriteLine("Listening to Syslog events. Press any key to terminate");

            var ku = CreateUploader(UploadTimespan, outputFileName, kscbAdmin, kscbIngest, directIngest, tableName, resetTable);
            Task task = Task.Factory.StartNew(() =>
            {
                RunUploader(ku, _converter, queryFile);
            });

            string readline = Console.ReadLine();
            listener.Stop();

            ku.OnCompleted();
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

        private static void Listener_EntryReceived(object sender, SyslogEntryEventArgs e)
        {
            var parseErrors = e.ServerEntry.ParseErrorMessages;
            if (parseErrors != null && parseErrors.Count > 0)
            {
                var strErrors = "Parser errors encounered: " + string.Join(Environment.NewLine, parseErrors);
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

        public static void InvokeWinLog(CommandLineApplication command)
        {
            command.Description = "Realtime filter of Winlog Events";

            command.ExtendedHelpText = Environment.NewLine + "Use this option to filter OS or application log you see in EventVwr. This option can also be used with log file(s) on disk. Example is file(s) copied from another machine." + Environment.NewLine
                + Environment.NewLine + "Real-time session using WecFilter xml"
                + Environment.NewLine + "\tRealtimeKql winlog --wecfile=WecFilter.xml --readexisting --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EvtxOutput --adxdirect --adxreset" + Environment.NewLine
                + Environment.NewLine + "Real-time session using Log"
                + Environment.NewLine + "\tRealtimeKql winlog --log=\"Azure Information Protection\" --readexisting --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=AzInfoProtectOutput --adxdirect --adxreset" + Environment.NewLine
                + Environment.NewLine + "Note: To use real-time mode, the tool must be run with winlog reader permissions" + Environment.NewLine
                + Environment.NewLine + "Previously recorded Evtx Trace Log (.evtx files)"
                + Environment.NewLine + "\tRealtimeKql winlog --file=*.evtx --query=ProcessCreation.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=SecurityEvtx" + Environment.NewLine
                + Environment.NewLine + "When Kusto is not accessible, we can log the data to a text file."
                + Environment.NewLine + "\tRealtimeKql winlog --log=\"Azure Information Protection\" --readexisting --query=QueryFile.csl --outputjson=AzInfoProtectionLog.json";

            command.HelpOption("-?|-h|--help");

            // input
            var lognameOption = command.Option("-l|--log <value>",
                "log can be one of the windows logs Application, Security, Setup, System, Forwarded Events or any of the Applications and Services Logs. eg, --logname=Security",
                CommandOptionType.SingleValue);

            var readExistingOption = command.Option("-e|--readexisting",
                "By default, only the future log entries are read. Use this option to start reading the events from the beginning of the log.",
                CommandOptionType.NoValue);

            var wecFileOption = command.Option("-w|--wecfile <value>",
                "Optional: Query file that contains the windows event log filtering using structured xml query format. Refer, https://docs.microsoft.com/en-us/windows/win32/wes/consuming-events",
                CommandOptionType.SingleValue);

            var filterPatternOption = command.Option("-f|--file <value>",
                "File pattern to filter files by. eg, --file=*.evtx",
                CommandOptionType.SingleValue);

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
                            directIngestOption.HasValue(), 
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
                            directIngestOption.HasValue(),
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
                + Environment.NewLine + "\tRealtimeKql etw --session=tcp --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EtwTcp --adxdirect --adxreset" + Environment.NewLine
                + Environment.NewLine + "Note: To use real-time mode, the tool must be run with ETW reader permissions" + Environment.NewLine
                + Environment.NewLine + "Previously recorded ETL Trace Log (.etl files)"
                + Environment.NewLine + "\tRealtimeKql etw --filter=*.etl --query=QueryFile.csl --adxcluster=CDOC.kusto.windows.net --adxdatabase=GeorgiTest --adxtable=EtwTcp" + Environment.NewLine
                + Environment.NewLine + "When Kusto is not accessible, we can log the data to a text file."
                + Environment.NewLine + "\tRealtimeKql etw --session=tcp --query=QueryFile.csl --outputjson=Tcp.json" + Environment.NewLine
                + Environment.NewLine + "Note: Logman can be used to start a ETW trace. In this example we are creating a trace session named tcp with Tcp Provider guid." + Environment.NewLine
                + Environment.NewLine + "\tlogman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets" + Environment.NewLine
                + Environment.NewLine + "When done, stopping the trace session is using command,"
                + Environment.NewLine + "\tlogman.exe stop tcp -ets" + Environment.NewLine
                + Environment.NewLine + "If you are getting the error Unexpected TDH status 1168, this indicates ERROR_NOT_FOUND."
                + Environment.NewLine + "Only users with administrative privileges, users in the Performance Log Users group, and applications running as LocalSystem, LocalService, NetworkService could do that. It could mean that you are running the application as someone who doesnt belong to the above mentioned user groups.To grant a restricted user the ability to consume events in real time, add them to the Performance Log Users group.";

            command.HelpOption("-?|-h|--help");

            // input
            var sessionOption = command.Option("-s|--session <value>",
                "Name of the ETW Session to attach to. eg, --session=tcp. tcp is the name of the session started using logman or such tools.",
                CommandOptionType.SingleValue);

            var filterPatternOption = command.Option("-f|--file <value>",
                "File pattern to filter files by. eg, --filter=*.etl",
                CommandOptionType.SingleValue);

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
                            directIngestOption.HasValue(),
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
                            directIngestOption.HasValue(),
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
