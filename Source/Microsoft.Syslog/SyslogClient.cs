// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog
{
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Microsoft.Syslog.Model;

    public class SyslogClient
    {
        IPEndPoint _target; 
        UdpClient _udpClient;
        public UdpClient Client => _udpClient; 

        public SyslogClient(IPEndPoint target)
        {
            _target = target; 
            _udpClient = new UdpClient();
        }

        public SyslogClient(string ipAddress, int port = 514)
        {
            var addr = IPAddress.Parse(ipAddress);
            _target = new IPEndPoint(addr, port);
            _udpClient = new UdpClient();
        }

        public void Send(SyslogEntry entry)
        {
            var payload = SyslogSerializer.Serialize(entry);
            Send(payload);
        }

        //can be used to send non-stanard entries
        public void Send(string payload)
        {
            var dgram = Encoding.UTF8.GetBytes(payload);
            _udpClient.Send(dgram, dgram.Length, _target);
        }
    }
}
