using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net;

namespace CIPS.CTC.NET
{
    public class TCPServer : CommResource
    {
        /// <summary>
        /// 服务端
        /// </summary>
        private System.Net.Sockets.TcpListener hServer;
        /// <summary>
        /// 
        /// </summary>
        private List<System.Net.Sockets.TcpClient> ar_Client = new List<System.Net.Sockets.TcpClient>();

        /// <summary>
        /// 接收线程
        /// </summary>
        private System.Threading.Thread hThread_Recv;
        /// <summary>
        /// 发送线程
        /// </summary>
        private System.Threading.Thread hThread_Send;
        /// <summary>
        /// 接收连接线程
        /// </summary>
        private System.Threading.Thread hThread_Accept;
        /// <summary>
        /// TCP端口检测
        /// </summary>
        private CTCClientCmd.ComNetConfig hNetConfig = null;

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

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TCPServer(int port,System .Net .IPEndPoint ipe)
        {
            I_PORT = port;
            string hostna = Dns.GetHostName();
            IPAddress[] ar_ip = Dns.GetHostAddresses(hostna);
            if (ar_ip.Length <= 0)
                return;
            C_IP = ar_ip[0].ToString();

            if (ipe == null)
                hServer = new System.Net.Sockets.TcpListener(ar_ip[0], I_PORT);
            else
            {
                hServer = new System.Net.Sockets.TcpListener(ar_ip[0], I_PORT);
                hServer.Server.Bind(ipe);
            }
            hServer.Start();
            hNetConfig = new CTCClientCmd.ComNetConfig(CTCClientCmd.CTCRes_ID.GetID(), "", CTCClientCmd.ComNetConfig.E_NET_TYPE.I_TCP, C_IP, I_PORT);

            hThread_Recv = new System.Threading.Thread(Thread_TCPRecv);
            hThread_Recv.Name = "TCP Recv Thread";
            hThread_Recv.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Recv.IsBackground = true;
            hThread_Recv.Start();


            hThread_Send = new System.Threading.Thread(Thread_TCPSend);
            hThread_Send.Name = "TCP Send Thread";
            hThread_Send.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Send.IsBackground = true;
            hThread_Send.Start();

            hThread_Accept = new System.Threading.Thread(Thread_TCPAccept);
            hThread_Accept.Name = "TCP Accept Thread";
            hThread_Accept.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Accept.IsBackground = true;
            hThread_Accept.Start();

        }
        #endregion

        #region 建立连接
        /// <summary>
        /// 
        /// </summary>
        private void Connect()
        {
        }
        #endregion

        #region 关闭连接
        /// <summary>
        /// 
        /// </summary>
        private void DisConnect()
        {
            hServer.Stop();
        }
        #endregion

        #region 清除接收、发送缓冲区内容
        /// <summary>
        /// 
        /// </summary>
        private void ClearWriteBuffer()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        private void ClearReadBuffer()
        {
        }
        #endregion

        #region 接受连接线程
        /// <summary>
        /// 接受连接线程
        /// </summary>
        private void Thread_TCPAccept()
        {
            while (true)
            {
                if (hServer.Pending())
                {
                    try
                    {
                        System.Net.Sockets.TcpClient client = hServer.AcceptTcpClient();
                        ar_Client.Add(client);

                    }
                    finally
                    {
                    }
                }
                System.Threading.Thread.Sleep(2000);
            }
        }
        #endregion

        #region 接收线程
        /// <summary>
        /// 接收线程
        /// </summary>
        private void Thread_TCPRecv()
        {
            byte[] data_recv = new byte[I_RCV_BUF_SIZE];
            while (true)
            {
                //首先判断连接是否存在，如果不存在则删除连接
                for (int i = 0; i < ar_Client.Count; i++)
                {
                    System.Net.Sockets.TcpClient client = ar_Client[i];
                    if (client.Connected == false)
                    {
                        client.Close();
                        ar_Client.Remove(client);
                        i--;
                        continue;
                    }
                    System.Net.Sockets.NetworkStream ns = client.GetStream();
                    try
                    {
                        if (ns.CanRead)
                        {
                            int len = ns.Read(data_recv, 0, I_RCV_BUF_SIZE - 1);
                            Add(data_recv, len);
                        }
                    }
                    finally
                    {
                        ns.Close();
                    }
                }
                //休眠100毫秒
                System.Threading.Thread.Sleep(100);
            }
        }
        #endregion

        #region 发送数据线程
        /// <summary>
        /// 发送数据线程
        /// </summary>
        private void Thread_TCPSend()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(50);
                if (ar_Client.Count  <=0) continue;
                try
                {
                    if (hQueue_Send.Count <= 0) continue;
                    byte[] d = (byte[])hQueue_Send.Dequeue();
                    Send(d);
                }
                catch
                {
                }

            }

        }
        /// <summary>
        /// 数据发送处理
        /// </summary>
        /// <param name="d"></param>
        private void Send(byte[] d)
        {
            if (d == null || d.Length <= 0) return;
            for (int i = 0; i < ar_Client.Count; i++)
            {
                System.Net.Sockets.TcpClient client = ar_Client[i];
                if (client.Connected == false)
                {
                    client.Close();
                    ar_Client.Remove(client);
                    i--;
                    continue;
                }
                System.Net.Sockets.NetworkStream ns = client.GetStream();
                try
                {
                    ns.Write(d, 0, d.Length);
                }
                finally
                {
                    ns.Close();
                }
            }
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

    }
}
