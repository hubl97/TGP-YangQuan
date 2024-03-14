using System;
using System.Collections.Generic;
using System.Text;

namespace CIPS.CTC.NET
{
    /// <summary>
    /// 通信类型
    /// </summary>
    public enum E_NET_Type
    {
        /// <summary>
        /// 422通信
        /// </summary>
        I_COMPORT_422=1,
        /// <summary>
        /// 232通信
        /// </summary>
        I_COMPORT_232=11,
        /// <summary>
        /// 485通信
        /// </summary>
        I_COMPORT_485=12,
        /// <summary>
        /// TCP客户端
        /// </summary>
        I_TCP_CLIENT=2,
        /// <summary>
        /// TCP服务端
        /// </summary>
        I_TCP_SERVER=3,
        /// <summary>
        /// UDP无连接
        /// </summary>
        I_UDP_CLIENT=4,
        /// <summary>
        /// UDP基于连接
        /// </summary>
        I_UDP_CONNECTED=5,
        /// <summary>
        /// CIPSMQ的TCP客户端
        /// </summary>
        I_TCPCLIENT_MQ=6,
        /// <summary>
        /// CIPSMQ的TCP服务端
        /// </summary>
        I_TCPSERVER_MQ=7,
        /// <summary>
        /// 基于CIPSMQ的客户端
        /// </summary>
        I_UDPCLIENT_MQ=8
    }
}
