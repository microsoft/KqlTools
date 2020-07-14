// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Xml.Linq;

    /// <summary>
    ///     Class to encapsulate common XML Functions
    /// </summary>
    public static class CommonXmlFunctions
    {
        /// <summary>
        ///     Get the values from the System area for the Windows Event
        /// </summary>
        /// <param name="xElement">the XElement for the Windows Event</param>
        /// <param name="attributeName">The attribute to retrieve</param>
        /// <returns>Returns the value from the system name property of the event</returns>
        internal static KeyValuePair<string, object> GetXmlSystemNameValue(XElement xElement, string attributeName)
        {
            try
            {
                switch (attributeName)
                {
                    case "Provider":
                    case "TimeCreated":
                        return new KeyValuePair<string, object>(attributeName, xElement.FirstAttribute.Value);
                    case "Execution":
                        return new KeyValuePair<string, object>(attributeName,
                            string.Format("{0}:{1}", xElement.FirstAttribute.Value,
                                xElement.FirstAttribute.NextAttribute.Value));
                    case "Security":
                        return new KeyValuePair<string, object>(attributeName,
                            xElement.FirstAttribute != null ? xElement.FirstAttribute.Value : string.Empty);
                    case "EventID":
                    case "Version":
                    case "Level":
                    case "Task":
                    case "Opcode":
                    case "Keywords":
                    case "EventRecordID":
                    case "Correlation":
                    case "Channel":
                    case "Computer":
                        return new KeyValuePair<string, object>(attributeName, xElement.Value);
                    default:
                        throw new ArgumentOutOfRangeException("OutputFileType");
                }
            }
            catch (Exception)
            {
                return new KeyValuePair<string, object>();
            }
        }

        /// <summary>
        ///     Retrieve the Extended Properties from the windows event xml
        /// </summary>
        /// <param name="xml">The XML string of the event</param>
        /// <returns>Returns a dictionary of key value pairs of the extended properties of a windows event</returns>
        public static Dictionary<string, object> ConvertExtendedPropertiesToDictionary(string xml)
        {
            var xmlElement = XElement.Parse(xml);

            return ConvertExtendedPropertiesToDictionary(xmlElement);
        }

        /// <summary>
        ///     Retrieve the Extended Properties from the windows event xml
        /// </summary>
        /// <param name="xml">The XML element of the event</param>
        /// <returns>Returns a dictionary of key value pairs of the extended properties of a windows event</returns>
        public static Dictionary<string, object> ConvertExtendedPropertiesToDictionary(XElement xmlElement)
        {
            try
            {
                var eventData = xmlElement.Element(WefXmlElementNames.EventData);

                var extendedPropertyDictionary = new Dictionary<string, object>();

                foreach (var data in eventData.Elements(WefXmlElementNames.Data))
                {
                    var name = data.Attribute("Name").Value;
                    var value = data.Value;
                    extendedPropertyDictionary.Add(name, value);
                }

                return extendedPropertyDictionary;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     Retrieve the System properties from the windows event xml
        /// </summary>
        /// <param name="xml">The XML string of the event</param>
        /// <returns>Returns a dictionary of key value pairs of the system properties of a windows event</returns>
        public static Dictionary<string, object> ConvertSystemPropertiesToDictionary(string xml)
        {
            var xmlElement = XElement.Parse(xml);

            return ConvertSystemPropertiesToDictionary(xmlElement);
        }

        /// <summary>
        ///     Retrieve the System properties from the windows event xml
        /// </summary>
        /// <param name="xml">The XML element of the event</param>
        /// <returns>Returns a dictionary of key value pairs of the system properties of a windows event</returns>
        public static Dictionary<string, object> ConvertSystemPropertiesToDictionary(XElement xmlElement)
        {
            var systemData = xmlElement.Element(WefXmlElementNames.System);

            var systemPropertyDictionary = new Dictionary<string, object>();

            foreach (var data in systemData.Descendants())
            {
                var attributeName = data.Name.LocalName;

                var tempKeyValueToAdd = GetXmlSystemNameValue(data, attributeName);

                systemPropertyDictionary.Add(tempKeyValueToAdd.Key, tempKeyValueToAdd.Value);
            }

            return systemPropertyDictionary;
        }

        /// <summary>
        ///     Retrieve the Extended Properties from the windows event xml
        /// </summary>
        /// <param name="xml">The Json of the event</param>
        /// <returns>Returns a dictionary of key value pairs of the extended properties of a windows event</returns>
        internal static Dictionary<string, object> ConvertExtendedPropertiesToDictionary(
            Dictionary<string, object> dic,
            string extendedPropertyFieldName,
            string delimiter)
        {
            try
            {
                var extendedPropertyDictionary = new Dictionary<string, object>();

                if (dic.ContainsKey(extendedPropertyFieldName))
                {
                    var eventData = (string) dic[extendedPropertyFieldName];
                    var pairs = eventData.Split(new string[]
                    {
                        delimiter
                    }, StringSplitOptions.None);

                    for (int i = 0; i < pairs.Length; i++)
                    {
                        extendedPropertyDictionary.Add(i.ToString(), pairs[i]);
                    }
                }

                return extendedPropertyDictionary;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     Gets the safe value from the dictionary of keys and values.
        /// </summary>
        /// <param name="expandoDict">The expando dictionary.</param>
        /// <param name="eventFieldName">The event field name.</param>
        /// <returns>The safe value or "~" if no item is found.</returns>
        public static string GetSafeExpandoObjectValue(IDictionary<string, object> expandoDict, string eventFieldName)
        {
            string defaultValue = string.Empty;

            try
            {
                if ((expandoDict == null) || !expandoDict.ContainsKey(eventFieldName))
                {
                    return defaultValue;
                }

                return expandoDict[eventFieldName].ToString();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        ///     Parse the System portion of a Windows event into the [evt] parameter object
        /// </summary>
        /// <param name="evt">the dynamic object receiving the converted data</param>
        /// <param name="systemData">the xElement of the system data portion of the windows event</param>
        public static dynamic ParseSystemData(XElement systemData)
        {
            dynamic evt = new ExpandoObject();

            var xeProvider = systemData.Element(ElementNames.Provider);
            evt.Provider = xeProvider.Attribute("Name").Value;

            var xeTimeCreated = systemData.Element(ElementNames.TimeCreated);
            evt.TimeCreated = DateTime.Parse(xeTimeCreated.Attribute("SystemTime").Value);

            evt.Computer = systemData.Element(ElementNames.Computer).Value;

            var xElement = systemData.Element(ElementNames.EventId);
            if (xElement != null)
            {
                evt.EventId = int.Parse(xElement.Value);
            }

            var xeVersion = systemData.Element(ElementNames.Version);
            if (xeVersion != null)
            {
                evt.Version = int.Parse(xeVersion.Value);
            }

            var xeLevel = systemData.Element(ElementNames.Level);
            if (xeLevel != null)
            {
                evt.Level = int.Parse(xeLevel.Value);
            }

            var xeOpcode = systemData.Element(ElementNames.Opcode);
            if (xeOpcode != null)
            {
                evt.Opcode = int.Parse(xeOpcode.Value);
            }

            var xeTask = systemData.Element(ElementNames.Task);
            if (xeTask != null)
            {
                evt.Task = int.Parse(xeTask.Value);
            }

            var xeChannel = systemData.Element(ElementNames.Channel);
            if (xeChannel != null)
            {
                evt.Channel = xeChannel.Value;
            }

            var xeSecurity = systemData.Element(ElementNames.Security);
            var xUserId = xeSecurity?.Attribute("UserID");
            if (xUserId != null)
            {
                evt.Security = xUserId.Value;
            }

            var xeEventRecordId = systemData.Element(ElementNames.EventRecordId);
            if (xeEventRecordId != null)
            {
                evt.EventRecordId = long.Parse(xeEventRecordId.Value);
            }

            var xeKeywords = systemData.Element(ElementNames.Keywords);
            if (xeKeywords != null)
            {
                evt.Keywords = xeKeywords.Value;
            }

            var xeCorrelation = systemData.Element(ElementNames.Correlation);
            var xAttribute = xeCorrelation?.Attribute("ActivityID");
            if (xAttribute != null)
            {
                evt.Correlation = Guid.Parse(xAttribute.Value);
            }

            var xeExecution = systemData.Element(ElementNames.Execution);
            if (xeExecution != null)
            {
                evt.ProcessID = int.Parse(xeExecution.Attribute("ProcessID").Value);
                evt.ThreadID = int.Parse(xeExecution.Attribute("ThreadID").Value);
            }

            return evt;
        }

        /// <summary>
        ///     Parse the extended data properties from a windows event into the receiving dynamic parameter object
        /// </summary>
        /// <param name="eventData">the xElement of the windows event</param>
        /// <param name="eventDataProperties">the properties created from teh windows eventdata</param>
        public static ExpandoObject ParseEventData(XElement eventData)
        {
            int dataNameCounter = 0;

            var namedProperties = new ExpandoObject();
            var dict = (IDictionary<string, object>) namedProperties;

            foreach (var data in eventData.Elements(ElementNames.Data))
            {
                dataNameCounter++;
                var nameAttr = data.Attribute("Name");
                if (nameAttr == null)
                {
                    dict.Add($"{dataNameCounter:D2}", data.Value);
                }
                else
                {
                    var name = nameAttr.Value;
                    var value = data.Value;

                    // If the same item is being added[rare occurrance], continue
                    if (dict.ContainsKey(name))
                    {
                        if (dict[name].Equals(value))
                        {
                            continue;
                        }

                        dict[name] = dict[name] + $" - {value}";
                        continue;
                    }

                    dict.Add(name, value);
                }
            }

            // If all else fails, try to get the nested properties if they exist.
            if (((IDictionary<string, object>) namedProperties).Keys.Count == 0)
            {
                namedProperties = GetNestedAttributes(eventData);
            }

            // Return the converted Expando object
            return namedProperties;
        }

        /// <summary>
        ///     Parse the extended data properties from a windows event into the receiving dynamic parameter object
        /// </summary>
        /// <param name="userData">the xElement of the windows event</param>
        public static ExpandoObject ParseUserData(XElement userData)
        {
            var namedProperties = new ExpandoObject();

            foreach (var data in userData.Elements(ElementNames.RuleAndFileData))
            {
                foreach (XElement element in data.Elements())
                {
                    var name = element.Name.LocalName;
                    var value = element.Value.Replace("\"", "'");

                    // If the same item is being added[rare occurrance], continue
                    var dict = (IDictionary<string, object>) namedProperties;
                    if (dict.ContainsKey(name))
                    {
                        if (dict[name].Equals(value))
                        {
                            continue;
                        }

                        dict[name] = dict[name] + $" - {value}";
                        continue;
                    }

                    dict.Add(name, value);
                }
            }

            foreach (var data in userData.Elements(ElementNames.LogFileCleared))
            {
                foreach (XElement element in data.Elements())
                {
                    var name = element.Name.LocalName;
                    var value = element.Value.Replace("\"", "'");

                    // If the same item is being added[rare occurrance], continue
                    var dict = (IDictionary<string, object>) namedProperties;
                    if (dict.ContainsKey(name))
                    {
                        if (dict[name].Equals(value))
                        {
                            continue;
                        }

                        dict[name] = dict[name] + $" - {value}";
                        continue;
                    }

                    dict.Add(name, value);
                }
            }

            foreach (var data in userData.Elements(ElementNames.EventNs))
            {
                foreach (XElement element in data.Elements())
                {
                    var name = element.Name.LocalName;
                    var value = element.Value.Replace("\"", "'");

                    // If the same item is being added[rare occurrance], continue
                    var dict = (IDictionary<string, object>) namedProperties;
                    if (dict.ContainsKey(name))
                    {
                        if (dict[name].Equals(value))
                        {
                            continue;
                        }

                        dict[name] = dict[name] + $" - {value}";
                        continue;
                    }

                    dict.Add(name, value);
                }
            }

            foreach (var data in userData.Elements(ElementNames.StatusData))
            {
                foreach (XElement element in data.Elements())
                {
                    var name = element.Name.LocalName;
                    var value = element.Value.Replace("\"", "'");

                    // If the same item is being added[rare occurrance], continue
                    var dict = (IDictionary<string, object>) namedProperties;
                    if (dict.ContainsKey(name))
                    {
                        if (dict[name].Equals(value))
                        {
                            continue;
                        }

                        dict[name] = dict[name] + $" - {value}";
                        continue;
                    }

                    dict.Add(name, value);
                }
            }

            // If all else fails, try to get the nested properties if they exist.
            if (((IDictionary<string, object>) namedProperties).Keys.Count == 0)
            {
                namedProperties = GetNestedAttributes(userData);
            }

            return namedProperties;
        }

        private static ExpandoObject GetNestedAttributes(XElement extendedElement)
        {
            ExpandoObject namedProperties = new ExpandoObject();

            foreach (var data in extendedElement.Elements())
            {
                foreach (XElement element in data.Elements())
                {
                    var name = element.Name.LocalName;
                    var value = element.Value.Replace("\"", "'");

                    // If the same item is being added[rare occurrance], continue
                    var dict = (IDictionary<string, object>) namedProperties;
                    if (dict.ContainsKey(name))
                    {
                        if (dict[name].Equals(value))
                        {
                            continue;
                        }

                        dict[name] = dict[name] + $" - {value}";
                        continue;
                    }

                    dict.Add(name, value);
                }
            }

            return namedProperties;
        }

        /// <summary>
        ///     Extension method that turns a dictionary of string and object to an ExpandoObject
        /// </summary>
        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>) expando;

            // go through the items in the dictionary and copy over the key value pairs)
            foreach (var kvp in dictionary)
            {
                // if the value can also be turned into an ExpandoObject, then do it!
                if (kvp.Value is IDictionary<string, object>)
                {
                    var expandoValue = ((IDictionary<string, object>) kvp.Value).ToExpando();
                    expandoDic.Add(kvp.Key, expandoValue);
                }
                else if (kvp.Value is ICollection)
                {
                    // iterate through the collection and convert any strin-object dictionaries
                    // along the way into expando objects
                    var itemList = new List<object>();
                    foreach (var item in (ICollection) kvp.Value)
                    {
                        if (item is IDictionary<string, object>)
                        {
                            var expandoItem = ((IDictionary<string, object>) item).ToExpando();
                            itemList.Add(expandoItem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }
                    }

                    expandoDic.Add(kvp.Key, itemList);
                }
                else
                {
                    expandoDic.Add(kvp);
                }
            }

            return expando;
        }
    }
}