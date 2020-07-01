// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class RxKqlCommonFunctions
    {
        private static readonly Regex[] PatternMatchingRegexs = { new Regex("\"[\\w ]*\""), new Regex("'[\\w ]*'") };

        public static string[] SupportedDateTimeFormatStrings =
        {
            "M/d/yyyy h:mm:ss tt",
            "M/d/yyyy h:mm tt",
            "MM/dd/yyyy hh:mm:ss",
            "M/d/yyyy h:mm:ss",
            "M/d/yyyy hh:mm tt",
            "M/d/yyyy hh tt",
            "M/d/yyyy h:mm",
            "M/d/yyyy h:mm",
            "MM/dd/yyyy hh:mm",
            "M/dd/yyyy hh:mm",
            "yyyy/MM/dd hh:mm:ss.fffffff",
            "yyyy/MM/dd HH:mm:ss.fffffff",
            "yyyy-MM-dd HH:mm:ss.fff",
            "M-d-yyyy h:mm:ss tt",
            "M-d-yyyy h:mm tt",
            "MM-dd-yyyy hh:mm:ss",
            "M-d-yyyy h:mm:ss",
            "M-d-yyyy hh:mm tt",
            "M-d-yyyy hh tt",
            "M-d-yyyy h:mm",
            "M-d-yyyy h:mm",
            "MM-dd-yyyy hh:mm",
            "M-dd-yyyy hh:mm",
            "yyyy-MM-dd hh:mm:ss.fffffff",
            "yyyy-MM-dd HH:mm:ss.fffffff",
            "yyyy-MM-dd HH:mm:ss.fff"
        };

        public static string LongToIpAddress(long longIp)
        {
            string ip = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                int num = (int) (longIp / Math.Pow(256, 3 - i));
                longIp = longIp - (long) (num * Math.Pow(256, 3 - i));
                if (i == 0)
                {
                    ip = num.ToString();
                }
                else
                {
                    ip = ip + "." + num;
                }
            }

            return ip;
        }

        public static long IpAddressToLong(string ip)
        {
            double num = 0;

            if (ip != null && ip.Count(f => f == '.') == 3)
            {
                ip = ReplaceQuotedString(ip);
                ip = RemoveIpv6CompliantIpAddressTokens(ip);

                if (!string.IsNullOrEmpty(ip))
                {
                    var ipBytes = ip.Split('.');
                    for (int i = ipBytes.Length - 1; i >= 0; i--)
                    {
                        num += (int.Parse(ipBytes[i]) % 256) * Math.Pow(256, 3 - i);
                    }
                }
            }

            return (long) num;
        }

        public static string ToJson(ScalarValue booleanExpression, bool indented = false)
        {
            var jset = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            string json = JsonConvert.SerializeObject(booleanExpression, Formatting.Indented, jset);
            return json;
        }

        public static ScalarValue ToBooleanExpression(string args)
        {
            var jset = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var booleanExpression = (ScalarValue) JsonConvert.DeserializeObject(args, jset);
            return booleanExpression;
        }

        public static string ToJson(ExtendOperator extendOperator, bool indented = false)
        {
            var jset = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            string json = JsonConvert.SerializeObject(extendOperator, Formatting.Indented, jset);
            return json;
        }

        public static ExtendOperator ToExtendOperator(string args)
        {
            var jset = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            ExtendOperator extendOperator = (ExtendOperator) JsonConvert.DeserializeObject(args, jset);
            return extendOperator;
        }

        public static string ReplaceQuotedString(string originalValue)
        {
            return originalValue.Replace("\"", string.Empty).Replace("'", string.Empty);
        }

        public static string RemoveIpv6CompliantIpAddressTokens(string originalValue)
        {
            return originalValue.Replace("::ffff:", string.Empty).Replace("::FFFF:", string.Empty);
        }

        public static long GetDatePart(string datepart, DateTime datetime)
        {
            long returnvalue = 0;
            switch (datepart.ToLower())
            {
                case "year":
                case "yyyy":
                case "yy":
                case "y":
                    returnvalue = datetime.Year;
                    break;
                case "quarter":
                case "qq":
                case "q":
                    returnvalue = GetQuarter(datetime);
                    break;
                case "month":
                case "mm":
                case "m":
                    returnvalue = datetime.Month;
                    break;
                case "dayofyear":
                case "doy":
                case "dy":
                    returnvalue = datetime.DayOfYear;
                    break;
                case "dayofweek":
                case "weekday":
                case "dw":
                case "wd":
                    returnvalue = (int) datetime.DayOfWeek;
                    break;
                case "day":
                case "dd":
                case "d":
                    returnvalue = datetime.Day;
                    break;
                case "week":
                case "wk":
                case "ww":
                    returnvalue = WeekOfYear(datetime);
                    break;
                case "hour":
                case "hh":
                case "h":
                    returnvalue = datetime.Hour;
                    break;
                case "minute":
                case "mi":
                case "n":
                    returnvalue = datetime.Minute;
                    break;
                case "second":
                case "ss":
                case "s":
                    returnvalue = datetime.Second;
                    break;
                case "millisecond":
                case "ms":
                    returnvalue = datetime.Millisecond;
                    break;
                case "ticks":
                case "ti":
                case "t":
                    returnvalue = datetime.Ticks;
                    break;
                default:
                    throw new NotImplementedException($"Not implemented DatePart {datepart}");
            }

            return returnvalue;
        }

        private static int GetQuarter(DateTime date)
        {
            if (date.Month >= 4 && date.Month <= 6)
            {
                return 1;
            }

            if (date.Month >= 7 && date.Month <= 9)
            {
                return 2;
            }

            if (date.Month >= 10 && date.Month <= 12)
            {
                return 3;
            }

            return 4;
        }

        private static readonly int[] MoveByDays =
        {
            4,
            5,
            6,
            7,
            8,
            9,
            10,
        };

        private static int WeekOfYear(DateTime date)
        {
            DateTime startOfYear = new DateTime(date.Year, 1, 1);
            DateTime endOfYear = new DateTime(date.Year, 12, 31);

            // ISO 8601 weeks start with Monday 
            // The first week of a year includes the first Thursday 
            // This means that Jan 1st could be in week 51, 52, or 53 of the previous year...
            int numberDays = date.Subtract(startOfYear).Days +
                             MoveByDays[(int) startOfYear.DayOfWeek];
            int weekNumber = numberDays / 7;
            switch (weekNumber)
            {
                case 0:
                    // Before start of first week of this year - in last week of previous year
                    weekNumber = WeekOfYear(startOfYear.AddDays(-1));
                    break;
                case 53:
                    // In first week of next year.
                    if (endOfYear.DayOfWeek < DayOfWeek.Thursday)
                    {
                        weekNumber = 1;
                    }

                    break;
            }

            return weekNumber;
        }

        public static bool GetBetweenValue(string target, object min, object max)
        {
            try
            {
                // Numeric Between
                long expectedTarget;
                long expectedMin;
                long expectedMax;
                if (long.TryParse(target, out expectedTarget) && long.TryParse(min.ToString(), out expectedMin) &&
                    long.TryParse(max.ToString(), out expectedMax))
                {
                    return expectedMin <= expectedTarget && expectedTarget <= expectedMax;
                }

                // DateTime Between
                DateTime expectedTargetDate;
                DateTime expectedMinDate;
                DateTime expectedMaxDate;
                if (DateTime.TryParse(target, out expectedTargetDate) && DateTime.TryParse(min.ToString(), out expectedMinDate) &&
                    DateTime.TryParse(max.ToString(), out expectedMaxDate))
                {
                    return expectedMinDate <= expectedTargetDate && expectedTargetDate <= expectedMaxDate;
                }
            }
            catch (Exception ex)
            {
                throw new NotImplementedException($"Between values are incorrect: {ex.Message}");
            }

            // Default value, or unsupported code path.
            return false;
        }

        public static bool EvaluateBetweenFunction(IDictionary<string, object> value, string column, object lowValue, object highValue)
        {
            var evt = (IDictionary<string, object>) value;

            string actual = evt[column].ToString();

            // Numeric Between
            long expectedLowValue;
            long expectedHighValue;
            DateTime expectedLowValueDate;
            DateTime expectedHighValueDate;

            string lowValueStr = ReplaceQuotedString(lowValue.ToString());
            string highValueStr = ReplaceQuotedString(highValue.ToString());

            // Get the value from the dictionary, if present
            if (evt.ContainsKey(lowValueStr))
            {
                lowValueStr = evt[lowValueStr].ToString();
            }

            if (evt.ContainsKey(highValueStr))
            {
                highValueStr = evt[highValueStr].ToString();
            }

            // If both Low and High values are the same type and can parse, allow the call, or throw an exception.
            if ((long.TryParse(lowValueStr, out expectedLowValue) && long.TryParse(highValueStr, out expectedHighValue)) ||
                (DateTime.TryParse(lowValueStr, out expectedLowValueDate) && DateTime.TryParse(highValueStr, out expectedHighValueDate)))
            {
                return GetBetweenValue(actual, lowValueStr, highValueStr);
            }

            throw new ArgumentException(
                $"There is a problem with the arguments for the Between operation.  Low Value: {lowValue} High Value: {highValue}");
        }

        public static bool EvaluateMatchesRegex(object input, object pattern)
        {
            string inputString = $"{input}";
            string patternString = $"{pattern}";

            // Evaluate for matches and return if any matches exist in input
            Match match = Regex.Match(inputString, patternString, RegexOptions.None, TimeSpan.FromSeconds(5));

            return match.Success;
        }

        public static bool EvaluateBetween(object value, object low, object high)
        {
            if (value is int)
            {
                var lowInt = Convert.ToInt32(low);
                var highInt = Convert.ToInt32(high);
                return CompareLessEqual(lowInt, (int) value) && CompareLessEqual((int) value, highInt);
            }

            if (value is long)
            {
                var lowLong = Convert.ToInt64(low);
                var highLong = Convert.ToInt64(high);
                return CompareLessEqual(lowLong, (long) value) && CompareLessEqual((long) value, highLong);
            }

            if (value is DateTime)
            {
                var lowDate = Convert.ToDateTime(low);
                var highDate = Convert.ToDateTime(high);
                return CompareLessEqual(lowDate, (DateTime) value) && CompareLessEqual((DateTime) value, highDate);
            }

            throw new NotImplementedException();
        }

        private static bool CompareLessEqual<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool EvaluateAgoFunction(IDictionary<string, object> value, string column, string increment, string op)
        {
            var evt = (IDictionary<string, object>) value;

            DateTime actual = Convert.ToDateTime(evt[column].ToString());

            DateTime now = DateTime.UtcNow;

            TimeSpan result;
            if (!DateTimeParser.TryParseAgoValue(increment, out result))
            {
                throw new ArgumentException("Parameter of ago function is expected to be in 'ago(a_timespan)' format.");
            }

            DateTime expected = now.Add(-result);

            switch (op)
            {
                case "==":
                    return actual == expected;
                case ">":
                    return actual > expected;
                case "<":
                    return actual < expected;
            }

            throw new ArgumentException(
                $"There is a problem with the arguments for the Ago operation.  Date Value: {expected}");
        }

        public static dynamic ConvertJsonToDymanicEx(string json)
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            JsonSerializer serializer = new JsonSerializer();
            IDictionary<string, object> dictionary = serializer.Deserialize<IDictionary<string, object>>(reader);
            return CreateDynamicFromDictionary(dictionary);
        }

        /// <summary>
        ///     Convert a supplied JSON string to a dictionary
        /// </summary>
        /// <param name="json">the json string</param>
        /// <returns></returns>
        public static dynamic ConvertJsonToDymanic(string json)
        {
            try
            {
                IDictionary<string, object> values = JsonConvert.DeserializeObject<IDictionary<string, object>>(json);

                return values;
            }
            catch (Exception)
            {
                int dataNameCounter = 0;
                Dictionary<string, object> returnDictionaryOfUnnamedValues = new Dictionary<string, object>();
                var listOfValues = JsonConvert.DeserializeObject<List<string>>(json);
                foreach (var data in listOfValues)
                {
                    dataNameCounter++;
                    returnDictionaryOfUnnamedValues.Add($"{dataNameCounter:D2}", data);
                }

                return returnDictionaryOfUnnamedValues;
            }
        }

        /// <summary>
        ///     Create a dynamic from the supplied dictionary.
        /// </summary>
        /// <param name="dictionary">the dictionary</param>
        /// <returns></returns>
        public static dynamic CreateDynamicFromDictionary(IDictionary<string, object> dictionary)
        {
            dynamic expandoObject = new ExpandoObject();
            IDictionary<string, object> objects = expandoObject;

            foreach (var item in dictionary)
            {
                bool processed = false;

                if (item.Value == null)
                {
                    objects.Add(item.Key, null);
                    continue;
                }

                if (item.Value is IDictionary<string, object> || IsValidJson(item.Value.ToString()))
                {
                    objects.Add(item.Key,
                        CreateDynamicFromDictionary((IDictionary<string, object>) ConvertJsonToDymanic(item.Value.ToString())));

                    processed = true;
                }
                else if (item.Value is ICollection)
                {
                    List<object> itemList = new List<object>();

                    foreach (var item2 in (ICollection) item.Value)
                    {
                        if (item2 is IDictionary<string, object> || IsValidJson(item2.ToString()))
                        {
                            itemList.Add(CreateDynamicFromDictionary((IDictionary<string, object>) ConvertJsonToDymanic(item2.ToString())));
                        }
                        else
                        {
                            itemList.Add(CreateDynamicFromDictionary(new Dictionary<string, object>
                            {
                                { "Unknown", item2 }
                            }));
                        }
                    }

                    if (itemList.Count > 0)
                    {
                        objects.Add(item.Key, itemList);
                        processed = true;
                    }
                }

                if (!processed)
                {
                    objects.Add(item.Key, item.Value);
                }
            }

            return expandoObject;
        }

        /// <summary>
        ///     Verify if JSON string is valid, and if so, convert
        /// </summary>
        /// <param name="strInput">the string to test for JSON validity</param>
        /// <returns></returns>
        public static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }

            return false;
        }

        public static string SingleOrDoubleQuotedString(string targetString)
        {
            foreach (Regex r in PatternMatchingRegexs)
            {
                MatchCollection mc = r.Matches(targetString);

                if (mc.Count > 0)
                {
                    return r.ToString()[0].ToString() == "'" ? r.ToString()[0].ToString() : "\\\"";
                }
            }

            return string.Empty;
        }

        public static bool IsNumericValue(string targetString)
        {
            int expectedInt;
            long expectedLong;
            if (int.TryParse(targetString, out expectedInt) || long.TryParse(targetString, out expectedLong))
            {
                return true;
            }

            return false;
        }

        public static bool TryConvert<T>(object obj, out T result)
        {
            try
            {
                result = (T) Convert.ChangeType(obj, typeof(T));
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        public static bool TryConvert(object obj, Type type, out object result)
        {
            try
            {
                result = Convert.ChangeType(obj, type);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static object GetImmediateIfValue(IDictionary<string, object> evt, List<ScalarValue> argumentScalarValues)
        {
            var predicate = argumentScalarValues[0].GetValue(evt);

            if (Convert.ToBoolean(predicate))
            {
                return argumentScalarValues[1].GetValue(evt);
            }

            return argumentScalarValues[2].GetValue(evt);
        }

        public static string ReverseString(string originalStr)
        {
            // Reverse the provided string quickly, iteration backwards
            char[] returnChars = new char[originalStr.Length];
            int charPosition = 0;

            // Iterate across the string chars
            for (int i = originalStr.Length - 1; i >= 0; i--)
            {
                returnChars[charPosition++] = originalStr[i];
            }

            // return the value
            return new string(returnChars);
        }

        public static string TrimStartWithRegex(string pattern, string originalValue)
        {
            if (Regex.Match(originalValue, "^(" + pattern + ")").Success)
            {
                Regex rgx = new Regex(pattern);
                return rgx.Replace(originalValue, string.Empty, 1);
            }

            return originalValue;
        }

        public static string TrimEndWithRegex(string pattern, string originalValue)
        {
            if (Regex.Match(originalValue, "(" + pattern + ")$").Success)
            {
                MatchCollection matches = Regex.Matches(originalValue, pattern);
                Match lastMatch = matches[matches.Count - 1];
                return originalValue.Remove(lastMatch.Index, lastMatch.Length).Insert(lastMatch.Index, string.Empty);
            }

            return originalValue;
        }
    }
}