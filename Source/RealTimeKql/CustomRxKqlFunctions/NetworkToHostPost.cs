using System;
using System.Net;
using System.Reactive.Kql;

namespace RealTimeKql
{
    // https://docs.microsoft.com/en-us/windows/win32/api/winsock/nf-winsock-ntohs
    // Use this helper function to convert a u_short from TCP/IP network byte order to host byte order 
    // (which is little-endian on Intel processors).
    public static partial class CustomScalarFunctions
    {
        [KqlScalarFunction("ntohs")]
        public static int NetworkToHostPort(ushort port)
        {
            short res = IPAddress.NetworkToHostOrder((short)port);
            string paddedRes = Convert.ToString(res, 2);
            paddedRes.PadLeft(8, '0');
            return Convert.ToInt32(paddedRes, 2);
        }
    }
}