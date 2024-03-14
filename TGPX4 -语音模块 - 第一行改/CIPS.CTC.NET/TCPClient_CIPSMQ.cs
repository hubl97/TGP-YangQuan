using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;

namespace CIPS.CTC.NET
{
    public  class TCPClient_CIPSMQ:CommResource
    {
        /// <summary>
        /// 
        /// </summary>
        private CIPS.MQ.MqClient mqClient_TCP = null;
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
        public TCPClient_CIPSMQ(string ip, int port, string res_na,System .Net .IPEndPoint ipe)
        {
            C_IP = ip;
            I_PORT = port;
            if (ipe != null)
                mqClient_TCP = new CIPS.MQ.MqClient(null, ip, port, ipe);
            else
                mqClient_TCP = new CIPS.MQ.MqClient(null, ip, port);
            mqClient_TCP.KeepMinutes = -1;


            mqClient_TCP.myEvent.DataArrived += new CIPS.MQ.MqEvent.DataArrivedEvt(myEvent_DataArrived);
            hNetConfig = new CTCClientCmd.ComNetConfig(CTCClientCmd.CTCRes_ID.GetID(), res_na, CTCClientCmd.ComNetConfig.E_NET_TYPE.I_TCP, ip, port);
            mqClient_TCP.ConnectionChanged += new CIPS.MQ.MqClient.ConnectStatusChanged(mqClient_TCP_ConnectionChanged);

            mqClient_TCP.Send(new byte[] { 0, 0,0,0,0,0 });
            Thread hThread_Send = new System.Threading.Thread(Thread_SendPort);
            hThread_Send.Name = "CIPSMQ Send Thread";
            hThread_Send.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Send.IsBackground = true;
            hThread_Send.Start();

        }

        #endregion

        #region 连接状态发生变化
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="connected"></param>
        void mqClient_TCP_ConnectionChanged(CIPS.MQ.MqClient client, bool connected)
        {
            //throw new Exception("The method or operation is not implemented.");
            if (connected)
                ChangeStatus((byte)CTCClientCmd.ComNetConfig.E_NET_STATUS.I_CONNECT);
            else
                ChangeStatus((byte)CTCClientCmd.ComNetConfig.E_NET_STATUS.I_DISCONNECT);
        }

        #endregion

        #region 到达客户端的数据
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
        /// 
        /// </summary>
        /// <param name="d"></param>
        private void WritePort(byte[] d)
        {
            mqClient_TCP.Send(d, 0, d.Length);
        }
        /// <summary>
        /// 立即发送并接收反馈
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public override string Send_Request(byte[] d)
        {
            //return base.Send_Request(d);
            byte [] res=null ;
            CIPS.MQ.MqDefine.RET ret = mqClient_TCP.SendRequest(d, out res);
            if (ret != CIPS.MQ.MqDefine.RET.Success)
                return "发送失败";
            else
            {
                if (res != null)
                {
                    CIPS.Graph.Command cd = new CIPS.Graph.Command(res);
                    //return  BitConverter.ToString(res);
                    return cd.Note;
                }
                else
                {
                    return "";
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
            if( mqClient_TCP.Connected)
                return true;
            return false;
            //if (mqClient_TCP.Connected) return true;
            //return false;
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
