using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftSunlight.WebSocket.Utils
{
    public class HttpUtils
    {
        /// <summary>
        /// 判断是否是websocket数据
        /// </summary>
        /// <param name="requestDatas"></param>
        /// <returns></returns>
        public static bool IsWebSocket(byte[] requestDatas)
        {
            try
            {
                WebSocketFrame webSocketFrame = WebSocketConvert.ConvertToFrame(requestDatas);
                long frameBytes = 2;
                if (webSocketFrame.PayloadLen <= 125)
                {
                    frameBytes += webSocketFrame.PayloadLen;
                }
                else if (webSocketFrame.PayloadLen == 126)
                {
                    frameBytes += 2;
                    frameBytes += webSocketFrame.ExtPayloadLen;
                }
                else if (webSocketFrame.PayloadLen == 127)
                {
                    frameBytes += 8;
                    frameBytes += webSocketFrame.ExtPayloadLen;
                }
                if (webSocketFrame.Mask)
                {
                    frameBytes += 4;
                }
                if (frameBytes == requestDatas.Length)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        /// <summary>
        /// 判断是否是http请求
        /// </summary>
        /// <param name="requestDatas"></param>
        /// <returns></returns>
        public static bool IsHttpRequest(byte[] requestDatas)
        {
            //刺探性的获取前10个字节
            int length = 10;
            if (length > requestDatas.Length)
            {
                length = requestDatas.Length;
            }
            byte[] bytes = new byte[length];
            Array.Copy(requestDatas, bytes, length);
            string str = Encoding.UTF8.GetString(bytes);
            Type type = typeof(HttpRequestMethod);
            if (Regex.IsMatch(str, @"(?is)" + string.Join("|", type.GetFields().Select(p => p.Name))))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取http请求
        /// </summary>
        /// <param name="requestDatas"></param>
        /// <returns></returns>
        public static SimpleHttpRequest GetSimpleHttpRequest(byte[] requestDatas)
        {
            if (requestDatas.Length <= 0)
            {
                return null;
            }
            SimpleHttpRequest simpleHttpRequest = new SimpleHttpRequest();
            int requestBodyStart = int.MaxValue;
            int lastSpaceCharIndex = 0;
            for (int i = 0; i < requestDatas.Length; i++)
            {
                if ((requestDatas[i] == 13 && requestDatas[i + 1] == 10) || i == requestDatas.Length - 1)
                {
                    //遇到空行，则下一行是表单域
                    if (i - lastSpaceCharIndex == 0)
                    {
                        if (requestBodyStart == int.MaxValue)
                        {
                            requestBodyStart = i + 2;
                            lastSpaceCharIndex = requestBodyStart;
                        }
                        break;
                    }
                    if (i <= lastSpaceCharIndex)
                    {
                        continue;
                    }
                    if (i < requestBodyStart)
                    {
                        string tempStr = Encoding.UTF8.GetString(requestDatas, lastSpaceCharIndex, i == requestDatas.Length ? i - lastSpaceCharIndex + 1 : i - lastSpaceCharIndex);
                        if (lastSpaceCharIndex == 0)
                        {
                            string[] tempArr = tempStr.Split(' ');
                            simpleHttpRequest.Method = tempArr[0];
                            string requestUri = tempArr[1];
                            string[] urlAndQueryString = requestUri.Split('?');
                            if (urlAndQueryString.Length >= 2)
                            {
                                simpleHttpRequest.RequstUrl = urlAndQueryString[0];
                                string[] queryStrings = urlAndQueryString[1].Split('&');
                                foreach (var queryString in queryStrings)
                                {
                                    string[] keyvalues = queryString.Split('=');
                                    if (keyvalues.Length >= 2)
                                    {
                                        if (simpleHttpRequest.QueryString == null)
                                        {
                                            simpleHttpRequest.QueryString = new Dictionary<string, List<string>>();
                                        }
                                        simpleHttpRequest.QueryString[keyvalues[0].Trim()].Add(keyvalues[1].Trim());
                                    }
                                }
                            }
                            else if (urlAndQueryString.Length >= 1)
                            {
                                simpleHttpRequest.RequstUrl = urlAndQueryString[0];
                            }
                            simpleHttpRequest.Version = tempArr[2];
                        }
                        else
                        {
                            //请求头
                            if (simpleHttpRequest.RequestHeaders == null)
                            {
                                simpleHttpRequest.RequestHeaders = new Dictionary<string, object>();
                            }
                            Match m = Regex.Match(tempStr, @"(?is)(?<key>[^:]*)\s*:\s*(?<value>.*)");
                            if (m.Success)
                            {
                                string key = m.Groups["key"].Value;
                                string value = m.Groups["value"].Value;
                                if (key.Equals("Content-Length", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    int length = 0;
                                    try
                                    {
                                        length = Convert.ToInt32(value);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    simpleHttpRequest.RequestHeaders[key] = length;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        simpleHttpRequest.RequestHeaders[key] = value;
                                    }
                                }
                            }
                        }
                    }
                    {
                        lastSpaceCharIndex = i + 2;
                    }
                }
            }
            return simpleHttpRequest;
        }
        /// <summary>
        /// 获取http响应
        /// </summary>
        /// <param name="responseDatas"></param>
        /// <returns></returns>
        public static SimpleHttpResponse GetSimpleHttpResponse(byte[] responseDatas)
        {
            if (responseDatas.Length <= 0)
            {
                return null;
            }
            SimpleHttpResponse simpleHttpResponse = new SimpleHttpResponse();
            int requestBodyStart = int.MaxValue;
            int lastSpaceCharIndex = 0;
            for (int i = 0; i < responseDatas.Length; i++)
            {
                if ((responseDatas[i] == 13 && responseDatas[i + 1] == 10) || i == responseDatas.Length - 1)
                {
                    //遇到空行，则下一行是表单域
                    if (i - lastSpaceCharIndex == 0)
                    {
                        if (requestBodyStart == int.MaxValue)
                        {
                            requestBodyStart = i + 2;
                            lastSpaceCharIndex = requestBodyStart;
                        }
                        break;
                    }
                    if (i <= lastSpaceCharIndex)
                    {
                        continue;
                    }
                    if (i < requestBodyStart)
                    {
                        string tempStr = Encoding.UTF8.GetString(responseDatas, lastSpaceCharIndex, i == responseDatas.Length ? i - lastSpaceCharIndex + 1 : i - lastSpaceCharIndex);
                        if (lastSpaceCharIndex == 0)
                        {
                            string[] tempArr = tempStr.Split(' ');
                            simpleHttpResponse.Version = tempArr[0];
                            simpleHttpResponse.StatusCode = tempArr[1];
                            if (tempArr.Length > 3)
                            {
                                simpleHttpResponse.StatusMessage = string.Join(" ", tempArr.Skip(2));
                            }
                            else
                            {
                                simpleHttpResponse.StatusMessage = tempArr[2];
                            }
                        }
                        else
                        {
                            //请求头
                            if (simpleHttpResponse.ResponseHeaders == null)
                            {
                                simpleHttpResponse.ResponseHeaders = new Dictionary<string, object>();
                            }
                            Match m = Regex.Match(tempStr, @"(?is)(?<key>[^:]*)\s*:\s*(?<value>.*)");
                            if (m.Success)
                            {
                                string key = m.Groups["key"].Value;
                                string value = m.Groups["value"].Value;
                                if (key.Equals("Content-Length", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    int length = 0;
                                    try
                                    {
                                        length = Convert.ToInt32(value);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    simpleHttpResponse.ResponseHeaders[key] = length;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        simpleHttpResponse.ResponseHeaders[key] = value;
                                    }
                                }
                            }
                        }
                    }
                    {
                        lastSpaceCharIndex = i + 2;
                    }
                }
            }
            return simpleHttpResponse;
        }
        /// <summary>
        /// 获取WebSocket数据帧长度
        /// </summary>
        /// <param name="requestDatas"></param>
        /// <returns></returns>
        public static long GetWebSocketFrameLength(byte[] requestDatas)
        {
            try
            {
                WebSocketFrame webSocketFrame = WebSocketConvert.ConvertToFrame(requestDatas);
                long frameBytes = 2;
                if (webSocketFrame.PayloadLen <= 125)
                {
                    frameBytes += webSocketFrame.PayloadLen;
                }
                else if (webSocketFrame.PayloadLen == 126)
                {
                    frameBytes += 2;
                    frameBytes += webSocketFrame.ExtPayloadLen;
                }
                else if (webSocketFrame.PayloadLen == 127)
                {
                    frameBytes += 8;
                    frameBytes += webSocketFrame.ExtPayloadLen;
                }
                if (webSocketFrame.Mask)
                {
                    frameBytes += 4;
                }
                return frameBytes;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }
    }
}
