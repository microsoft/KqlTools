using System;
using System.Collections.Generic;
using System.Linq;

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

        public CommandLineParser(List<Subcommand> inputs, List<Subcommand> outputs, Option query, string[] args)
        {
            Queries = new List<string>();
            _allInputSubcommands = inputs;
            _allOutputSubcommands = outputs;
            _allSubcommandNames = new List<string>();
            _query = query;
            _args = args;
            _currentIndex = 0;

            foreach(var scmd in inputs)
            {
                _allSubcommandNames.Add(scmd.Name);
            }
            foreach(var scmd in outputs)
            {
                _allSubcommandNames.Add(scmd.Name);
            }
        }

        public bool Parse()
        {
            // See if help output needs to be printed
            if(_args.Length < 1
                || _args.Length == 1 && (_args[0] == "--help" || _args[0] == "-h")
                || _args.Length ==2 && (_args[1] == "--help" || _args[1] == "-h"))
            {
                PrintHelp();
                return true;
            }

            // Parsing input
            if(_allInputSubcommands.Where(x => x.Name == _args[_currentIndex]).Count() == 0)
            {
                Console.WriteLine($"ERROR! Input source {_args[_currentIndex]} not recognized.");
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
                    Console.WriteLine($"ERROR! Output source {_args[_currentIndex]} not recognized.");
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
            Console.Write($"ERROR! Missing these required arguments or options for {subcommand.Name}:");
            if (subcommand.Argument != null
                && subcommand.Argument.IsRequired
                && subcommand.Argument.Value == null)
            {
                Console.Write($"\t{subcommand.Argument.FriendlyName}");
            }

            if (subcommand.Options == null)
            {
                Console.WriteLine();
                return;
            }

            foreach (var opt in subcommand.Options)
            {
                if (opt.IsRequired && opt.Value == null)
                {
                    Console.Write($"\t{opt.LongName}");
                }
            }
            Console.WriteLine();
        }

        private bool ParseArgument(Subcommand subcommand, ref int requiredItemsRemaining)
        {
            if (subcommand.Argument.IsRequired)
            {
                if (_allSubcommandNames.Contains(_args[_currentIndex]) || _args[_currentIndex].StartsWith("-"))
                {
                    // Looks like user entered different subcommand or an option instead of the required argument
                    Console.WriteLine($"ERROR! {subcommand.Argument.FriendlyName} for {subcommand.Name} is missing.");
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
                                Console.WriteLine($"ERROR! Problem parsing option {opt.LongName}");
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
                            Console.WriteLine($"ERROR! Missing required options for {subcommand.Name}");
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
                Console.WriteLine($"ERROR! Unknown option specified: {_args[_currentIndex]}");
                return false;
            }
            if (ParseValueOption(out var query))
            {
                if(!query.Contains(".kql"))
                {
                    Console.WriteLine($"ERROR! This item was passed in as a query file but doesn't appear to be a .kql file: {query}");
                    return false;
                }
                Queries.Add(query);

                while (_currentIndex < _args.Length)
                {
                    if (!_args[_currentIndex].Contains(".kql"))
                    {
                        Console.WriteLine($"ERROR! This item was passed in as a query file but doesn't appear to be a .kql file: {query}");
                        return false;
                    }

                    Queries.Add(_args[_currentIndex]);
                    _currentIndex++;
                }

                return true;
            }
            else
            {
                Console.WriteLine($"ERROR! Problem parsing query file from command line arguments.");
                return false;
            }
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
