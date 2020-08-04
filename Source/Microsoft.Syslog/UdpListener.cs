// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Microsoft.Syslog.Internals;
    using Microsoft.Syslog.Model;

    public class UdpListener: Observable<UdpPacket>, IDisposable
    {
        public UdpClient PortListener { get; private set; }
        private bool disposeClientOnDispose;
        public event EventHandler<SyslogErrorEventArgs> Error;

        private Thread _thread; 
        private bool _running;
        
        public UdpListener(IPAddress address = null, int port = 514, int bufferSize = 10 * 1024 * 1024)
        {
            address = address ?? IPAddress.Parse("127.0.0.1");
            var endPoint = new IPEndPoint(address, port);
            PortListener = new UdpClient(endPoint);
            PortListener.Client.ReceiveBufferSize = bufferSize;
            disposeClientOnDispose = true;
        }

        public UdpListener(UdpClient udpClient)
        {
            PortListener = udpClient;
            disposeClientOnDispose = false;
        }

        public void Start()
        {
            if (_running)
            {
                return; 
            }
            _running = true;

            // Important: we need real high-pri thread here, not pool thread from Task.Run()
            // Note: going with multiple threads here results in broken messages, the recieved message gets cut-off
            _thread = new Thread(RunListenerLoop);
            _thread.Priority = ThreadPriority.Highest;
            _thread.Start();
        }

        public void Stop()
        {
            if (_running)
            {
                PortListener.Close();
            }
            _running = false;
            Thread.Sleep(10);
        }

        private void RunListenerLoop()
        {
            try
            {
                var remoteIp = new IPEndPoint(IPAddress.Any, 0);
                while (_running)
                {
                    var bytes = PortListener.Receive(ref remoteIp);
                    var packet = new UdpPacket() { ReceivedOn = DateTime.UtcNow, SourceIpAddress = remoteIp.Address, Data = bytes };
                    Broadcast(packet);
                }
            }
            catch (Exception ex)
            {
                if (!_running)
                    return; // it is closing socket
                OnError(ex); 
            }
        }

        private void OnError(Exception error)
        {
            Error?.Invoke(this, new SyslogErrorEventArgs(error));
        }

        public void Dispose()
        {
            if (disposeClientOnDispose && PortListener != null)
            {
                PortListener.Dispose();
            }
        }
    }

}
