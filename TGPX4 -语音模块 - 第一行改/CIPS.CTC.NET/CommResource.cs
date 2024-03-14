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
        /// ����Բ�λ�����
        /// </summary>
        protected PRC_Tool.Queue_Cir hQueue_Cir_Recv = new PRC_Tool.Queue_Cir();
        /// <summary>
        /// ���͸�TW����Ϣ����,ȡ������ͨ��my_Portд�봮����
        /// </summary>
        protected PRC_Tool.SafeQueue hQueue_Send = new PRC_Tool.SafeQueue();
        /// <summary>
        /// 
        /// </summary>
        protected PRC_Tool.SafeQueue hQueue_Recv = new PRC_Tool.SafeQueue();
        /// <summary>
        /// ���ճ�ʱʱ�䶨�壬�ɵ����޸�
        /// </summary>
        public int I_Time_Out = 1000;
        /// <summary>
        /// ��Ƶ
        /// </summary>
        private static Int64 HZ_FREQ = 0;

        #region ���캯��

        /// <summary>
        /// ���캯��
        /// </summary>
        public CommResource()
        {
            QueryPerformanceFrequency(ref HZ_FREQ);
        }
        #endregion

        #region �ѽ������ݷŵ�Բ�λ�����
        /// <summary>
        /// �ѽ������ݷŵ�Բ�λ�����
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

        #region �ѽ��յ����ݷŵ�����Q��
        /// <summary>
        /// �ѽ��յ����ݷŵ�����Q��
        /// </summary>
        /// <param name="a"></param>
        /// <param name="len"></param>
        public void Add_QueueRecv(byte[] a)
        {
            hQueue_Recv.Enqueue(a);
        }
        #endregion

        #region ��ȡ����������
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
        /// ��ȡ����,
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

        #region ����������ʽ��ȡ��һֱ��������
        /// <summary>
        /// ����������ʽ��ȡ��һֱ�������ݷ���
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

        #region ����������ȡ��һֱ����������
        /// <summary>
        /// ����������ȡ��һֱ����������
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

        #region ��ȡ����Q������
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

        #region ���ټ������ļ�ʱ
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

        #region ��Ҫ�������ݷ��뷢�ͻ�����
        /// <summary>
        /// ���ͣ����ݷ���
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

        #region ͨ�Ŷ˿ڵ��������ӣ��麯��������ʵ��
        /// <summary>
        /// ͨ�Ŷ˿ڵ��������ӣ��麯��������ʵ��
        /// </summary>
        public virtual void Reset()
        {
        }
        #endregion

        #region ͨ���Ƿ����������ڴ����жϴ���״̬������TCP�ж����ӣ�UDP�ж��ܷ��͡��麯��������ʵ��
        /// <summary>
        /// ͨ���Ƿ����������ڴ����жϴ���״̬������TCP�ж����ӣ�UDP�ж��ܷ��͡��麯��������ʵ��
        /// </summary>
        /// <returns></returns>
        public virtual bool bOK()
        {
            return false;
        }
        #endregion

        #region �õ���Դ���ã��麯��������ʵ��
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

        #region �ı���Դ״̬���麯��������ʵ��
        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public virtual void ChangeStatus(byte b)
        {
        }
        #endregion

        #region ����˿�ͨ�����ݣ����ͻ��˲鿴
        /// <summary>
        /// ����˿�ͨ�����ݣ����ͻ��˲鿴
        /// </summary>
        /// <param name="obj"></param>
        public virtual void PushData(byte[] d, CTCClientCmd.CTCClientCmd.E_DATA_TYPE edt)
        {
            
        }
        #endregion
    }
}
