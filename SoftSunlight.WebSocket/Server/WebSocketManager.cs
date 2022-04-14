using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SoftSunlight.WebSocket.Server
{
    internal class WebSocketManager
    {
        private static Dictionary<TcpClient, WebSocket> tcpClient2WebSocket = new Dictionary<TcpClient, WebSocket>();
        private static readonly object lockObj = new object();
        public static void Add(TcpClient tcpClient)
        {
            lock (lockObj)
            {
                if (!tcpClient2WebSocket.ContainsKey(tcpClient))
                {
                    tcpClient2WebSocket[tcpClient] = new DefaultWebSocket(tcpClient);
                }
            }
        }

        public static WebSocket Get(TcpClient tcpClient)
        {
            if (tcpClient2WebSocket.ContainsKey(tcpClient))
            {
                return tcpClient2WebSocket[tcpClient];
            }
            return null;
        }

    }
}
