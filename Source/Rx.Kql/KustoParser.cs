// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

#pragma warning disable 618

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Linq;
    using System.Reactive.Kql.CustomTypes;
    using System.Reactive.Kql.ExceptionTypes;
    using System.Reflection;
    using System.Text;
    using Kusto.Language;
    using Kusto.Language.Syntax;

    public static class KustoParser
    {
        private static readonly Type[] _tableOperators;

        public enum Operators
        {
            [Description(">")]
            GreaterThan,

            [Description(">=")]
            GreaterThanOrEqualTo,

            [Description("<")]
            LessThan,

            [Description("<=")]
            LessThanOrEqualTo,

            [Description("==")]
            EqualTo,

            [Description("!=")]
            NotEqualTo
        }

        static KustoParser()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            _tableOperators = asm.GetTypes()
                .Where(t => t != typeof(TableOperator))
                .Where(t => typeof(TableOperator).IsAssignableFrom(t))
                .ToArray();
        }

        internal static TableOperator ParseTableOperator(string text)
        {
            string s = text.Trim();
            int index = s.IndexOf(' ');
            string opName = s.Substring(0, index);
            string args = s.Substring(index + 1).Trim();

            Type opType = _tableOperators.Where(t => t.Name.ToLower().StartsWith(opName)).FirstOrDefault();
            if (opType == null)
            {
                throw new NotImplementedException("Unknown operator: " + opName);
            }

            var op = (TableOperator) Activator.CreateInstance(opType, args);
            return op;
        }

        public static Query Parse(string queryText)
        {
            return new Query(queryText);
        }
    }

    public class Query
    {
        public string Table { get; set; }

        public List<TableOperator> Pipeline { get; private set; }

        public Query(string queryText)
        {
            Pipeline = new List<TableOperator>();
            string[] tokens = queryText.Split('|');
            Table = tokens[0].Trim();

            for (int i = 1; i < tokens.Length; i++)
            {
                var op = KustoParser.ParseTableOperator(tokens[i]);
                Pipeline.Add(op);
            }
        }

        public Query()
        {
            Pipeline = new List<TableOperator>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Table);
            sb.AppendLine();
            foreach (var op in Pipeline)
            {
                sb.Append("| ");
                sb.AppendLine(op.ToString());
            }

            return sb.ToString();
        }
    }

    public abstract class TableOperator
    {
        protected static readonly object parserLock = new object();
    }

    public class LimitOperator : TableOperator
    {
        public int Limit { get; private set; }

        public LimitOperator(string args)
        {
            Limit = int.Parse(args);
        }

        public override string ToString()
        {
            return "limit " + Limit;
        }
    }

    public class WhereOperator : TableOperator
    {
        public ScalarValue Expression { get; set; }

        public WhereOperator()
        {
        }

        public WhereOperator(string args)
        {
            Expression = ParseExpressionKusto(args);
        }

        private ScalarValue ParseExpressionKusto(string where)
        {
            where = where.Trim();
            if (where.StartsWith("where "))
            {
                where = where.Substring("where ".Length);
            }

            KustoCode query;
            lock (parserLock)
            {
                query = KustoCode.Parse(where);
            }

            var diagnostics = query.GetSyntaxDiagnostics()
                .Select(d => $"({d.Start}..{d.Start + d.Length}): {d.Message}");
            if (diagnostics.Any())
            {
                var errors = string.Join("\n", diagnostics);
                throw new QueryParsingException($"Error parsing expression {where}: {errors}");
            }

            var syntax = query.Syntax.GetDescendants<Statement>()[0];
            return syntax.Accept(new ScalarValueConverter());
        }

        public bool Evaluate(IDictionary<string, object> evt)
        {
            return Convert.ToBoolean(Expression.GetValue(evt));
        }

        public override string ToString()
        {
            return "where " + Expression.ToString();
        }
    }

    public class ProjectOperator : TableOperator
    {
        public string[] Fields { get; set; }

        public List<RxKqlScalarValue> Expressions;

        public ProjectOperator()
        {
            Expressions = new List<RxKqlScalarValue>();
        }

        private List<RxKqlScalarValue> ParseExpressionKusto(string project)
        {
            project = project.Trim();
            if (!project.StartsWith("project "))
            {
                project = "project " + project;
            }

            KustoCode query;
            lock (parserLock)
            {
                query = KustoCode.Parse(project);
            }

            var diagnostics = query.GetSyntaxDiagnostics()
                .Select(d => $"({d.Start}..{d.Start + d.Length}): {d.Message}");

            if (diagnostics.Any())
            {
                var errors = string.Join("\n", diagnostics);
                throw new QueryParsingException($"Error parsing expression {project}: {errors}");
            }

            var syntax = query.Syntax.GetDescendants<Statement>()[0];
            return syntax.Accept(new ListRxKqlScalarValueConverter());
        }

        public ProjectOperator(string args)
        {
            Expressions = ParseExpressionKusto(args);
        }

        public dynamic Project(IDictionary<string, object> instance)
        {
            var result = new ExpandoObject();

            foreach (var exp in Expressions)
            {
                string name = exp.Left;
                var value = exp.Right.GetValue(instance);

                // As per Kusto functionality, if extending an existing field in a query
                // the value of the extended field overrides the object value
                if (((IDictionary<string, object>) result).ContainsKey(name))
                {
                    ((IDictionary<string, object>) result)[name] = value;
                }
                else
                {
                    ((IDictionary<string, object>) result).Add(name, value);
                }
            }

            return result;
        }

        public override string ToString()
        {
            string prefix = string.Empty;
            StringBuilder sb = new StringBuilder("project ");
            foreach (var f in Expressions)
            {
                if (f.Left.Equals(f.Right.ToString()))
                {
                    sb.Append(f.Left);
                }
                else
                {
                    sb.Append(f.Left);
                    sb.Append($" {f.Operator} ");
                    sb.Append(f.Right);
                }

                if (f != Expressions.Last())
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }
    }

    public class ProjectAwayOperator : TableOperator
    {
        public string[] Fields { get; set; }

        public List<RxKqlScalarValue> Expressions;

        public ProjectAwayOperator()
        {
            Expressions = new List<RxKqlScalarValue>();
        }

        private List<RxKqlScalarValue> ParseExpressionKusto(string projectaway)
        {
            projectaway = projectaway.Trim();
            if (!projectaway.StartsWith("project-away "))
            {
                projectaway = "project-away " + projectaway;
            }

            KustoCode query;
            lock (parserLock)
            {
                query = KustoCode.Parse(projectaway);
            }

            var diagnostics = query.GetSyntaxDiagnostics()
                .Select(d => $"({d.Start}..{d.Start + d.Length}): {d.Message}");

            if (diagnostics.Any())
            {
                var errors = string.Join("\n", diagnostics);
                throw new QueryParsingException($"Error parsing expression {projectaway}: {errors}");
            }

            var syntax = query.Syntax.GetDescendants<Statement>()[0];
            return syntax.Accept(new ListRxKqlScalarValueConverter());
        }

        public ProjectAwayOperator(string args)
        {
            Expressions = ParseExpressionKusto(args);
        }

        public dynamic ProjectAway(IDictionary<string, object> instance)
        {
            var result = new ExpandoObject();

            foreach (var exp in Expressions)
            {
                string name = exp.Left;

                // Remove the undesired value from the dictionary
                if (instance.ContainsKey(name))
                {
                    instance.Remove(name);
                }
            }

            return instance;
        }

        public override string ToString()
        {
            string prefix = string.Empty;
            StringBuilder sb = new StringBuilder("project-away ");
            foreach (var f in Expressions)
            {
                if (f.Left.Equals(f.Right.ToString()))
                {
                    sb.Append(f.Left);
                }
                else
                {
                    sb.Append(f.Left);
                    sb.Append($" {f.Operator} ");
                    sb.Append(f.Right);
                }

                if (f != Expressions.Last())
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }
    }

    public class ProjectKeepOperator : TableOperator
    {
        public string[] Fields { get; set; }

        public List<RxKqlScalarValue> Expressions;

        public ProjectKeepOperator()
        {
            Expressions = new List<RxKqlScalarValue>();
        }

        private List<RxKqlScalarValue> ParseExpressionKusto(string projectkeep)
        {
            projectkeep = projectkeep.Trim();
            if (!projectkeep.StartsWith("project-keep "))
            {
                projectkeep = "project-keep " + projectkeep;
            }

            KustoCode query;
            lock (parserLock)
            {
                query = KustoCode.Parse(projectkeep);
            }

            var diagnostics = query.GetSyntaxDiagnostics()
                .Select(d => $"({d.Start}..{d.Start + d.Length}): {d.Message}");

            if (diagnostics.Any())
            {
                var errors = string.Join("\n", diagnostics);
                throw new QueryParsingException($"Error parsing expression {projectkeep}: {errors}");
            }

            var syntax = query.Syntax.GetDescendants<Statement>()[0];
            return syntax.Accept(new ListRxKqlScalarValueConverter());
        }

        public ProjectKeepOperator(string args)
        {
            Expressions = ParseExpressionKusto(args);
        }

        public dynamic ProjectKeep(IDictionary<string, object> instance)
        {
            var result = new ExpandoObject();

            // Retrieve the list of expressions to keep, then LINQ them to the result
            var fieldsToKeep = Expressions.Select(n => n.Left);
            instance = instance
                .Where(p => !fieldsToKeep.Contains(p.Key))
                .ToDictionary(p => p.Key, p => p.Value);

            return instance;
        }

        public override string ToString()
        {
            string prefix = string.Empty;
            StringBuilder sb = new StringBuilder("project-keep ");
            foreach (var f in Expressions)
            {
                if (f.Left.Equals(f.Right.ToString()))
                {
                    sb.Append(f.Left);
                }
                else
                {
                    sb.Append(f.Left);
                    sb.Append($" {f.Operator} ");
                    sb.Append(f.Right);
                }

                if (f != Expressions.Last())
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }
    }

    public class EvaluateOperator : TableOperator
    {
        public ScalarFunction Expression;

        public EvaluateOperator()
        {
        }

        public EvaluateOperator(string args)
        {
            args = args.Replace("evaluate ", string.Empty);

            KustoCode query;
            lock (parserLock)
            {
                query = KustoCode.Parse(args);
            }

            var diagnostics = query.GetSyntaxDiagnostics()
                .Select(d => $"({d.Start}..{d.Start + d.Length}): {d.Message}");
            if (diagnostics.Any())
            {
                var errors = string.Join("\n", diagnostics);
                throw new QueryParsingException($"Error parsing expression {args}: {errors}");
            }

            var syntax = query.Syntax.GetDescendants<Statement>()[0];
            Expression = syntax.Accept(new ScalarValueConverter()) as ScalarFunction;
        }

        public override string ToString()
        {
            return "evaluate " + Expression.ToString();
        }

        public dynamic Evaluate(IDictionary<string, object> instance, bool returnOnlyNew = false)
        {
            return Expression.GetValue(instance);
        }

        private object GetProperty(IDictionary<string, object> instance, string propertyPath)
        {
            int dotIndex = propertyPath.IndexOf('.');
            if (dotIndex < 0)
            {
                return instance[propertyPath];
            }

            string name = propertyPath.Substring(0, dotIndex);
            string rest = propertyPath.Substring(dotIndex + 1);
            dynamic d = instance[name];

            return GetProperty(d, rest);
        }
    }

    public class ExtendOperator : TableOperator
    {
        public List<RxKqlScalarValue> Expressions;

        public ExtendOperator()
        {
            Expressions = new List<RxKqlScalarValue>();
        }

        private List<RxKqlScalarValue> ParseExpressionKusto(string extend)
        {
            extend = extend.Trim();
            if (!extend.StartsWith("extend "))
            {
                extend = "extend " + extend;
            }

            KustoCode query;
            lock (parserLock)
            {
                query = KustoCode.Parse(extend);
            }

            var diagnostics = query.GetSyntaxDiagnostics()
                .Select(d => $"({d.Start}..{d.Start + d.Length}): {d.Message}");

            if (diagnostics.Any())
            {
                var errors = string.Join("\n", diagnostics);
                throw new QueryParsingException($"Error parsing expression {extend}: {errors}");
            }

            var syntax = query.Syntax.GetDescendants<Statement>()[0];
            return syntax.Accept(new ListRxKqlScalarValueConverter());
        }

        public ExtendOperator(string args)
        {
            Expressions = ParseExpressionKusto(args);
        }

        public override string ToString()
        {
            string prefix = string.Empty;
            StringBuilder sb = new StringBuilder("extend ");
            foreach (var exp in Expressions)
            {
                sb.Append(prefix);
                prefix = ", ";
                sb.AppendFormat("{0} = {1}", exp.Left, exp.Right);
            }

            return sb.ToString();
        }

        public dynamic Extend(IDictionary<string, object> instance, bool returnOnlyNew = false)
        {
            var result = new ExpandoObject();
            var inst = instance;

            if (!returnOnlyNew)
            {
                foreach (string name in inst.Keys)
                {
                    ((IDictionary<string, object>) result).Add(name, inst[name]);
                }
            }

            foreach (var exp in Expressions)
            {
                string name = exp.Left;
                var value = exp.Right.GetValue(instance);

                // As per Kusto functionality, if extending an existing field in a query
                // the value of the extended field overrides the object value
                if (((IDictionary<string, object>) result).ContainsKey(name))
                {
                    ((IDictionary<string, object>) result)[name] = value;
                }
                else
                {
                    ((IDictionary<string, object>) result).Add(name, value);
                }
            }

            return result;
        }

        private object GetProperty(IDictionary<string, object> instance, string propertyPath)
        {
            int dotIndex = propertyPath.IndexOf('.');
            if (dotIndex < 0)
            {
                return instance[propertyPath];
            }

            string name = propertyPath.Substring(0, dotIndex);
            string rest = propertyPath.Substring(dotIndex + 1);
            dynamic d = instance[name];

            return GetProperty(d, rest);
        }
    }

    public abstract class BinaryExpression : ScalarValue
    {
        public string Operator { get; set; }

        public ScalarValue Left { get; set; }

        public ScalarValue Right { get; set; }

        public BinaryExpression()
        {
        }

        public override string ToString()
        {
            return Left.ToString() + ' ' + Operator + ' ' + Right.ToString();
        }
    }
}