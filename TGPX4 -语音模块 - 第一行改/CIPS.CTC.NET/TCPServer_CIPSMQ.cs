using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Data;
using System.Threading;

namespace CIPS.CTC.NET
{
    public  class TCPServer_CIPSMQ:CommResource
    {
        /// <summary>
        /// 
        /// </summary>
        private CIPS.MQ.MqServer mqServer_TCP = null;
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

        #region 构造函数
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="res_na"></param>
        public TCPServer_CIPSMQ(string ip, int port, string res_na,System .Net .IPEndPoint ipe)
        {
            C_IP = ip;
            I_PORT = port;
            if( ipe ==null )
                mqServer_TCP = new CIPS.MQ.MqServer(null, I_PORT);
            else 
            {
                mqServer_TCP = new CIPS.MQ.MqServer(null, I_PORT);
                
            }
            mqServer_TCP.myEvent.DataArrived += new CIPS.MQ.MqEvent.DataArrivedEvt(myEvent_DataArrived);
            
            hNetConfig = new CTCClientCmd.ComNetConfig(CTCClientCmd.CTCRes_ID.GetID(), res_na, CTCClientCmd.ComNetConfig.E_NET_TYPE.I_TCP, ip, port);

            Thread hThread_Send = new System.Threading.Thread(Thread_SendPort);
            hThread_Send.Name = "CIPSMQ Send Thread";
            hThread_Send.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Send.IsBackground = true;
            hThread_Send.Start();

        }
        #endregion

        #region 数据接收到达
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        void myEvent_DataArrived(object sender, CIPS.MQ.RcvQueue.RcvData msg)
        {
            //throw new Exception("The method or operation is not implemented.");
            if (msg == null || msg.rcvData == null) return;
            Add_QueueRecv(msg.rcvData);
        }
        #endregion

        #region 发送线程
        /// <summary>
        /// 串口发送线程
        /// </summary>
        private void Thread_SendPort()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(50);
                try
                {
                    if (hQueue_Send.Count <= 0) continue;
                    byte[] d = (byte[])hQueue_Send.Dequeue();
                    WritePort(d);
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// 同时给多个链接上的客户端发送股道封锁命令
        /// </summary>
        /// <param name="d"></param>
        private void WritePort(byte[] d)
        {
            IPAddress[] ips=mqServer_TCP .ListClients();
            if( ips ==null || ips .Length <=0) return ;
            foreach (IPAddress ip in ips)
            {
                //int id = mqServer_TCP.GetClientIdByIp(ip);
                //mqServer_TCP.Send(id, d);
                List<int> ar_id = mqServer_TCP.GetClientIdByIpNew(ip);//20170428 Mozart
                if (ar_id.Count > 0)
                {
                    foreach (int id in ar_id)
                    {
                        mqServer_TCP.Send(id, d);
                    }
                }
            }
        }
        #endregion

        #region 判断通道状态
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool bOK()
        {
            if (mqServer_TCP.ConnectionCount>0) return true;
            return false;
            //return base.bOK();
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

        #region 改变通道状态
        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public override void ChangeStatus(byte b)
        {
            hNetConfig.ChangeStatus((CTCClientCmd.ComNetConfig.E_NET_STATUS)b);
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

        #region 复位通道
        /// <summary>
        /// 
        /// </summary>
        public override void Reset()
        {
            //base.Reset();
        }
        #endregion

    }
}
