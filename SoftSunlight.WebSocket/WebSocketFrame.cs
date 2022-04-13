using System;
using System.Collections.Generic;
using System.Text;

namespace SoftSunlight.WebSocket
{
    /// <summary>
    /// WebSocket数据帧
    /// </summary>
    public class WebSocketFrame
    {
        /// <summary>
        /// 是否结束标识位
        /// </summary>
        public bool Fin { get; set; }
        /// <summary>
        /// RSV1
        /// </summary>
        public bool Rsv1 { get; set; }
        /// <summary>
        /// RSV2
        /// </summary>
        public bool Rsv2 { get; set; }
        /// <summary>
        /// RSV3
        /// </summary>
        public bool Rsv3 { get; set; }
        /// <summary>
        /// opcode
        /// 0 表示附加数据帧
        /// 1 表示文本数据帧
        /// 2 表示二进制数据帧
        /// 3-7 暂无定义，为以后的非控制帧保留
        /// 8 表示连接关闭
        /// 9 表示ping
        /// 10 表示pong
        /// 11-15暂无定义，为以后的控制帧保留
        /// </summary>
        public int Opcode { get; set; }
        /// <summary>
        /// 掩码标识位，客户端->服务端为true,服务端->客户端为false,否则就断开连接
        /// </summary>
        public bool Mask { get; set; }
        /// <summary>
        /// 数据长度
        /// </summary>
        public int PayloadLen { get; set; }
        /// <summary>
        /// 扩展数据长度
        /// </summary>
        public long ExtPayloadLen { get; set; }
        /// <summary>
        /// Masking key
        /// </summary>
        public byte[] MaskingKey { get; set; }
        /// <summary>
        /// 消息数据
        /// </summary>
        public byte[] PayloadData { get; set; }
    }
}
