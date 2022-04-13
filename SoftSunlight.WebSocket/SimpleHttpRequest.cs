using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SoftSunlight.WebSocket
{
    /// <summary>
    /// 简易版http请求类，只为WebSocket握手时使用
    /// </summary>
    public class SimpleHttpRequest
    {
        /// <summary>
        /// 获取url或表单域中的参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> this[string key]
        {
            get
            {
                return QueryString.ContainsKey(key) ? QueryString[key] : null;
            }
        }
        /// <summary>
        /// 请求方法
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string RequstUrl { get; set; }
        /// <summary>
        /// http版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// http请求头
        /// </summary>
        public Dictionary<string, object> RequestHeaders { get; set; }
        /// <summary>
        /// url地址中的请求参数
        /// </summary>
        public Dictionary<string, List<string>> QueryString { get; set; }
    }

    /// <summary>
    /// http request methods
    /// </summary>
    public class HttpRequestMethod
    {
        public readonly string Connect = "CONNECT";
        public readonly string DELETE = "DELETE";
        public readonly string GET = "GET";
        public readonly string HEAD = "HEAD";
        public readonly string OPTIONS = "OPTIONS";
        public readonly string PATCH = "PATCH";
        public readonly string POST = "POST";
        public readonly string PUT = "PUT";
        public readonly string TRACE = "TRACE";
    }
}
