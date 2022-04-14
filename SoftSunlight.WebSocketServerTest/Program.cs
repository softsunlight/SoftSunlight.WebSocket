using SoftSunlight.WebSocket.Server;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SoftSunlight.WebSocketServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            WebSocketServer server = new WebSocketServer("127.0.0.1", 5001);
            server.Start();
            while (true)
            {
                WebSocket.Server.WebSocket webSocket = server.AcceptWebSocket();
                webSocket.OnMessage += (byte[] data) =>
                {
                    webSocket.Send(Encoding.UTF8.GetString(data));
                };
            }
        }
    }
}
