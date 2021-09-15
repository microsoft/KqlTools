using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RealTimeKqlLibrary;

namespace RealTimeKql
{
    public class CommandLineParser
    {
        public Subcommand InputSubcommand { get; private set; }
        public Subcommand OutputSubcommand { get; private set; }
        public List<string> Queries { get; private set; }
        
        private readonly List<Subcommand> _allInputSubcommands;
        private readonly List<Subcommand> _allOutputSubcommands;
        private readonly List<string> _allSubcommandNames;
        private readonly Option _query;
        private readonly string[] _args;
        private int _currentIndex;

        private readonly BaseLogger _logger;
        
        public CommandLineParser(BaseLogger logger, string[] args)
        {
            _logger = logger;

            Queries = new List<string>();
            _allInputSubcommands = new List<Subcommand>();
            _allOutputSubcommands = new List<Subcommand>();
            _allSubcommandNames = new List<string>();
            _query = new Option("query", "q",
                "Optional, apply this KQL query to the input stream. If omitted, the stream is propagated without processing to the output. eg, --query=file.kql");
            _args = args;
            _currentIndex = 0;

            GetAllSubcommands();
        }

        public bool Parse()
        {
            if(_args.Length < 1 
                || _args.Length >=2 && (_args[1] == "--help" || _args[1] == "-h"))
            {
                PrintHelp();
                return true;
            }

            // Parsing input
            if(_allInputSubcommands.Where(x => x.Name == _args[_currentIndex]).Count() == 0)
            {
                _logger.Log(LogLevel.ERROR, $"ERROR! Input source {_args[_currentIndex]} not recognized.");
                return false;
            }

            var input = _allInputSubcommands.Find(x => x.Name == _args[_currentIndex]);
            if (ParseSubcommand(input)) InputSubcommand = input;
            else return false; // Parsing subcommand failed
            if (CheckIndexOutOfBounds()) return true; // Parsing has finished

            // Parsing for output
            if (_allOutputSubcommands.Where(x => x.Name == _args[_currentIndex]).Count() > 0)
            {
                var output = _allOutputSubcommands.Find(x => x.Name == _args[_currentIndex]);
                if (ParseSubcommand(output)) OutputSubcommand = output;
                else return false; // Parsing subcommand failed
                if (CheckIndexOutOfBounds()) return true; // Parsing has finished

                // Parsing for optional query
                if (_args[_currentIndex].StartsWith("-"))
                {
                    if (!ParseQueries()) return false;
                }
            }
            else
            {
                // No output specified, parsing query file(s)
                if (_args[_currentIndex].StartsWith("-"))
                {
                    if (!ParseQueries()) return false;
                }
                else
                {
                    _logger.Log(LogLevel.ERROR, $"ERROR! Output source {_args[_currentIndex]} not recognized.");
                    return false;
                }
            }

            return true;
        }

        private void PrintHelp()
        {
            if(_args.Length <= 1 || !_allSubcommandNames.Contains(_args[0]))
            {
                PrintMainHelp();
                return;
            }

            Subcommand scmd;
            bool isInput;
            if(_allInputSubcommands.Where(x => x.Name == _args[0]).Count() > 0)
            {
                scmd = _allInputSubcommands.Find(x => x.Name == _args[0]);
                isInput = true;
            }
            else
            {
                scmd = _allOutputSubcommands.Find(x => x.Name == _args[0]);
                isInput = false;
            }

            GetArgAndOptStrings(scmd, out var argsStr, out var optionsStr);
            PrintSubcommandHelp(scmd, argsStr, optionsStr, isInput);
        }

        private void GetArgAndOptStrings(Subcommand scmd, out string argString, out string optString)
        {
            argString = "";
            optString = "";

            if (scmd.Argument != null)
            {
                char argPadLeft = scmd.Argument.IsRequired ? '<' : '[';
                char argPadRight = scmd.Argument.IsRequired ? '>' : ']';
                argString = scmd.Argument.FriendlyName.PadNonWhiteSpaceChar(argPadLeft, argPadRight);
            }

            if (scmd.Options != null)
            {
                char optPadLeft = scmd.Options.Where(opt => opt.IsRequired).Count() > 0 ? ' ' : '[';
                char optPadRight = scmd.Options.Where(opt => opt.IsRequired).Count() > 0 ? ' ' : ']';
                optString = "options".PadNonWhiteSpaceChar(optPadLeft, optPadRight);
            }

            if (!string.IsNullOrEmpty(argString))
            {
                argString += " ";
            }

            if (!string.IsNullOrEmpty(optString))
            {
                optString += " ";
            }
        }

