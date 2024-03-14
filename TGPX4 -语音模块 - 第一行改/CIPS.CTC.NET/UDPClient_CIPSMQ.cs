using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;

namespace CIPS.CTC.NET
{
    public class UDPClient_CIPSMQ : CommResource
    {
        /// <summary>
        /// 
        /// </summary>
        private CIPS.MQ.UdpReceiver  mqClient_UDP = null;
        /// <summary>
        /// 
        /// </summary>
        private CTCClientCmd.ComNetConfig hNetConfig = null;
        /// <summary>
        /// 
        /// </summary>
        private string C_IP = "127.0.0.1";
        /// <summary>
        /// 
        /// </summary>
        private int I_PORT = 0;
        /// <summary>
        /// 
        /// </summary>
        private Mutex hMutex = new Mutex();
        /// <summary>
        /// 
        /// </summary>
        private bool bRecvOk = false;

        #region 构造函数
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="res_na"></param>
        public UDPClient_CIPSMQ(string ip, int port, string res_na,System .Net .IPEndPoint ipe)
        {
            C_IP = ip;
            I_PORT = port;
            if( ipe ==null )
                mqClient_UDP = new CIPS.MQ.UdpReceiver(null, ip, port);
            else
                mqClient_UDP = new CIPS.MQ.UdpReceiver(null, ip, port,ipe );
            mqClient_UDP.PackageReceiveFinished += new CIPS.MQ.UdpReceiver.PackageReceiveFinishedEvent(mqClient_UDP_PackageReceiveFinished);
            hNetConfig = new CTCClientCmd.ComNetConfig(CTCClientCmd.CTCRes_ID.GetID(), res_na, CTCClientCmd.ComNetConfig.E_NET_TYPE.I_UDP, ip, port);

        }


        #endregion

        #region 数据到达处理
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buffer"></param>
        /// <param name="Index"></param>
        void mqClient_UDP_PackageReceiveFinished(CIPS.MQ.UdpReceiver sender, byte[] buffer, byte Index)
        {
            //throw new Exception("The method or operation is not implemented.");
            hMutex.WaitOne();
            try
            {
                if (buffer != null)
                {
                    hQueue_Recv.Enqueue(buffer);
                    bRecvOk = true;
                }
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
        }
        #endregion

        #region 清除接收数据，启动重新接收
        /// <summary>
        /// 
        /// </summary>
        public override void Reset()
        {
            mqClient_UDP.Clear();
        }
        #endregion

        #region 判断广播接收是否正常
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool bOK()
        {
            //return base.bOK();
            return bRecvOk ;
        }
        #endregion

        #region 得到通信配置
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object GetConfig()
        {
            return hNetConfig;
            //return base.GetConfig();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sysid"></param>
        /// <param name="restp"></param>
        /// <param name="chid"></param>
        public override void SetConfig(int sysid, CTCClientCmd.CTCResource.E_Resource_Type restp, byte chid)
        {
            //base.SetConfig(sysid, restp, chid);
            hNetConfig.I_SubSys_ID = sysid;
            hNetConfig.e_ResType = restp;
            hNetConfig.I_CHID = chid;
        }

        #endregion

        #region 保存端口数据
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="edt"></param>
        public override void PushData(byte[] d, CTCClientCmd.CTCClientCmd.E_DATA_TYPE edt)
        {
            hNetConfig.Push(d, edt);
            //base.PushData(d, edt);
        }
        #endregion


    }
}
