using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SoftSunlight.WebSocket.Server
{
    /// <summary>
    /// WebSocket管理
    /// </summary>
    internal class WebSocketManager
    {
        private static Dictionary<TcpClient, WebSocket> tcpClient2WebSocket = new Dictionary<TcpClient, WebSocket>();
        private static readonly object lockObj = new object();
        /// <summary>
        /// 添加WebSocket
        /// </summary>
        /// <param name="tcpClient"></param>
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
        /// <summary>
        /// 获取WebSocket
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        public static WebSocket Get(TcpClient tcpClient)
        {
            if (tcpClient2WebSocket.ContainsKey(tcpClient))
            {
                return tcpClient2WebSocket[tcpClient];
            }
            return null;
        }
        /// <summary>
        /// 移除WebSocket
        /// </summary>
        /// <param name="tcpClient"></param>
        public static void Remove(TcpClient tcpClient)
        {
            if (tcpClient2WebSocket.ContainsKey(tcpClient))
            {
                tcpClient2WebSocket.Remove(tcpClient);
            }
        }
    }
}
