// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class CslParagraph
    {
        public string Description;
        public string Comment;
        public string Query;
    }

    public class CslParser
    {
        public static CslParagraph[] ReadFile(string cslFile)
        {
            var queries = new List<CslParagraph>();
            StringBuilder comments = new StringBuilder();
            StringBuilder query = new StringBuilder();
            string[] lines = File.ReadAllLines(cslFile);

            var functions = ReadFunctionsFromFile(cslFile);
            foreach (var f in functions)
            {
                GlobalFunctions.Add(f.Name, f);
            }

            foreach (string line in lines)
            {
                if (String.IsNullOrWhiteSpace(line) && query.Length > 0)
                {
                    var result = new CslParagraph
                    {
                        Comment = comments.ToString(),
                        Query = query.ToString()
                    };

                    // Only allow one Comments line to be read, usually the first one!
                    if (string.IsNullOrEmpty(comments.ToString()))
                    {
                        comments = new StringBuilder();
                    }

                    query = new StringBuilder();

                    // Ignore system commands in the file
                    if (!string.IsNullOrWhiteSpace(result.Query) && !result.Query.StartsWith("."))
                    {
                        queries.Add(result);
                    }
                }

                string currentLine = line.Trim();

                if (currentLine.StartsWith("//"))
                {
                    comments.AppendLine(currentLine);
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        query.AppendLine(currentLine);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(query.ToString()) && query.Length > 0)
            {
                var result = new CslParagraph
                {
                    Comment = comments.ToString(),
                    Query = query.ToString()
                };

                // Ignore system commands in the file
                if (!result.Query.StartsWith("."))
                {
                    queries.Add(result);
                }
            }

            return queries.ToArray();
        }

        public static CslFunction[] ReadFunctionsFromFile(string cslFile)
        {
            List<CslFunction> result = new List<CslFunction>();

            StringBuilder comments = new StringBuilder();
            StringBuilder query = new StringBuilder();
            string[] lines = File.ReadAllLines(cslFile);

            return ReadAllLines(lines);
        }

        public static CslFunction[] ReadFunctionsFromQuery(string queryString)
        {
            string[] lines = queryString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            return ReadAllLines(lines);
        }

        private static CslFunction[] ReadAllLines(string[] lines)
        {
            List<CslFunction> result = new List<CslFunction>();

            StringBuilder comments = new StringBuilder();
            StringBuilder query = new StringBuilder();

            foreach (string line in lines)
            {
                if (String.IsNullOrWhiteSpace(line) && query.Length > 0)
                {
                    string text = query.ToString().Trim();
                    if (text.StartsWith(".create-or-alter function"))
                    {
                        var func = new CslFunction(comments.ToString(), text);
                        result.Add(func);
                    }
                    comments = new StringBuilder();
                    query = new StringBuilder();
                }

                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("//"))
                {
                    comments.AppendLine(trimmedLine);
                }
                else
                {
                    // Force each line after trimming to have a preceding space, for downstream parsing ease.
                    query.Append($" {trimmedLine}");
                }
            }

            if (query.Length > 0)
            {
                string text = query.ToString().Trim();
                if (text.StartsWith(".create-or-alter function"))
                {
                    var func = new CslFunction(comments.ToString(), text);
                    result.Add(func);
                }
            }

            return result.ToArray();
        }
    }

    public class CslFunction
    {
        public string Comments { get; private set; }

        public string Name { get; private set; }

        public List<Tuple<string, string>> Arguments { get; private set; }

        public ScalarValue Body { get; private set; }

        public CslFunction(string comments, string text)
        {
            Comments = comments;
            int startBody = text.IndexOf('{') + 1;
            int endBody = text.IndexOf('}');
            string body = text.Substring(startBody, endBody - startBody);

            WhereOperator where = new WhereOperator(body);
            Body = where.Expression;

            int nameEnd = text.IndexOf('(');
            int nameStart = text.LastIndexOf(' ', nameEnd) + 1;
            Name = text.Substring(nameStart, nameEnd - nameStart);
            int argsEnd = text.IndexOf(')');

            var argString = text.Substring(nameEnd + 1, argsEnd - nameEnd - 1);
            var tokens = argString.Split(',');
            Arguments = new List<Tuple<string, string>>();

            foreach (var t in tokens)
            {
                string[] parts = t.Split(':');
                string argName = parts[0].Trim();
                string argType = parts[1].Trim();
                Arguments.Add(new Tuple<string, string>(argName, argType));
            }
        }

        public override string ToString()
        {
            string separator = string.Empty;
            StringBuilder sb = new StringBuilder(Name);
            sb.Append('(');
            foreach (var t in Arguments)
            {
                sb.Append(separator);
                sb.Append(t.Item1);
                sb.Append(" : ");
                sb.Append(t.Item2);
                separator = ", ";
            }

            return sb.ToString();
        }
    }
}