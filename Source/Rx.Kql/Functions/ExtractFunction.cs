// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text.RegularExpressions;

    [Description("extract")]
    public class ExtractFunction : ScalarFunction
    {
        private Regex regEx;

        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ExtractFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            // Implement extract(regex, captureGroup, text[, typeLiteral])
            string regExpression = Arguments[0].GetValue(evt).ToString();
            string captureGroup = Arguments[1].GetValue(evt).ToString();
            string contentText = Arguments[2].GetValue(evt).ToString();

            if (regEx == null)
            {
                regEx = new Regex(regExpression, RegexOptions.Compiled | RegexOptions.Singleline);
            }

            var matchResult = regEx.Match(contentText);

            string returnValue = string.Empty;
            if (int.TryParse(captureGroup, out int result) &&
                matchResult.Groups.Count >= result &&
                matchResult.Groups[result].Success == true)
            {
                returnValue = matchResult.Groups[result].Value;
            }

            string typeLiteral;
            if (Arguments.Count == 4)
            {
                typeLiteral = Arguments[3].GetValue(evt).ToString();

                switch (typeLiteral)
                {
                    case "bool":
                        if (bool.TryParse(returnValue, out bool parseBooleanResult))
                        {
                            return parseBooleanResult;
                        }
                        return false;

                    case "datetime":
                        if (DateTime.TryParse(returnValue, out DateTime parseDateTimeResult))
                        {
                            return parseDateTimeResult;
                        }
                        return DateTime.MinValue;

                    case "guid":
                        if (Guid.TryParse(returnValue, out Guid parseGuidResult))
                        {
                            return parseGuidResult;
                        }
                        return Guid.Empty;

                    case "int":
                        if (int.TryParse(returnValue, out int parseIntResult))
                        {
                            return parseIntResult;
                        }
                        return int.MinValue;

                    case "long":
                        if (long.TryParse(returnValue, out long parseLongResult))
                        {
                            return parseLongResult;
                        }
                        return long.MinValue;

                    case "real":
                        if (double.TryParse(returnValue, out double parseRealResult))
                        {
                            return parseRealResult;
                        }
                        return double.MinValue;

                    case "timespan":
                        if (TimeSpan.TryParse(returnValue, out TimeSpan parseTimeSpanResult))
                        {
                            return parseTimeSpanResult;
                        }
                        return TimeSpan.MinValue;

                    case "decimal":
                        if (decimal.TryParse(returnValue, out decimal parseDecimalResult))
                        {
                            return parseDecimalResult;
                        }
                        return decimal.MinValue;

                    case "string":
                    default:
                        return returnValue;
                }
            }

            return returnValue;
        }
    }
}