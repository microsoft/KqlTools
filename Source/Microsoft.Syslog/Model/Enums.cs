// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Model
{

    public enum PayloadType
    {

        /// <summary>String payload is missing syslog prefix &lt;n&gt;.</summary>
        NotSyslog,
    
        /// <summary>RFC-5424 compliant entry.</summary>
        Rfc5424,

        /// <summary>Old format, aka BSD format, described in RFC-3164; or close to it.</summary>
        Rfc3164,
        /// <summary>List of key-value pairs, Sophos Web app firewall 
        ///   (https://docs.sophos.com/nsg/sophos-firewall/v16058/Help/en-us/webhelp/onlinehelp/index.html#page/onlinehelp/WAFLogs.html).</summary>
        KeyValuePairs,

        /// <summary>Plain text not following particular standard.</summary>
        PlainText,
    }



    // Defined in RFC-5424

    public enum Severity : byte
    {
        Emergency = 0,
        Alert,
        Critical,
        Error,
        Warning,
        Notice,
        Informational,
        Debug
    }

    public enum Facility : byte
    {
        Kernel = 0,
        UserLevel,
        MailSystem,
        SystemDaemons,
        Authorization,
        Syslog,
        Printer,
        News,
        Uucp,
        Clock,
        SecurityAuth,
        Ftp,
        Ntp,
        LogAudit,
        LogAlert,
        ClockDaemon,
        Local0,
        Local1,
        Local2,
        Local3,
        Local4,
        Local5,
        Local6,
        Local7
    }


}
