using SoftSunlight.WebSocket.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SoftSunlight.WebSocket.Client
{
    /// <summary>
    /// WebSocket客户端
    /// </summary>
    public class WebSocketClient
    {
        /// <summary>
        /// WebSocket地址
        /// </summary>
        private string url;
        /// <summary>
        /// tcp客户端
        /// </summary>
        private TcpClient client;
        /// <summary>
        /// WebSocket连接打开事件
        /// </summary>
        public Action OnOpen { get; set; }
        /// <summary>
        /// WebSocket消息接收事件
        /// </summary>
        public Action<byte[]> OnMessage { get; set; }
        /// <summary>
        /// WebSocket连接关闭事件
        /// </summary>
        public Action OnClose { get; set; }
        /// <summary>
        /// WebSocket错误事件
        /// </summary>
        public Action<string> OnError { get; set; }
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="url">WebSocket地址</param>
        public WebSocketClient(string url)
        {
            this.url = url;
        }
        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="txtMessage"></param>
        public void Send(string txtMessage)
        {
            byte[] payloadDatas = Encoding.UTF8.GetBytes(txtMessage);
            Send(OpcodeEnum.Text, payloadDatas);
        }
        /// <summary>
        /// 发送二进制数据
        /// </summary>
        /// <param name="datas"></param>
        public void Send(byte[] datas)
        {
            Send(OpcodeEnum.Binary, datas);
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="sendDatas"></param>
        private void Send(OpcodeEnum opcode, byte[] sendDatas)
        {
            //构造数据帧
            WebSocketFrame webSocketFrame = new WebSocketFrame();
            webSocketFrame.Fin = true;
            webSocketFrame.Opcode = (int)opcode;
            //客户端发送的帧必须经过掩码处理，否则断开WebSocket连接
            webSocketFrame.Mask = true;
            webSocketFrame.MaskingKey = BitConverter.GetBytes(new Random().Next(int.MinValue, int.MaxValue));
            if (sendDatas == null)
            {
                webSocketFrame.PayloadLen = 0;
            }
            else
            {
                byte[] encodeData = new byte[sendDatas.Length];
                //掩码处理
                for (var i = 0; i < sendDatas.Length; i++)
                {
                    encodeData[i] = (byte)(sendDatas[i] ^ webSocketFrame.MaskingKey[i % 4]);
                }
                webSocketFrame.PayloadData = encodeData;
                webSocketFrame.ExtPayloadLen = webSocketFrame.PayloadData.Length;
                if (webSocketFrame.PayloadData.Length > 126 && webSocketFrame.PayloadData.Length <= ushort.MaxValue)
                {
                    webSocketFrame.PayloadLen = 126;
                }
                else
                {
                    webSocketFrame.PayloadLen = 127;
                }
            }
            if (client.Connected)
            {
                NetworkStream networkStream = null;
                try
                {
                    networkStream = client.GetStream();
                    byte[] data = WebSocketConvert.ConvertToByte(webSocketFrame);
                    networkStream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {

                }
            }

        }
        /// <summary>
        /// 启动WebSocket
        /// </summary>
        public void Start()
        {
            //ws://127.0.0.1:500/aaa?p=1
            //wss://127.0.0.1:500/aaa?p=1
            Regex ipAndPortReg = new Regex(@"(?is)wss?://(?<ip>[^:]*)(:(?<port>\d+))?(?<requestPath>/?.*)");
            Match ipAndPortMatch = ipAndPortReg.Match(url);
            string ip = string.Empty;
            int port = 0;
            string requestPath = string.Empty;
            if (ipAndPortMatch.Success)
            {
                ip = ipAndPortMatch.Groups["ip"].Value;
                try
                {
                    port = Convert.ToInt32(ipAndPortMatch.Groups["port"].Value);
                }
                catch (Exception ex)
                {

                }
                requestPath = ipAndPortMatch.Groups["requestPath"].Value;
            }
            if (string.IsNullOrEmpty(requestPath))
            {
                requestPath = "/";
            }
            if (client == null)
            {
                client = new TcpClient();
            }
            if (!client.Connected)
            {
                client.Connect(ip, port);
            }
            NetworkStream networkStream = client.GetStream();
            //发送websocket升级协议
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"GET {requestPath} HTTP/1.1\r\n");
            stringBuilder.Append($"Host: {ip}:{port}\r\n");
            stringBuilder.Append($"Connection: Upgrade\r\n");
            stringBuilder.Append($"Sec-WebSocket-Key: {GetWebSocketKey()}\r\n");
            stringBuilder.Append($"Sec-WebSocket-Version: 13\r\n");
            stringBuilder.Append($"Upgrade: websocket\r\n");
            stringBuilder.Append($"\r\n");
            byte[] data = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            networkStream.Write(data, 0, data.Length);
            List<WebSocketFrame> frames = new List<WebSocketFrame>();
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[256];
                    MemoryStream memoryStream = new MemoryStream();
                    int recvTotals = 0;
                    do
                    {
                        int readed = networkStream.Read(buffer, 0, buffer.Length);
                        memoryStream.Write(buffer, 0, readed);
                        recvTotals += readed;
                    } while (networkStream.DataAvailable);
                    if (recvTotals > 0)
                    {
                        byte[] responseDatas = memoryStream.ToArray();
                        if (HttpUtils.IsWebSocket(responseDatas))
                        {
                            WebSocketFrame webSocketFrame = WebSocketConvert.ConvertToFrame(responseDatas);
                            frames.Add(webSocketFrame);
                            if (webSocketFrame.Fin)
                            {
                                //服务端发送的数据帧不作掩码处理
                                if (webSocketFrame.Mask)
                                {
                                    break;
                                }
                                if (webSocketFrame.Opcode == (int)OpcodeEnum.Text || webSocketFrame.Opcode == (int)OpcodeEnum.Binary)
                                {
                                    OnMessage?.Invoke(frames.SelectMany(p => p.PayloadData).ToArray());
                                }
                                else
                                {
                                    if (webSocketFrame.Opcode == (int)OpcodeEnum.Close)
                                    {
                                        break;
                                    }
                                    else if (webSocketFrame.Opcode == (int)OpcodeEnum.Ping)
                                    {
                                        Send(OpcodeEnum.Pong, null);
                                    }
                                }
                                frames.Clear();
                            }
                        }
                        else
                        {
                            SimpleHttpResponse simpleHttpResponse = HttpUtils.GetSimpleHttpResponse(responseDatas);
                            if (simpleHttpResponse == null)
                            {
                                OnError?.Invoke("服务端无响应!");
                                OnClose?.Invoke();
                            }
                            else
                            {
                                if (simpleHttpResponse.StatusCode == "101")
                                {
                                    if (simpleHttpResponse.ResponseHeaders != null && simpleHttpResponse.ResponseHeaders.ContainsKey("Sec-WebSocket-Accept"))
                                    {
                                        OnOpen?.Invoke();
                                    }
                                    else
                                    {
                                        OnError?.Invoke("the WebSocket not Accept");
                                        OnClose?.Invoke();
                                    }
                                }
                                else
                                {
                                    OnError?.Invoke(simpleHttpResponse.StatusMessage);
                                    OnClose?.Invoke();
                                }
                            }
                        }
                    }
                    else
                    {
                        //对方关闭了连接，接收数据会一直为空
                        if (client != null)
                        {
                            client.Close();
                        }
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (client != null)
                    {
                        client.Close();
                    }
                    //Log.Write("接收数据异常", ex);
                    break;
                }
            }
            OnClose?.Invoke();
        }

        /// <summary>
        /// 获取Sec-WebSocket-Key
        /// </summary>
        /// <returns></returns>
        private string GetWebSocketKey()
        {
            byte[] data = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                data[i] = (byte)new Random((int)DateTime.Now.Ticks).Next(0, byte.MaxValue);
                Thread.Sleep(1);
            }
            return Convert.ToBase64String(data);
        }

    }
}
