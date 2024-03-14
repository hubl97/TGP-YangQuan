using System;
using System.Collections.Generic;
using System.Text;

namespace CIPS.CTC.NET
{
    /// <summary>
    /// ͨ������
    /// </summary>
    public enum E_NET_Type
    {
        /// <summary>
        /// 422ͨ��
        /// </summary>
        I_COMPORT_422=1,
        /// <summary>
        /// 232ͨ��
        /// </summary>
        I_COMPORT_232=11,
        /// <summary>
        /// 485ͨ��
        /// </summary>
        I_COMPORT_485=12,
        /// <summary>
        /// TCP�ͻ���
        /// </summary>
        I_TCP_CLIENT=2,
        /// <summary>
        /// TCP�����
        /// </summary>
        I_TCP_SERVER=3,
        /// <summary>
        /// UDP������
        /// </summary>
        I_UDP_CLIENT=4,
        /// <summary>
        /// UDP��������
        /// </summary>
        I_UDP_CONNECTED=5,
        /// <summary>
        /// CIPSMQ��TCP�ͻ���
        /// </summary>
        I_TCPCLIENT_MQ=6,
        /// <summary>
        /// CIPSMQ��TCP�����
        /// </summary>
        I_TCPSERVER_MQ=7,
        /// <summary>
        /// ����CIPSMQ�Ŀͻ���
        /// </summary>
        I_UDPCLIENT_MQ=8
    }
}
