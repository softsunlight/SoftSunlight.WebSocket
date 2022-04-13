using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace SoftSunlight.WebSocket
{
    /// <summary>
    /// 简易版http响应类，只为WebSocket握手时使用
    /// </summary>
    public class SimpleHttpResponse
    {
        /// <summary>
        /// http版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 响应码
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// 相应消息
        /// </summary>
        public string StatusMessage { get; set; }
        /// <summary>
        /// http响应头
        /// </summary>
        public Dictionary<string, object> ResponseHeaders { get; set; }
        /// <summary>
        /// Content-Type
        /// </summary>
        public string ContentType
        {
            get
            {
                return ResponseHeaders == null || !ResponseHeaders.ContainsKey("Content-Type") ? "" : ResponseHeaders["Content-Type"].ToString();
            }
            set
            {
                if (ResponseHeaders == null)
                {
                    ResponseHeaders = new Dictionary<string, object>();
                }
                ResponseHeaders["Content-Type"] = value;
            }
        }
        /// <summary>
        /// Content-Length
        /// </summary>
        public int ContentLength
        {
            get
            {
                int length = 0;
                try
                {
                    if (ResponseHeaders != null && ResponseHeaders.ContainsKey("Content-Length"))
                    {
                        length = Convert.ToInt32(ResponseHeaders["Content-Length"]);
                    }
                }
                catch (Exception ex)
                {

                }
                return length;
            }
            set
            {
                if (ResponseHeaders == null)
                {
                    ResponseHeaders = new Dictionary<string, object>();
                }
                ResponseHeaders["Content-Length"] = value;
            }
        }
        /// <summary>
        /// 响应体
        /// </summary>
        public byte[] ResponseBody { get; set; }
        /// <summary>
        /// 响应流
        /// </summary>
        public NetworkStream ResponseStream { get; set; }

        /// <summary>
        /// 写入网络流中
        /// </summary>
        public void Write()
        {
            StringBuilder stringBuilder = new StringBuilder();
            //构造相应头
            stringBuilder.Append(Version).Append(" ").Append(StatusCode).Append(" ").Append(StatusMessage).Append(Environment.NewLine);
            if (ResponseHeaders != null && ResponseHeaders.Count > 0)
            {
                foreach (string key in ResponseHeaders.Keys)
                {
                    stringBuilder.Append(key).Append(":").Append(ResponseHeaders[key]).Append(Environment.NewLine);
                }
            }
            if (ResponseBody != null && ResponseBody.Length > 0)
            {
                stringBuilder.Append("Content-Length:").Append(ResponseBody.Length).Append(Environment.NewLine);
                stringBuilder.Append(Environment.NewLine);
            }
            else
            {
                stringBuilder.Append("Content-Length:").Append(0).Append(Environment.NewLine);
                stringBuilder.Append(Environment.NewLine);
            }
            if (ResponseStream != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                ResponseStream.Write(data, 0, data.Length);
                if (ResponseBody != null)
                {
                    ResponseStream.Write(ResponseBody, 0, ResponseBody.Length);
                }
            }
        }

    }
}
