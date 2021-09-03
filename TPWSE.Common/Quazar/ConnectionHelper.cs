using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace QuazarAPI.Networking.Standard
{
    public static class ConnectionHelper
    {
        public static TcpClient Connect(IPAddress address, int port)
        {
            return new TcpClient(address.ToString(), port);
        }
    }
}
