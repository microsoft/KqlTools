using System;
using System.Collections.Generic;

namespace RealTimeKql
{
    public static class SyslogMessageGenerator
    {
        // Some predefined constant ip addresses; we inject them into messages, and then check if all of them 
        //  were detected in the received and parsed messages

        public static string Ip1 = "100.101.102.1";
        public static string Ip2 = "100.101.102.2";
        public static string Ip3 = "100.101.102.3";
        public static string Ip4 = "100.101.102.4";
        public static string Ip5 = "2223:10e2:80:7342::6"; //IPv6
        public static string Ip6 = "2234:10e2:00:7432::9";

        public static string[] AllIpAddresses = new string[] { Ip1, Ip2, Ip3, Ip4, Ip5, Ip6 }; 

        // Message templates for syslog messages of different format. We will inject IP addresses before sending

        static string MessageTemplate_RFC5424 = @"<14>1 2020-06-25T08:18:31.784Z dd33xd-j14x-abcd-2b RT_FLOW - RT_FLOW_SESSION_DENY
[junos@2543.1.1.1.2.22 source-address='{0}' source-port='162' destination-address='{1}' destination-port='162' service-name='None' protocol-id='17' icmp-type='0'
policy-name='57432(global)' source-zone-name='dmz' destination-zone-name='untrust' application='UNKNOWN' nested-application='UNKNOWN' username='N/A' roles='N/A'
packet-incoming-interface='combo.111' encrypted='UNKNOWN' reason='policy deny']".Replace(Environment.NewLine, " ").Replace('\'', '"');
        
        static string MessageTemplate_RFC3164 = @"<187>Jun 25 00:18:31 PST: BN3-0101-0803-01T0: %STKUNIT1-M:CP %SNMP-3-SNMP_AUTH_FAIL:
SNMP Authentication failure for SNMP request from host {0}-Error:Snmp Unknown community".Replace(Environment.NewLine, " ");

        // ex: https://docs.sophos.com/nsg/sophos-firewall/v16058/Help/en-us/webhelp/onlinehelp/index.html#page/onlinehelp/WAFLogs.html
        static string MessageTemplate_KeyValuePairs = @"<189> date=2020-06-25 time=01:18:32 devname=xyz-dd33d-f0peg-1a devid=FG1K5D3I15802556 logid=0000000013 type=traffic
subtype=forward level=notice vd=FOPEG srcip={0} srcport=49612 srcintf='FE_UNTRUST' dstip={1} dstport=45681 dstintf='EOP-INTERNAL'
poluuid=f74e865c-67ff-5544-0621-fba342e3d6ea sessionid=1948778396 proto=6 action=deny policyid=136 dstcountry='United States' srccountry='Spain'
trandisp=noop service='tcp/45681' duration=0 sentbyte=0 rcvdbyte=0 sentpkt=0 appcat='unscanned' crscore=30 craction=131072 crlevel=high".Replace(Environment.NewLine, " ").Replace('"', '\'');

        static string MessageTemplate_Text = @"<14>  Just some plain text with IP {0}";


        public static IEnumerable<string> CreateTestSyslogStream(int count)
        {
            // ingect known IPs into the messages
            var msg1 = string.Format(MessageTemplate_RFC5424, Ip1, Ip2);
            var msg2 = string.Format(MessageTemplate_RFC3164, Ip3);
            var msg3 = string.Format(MessageTemplate_KeyValuePairs, Ip5, Ip6); //IPv6
            var msg4 = string.Format(MessageTemplate_Text, Ip4);
            // send these 4 messages in a loop
            for(int i = 0; i < count/4; i++)
            {
                yield return msg1;
                yield return msg2;
                yield return msg3;
                yield return msg4;
            }
            yield break; 
        }
    }
}
