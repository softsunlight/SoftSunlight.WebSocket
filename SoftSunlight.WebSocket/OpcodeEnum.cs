using System;
using System.Collections.Generic;
using System.Text;

namespace SoftSunlight.WebSocket
{
    /// <summary>
    /// WebSocket Opcode枚举
    /// </summary>
    public enum OpcodeEnum
    {
        /// <summary>
        /// 附加数据帧
        /// </summary>
        Additional = 0,
        /// <summary>
        /// 文本数据帧
        /// </summary>
        Text = 1,
        /// <summary>
        /// 二进制数据帧
        /// </summary>
        Binary = 2,
        /// <summary>
        /// 连接关闭
        /// </summary>
        Close = 8,
        /// <summary>
        /// ping
        /// </summary>
        Ping = 9,
        /// <summary>
        /// pong
        /// </summary>
        Pong = 10,
    }
}
