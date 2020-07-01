// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using Kusto.Language;
    using Kusto.Language.Syntax;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Kql.ExceptionTypes;
    using System.Reactive.Subjects;
    using System.Text;

    public class SummarizeOperator : TableOperator, IObserver<IDictionary<string, object>>, IObservable<IDictionary<string, object>>
    {
        public Dictionary<string, ScalarValue> Groups { get; set; }

        private readonly Subject<IDictionary<string, object>> _subject = new Subject<IDictionary<string, object>>();

        private ScalarProperty _timeExpression;

        private TimeSpan _duration;

        private char _durationUnit;

        private ScalarSummarizer scalarSummarizer;

        public IObservable<IDictionary<string, object>> Source { get; set; }

        private DateTime _flushTarget = DateTime.MinValue;

        private DimensionIndex<IDictionary<string, object>> _aggregations;

        public SummarizeOperator(string summarize)
        {
            scalarSummarizer = ParseExpressionKusto(summarize) as ScalarSummarizer;
            ParseGroupings();

            _aggregations = new DimensionIndex<IDictionary<string, object>>();
        }

        private ScalarValue ParseExpressionKusto(string summarize)
        {
            summarize = summarize.Trim();
            if (!summarize.StartsWith("summarize "))
            {
                summarize = "summarize " + summarize;
            }

            KustoCode query;
            lock (parserLock)
            {
                query = KustoCode.Parse(summarize);
            }

            var diagnostics = query.GetSyntaxDiagnostics()
                .Select(d => $"({d.Start}..{d.Start + d.Length}): {d.Message}");

            if (diagnostics.Any())
            {
                var errors = string.Join("\n", diagnostics);
                throw new QueryParsingException($"Error parsing expression {summarize}: {errors}");
            }

            var syntax = query.Syntax.GetDescendants<Statement>()[0];
            return syntax.Visit(new ScalarValueConverter());
        }

        private void ParseGroupings()
        {
            Groups = new Dictionary<string, ScalarValue>();

            foreach (var item in scalarSummarizer.GroupingElements.List)
            {
                if (item is ScalarFunction scalarBinFunction)
                {
                    if (scalarBinFunction.Name == "bin")
                    {
                        var propertyName = scalarBinFunction.Arguments[0] as ScalarProperty;
                        _timeExpression = new ScalarProperty(propertyName.Value);

                        var timeValue = scalarBinFunction.Arguments[1] as ScalarConst;
                        _duration = (TimeSpan)timeValue.Value;

                        if (_duration.Seconds > 0)
                        {
                            _durationUnit = 's';
                        }
                        else if (_duration.Minutes > 0)
                        {
                            _durationUnit = 'm';
                        }
                        else if (_duration.Hours > 0)
                        {
                            _durationUnit = 'h';
                        }
                        else if (_duration.Days > 0)
                        {
                            _durationUnit = 'd';
                        }
                    }
                }
                else if (item is ScalarProperty scalarProperty)
                {
                    Groups.Add(scalarProperty.Value, scalarProperty);
                }
            }
        }

        private string ToStringDuration()
        {
            switch (_durationUnit)
            {
                case 's':
                    return $"{_duration.Seconds}{_durationUnit}";

                case 'm':
                    return $"{_duration.Minutes}{_durationUnit}";

                case 'h':
                    return $"{_duration.Hours}{_durationUnit}";

                case 'd':
                    return $"{_duration.Days}{_durationUnit}";

                default:
                    return "unknown Duration";
            }
        }

        public void OnCompleted()
        {
            Flush();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(IDictionary<string, object> currentItem)
        {
            var timeField = _timeExpression.GetValue(currentItem);
            DateTime currentRowTime = timeField is DateTime ? (DateTime)timeField : DateTime.Parse(timeField.ToString());

            if (_flushTarget == DateTime.MinValue)
            {
                _flushTarget = CalculateFlushTarget(currentRowTime);
            }

            if (currentRowTime >= _flushTarget)
            {
                Flush();
            }

            var coordinates = new List<string>();
            foreach (var g in Groups)
            {
                coordinates.Add(g.Value.GetValue(currentItem).ToString());
            }

            var cell = _aggregations.FindCell(coordinates.ToArray());
            if (cell.Value == null)
            {
                cell.Value = new Dictionary<string, object>
                {
                    { _timeExpression.Value, _flushTarget }
                };

                foreach (var g in Groups)
                {
                    cell.Value.Add(g.Key, g.Value.GetValue(currentItem));
                }

                foreach (var a in scalarSummarizer.Aggregations)
                {
                    cell.Value.Add(a.Key, a.Value.DefaultValue);
                }
            }

            foreach (var a in scalarSummarizer.Aggregations)
            {
                a.Value.AddEvent(
                    currentItem,
                    a.Value.Arguments?[0],
                    cell.Value,
                    a.Key);
            }
        }

        /// <summary>
        ///     This method calculates the flush threshold, so that aggregations are for windows
        ///     representing rounding of the clock value, and not relative to the start event time
        /// </summary>
        /// <param name="firstEventTime">The occurence time of the first event</param>
        private DateTime CalculateFlushTarget(DateTime firstEventTime)
        {
            DateTime retVal = firstEventTime;

            switch (_durationUnit)
            {
                case 's':
                    retVal = Truncate(firstEventTime, TimeSpan.FromSeconds(1));
                    break;

                case 'm':
                    retVal = Truncate(firstEventTime, TimeSpan.FromMinutes(1));
                    break;

                case 'h':
                    retVal = Truncate(firstEventTime, TimeSpan.FromHours(1));
                    break;

                case 'd':
                    retVal = Truncate(firstEventTime, TimeSpan.FromDays(1));
                    break;
            }

            return retVal.AddTicks(_duration.Ticks);
        }

        private static DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime; // do not modify "guard" values
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        private void Flush()
        {
            int itemCounter = 0;
            foreach (var cell in _aggregations.GetAllCells())
            {
                itemCounter++;
                _subject.OnNext(cell.Value);
            }

            _aggregations = new DimensionIndex<IDictionary<string, object>>();

            // Calculate Next Flush Target
            _flushTarget = _flushTarget.AddTicks(_duration.Ticks);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("summarize ");
            string delimiter = string.Empty;
            foreach (var a in scalarSummarizer.Aggregations)
            {
                sb.Append(delimiter);
                sb.Append(a.Key);
                sb.Append(" = ");
                sb.Append(a.Value);
                delimiter = ", ";
            }

            if (scalarSummarizer.Aggregations.Count == 0)
            {
                return sb.ToString();
            }

            sb.Append(" by ");

            foreach (var g in Groups)
            {
                sb.Append($"{g.Key}, ");
            }

            sb.Append($"bin({_timeExpression} , {ToStringDuration()})");

            return sb.ToString();
        }

        private IDisposable _sourceSubscription = null;

        public IDisposable Subscribe(IObserver<IDictionary<string, object>> observer)
        {
            if (Source != null && _sourceSubscription == null)
            {
                _sourceSubscription = Source.Subscribe(this);
            }

            return _subject.Subscribe(observer);
        }
    }

    public class SummaryBucket
    {
        public DateTime Timestamp { get; set; }

        public IDictionary<string, object> AggregatedSoFar { get; set; }

        public int Count { get; set; }

        public string Key { get; set; }
    }
}