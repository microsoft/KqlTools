// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using Kusto.Language;
    using Kusto.Language.Parsing;
    using Kusto.Language.Syntax;
    using Newtonsoft.Json;

    public static class RxExtensions
    {
        public static IObservable<IDictionary<string, object>> KustoQuery(this IObservable<IDictionary<string, object>> source, string query)
        {
            var kq = KustoCode.Parse(query);
            IDictionary<string, object> letValues = new Dictionary<string, object>();

            if (kq.Syntax.GetDescendants<Statement>().Count > 1)
            {
                var statementList = kq.Syntax.GetDescendants<Statement>().ToList();

                var letStatements = statementList.Where(x => x.Kind == SyntaxKind.LetStatement);
                var queryStatement = statementList.FirstOrDefault(x => x.Kind == SyntaxKind.ExpressionStatement);
            }

            var lexicalTokens = KustoLexer.GetTokens(query, alwaysProduceEOF: true);
            string[] pipeline = SplitExpressions(lexicalTokens).ToArray();
            var result = source;

            foreach (string p in pipeline)
            {
                string stage = p.Trim();
                int index = stage.IndexOf(' ');
                string op = stage.Substring(0, index);
                string args = stage.Substring(index + 1);

                switch (op)
                {
                    case "where":
                        result = result.Where(args);
                        break;

                    case "limit":
                        result = result.Take(int.Parse(args));
                        break;

                    case "project":
                        if (stage.Contains("="))
                        {
                            result = result.ProjectExpressions(args);
                        }
                        else
                        {
                            result = result.ProjectValues(args);
                        }
                        break;

                    case "evaluate":
                        result = result.Evaluate(args);
                        break;

                    case "extend":
                        result = result.Extend(args);
                        break;

                    case "summarize":
                        result = result.Summarize(args);
                        break;

                    default:
                        throw new NotImplementedException($"KustoQuery observable does not implement the operator: {op}");
                }
            }
            return result;
        }

        public static IObservable<IDictionary<string, object>> Where(this IObservable<IDictionary<string, object>> source, string expression)
        {
            WhereOperator where = new WhereOperator(expression);

            return Observable.Create<IDictionary<string, object>>(
                observer => source.Subscribe(e =>
                {
                    try
                    {
                        if (where.Evaluate(e))
                        {
                            observer.OnNext(e);
                        }
                    }
                    catch (Exception ex)
                    {
                        RxKqlEventSource.Log.LogException(JsonConvert.SerializeObject(new { Expression = expression, Exception = ex.ToString() }));
                        observer.OnError(ex);
                    }
                }));
        }

        public static IObservable<IDictionary<string, object>> ProjectValues(this IObservable<IDictionary<string, object>> source, string fieldList)
        {
            var fields = new List<string>(fieldList.Split(',').Select(s => s.Trim()));

            return source.Select(e =>
            {
                var result = new ExpandoObject();
                var res = (IDictionary<string, object>)result;

                foreach (string name in e.Keys)
                {
                    if (fields.Contains(name, StringComparer.Ordinal))
                    {
                        res.Add(name, e[name]);
                    }
                }

                return result;
            });
        }

        public static IObservable<IDictionary<string, object>> ProjectExpressions(this IObservable<IDictionary<string, object>> source, string expression)
        {
            var project = new ProjectOperator(expression);
            return Observable.Create<IDictionary<string, object>>(
                observer => source.Subscribe(e =>
                {
                    try
                    {
                        var r = project.Project(e);
                        observer.OnNext(r);
                    }
                    catch (Exception ex)
                    {
                        RxKqlEventSource.Log.LogException(ex.ToString());
                        observer.OnError(ex);
                    }
                }));
        }

        public static IObservable<IDictionary<string, object>> Extend(this IObservable<IDictionary<string, object>> source, string expression)
        {
            var extend = new ExtendOperator(expression);
            return Observable.Create<IDictionary<string, object>>(
                observer => source.Subscribe(e =>
                {
                    try
                    {
                        var r = extend.Extend(e);
                        observer.OnNext(r);
                    }
                    catch (Exception ex)
                    {
                        RxKqlEventSource.Log.LogException(ex.ToString());
                        observer.OnError(ex);
                    }
                }));
        }

        public static IObservable<IDictionary<string, object>> Evaluate(this IObservable<IDictionary<string, object>> source, string expression)
        {
            var evaluate = new EvaluateOperator(expression);
            return Observable.Create<IDictionary<string, object>>(
                observer => source.Subscribe(e =>
                {
                    try
                    {
                        var r = evaluate.Evaluate(e);
                        observer.OnNext(r);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }));
        }

        public static IObservable<IDictionary<string, object>> Summarize(this IObservable<IDictionary<string, object>> source, string expression)
        {
            // Most Rx operators are using anonymous wrappers
            // In SummarizeOperator we have a Subject to make the code easier to understand
            // This results in slightly different way of instantiation

            SummarizeOperator summarize = new SummarizeOperator(expression)
            {
                Source = source
            };

            return summarize;
        }

        public static IObservable<IDictionary<string, object>> ToDynamic<T, P>(this IObservable<T> source, Func<T, P> payload)
        {
            return source.Select(e =>
            {
                var data = payload(e);
                // This is simple but inefficient implementation that uses Reflection
                // For better performance see how Tx (LINQ to Events) builds lambda expressions
                ExpandoObject exp = new ExpandoObject();
                foreach (var p in typeof(P).GetProperties())
                {
                    // Get the value
                    var value = p.GetValue(data);

                    if (int.TryParse(value.ToString(), out int returnInt))
                    {
                        ((IDictionary<string, object>)exp).Add(p.Name, value);
                        continue;
                    }

                    if (long.TryParse(value.ToString(), out long returnLong))
                    {
                        ((IDictionary<string, object>)exp).Add(p.Name, value);
                        continue;
                    }

                    if (DateTime.TryParse(value.ToString(), out DateTime returnDateTime))
                    {
                        ((IDictionary<string, object>)exp).Add(p.Name, value);
                        continue;
                    }

                    ((IDictionary<string, object>)exp).Add(p.Name, value.ToString());
                }
                return exp;
            });
        }

        private static IEnumerable<string> SplitExpressions(LexicalToken[] lexicalTokens)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (LexicalToken lexicalToken in lexicalTokens)
            {
                if (lexicalToken.Text != "|")
                {
                    stringBuilder.Append($" {lexicalToken.Text}");
                }
                else
                {
                    string retVal = stringBuilder.ToString();
                    stringBuilder.Clear();

                    if (!string.IsNullOrEmpty(retVal))
                    {
                        yield return retVal;
                    }
                }
            }

            yield return stringBuilder.ToString();
        }
    }
}