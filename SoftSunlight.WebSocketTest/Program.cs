using SoftSunlight.WebSocket;
using System;
using System.Text;

namespace SoftSunlight.WebSocketTest
{
    class Program
    {
        static void Main(string[] args)
        {
            WebSocketClient client = new WebSocketClient("ws://127.0.0.1:5000");
            client.OnOpen += () =>
            {
                Console.WriteLine("open");
                client.Send("11");
            };
            client.OnMessage += (byte[] data) =>
            {
                Console.WriteLine(Encoding.UTF8.GetString(data));
            };
            client.OnClose += () =>
            {
                Console.WriteLine("close");
            };
            client.Start();
        }
    }
}
