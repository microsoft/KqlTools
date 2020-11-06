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

    /// <summary>
    /// Class <c>CslParagraph</c> contains a KQL query, as a string, and associated metadata.
    /// </summary>
    public class CslParagraph
    {
        public string Description;
        public string Comment;
        public string Query;
    }

    /// <summary>
    /// Class <c>CslParser</c> provides methods for parsing *csl file into CslParagraph or CslFunction instances.
    /// </summary>
    /// <see cref="CslParagraph"/>
    /// <see cref="CslFunction"/>
    public class CslParser
    {
        /// <summary>
        /// Reads the specified *.csl file and returns an array of one or more CslParagraph instances.
        /// </summary>
        /// <param name="cslFile">string - the path to the *.csl file.</param>
        /// <returns>CslParagraph[] - array with one or more CslParagraph instances.</returns>
        /// <see cref="CslParagraph"/>
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

        /// <summary>
        /// Reads the specified *.csl file and returns an array of one or more CslFunction instances.
        /// </summary>
        /// <param name="cslFile">string - the path to the *.csl file.</param>
        /// <returns>CslFunction[] - array with one or more CslFunction instances.</returns>
        /// <see cref="CslFunction"/>
        public static CslFunction[] ReadFunctionsFromFile(string cslFile)
        {
            List<CslFunction> result = new List<CslFunction>();

            StringBuilder comments = new StringBuilder();
            StringBuilder query = new StringBuilder();
            string[] lines = File.ReadAllLines(cslFile);

            return ReadAllLines(lines);
        }

        /// <summary>
        /// Returns an array of one or more CslFunction instances from the input string with one or more KQL statements.
        /// </summary>
        /// <param name="queryString">string - string with one or more KQL statements.</param>
        /// <returns>CslFunction[] - array with one or more CslFunction instances.</returns>
        /// <see cref="CslFunction"/>
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

    /// <summary>
    /// Class <c>CslFunction</c> defines a KQL Function and associated metadata.
    /// </summary>
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