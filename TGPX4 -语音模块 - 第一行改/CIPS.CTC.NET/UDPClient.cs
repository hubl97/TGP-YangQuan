using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Data;

namespace CIPS.CTC.NET
{
    public class UDPClient : CommResource
    {
        /// <summary>
        /// 
        /// </summary>
        private System.Net.Sockets.UdpClient hClient = null;
        /// <summary>
        /// UDP端口检测
        /// </summary>
        private CTCClientCmd.ComNetConfig hNetConfig = null;
        /// <summary>
        /// 接收线程
        /// </summary>
        private System.Threading.Thread hThread_Recv;
        /// <summary>
        /// 发送线程
        /// </summary>
        private System.Threading.Thread hThread_Send;
        /// <summary>
        /// 
        /// </summary>
        private string C_IP = "127.0.0.1";
        /// <summary>
        /// 
        /// </summary>
        private int I_PORT = 9001;
        /// <summary>
        /// 
        /// </summary>
        private int I_RCV_BUF_SIZE = 1000;
        /// <summary>
        /// 
        /// </summary>
        private System.Net.IPEndPoint RecvIpEndPoint;
        /// <summary>
        /// 
        /// </summary>
        private System.Net.IPEndPoint SndIpEndPoint;
        /// <summary>
        /// 接收任务
        /// </summary>
        private PRC_Tool.BaseTask hTask_Recv = null;
        /// <summary>
        /// 发送任务
        /// </summary>
        private PRC_Tool.BaseTask hTask_Send = null;
        /// <summary>
        /// 
        /// </summary>
        private bool bAddQueue = false;

