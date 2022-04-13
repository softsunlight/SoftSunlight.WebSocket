﻿using SoftSunlight.WebSocket.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SoftSunlight.WebSocket
{
    /// <summary>
    /// WebSocket服务端
    /// </summary>
    public class WebSocketServer
    {
        /// <summary>
        /// tcp服务端
        /// </summary>
        private TcpListener tcpListener;
        /// <summary>
        /// WebSocket监听的IP地址
        /// </summary>
        private string ip;
        /// <summary>
        /// 端口
        /// </summary>
        private int port;
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="ip">WebSocket监听的IP地址</param>
        /// <param name="port">监听的端口</param>
        public WebSocketServer(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void Start()
        {
            try
            {
                if (tcpListener == null)
                {
                    if (string.IsNullOrEmpty(ip))
                    {
                        throw new ArgumentNullException("the parameter ip is must!");
                    }
                    tcpListener = new TcpListener(IPAddress.Parse(ip), port);
                }
                tcpListener.Start();
                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Task.Run(() =>
                    {
                        List<WebSocketFrame> frames = new List<WebSocketFrame>();
                        while (true)
                        {
                            //http1.1 默认是持久链接
                            try
                            {
                                NetworkStream networkStream = tcpClient.GetStream();
                                MemoryStream memoryStream = new MemoryStream();
                                int recvTotals = 0;
                                int readed = 0;
                                do
                                {
                                    byte[] buffer = new byte[512];
                                    readed = networkStream.Read(buffer, 0, buffer.Length);
                                    memoryStream.Write(buffer, 0, readed);
                                    recvTotals += readed;
                                } while (networkStream.DataAvailable);
                                if (recvTotals > 0)
                                {
                                    byte[] requestDatas = memoryStream.ToArray();
                                    if (memoryStream != null)
                                    {
                                        memoryStream.Close();
                                        memoryStream.Dispose();
                                        memoryStream = null;
                                    }
                                    if (HttpUtils.IsWebSocket(requestDatas))
                                    {
                                        WebSocketFrame webSocketFrame = WebSocketConvert.ConvertToFrame(requestDatas);
                                        frames.Add(webSocketFrame);
                                        if (webSocketFrame.Fin)
                                        {
                                            //if (webSocketFrame.Opcode == OpcodeEnum.Text || webSocketFrame.Opcode == OpcodeEnum.Binary)
                                            //{
                                            //    //httpContext.WebSocket
                                            //}
                                            //开始处理WebSocket请求
                                        }
                                    }
                                    else
                                    {
                                        StringBuilder responseBuilder = new StringBuilder();
                                        SimpleHttpResponse simpleHttpResponse = new SimpleHttpResponse();
                                        try
                                        {
                                            SimpleHttpRequest simpleHttpRequest = HttpUtils.GetSimpleHttpRequest(requestDatas);
                                            simpleHttpResponse.Version = simpleHttpRequest == null ? "HTTP/1.1" : simpleHttpRequest.Version;
                                            simpleHttpResponse.ResponseStream = tcpClient.GetStream();
                                            if (simpleHttpRequest == null)
                                            {
                                                //Bad Request 400
                                                simpleHttpResponse.StatusCode = "400";
                                                simpleHttpResponse.StatusMessage = "Bad Request";
                                            }
                                            else
                                            {
                                                if (simpleHttpRequest.RequestHeaders != null && simpleHttpRequest.RequestHeaders.ContainsKey("Upgrade") && simpleHttpRequest.RequestHeaders["Upgrade"].Equals("websocket"))
                                                {
                                                    //
                                                    var buffer = Encoding.UTF8.GetBytes(simpleHttpRequest.RequestHeaders["Sec-WebSocket-Key"].ToString() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
                                                    var data = SHA1.Create().ComputeHash(buffer);
                                                    simpleHttpResponse.StatusCode = "101";
                                                    simpleHttpResponse.StatusMessage = "Switching Protocols";
                                                    if (simpleHttpResponse.ResponseHeaders == null)
                                                    {
                                                        simpleHttpResponse.ResponseHeaders = new Dictionary<string, object>();
                                                    }
                                                    simpleHttpResponse.ResponseHeaders.Add("Connection", "Upgrade");
                                                    simpleHttpResponse.ResponseHeaders.Add("Upgrade", "websocket");
                                                    simpleHttpResponse.ResponseHeaders.Add("Sec-WebSocket-Accept", Convert.ToBase64String(data));
                                                }
                                                else
                                                {
                                                    simpleHttpResponse.StatusCode = "400";
                                                    simpleHttpResponse.StatusMessage = "Bad Request";
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        finally
                                        {
                                            if (simpleHttpResponse != null)
                                            {
                                                simpleHttpResponse.Write();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //不知道为什么，每次有个空连接
                                    if (tcpClient != null)
                                    {
                                        tcpClient.Close();
                                        tcpClient.Dispose();
                                        tcpClient = null;
                                    }
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (tcpClient != null)
                                {
                                    tcpClient.Close();
                                    tcpClient.Dispose();
                                    tcpClient = null;
                                }
                                break;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
