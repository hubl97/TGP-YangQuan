using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net;

namespace CIPS.CTC.NET
{
    public class TCPClient : CommResource
    {
        /// <summary>
        /// 客户客户端
        /// </summary>
        private System.Net.Sockets.TcpClient hClient;
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
        private int I_SND_BUF_SIZE = 1000;
        int Rcv_buf_size;
        int Snd_buf_size; 
        int Snd_timeout;
        int rRcv_timeout;

        #region 构造函数

        /// <summary>
        /// 
        /// </summary>
        public TCPClient()
        {
            string hostna = Dns.GetHostName();
            IPAddress[] ar_ip = Dns.GetHostAddresses(hostna);
            if (ar_ip.Length <= 0)
                return;
            C_IP = ar_ip[0].ToString();
            Init(C_IP, I_PORT, 1024 * 10, 1024 * 10, 1000 * 60, 1000 * 60);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TCPClient(string ip, int port)
        {
            Init(ip, port, 1024 * 10, 1024 * 10, 1000  , 1000  );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"><地址/param>
        /// <param name="port"><端口/param>
        /// <param name="rcv_buf_size"><接收缓冲区大小/param>
        /// <param name="snd_buf_size"><发送缓冲区大小/param>
        /// <param name="snd_timeout"><接收超时/param>
        /// <param name="rcv_timeout"><发送超时/param>
        public TCPClient(string ip, int port, int rcv_buf_size, int snd_buf_size, int snd_timeout, int rcv_timeout)
        {
            Init(ip, port, rcv_buf_size, snd_buf_size, snd_timeout, rcv_timeout);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="rcv_buf_size"></param>
        /// <param name="snd_buf_size"></param>
        /// <param name="snd_timeout"></param>
        /// <param name="rcv_timeout"></param>
        private void Init(string ip, int port, int rcv_buf_size, int snd_buf_size, int snd_timeout, int rcv_timeout)
        {
            C_IP = ip;
            I_PORT = port;
            I_RCV_BUF_SIZE = rcv_buf_size;
            I_SND_BUF_SIZE = snd_buf_size;

            Rcv_buf_size = rcv_buf_size;
            Snd_buf_size=snd_buf_size;
            Snd_timeout = snd_timeout;
            rRcv_timeout=rcv_timeout;

            hQueue_Cir_Recv = new PRC_Tool.Queue_Cir(Math.Max(I_RCV_BUF_SIZE + 100, 10000));

            hThread_Recv = new System.Threading.Thread(Thread_TCPInt);
            hThread_Recv.Name = "TCP intt Thread";
            hThread_Recv.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Recv.IsBackground = true;
            hThread_Recv.Start();

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
           
        }
        #endregion

        #region IP\端口绑定
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

        #region 建立连接
        /// <summary>
        /// 连接断开后，重新建立连接
        /// </summary>
        private void Connect()
        {
            try
            {
                hClient.Connect(C_IP, I_PORT);
            }
            catch 
            {
                hClient.Close();
                try
                {
                    hClient = new System.Net.Sockets.TcpClient(C_IP, I_PORT);
                }
                catch { }
            }
            System.Threading.Thread.Sleep(1000);
        }
        #endregion

        #region 关闭连接
        /// <summary>
        /// 
        /// </summary>
        private void DisConnect()
        {
            hClient.Close();
        }
        #endregion

        #region 清除接收、发送缓冲区内容
        private void ClearWriteBuffer()
        {
            
        }
        private void ClearReadBuffer()
        {
        }
        #endregion
        #region 初始化线程
        /// <summary>
        /// 接收线程
        /// </summary>
        private void Thread_TCPInt()
        {
            byte[] data_recv = new byte[I_RCV_BUF_SIZE];
            while (true)
            {
                //首先判断连接是否存在，如果不存在则尝试建立连接
                if (hClient == null)
                {
                    try
                    {
                        hClient = new System.Net.Sockets.TcpClient(C_IP, I_PORT);
                        hClient.ReceiveBufferSize = Rcv_buf_size;
                        hClient.ReceiveTimeout = rRcv_timeout;
                        hClient.SendBufferSize = Snd_buf_size;
                        hClient.SendTimeout = Snd_timeout;
                    }
                    catch{}
                }
                else
                {
                    //连接断开，重新建立连接
                    if (hClient.Connected == false)
                    {
                        try
                        {
                            hClient = new System.Net.Sockets.TcpClient(C_IP, I_PORT);
                            hClient.ReceiveBufferSize = Rcv_buf_size;
                            hClient.ReceiveTimeout = rRcv_timeout;
                            hClient.SendBufferSize = Snd_buf_size;
                            hClient.SendTimeout = Snd_timeout;
                        }
                        catch { }
                    }
                    else

                    return;
                }
                //休眠100毫秒
                System.Threading.Thread.Sleep(50);
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
                if (hClient == null) continue;
                //首先判断连接是否存在，如果不存在则尝试建立连接
                if (hClient.Connected == false)
                {
                    Connect();
                    if (hClient.Connected == false)
                    {
                        System.Threading.Thread.Sleep(1000);
                        continue;
                    }
                }
                System.Net.Sockets.NetworkStream ns = hClient.GetStream();
                try
                {
                    if (ns.CanRead)
                    {
                        
                        int len = ns.Read(data_recv, 0, I_RCV_BUF_SIZE - 1);
                        Add(data_recv, len);
                    }
                }
                catch{}
                finally
                {
          //          ns.Close();
                }
                //休眠10毫秒
                System.Threading.Thread.Sleep(10);
            }
        }
        #endregion

        #region 发送数据线程
        /// <summary>
        /// 
        /// </summary>
        private void Thread_TCPSend()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10);
                if (hClient == null) continue;
                if (hClient .Connected==false ) continue;
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
        /// 
        /// </summary>
        /// <param name="d"></param>
        private void Send(byte[] d)
        {
            if (d == null || d.Length <= 0) return;
            System.Net.Sockets.NetworkStream ns = hClient.GetStream();
            try
            {
                ns.Write(d, 0, d.Length);
            }
            finally
            {
          //      ns.Close();
            }
        }
        #endregion
    }
}
