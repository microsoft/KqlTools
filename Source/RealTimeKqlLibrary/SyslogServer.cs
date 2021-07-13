using Microsoft.Syslog;
using Microsoft.Syslog.Model;
using Microsoft.Syslog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace RealTimeKqlLibrary
{
    public class SyslogServer : EventComponent
    {
        private readonly string _adapterName;
        private readonly int _udpport;
        private Observable<IDictionary<string, object>> _eventStream;

        public SyslogServer(string adapterName, int udpport, IOutput output, params string[] queries)
            : base(output, queries)
        {
            _adapterName = adapterName;
            _udpport = udpport;
            _eventStream = new Observable<IDictionary<string, object>>();
        }

        public override bool Start()
        {
            // Setting up pipeline
            if (!Start(_eventStream, "syslogserver", true)) return false;

            // Set up for listening on port
            IPAddress localIp = null;
            if (!string.IsNullOrEmpty(_adapterName))
            {
                localIp = GetLocalIp(_adapterName);
            }
            else
            {
                localIp = IPAddress.IPv6Any;
            }
            var endPoint = new IPEndPoint(localIp, _udpport);
            var PortListener = new UdpClient(AddressFamily.InterNetworkV6);
            PortListener.Client.DualMode = true;
            PortListener.Client.Bind(endPoint);
            PortListener.Client.ReceiveBufferSize = 10 * 1024 * 1024;

            // Setting up syslog parser
            var parser = SyslogParser.CreateDefault();
            parser.AddValueExtractors(new SyslogKeywordValuesExtractor(), new SyslogPatternBasedValuesExtractor());
            
            // Setting up syslog listener
            var listener = new SyslogListener(parser, PortListener);
            listener.Error += Listener_Error;
            listener.EntryReceived += Listener_EntryReceived;
            listener.Subscribe(ConvertToDictionary);
            listener.Start();

            return true;
        }

        private static IPAddress GetLocalIp(string adapterName)
        {
            // return IPAddress.Parse("127.0.0.1");
            UnicastIPAddressInformation unicastIPAddressInformation = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(i => i.Name == adapterName)
                .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                .FirstOrDefault(i =>
                    //i.PrefixOrigin != PrefixOrigin.WellKnown
                    //&& 
                    i.Address.AddressFamily.Equals(AddressFamily.InterNetwork)
                    && !IPAddress.IsLoopback(i.Address)
                    && i.Address != IPAddress.None);

            IPAddress localAddr = null;
            if (unicastIPAddressInformation != null)
            {
                localAddr = unicastIPAddressInformation.Address;
            }

            if (localAddr == null)
            {
                throw new Exception($"Unable to find local address for adapter {adapterName}.");
            }

            return localAddr;
        }

        private void Listener_Error(object sender, SyslogErrorEventArgs e)
        {
            Stop();
            _output.OutputError(e.Error);
        }

        private void Listener_EntryReceived(object sender, SyslogEntryEventArgs e)
        {
            var parseErrors = e.ServerEntry.ParseErrorMessages;
            if(parseErrors != null && parseErrors.Count > 0)
            {
                Stop();
                var strErrors = "Parser errors encountered: " + string.Join(Environment.NewLine, parseErrors);
                _output.OutputError(new Exception(strErrors));
            }
        }

        private void ConvertToDictionary(ServerSyslogEntry serverEntry)
        {
            _eventStream.Broadcast(SyslogEntryToDictionaryConverter.Convert(serverEntry));
        } 
    }
}
