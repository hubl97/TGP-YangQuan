using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CIPS.CTC.NET
{
    /// <summary>
    /// 
    /// </summary>
    public class CommResource //: IComm
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PerformanceFrequency"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PerformanceCount"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern bool QueryPerformanceCounter(ref long PerformanceCount);
        /// <summary>
        /// 接收圆形缓冲区
        /// </summary>
        protected PRC_Tool.Queue_Cir hQueue_Cir_Recv = new PRC_Tool.Queue_Cir();
        /// <summary>
        /// 发送给TW的消息队列,取出来后通过my_Port写入串口中
        /// </summary>
        protected PRC_Tool.SafeQueue hQueue_Send = new PRC_Tool.SafeQueue();
        /// <summary>
        /// 
        /// </summary>
        protected PRC_Tool.SafeQueue hQueue_Recv = new PRC_Tool.SafeQueue();
        /// <summary>
        /// 接收超时时间定义，可单独修改
        /// </summary>
        public int I_Time_Out = 1000;
        /// <summary>
        /// 主频
        /// </summary>
        private static Int64 HZ_FREQ = 0;

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public CommResource()
        {
            QueryPerformanceFrequency(ref HZ_FREQ);
        }
        #endregion

        #region 把接收内容放到圆形缓冲区
        /// <summary>
        /// 把接收内容放到圆形缓冲区
        /// </summary>
        /// <param name="a"></param>
        /// <param name="len"></param>
        public  void Add(byte[] a, int len)
        {
            if (len <= 0) return;
            byte[] b = new byte[len];
            Buffer.BlockCopy(a, 0, b, 0, len);
            hQueue_Cir_Recv.Add(b);
        }
        #endregion

        #region 把接收的内容放到接收Q中
        /// <summary>
        /// 把接收的内容放到接收Q中
        /// </summary>
        /// <param name="a"></param>
        /// <param name="len"></param>
        public void Add_QueueRecv(byte[] a)
        {
            hQueue_Recv.Enqueue(a);
        }
        #endregion

        #region 读取铁科研内容
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Read_HumpTBZK(ref byte[] data, ref int len)
        {
            byte[] d = hQueue_Cir_Recv.Read_HumpTBZK();
            if (d == null || d.Length <= 0) return false;
            len = d.Length;
            Buffer.BlockCopy(d, 0, data,0, len);
            return true;
        }
        /// <summary>
        /// 读取数据,
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public bool Read(ref byte[] data, ref int len)
        {
            byte[] d = hQueue_Cir_Recv.Read();
            if (d == null || d.Length <= 0) return false ;
            len = d.Length;
            Buffer.BlockCopy(d, 0, data, 0, len);
            return true ;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="vt"></param>
        /// <param name="data"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public bool Read_To(byte vh, byte vt, ref byte[] data, ref int len)
        {
            byte[] d = hQueue_Cir_Recv.Read_To(vh, vt);

            if (d == null || d.Length <= 0) return false;
            len = d.Length;
            Buffer.BlockCopy(d, 0, data, 0, len);
            return true;
        }
        #endregion

        #region 采用阻塞方式读取，一直到有内容
        /// <summary>
        /// 采用阻塞方式读取，一直到有内容返回
        /// </summary>
        /// <param name="data"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public bool Read_Blocking(ref  byte[] data, ref int len)
        {
            bool res = false;
            while (res == false)
            {
                res = Read(ref data, ref len);
                if (res == false)
                    System.Threading.Thread.Sleep(300);
            }
            return res;
        }
        #endregion

        #region 根据条件读取，一直到条件满足
        /// <summary>
        /// 根据条件读取，一直到条件满足
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public int Read_To_Blocking(byte vh, byte vt, ref byte[] data, ref int len)
        {

            //DateTime dt1 = DateTime.Now;
            Int64 time_Ori = 0;
            QueryPerformanceCounter(ref time_Ori);
            while (true)
            {
                Int64 time_Cur = 0;
                QueryPerformanceCounter(ref time_Cur);
                int x = GetTime_Delta(time_Cur, time_Ori);
                if (x > I_Time_Out)
                    return E_DATARECV_TYPE.TIMEOUT;

                byte[] d = hQueue_Cir_Recv.Read_To(vh, vt);
                if (d.Length > 0)
                {
                    len = d.Length;
                    Buffer.BlockCopy(d, 0, data, 0, len);
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            return E_DATARECV_TYPE.SUCC;
        }
        #endregion

        #region 读取接收Q的内容
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object  Read_RecvQueue()
        {
            if (hQueue_Recv.Count > 0)
            {
                object obj = hQueue_Recv.Dequeue();
                return obj;
            }
            return null;
        }
        #endregion

        #region 高速计数器的计时
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        private int GetTime_Delta(Int64 d1, Int64 d2)
        {
            int time_delta = (int)((double)(d1 - d2) / (double)(HZ_FREQ) * 1000.0f);
            if (time_delta < 0)
                time_delta = (int)((double)(Int64.MaxValue + d1 - d2) / (double)(HZ_FREQ) * 1000.0f);
            return time_delta;
        }
        #endregion

        #region 把要发生内容放入发送缓冲区
        /// <summary>
        /// 发送，数据发送
        /// </summary>
        /// <param name="d"></param>
        /// <param name="len"></param>
        public void Write(byte[] d, int len)
        {
            byte[] b = new byte[len];
            Buffer.BlockCopy(d, 0, b, 0, len);
            hQueue_Send.Enqueue(d);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        public void Write(byte[] d)
        {
            hQueue_Send.Enqueue(d);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public void Write(string data)
        {
            byte[] s = System.Text.Encoding.Default.GetBytes(data);
            hQueue_Send.Enqueue(s);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public virtual  string Send_Request(byte[] d)
        {
            return "";
        }
        #endregion

        #region 通信端口的重新连接，虚函数由子类实现
        /// <summary>
        /// 通信端口的重新连接，虚函数由子类实现
        /// </summary>
        public virtual void Reset()
        {
        }
        #endregion

        #region 通信是否正常，对于串口判断串口状态，对于TCP判断连接，UDP判断能否发送。虚函数由子类实现
        /// <summary>
        /// 通信是否正常，对于串口判断串口状态，对于TCP判断连接，UDP判断能否发送。虚函数由子类实现
        /// </summary>
        /// <returns></returns>
        public virtual bool bOK()
        {
            return false;
        }
        #endregion

        #region 得到资源配置，虚函数由子类实现
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object GetConfig()
        {
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void SetConfig(int sysid,CTCClientCmd.CTCResource .E_Resource_Type  restp,byte chid)
        {
        }
        #endregion

        #region 改变资源状态，虚函数由子类实现
        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public virtual void ChangeStatus(byte b)
        {
        }
        #endregion

        #region 保存端口通信数据，给客户端查看
        /// <summary>
        /// 保存端口通信数据，给客户端查看
        /// </summary>
        /// <param name="obj"></param>
        public virtual void PushData(byte[] d, CTCClientCmd.CTCClientCmd.E_DATA_TYPE edt)
        {
            
        }
        #endregion
    }
}
