using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SoftSunlight.WebSocket
{
    /// <summary>
    /// 将WebSocket转换类
    /// </summary>
    public class WebSocketConvert
    {
        /// <summary>
        /// 将WebSocketFrame转换为网络字节要发送的数据
        /// </summary>
        /// <param name="webSocketFrame"></param>
        /// <returns></returns>
        public static byte[] ConvertToByte(WebSocketFrame webSocketFrame)
        {
            if (webSocketFrame == null)
            {
                return null;
            }
            List<byte> frameByteList = new List<byte>();
            frameByteList.Add((byte)(128 + webSocketFrame.Opcode));
            byte seconedByteData = 0;
            List<byte> extPayloadLenBytes = new List<byte>();
            if (webSocketFrame.PayloadData.Length <= 125)
            {
                seconedByteData = (byte)(webSocketFrame.PayloadData.Length);
            }
            else if (webSocketFrame.PayloadData.Length <= ushort.MaxValue)
            {
                seconedByteData = 126;
                extPayloadLenBytes.AddRange(BitConverter.GetBytes((ushort)webSocketFrame.PayloadData.Length));
            }
            else
            {
                seconedByteData = 127;
                extPayloadLenBytes.AddRange(BitConverter.GetBytes((long)webSocketFrame.PayloadData.Length));
            }
            if (webSocketFrame.Mask)
            {
                seconedByteData += 128;
            }
            frameByteList.Add(seconedByteData);
            //扩展长度
            //高位在前
            extPayloadLenBytes.Reverse();
            frameByteList.AddRange(extPayloadLenBytes);
            if (webSocketFrame.Mask)
            {
                //掩码
                frameByteList.AddRange(webSocketFrame.MaskingKey);
            }
            frameByteList.AddRange(webSocketFrame.PayloadData);
            return frameByteList.ToArray();
        }
        /// <summary>
        /// 将网络数据转换为WebSocketFrame
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static WebSocketFrame ConvertToFrame(byte[] datas)
        {
            if (datas == null || datas.Length <= 0)
            {
                return null;
            }
            WebSocketFrame webSocketFrame = new WebSocketFrame();
            //先读第一个字节(FIN(1) RSV(1) RSV(1) RSV(1) opcode(4))
            BitArray firstByteBitArray = new BitArray(new byte[1] { datas[0] });
            webSocketFrame.Fin = firstByteBitArray.Get(firstByteBitArray.Length - 1);
            webSocketFrame.Rsv1 = firstByteBitArray.Get(firstByteBitArray.Length - 2);
            webSocketFrame.Rsv2 = firstByteBitArray.Get(firstByteBitArray.Length - 3);
            webSocketFrame.Rsv3 = firstByteBitArray.Get(firstByteBitArray.Length - 4);
            BitArray opcodeBitArray = new BitArray(4);
            for (var i = 0; i < 4; i++)
            {
                opcodeBitArray.Set(i, firstByteBitArray[i]);
            }
            int[] opcodeArray = new int[1];
            opcodeBitArray.CopyTo(opcodeArray, 0);
            webSocketFrame.Opcode = opcodeArray[0];
            //第二个字节(mask(1) 'payload len'(7))
            BitArray secondByteBitArray = new BitArray(new byte[1] { datas[1] });
            webSocketFrame.Mask = secondByteBitArray.Get(secondByteBitArray.Length - 1);
            BitArray payloadLenBitArray = new BitArray(7);
            for (var i = 0; i < 7; i++)
            {
                payloadLenBitArray.Set(i, secondByteBitArray[i]);
            }
            int[] payloadLenArray = new int[1];
            payloadLenBitArray.CopyTo(payloadLenArray, 0);
            webSocketFrame.PayloadLen = payloadLenArray[0];
            long realLen = webSocketFrame.PayloadLen;
            int maskKeyStart = 2;
            if (webSocketFrame.PayloadLen == 126)
            {
                realLen = BitConverter.ToUInt16(datas, 2);
                maskKeyStart = 4;
            }
            else if (webSocketFrame.PayloadLen == 127)
            {
                realLen = BitConverter.ToInt64(datas, 2);
                maskKeyStart = 12;
            }
            webSocketFrame.ExtPayloadLen = realLen;
            if (webSocketFrame.Mask)
            {
                //Mask
                byte[] maskKeyBytes = new byte[4];
                if (secondByteBitArray.Get(secondByteBitArray.Length - 1))
                {
                    Array.Copy(datas, maskKeyStart, maskKeyBytes, 0, maskKeyBytes.Length);
                }
                webSocketFrame.MaskingKey = maskKeyBytes;
            }
            if (realLen > 0)
            {
                //数据
                byte[] encodeDatas = new byte[realLen];
                Array.Copy(datas, maskKeyStart + webSocketFrame.MaskingKey.Length, encodeDatas, 0, realLen);
                if (webSocketFrame.Mask)
                {
                    //解码
                    byte[] decodeDatas = new byte[realLen];
                    for (var i = 0; i < encodeDatas.Length; i++)
                    {
                        decodeDatas[i] = (byte)(encodeDatas[i] ^ webSocketFrame.MaskingKey[i % 4]);
                    }
                    webSocketFrame.PayloadData = decodeDatas;
                }
                else
                {
                    webSocketFrame.PayloadData = encodeDatas;
                }
            }
            return webSocketFrame;
        }
    }
}
