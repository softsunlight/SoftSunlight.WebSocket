using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SoftSunlight.WebSocket.Server
{
    public abstract class WebSocket
    {
        protected TcpClient client;

        public TcpClient TcpClient { get { return client; } }

        /// <summary>
        /// 消息处理事件
        /// </summary>
        public Action<byte[]> OnMessage { get; set; }

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
            //服务端发送的帧不必经过掩码处理，否则断开WebSocket连接
            webSocketFrame.Mask = false;
            webSocketFrame.MaskingKey = null;
            webSocketFrame.PayloadData = sendDatas;
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
    }
}
