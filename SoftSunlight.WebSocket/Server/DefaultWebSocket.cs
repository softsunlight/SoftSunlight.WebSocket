using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SoftSunlight.WebSocket.Server
{
    internal class DefaultWebSocket : WebSocket
    {
        public DefaultWebSocket(TcpClient client)
        {
            this.client = client;
        }
    }
}