        #region 构造函数
        /*
        /// <summary>
        /// 
        /// </summary>
        public UDPClient()
        {
            string hostna=Dns .GetHostName();
            IPAddress[] ar_ip = Dns.GetHostAddresses(hostna);
            if (ar_ip.Length <= 0)
                return;
            C_IP = ar_ip[0].ToString();
            Init(C_IP, I_PORT , 1024 * 10, 1024 * 10, "");
        }
         */ 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param UDPClinet="port"></param>
        public UDPClient(string ip, int port,string res_na)
        {
            Init(ip, port, res_na, "",false );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="res_na"></param>
        /// <param name="c_bind"></param>
        public UDPClient(string ip, int port, string res_na,string c_bind)
        {
            Init(ip, port, res_na, c_bind,false );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="res_na"></param>
        /// <param name="c_bind"></param>
        public UDPClient(string ip, int port, string res_na, string c_bind,bool bq)
        {
            Init(ip, port, res_na, c_bind,bq );
        }
        /// <summary>
        /// UDP通信初始化
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="res_na"></param>
        /// <param name="c_bind"></param>
        private void Init(string ip, int port, string res_na, string c_bind,bool bq)
        {
            C_IP = ip;
            I_PORT = port;
            bAddQueue = bq;
            //I_RCV_BUF_SIZE = rcv_buf_size;
            //I_SND_BUF_SIZE = snd_buf_size;

            string[] cc = C_IP.Split('.');
            IPAddress ip_address = new IPAddress(new byte[] { Convert.ToByte(cc[0]), Convert.ToByte(cc[1]), Convert.ToByte(cc[2]), Convert.ToByte(cc[3]) });

            RecvIpEndPoint = new System.Net.IPEndPoint(IPAddress.Any, I_PORT);
            
            SndIpEndPoint = new IPEndPoint(ip_address, I_PORT);
            
            hClient = new System.Net.Sockets.UdpClient();
            
            hClient.EnableBroadcast = true;
            if( c_bind =="")
                hClient.Client.Bind(new IPEndPoint(IPAddress.Any, I_PORT));

            hClient.Ttl = 64;
            hQueue_Cir_Recv = new PRC_Tool.Queue_Cir(Math.Max(I_RCV_BUF_SIZE + 100, 10000));

            hNetConfig = new CTCClientCmd.ComNetConfig(CTCClientCmd.CTCRes_ID.GetID(), res_na, CTCClientCmd.ComNetConfig.E_NET_TYPE.I_UDP, ip, port);

            hTask_Recv = new PRC_Tool.BaseTask(CTCClientCmd.CTCRes_ID.GetID(), 50, res_na + "UDP接收任务" + C_IP + ":" + I_PORT.ToString());
            hTask_Recv.event_Interval += new PRC_Tool.BaseTask.Interval_Event(hTask_Recv_event_Interval);
            CTCClientCmd.BasetaskManager.Add(hTask_Recv.hTaskResource);
            hTask_Send = new PRC_Tool.BaseTask(CTCClientCmd.CTCRes_ID.GetID(),50, res_na + "UDP发送任务" + C_IP + ":" + I_PORT.ToString());
            hTask_Send.event_Interval += new PRC_Tool.BaseTask.Interval_Event(hTask_Send_event_Interval);
            CTCClientCmd.BasetaskManager.Add(hTask_Send.hTaskResource);
            /*
            hThread_Recv = new System.Threading.Thread(Thread_UDPRecv);
            hThread_Recv.Name = "UDP Recv Thread";
            hThread_Recv.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Recv.IsBackground = true;
            hThread_Recv.Start();
            

            hThread_Send = new System.Threading.Thread(Thread_UDPSend);
            hThread_Send.Name = "UDP Send Thread";
            hThread_Send.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Send.IsBackground = true;
            hThread_Send.Start();
             */ 
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipe"></param>
        public void Bind(System.Net.IPEndPoint ipe)
        {
            if (ipe == null) return;
            hClient.Client.Bind(ipe);
        }
        #endregion


        #region 接收线程
        /// <summary>
        /// 
        /// </summary>
        void hTask_Recv_event_Interval()
        {
            //throw new Exception("The method or operation is not implemented.");
            try
            {
                byte[] data = hClient.Receive(ref RecvIpEndPoint);
                if (data != null && data.Length > 0)
                {
                    if (bAddQueue)
                        Add_QueueRecv(data);
                    else 
                        Add(data, data.Length);
                }
            }
            catch (Exception e)
            {

            }
        }
        /*
        /// <summary>
        /// 接收线程
        /// </summary>
        private void Thread_UDPRecv()
        {
            //byte[] data_recv = new byte[I_RCV_BUF_SIZE];
            //System.Net.IPEndPoint RemoteIpEndPoint = new System.Net.IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    byte[] data = hClient.Receive(ref RecvIpEndPoint);
                    if (data != null && data.Length > 0)
                        Add(data, data.Length);
                }
                catch(Exception e)
                {
                    
                }
                //休眠100毫秒
                System.Threading.Thread.Sleep(100);
            }
        }
         */ 
        #endregion

        #region 发送数据线程
        /// <summary>
        /// 
        /// </summary>
        void hTask_Send_event_Interval()
        {
            //throw new Exception("The method or operation is not implemented.");
            try
            {
                if (hQueue_Send.Count <= 0) return ;
                byte[] d = (byte[])hQueue_Send.Dequeue();
                Send(d);
            }
            catch (Exception e)
            {
            }
        }
        /*
        /// <summary>
        /// 
        /// </summary>
        private void Thread_UDPSend()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(75);
                try
                {
                    if (hQueue_Send.Count <= 0) continue;
                    byte[] d = (byte[])hQueue_Send.Dequeue();
                    Send(d);
                }
                catch(Exception e)
                {
                }

            }

        }
         */ 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        private void Send(byte[] d)
        {
            if (d == null || d.Length <= 0) return;
            hClient.Send(d, d.Length, SndIpEndPoint);
        }
        #endregion


        #region 重新初始化串口
        /// <summary>
        /// 
        /// </summary>
        public override void Reset()
        {
            ChangeStatus((byte)CTCClientCmd.ComNetConfig.E_NET_STATUS.I_DISCONNECT);
        }
        #endregion

        #region 得到端口配置
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object GetConfig()
        {
            return hNetConfig;
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

        #region 修改资源状态
        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public override void ChangeStatus(byte b)
        {
            //base.ChangeStatus(b);
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
            //base.PushData(obj);
        }
        #endregion

        #region 判断通道状态 对于UDP始终为True
        public override bool bOK()
        {
            return true;
            //return base.bOK();
        }
        #endregion
    }
}
