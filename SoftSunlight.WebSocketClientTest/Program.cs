using SoftSunlight.WebSocket.Client;
using SoftSunlight.WebSocket.Server;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftSunlight.WebSocketTest
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 20; i++)
            {
                int j = i;
                Task.Run(() =>
                {
                    var id = j.ToString();
                    WebSocketClient client = new WebSocketClient("ws://127.0.0.1:5001");
                    client.OnOpen += () =>
                    {
                        Console.WriteLine($"{id}:open");
                        Task.Run(() =>
                        {
                            while (true)
                            {
                                client.Send(id);
                                Thread.Sleep(50);
                            }
                        });
                    };
                    client.OnMessage += (byte[] data) =>
                    {
                        Console.WriteLine($"{id}:" + Encoding.UTF8.GetString(data));
                    };
                    client.OnClose += () =>
                    {
                        Console.WriteLine($"{id}:close");
                    };
                    client.OnError += (errorMsg) =>
                    {
                        Console.WriteLine($"{id}:error:{errorMsg}");
                    };
                    client.Start();
                });
            }
            Console.Read();
        }
    }
}