        private void PrintMainHelp()
        {
            Console.WriteLine("Usage: RealTimeKql <input> [<arg>] [--options] [[<output>] [<args>]] [--query=<path>]");
            Console.WriteLine("\ninput commands");
            foreach(var input in _allInputSubcommands)
            {
                GetArgAndOptStrings(input, out var argsString, out var optString);
                Console.WriteLine($"\t{input.Name}\t{argsString}{optString}\t{input.HelpText}");
            }
            Console.WriteLine("\noutput commands");
            foreach (var output in _allOutputSubcommands)
            {
                GetArgAndOptStrings(output, out var argsString, out var optString);
                Console.WriteLine($"\t{output.Name}\t{argsString}{optString}\t{output.HelpText}");
            }

            Console.WriteLine("\nquery file");
            string valTag = " <file.kql> ";
            Console.WriteLine($"\t-{_query.ShortName}|--{_query.LongName}{valTag}\t{_query.HelpText}");

            Console.WriteLine("\nUse \"RealTimeKql [command] -h|--help\" for more information about a command.");
        }

        private void PrintSubcommandHelp(Subcommand scmd, string argsStr, string optionsStr, bool isInput)
        {
            var usage = "Usage: RealTimeKql ";
            if(isInput)
            {
                usage += $"{scmd.Name} {argsStr}{optionsStr}[[<output>] [<args>]] [--query=<path>]";
            }
            else
            {
                usage += $"<input> [<arg>] [--options] {scmd.Name} {argsStr}{optionsStr}[--query=<path>]";
            }

            Console.WriteLine($"\n{usage}");

            if(!string.IsNullOrEmpty(argsStr))
            {
                Console.WriteLine($"\n{scmd.Name} arg(s)");
                Console.WriteLine($"\t{scmd.Argument.FriendlyName}\t{scmd.Argument.HelpText}");
            }

            if(!string.IsNullOrEmpty(optionsStr))
            {
                Console.WriteLine($"\n{scmd.Name} option(s)");
                foreach(var opt in scmd.Options)
                {
                    string valTag = "";
                    if(!opt.IsFlag) valTag = " <value> ";
                    Console.WriteLine($"\t-{opt.ShortName}|--{opt.LongName}{valTag}\t{opt.HelpText}");
                }
            }
        }

        // Returns true if _currentIndex has exceeded _args.Length
        private bool CheckIndexOutOfBounds()
        {
            return _currentIndex >= _args.Length;
        }

        private bool ParseSubcommand(Subcommand subcommand)
        {
            _currentIndex++;
            var requiredItemsRemaining = subcommand.MinimumRequiredItems();

            // Checking for arguments / options
            var outOfBounds = CheckIndexOutOfBounds();
            if (outOfBounds && requiredItemsRemaining > 0)
            {
                PrintMissingItemsError(subcommand);
                return false;
            }
            else if (outOfBounds && requiredItemsRemaining == 0)
            {
                return true;
            }

            // Parsing for any arguments
            if (subcommand.Argument != null)
            {
                if (!ParseArgument(subcommand, ref requiredItemsRemaining)) return false;
            }

            // If there are no options, parsing for this subcommand is done
            if (subcommand.Options == null) return true;

            // Parsing for any options
            if (!ParseOptions(subcommand, ref requiredItemsRemaining)) return false;

            // Check if parsing finished properly
            if (requiredItemsRemaining == 0)
            {
                return true;
            }
            else
            {
                PrintMissingItemsError(subcommand);
                return false;
            }
        }

        private void PrintMissingItemsError(Subcommand subcommand)
        {
            var strBuilder = new StringBuilder($"ERROR! Missing these required arguments or options for {subcommand.Name}:");
            if (subcommand.Argument != null
                && subcommand.Argument.IsRequired
                && subcommand.Argument.Value == null)
            {
                strBuilder.Append($"\n{subcommand.Argument.FriendlyName}");
            }

            if (subcommand.Options == null)
            {
                _logger.Log(LogLevel.ERROR, strBuilder.ToString());
                return;
            }

            foreach (var opt in subcommand.Options)
            {
                if (opt.IsRequired && opt.Value == null)
                {
                    strBuilder.Append($"\n{opt.LongName}");
                }
            }

            _logger.Log(LogLevel.ERROR, strBuilder.ToString());
        }

