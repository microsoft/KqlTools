// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog.Helpers
{
    using System;
    using System.Xml.Linq;

    /// <summary>
    ///     Namespace mapping for Windows Events
    /// </summary>
    internal class WefXmlElementNames
    {
        private static readonly XNamespace ns = "http://schemas.microsoft.com/win/2004/08/events/event";
        public static readonly XName System = ns + "System";
        public static readonly XName EventData = ns + "EventData";
        public static readonly XName Data = ns + "Data";
    }

    /// <summary>
    ///     Namespace mapping for XML Conversion to CEF
    /// </summary>
    internal class CefConversionElementNames
    {
        private static readonly XNamespace ns = "http://schemas.microsoft.com/xmltocefconversion";
        public static readonly XName RootCefInformation = ns + "RootCefInformation";
        public static readonly XName EventDataMappings = ns + "EventDataMappings";
        public static readonly XName ExtendedDataMappings = ns + "ExtendedDataMappings";
        public static readonly XName CefRoot = ns + "CefRoot";
        public static readonly XName Mapping = ns + "Mapping";
    }

    /// <summary>
    ///     Namespace mapping for Windows Events
    /// </summary>
    public class ElementNames
    {
        private static readonly XNamespace ns = "http://schemas.microsoft.com/win/2004/08/events/event";

        private static readonly XNamespace nsud = "http://schemas.microsoft.com/win/2004/08/events/event/userdata";
        private static readonly XNamespace nsrafd = "http://schemas.microsoft.com/schemas/event/Microsoft.Windows/1.0.0.0";
        private static readonly XNamespace nssd = "http://schemas.microsoft.com/schemas/event/Microsoft.Windows/1.0.0.0";

        private static readonly XNamespace nsman = "http://manifests.microsoft.com/win/2004/08/windows/eventlog";
        private static readonly XNamespace event_ns = "Event_NS";

        public static readonly XName Data = ns + "Data";
        public static readonly XName EventData = ns + "EventData";

        public static readonly XName System = ns + "System";
        public static readonly XName Provider = ns + "Provider";
        public static readonly XName EventId = ns + "EventID";
        public static readonly XName Version = ns + "Version";
        public static readonly XName Level = ns + "Level";
        public static readonly XName Task = ns + "Task";
        public static readonly XName Opcode = ns + "Opcode";
        public static readonly XName Keywords = ns + "Keywords";
        public static readonly XName TimeCreated = ns + "TimeCreated";
        public static readonly XName EventRecordId = ns + "EventRecordID";
        public static readonly XName Correlation = ns + "Correlation";
        public static readonly XName Execution = ns + "Execution";
        public static readonly XName Channel = ns + "Channel";
        public static readonly XName Computer = ns + "Computer";
        public static readonly XName Security = ns + "Security";

        public static readonly XName UserData = ns + "UserData";
        public static readonly XName RuleAndFileData = nsrafd + "RuleAndFileData";
        public static readonly XName LogFileCleared = nsman + "LogFileCleared";
        public static readonly XName EventNs = event_ns + "EventXML";
        public static readonly XName StatusData = nssd + "StatusData";
    }
}