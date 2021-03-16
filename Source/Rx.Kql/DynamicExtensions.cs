// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class DynamicExtensions
    {
        public static object GetProperty(this IDictionary<string, object> instance, string propertyPath)
        {
            // Initialize target path values, the first being the requested, and adjust if nested with dotIndex
            string firstProperty = propertyPath;
            string remainingPath = string.Empty;

            int dotIndex = propertyPath.IndexOf('.');
            bool hasProperties = dotIndex > 0;

            // If the end of the path has been reached, return it's value
            if (!hasProperties && !propertyPath.Contains(".") && !propertyPath.Contains("["))
            {
                // Verify the field exists and prevent an exception
                if (instance.ContainsKey(propertyPath))
                {
                    return instance[propertyPath];
                }

                return string.Empty;
            }

            // If the property has sub properties, set values and continue
            if (dotIndex > 0)
            {
                // If the property path is nested, extract the first path, and the remainingPath
                firstProperty = propertyPath.Substring(0, dotIndex);
                remainingPath = propertyPath.Substring(dotIndex + 1);
            }

            if (firstProperty.Contains("["))
            {
                string[] property = firstProperty.Split('[');
                bool indexedStringName;
                Regex regex = new Regex(@"\[(.*?)\]");
                var matches = regex.Matches(firstProperty);

                // Trim off double and single quotes on the edges
                string indexedFieldName = matches[0].Groups[1].Value;
                indexedStringName = indexedFieldName.Contains("\"") || indexedFieldName.Contains("'");
                string fieldindex = TrimEdges(matches[0].Groups[1].Value, new List<string>
                {
                    "\"",
                    "'"
                }, StringComparison.InvariantCultureIgnoreCase);

                int index;
                bool IsInt = int.TryParse(fieldindex, out index);

                if (!IsInt)
                {
                    if (dotIndex < 0)
                    {
                        // If the field reference is of type Field[\"TextReference\"]
                        if (!string.IsNullOrEmpty(property[0]))
                        {
                            var baseInstance = (IDictionary<string, object>)instance[property[0]];

                            if (!baseInstance.ContainsKey(fieldindex))
                            {
                                return string.Empty;
                            }

                            return baseInstance[fieldindex];
                        }

                        // If the field reference is of type Field.[\"TextReference\"], the only way to arrive here.
                        if (!instance.ContainsKey(fieldindex))
                        {
                            return string.Empty;
                        }

                        return instance[fieldindex];
                    }

                    if (IsDynamicObject(instance[property[0]]) || IsGenericCollectionObject(instance[property[0]]))
                    {
                        var dict = (IDictionary<string, object>) instance[property[0]];
                        return GetProperty(dict, remainingPath);
                    }

                    if (IsGenericList(instance[property[0]]) || IsGenericObject(instance[property[0]]))
                    {
                        var dict = instance[property[0]].GetType()
                            .GetProperties()
                            .ToDictionary(x => x.Name, x => x.GetValue(instance[property[0]], null));
                        return GetProperty(dict, remainingPath);
                    }
                }

                // If the value is requested, and an inteter value, return it
                if (dotIndex < 0 && IsInt && indexedStringName)
                {
                    return instance[fieldindex];
                }

                // If the object is requested, return it, else a property is requested
                if (dotIndex < 0)
                {
                    List<object> list = (List<object>) instance[property[0]];
                    return list[index];
                }

                if (IsGenericList(instance[property[0]]))
                {
                    List<object> genericList = (List<object>) instance[property[0]];

                    if (IsDynamicObject(genericList[index]))
                    {
                        var dynamicdict = (IDictionary<string, object>) genericList[index];
                        return GetProperty(dynamicdict, remainingPath);
                    }

                    var dict = genericList[index].GetType()
                        .GetProperties()
                        .ToDictionary(x => x.Name, x => x.GetValue(genericList[index], null));
                    return GetProperty(dict, remainingPath);
                }
            }
            else
            {
                // Verify the field exists and prevent an exception
                if (instance.ContainsKey(firstProperty))
                {
                    if (IsDynamicObject(instance[firstProperty]) || IsGenericCollectionObject(instance[firstProperty]))
                    {
                        var dict = (IDictionary<string, object>) instance[firstProperty];
                        return GetProperty(dict, remainingPath);
                    }

                    if (IsGenericList(instance[firstProperty]) || IsGenericObject(instance[firstProperty]))
                    {
                        var dict = instance[firstProperty].GetType()
                            .GetProperties()
                            .ToDictionary(x => x.Name, x => x.GetValue(instance[firstProperty], null));
                        return GetProperty(dict, remainingPath);
                    }
                }
            }

            // If all else failed, return an empty string.
            return string.Empty;
        }

        public static bool IsGenericList(this object o)
        {
            var oType = o.GetType();
            return oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)) || oType.IsArray;
        }

        public static bool IsArray(this object o)
        {
            var oType = o.GetType();
            return oType.IsArray;
        }

        public static bool IsGenericObject(this object o)
        {
            var oType = o.GetType();
            return oType.IsGenericType;
        }

        public static bool IsDynamicObject(this object o)
        {
            var oType = o.GetType();
            return oType.Namespace != null && oType.Namespace.Equals("System.Dynamic");
        }

        public static bool IsGenericCollectionObject(this object o)
        {
            var oType = o.GetType();
            return oType.Namespace != null && oType.Namespace.Equals("System.Collections.Generic");
        }

        public static string TrimLast(this string input, string suffixToRemove,
            StringComparison comparisonType)
        {
            if (input != null && suffixToRemove != null
                && input.EndsWith(suffixToRemove, comparisonType))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }

            return input;
        }

        public static string TrimFirst(this string input, string prefixToRemove,
            StringComparison comparisonType)
        {
            if (input != null && prefixToRemove != null
                && input.StartsWith(prefixToRemove, comparisonType))
            {
                return input.Substring(prefixToRemove.Length, input.Length - 1);
            }

            return input;
        }

        public static string TrimEdges(this string input, List<string> stringsToRemove,
            StringComparison comparisonType)
        {
            foreach (string str in stringsToRemove)
            {
                input = TrimFirst(TrimLast(input, str, comparisonType), str, comparisonType);
            }

            return input;
        }
    }
}