        private bool ParseArgument(Subcommand subcommand, ref int requiredItemsRemaining)
        {
            if (subcommand.Argument.IsRequired)
            {
                if (_allSubcommandNames.Contains(_args[_currentIndex]) || _args[_currentIndex].StartsWith("-"))
                {
                    // Looks like user entered different subcommand or an option instead of the required argument
                    _logger.Log(LogLevel.ERROR, $"ERROR! {subcommand.Argument.FriendlyName} for {subcommand.Name} is missing.");
                    return false;
                }

                subcommand.Argument.Value = _args[_currentIndex];
                _currentIndex++;
                requiredItemsRemaining--;
            }
            else if (!_allSubcommandNames.Contains(_args[_currentIndex]) && !_args[_currentIndex].StartsWith("-"))
            {
                // Item in args doesn't appear to be another subcommand or option
                subcommand.Argument.Value = _args[_currentIndex];
                _currentIndex++;
            }

            return true;
        }

        private bool ParseOptions(Subcommand subcommand, ref int requiredItemsRemaining)
        {
            while (_currentIndex < _args.Length)
            {
                if (_args[_currentIndex].StartsWith("-"))
                {
                    // User specified an option
                    var isSubOption = false;
                    foreach (var opt in subcommand.Options)
                    {
                        var optName = _args[_currentIndex].Trim('-').Split('=')[0];
                        if (optName == opt.LongName || optName == opt.ShortName)
                        {
                            // Found the option user specified
                            isSubOption = true;
                            if(!opt.IsFlag && ParseValueOption(out var val))
                            {
                                opt.Value = val;
                            }
                            else if(opt.IsFlag)
                            {
                                opt.WasSet = true;
                                _currentIndex++;
                            }
                            else
                            {
                                _logger.Log(LogLevel.ERROR, $"ERROR! Problem parsing option {opt.LongName}");
                                return false;
                            }

                            if (opt.IsRequired) requiredItemsRemaining--;
                            break;
                        }
                    }

                    if(!isSubOption)
                    {
                        // option specified is not part of this subcommand
                        if(requiredItemsRemaining > 0)
                        {
                            _logger.Log(LogLevel.ERROR, $"ERROR! Missing required options for {subcommand.Name}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            return true;
        }

        private bool ParseValueOption(out string val)
        {
            if(_args[_currentIndex].Contains("="))
            {
                val = _args[_currentIndex].Substring(_args[_currentIndex].IndexOf("=")).TrimStart('=');
                _currentIndex++;
                return true;
            }
            else if(_currentIndex + 1 < _args.Length)
            {
                val = _args[_currentIndex + 1];
                _currentIndex += 2;
                return true;
            }
            val = null;
            return false;
        }

        private bool ParseQueries()
        {
            if(!(_args[_currentIndex].StartsWith("--query") || _args[_currentIndex].StartsWith("-q")))
            {
                _logger.Log(LogLevel.ERROR, $"ERROR! Unknown option specified: {_args[_currentIndex]}");
                return false;
            }
            if (ParseValueOption(out var query))
            {
                if(!query.Contains(".kql"))
                {
                    _logger.Log(LogLevel.ERROR, $"ERROR! This item was passed in as a query file but doesn't appear to be a .kql file: {query}");
                    return false;
                }
                Queries.Add(query);

                while (_currentIndex < _args.Length)
                {
                    if (!_args[_currentIndex].Contains(".kql"))
                    {
                        _logger.Log(LogLevel.ERROR, $"ERROR! This item was passed in as a query file but doesn't appear to be a .kql file: {query}");
                        return false;
                    }

                    Queries.Add(_args[_currentIndex]);
                    _currentIndex++;
                }

                return true;
            }
            else
            {
                _logger.Log(LogLevel.ERROR, $"ERROR! Problem parsing query file from command line arguments.");
                return false;
            }
        }

        private void GetAllSubcommands()
        {
            // Input subcommands
            // etw
            var sessionName = new Argument("session", "Name of the ETW Session to attach to", true);
            var etw = new Subcommand("etw", "Listen to real-time ETW session. See Event Trace Sessions in Perfmon", sessionName);
            _allInputSubcommands.Add(etw);
            _allSubcommandNames.Add("etw");

            // etl
            var etlFile = new Argument("file.etl", "Path to the .etl file to read", true);
            var etl = new Subcommand("etl", "Process the past event in Event Trace File (.etl) recorded via ETW", etlFile);
            _allInputSubcommands.Add(etl);
            _allSubcommandNames.Add("etl");

            // log
            var logName = new Argument("logname", "Name of the Windows log to attach to", true);
            var winlog = new Subcommand("winlog", "Listen for new events in a Windows OS log. See Windows Logs in Eventvwr", logName);
            _allInputSubcommands.Add(winlog);
            _allSubcommandNames.Add("winlog");

            // evtx
            var evtxFile = new Argument("file.evtx", "Path to the .evtx file to read", true);
            var evtx = new Subcommand("evtx", "Process the past events recorded in Windows log file on disk", evtxFile);
            _allInputSubcommands.Add(evtx);
            _allSubcommandNames.Add("evtx");

            // csv
            var csvFile = new Argument("file.csv", "Path to the .csv file to read", true);
            var csv = new Subcommand("csv", "Process past events recorded in Comma Separated File", csvFile);
            _allInputSubcommands.Add(csv);
            _allSubcommandNames.Add("csv");

            // syslog
            var syslogFile = new Argument("filepath", "Path to the log file to read", true);
            var syslog = new Subcommand("syslog", "Process real-time syslog messages written to local log file", syslogFile);
            _allInputSubcommands.Add(syslog);
            _allSubcommandNames.Add("syslog");

            // syslog server
            var syslogServerOptions = new List<Option>()
            {
                new Option("networkadapter", "na", 
                "Network Adapter Name. Optional, when not specified, listner listens on all adapters. Used along with UDP Port."),
                new Option("udpport", "p",
                "Optional. Listen to a UDP port for syslog messages. Default is port 514. eg, --udpport=514.")
                {
                    Value = "514"
                }
            };
            var syslogServer = new Subcommand("syslogserver", "Listen to syslog messages on a UDP port", null, syslogServerOptions);
            _allInputSubcommands.Add(syslogServer);
            _allSubcommandNames.Add("syslogserver");

            // Output subcommands
            // json
            var jsonFile = new Argument("file.json", "The path to the .json file to write to");
            var json = new Subcommand("json", 
                "Optional and default. Events printed to console in JSON format. If filename is specified immediately after, events will be written to the file in JSON format.",
                jsonFile);
            _allOutputSubcommands.Add(json);
            _allSubcommandNames.Add("json");
            OutputSubcommand = json;

            // table
            var table = new Subcommand("table", "Optional, events printed to console in table format");
            _allOutputSubcommands.Add(table);
            _allSubcommandNames.Add("table");

            // adx
            var adxOptions = new List<Option>()
            {
                new Option("adxauthority", "ad",
                "Azure Data Explorer (ADX) authority. Optional when not specified microsoft.com is used. eg, --adxauthority=microsoft.com")
                {
                    Value = "microsoft.com"
                },
                new Option("adxclientid", "aclid",
                "Azure Data Explorer (ADX) ClientId. Optional ClientId that has permissions to access Azure Data Explorer."),
                new Option("adxkey", "akey",
                "Azure Data Explorer (ADX) Access Key. Used along with ClientId"),
                new Option("adxcluster", "acl",
                "Azure Data Explorer (ADX) cluster address. eg, --adxcluster=CDOC.kusto.windows.net", true),
                new Option("adxdatabase", "adb",
                "Azure Data Explorer (ADX) database name. eg, --adxdatabase=TestDb", true),
                new Option("adxtable", "atb",
                "Azure Data Explorer (ADX) table name. eg, --adxtable=OutputTable", true),
                new Option("adxcreatereset", "acr",
                "If table doesn't exist, it is created. If table exists, data in table is dropped before new data is logged. eg, --adxcreatereset", false, true),
                new Option("adxdirectingest", "adi",
                "Default upload to ADX is using queued ingest. Use this option to do a direct ingest to ADX. eg, --adxdirectingest", false, true)
            };

            var adx = new Subcommand("adx", "Ingest output to Azure Data Explorer", null, adxOptions);
            _allOutputSubcommands.Add(adx);
            _allSubcommandNames.Add("adx");

            // blob
            var blobOptions = new List<Option>()
            {
                new Option("blobconnectionstring", "bcs", "Azure Blob Storage Connection string.", true, false),
                new Option("blobcontainername", "bcn", "Azure Blob Storage container name.", true, false)
            };

            var blob = new Subcommand("blob", "Ingest output to Azure Blob Storage", null, blobOptions);
            _allOutputSubcommands.Add(blob);
            _allSubcommandNames.Add("blob");

            // event log
            var eventlogOptions = new List<Option>()
            {
                new Option("logdefaultlog", "ldl", "Default log to use when writing to Windows Event Logs. When not specified, RealTimeKql is used."),
                new Option("logdefaultsource", "lds", "Default source to use when writing to Windows Event Logs. When not specified, RealTimeKql is used."),
            };
            var eventlog = new Subcommand("eventlog", "Write output to a local Windows event log", null, eventlogOptions);
            _allOutputSubcommands.Add(eventlog);
            _allSubcommandNames.Add("eventlog");
        }
    }

    public static class StringExtensions
    {
        public static string PadNonWhiteSpaceChar(this string s, char padLeft, char padRight)
        {
            if (char.IsWhiteSpace(padLeft) || char.IsWhiteSpace(padRight)) return s;
            return padLeft + s + padRight;
        }
    }
}
