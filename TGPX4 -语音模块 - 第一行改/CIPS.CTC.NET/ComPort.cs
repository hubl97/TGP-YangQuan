using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace CIPS.CTC.NET
{
    /// <summary>
    /// 
    /// </summary>
    public class ComPort : CommResource
    {
        /// <summary>
        /// ����ʵ��
        /// </summary>
        private System.IO.Ports.SerialPort my_Port = new System.IO.Ports.SerialPort();
        /// <summary>
        /// ���ڵ����ù������ڿͻ�����ʾ
        /// </summary>
        private CTCClientCmd.ComPortConfig hComPortConfig;
        /// <summary>
        /// ���ջ�������С
        /// </summary>
        private int ReadBufSize = 3000;
        /// <summary>
        /// ���ͻ�������С
        /// </summary>
        private int WriteBufSize = 3000;
        /// ��������
        /// </summary>
        private string _PortName
        {
            get
            {
                return my_Port.PortName;
            }
        }
        /// <summary>
        /// ���ڲ�����
        /// </summary>
        private int _PortBaudRate
        {
            get
            {
                return my_Port.BaudRate;
            }
        }
        /// <summary>
        /// У��
        /// </summary>
        private string _PortParity
        {
            get
            {
                if (my_Port.Parity == System.IO.Ports.Parity.Even)
                    return "żУ��";
                else if (my_Port.Parity == System.IO.Ports.Parity.Odd)
                    return "��У��";
                else if (my_Port.Parity == System.IO.Ports.Parity.Space)
                    return "У��λΪ��0";
                else if (my_Port.Parity == System.IO.Ports.Parity.Mark)
                    return "У��λΪ��1";
                else return "��У��";

            }
        }

        /// <summary>
        /// ����λ
        /// </summary>
        private int _PortDataBit
        {
            get
            {
                return my_Port.DataBits;
            }
        }
        /// <summary>
        /// ֹͣλ
        /// </summary>
        private string _PortStopBit
        {
            get
            {
                if (my_Port.StopBits == System.IO.Ports.StopBits.Two)
                    return "2λֹͣλ";
                else if (my_Port.StopBits == System.IO.Ports.StopBits.OnePointFive)
                    return "1.5λֹͣλ";
                else return "1λֹͣλ";
            }
        }
        /// <summary>
        /// ��ȡ�Ļ���
        /// </summary>
        private Mutex hMutex = new Mutex();
        /// <summary>
        /// ���ڽ����߳�
        /// </summary>
        //private System.Threading.Thread hThread_Recv = null;
        /// <summary>
        /// ���ڷ����߳�
        /// </summary>
        //private System.Threading.Thread hThread_Send = null;
        /// <summary>
        /// 
        /// </summary>
        private PRC_Tool.BaseTask hTask_Recv = null;
        /// <summary>
        /// 
        /// </summary>
        private PRC_Tool.BaseTask hTask_Send = null;
        /// <summary>
        /// 
        /// </summary>
        private string C_Res_Na = "";
        /// <summary>
        /// 
        /// </summary>
        private string C_PORT_NA_SET = "";
        private int I_Rate = 9600;
        private string res;
        private System.IO.Ports.Parity ParityBit =System.IO.Ports.Parity.None;
        private int Databit = 8;
        System.IO.Ports.StopBits Stopbit = System.IO.Ports.StopBits.None;
        /// <summary>
        /// �жϴ���״̬
        /// </summary>
        public bool bPortOK
        {
            get
            {
                if (my_Port == null ) return false;
                if (my_Port.IsOpen == false) return false;
                if (hTask_Send ==null || hTask_Recv==null   ) return false;
                return true;
            }
        }
        #region ���캯��
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portname">����</param>
        /// <param name="res_na"></param>
        /// <param name="res"></param>
        /// <param name="rate">����·</param>
        /// <param name="parity">У��λ</param>
        /// <param name="databit">����λ</param>
        /// <param name="stop">ֹͣλ</param>
        public ComPort(string portname,string res_na,out string res,int rate,System.IO.Ports.Parity parity,int databit,System.IO.Ports.StopBits stop)
        {
            C_Res_Na=res_na;
            C_PORT_NA_SET = portname;
            I_Rate = rate;
            ParityBit = parity;
            Databit = databit;
            Stopbit = stop;

            hComPortConfig = new CTCClientCmd.ComPortConfig(CTCClientCmd.CTCRes_ID.GetID(), C_Res_Na, CTCClientCmd.ComPortConfig.E_PORT_TYPE.I_RS422, _PortName, _PortBaudRate, _PortDataBit, _PortParity, _PortStopBit);

            res = OpenPort();

            hComPortConfig.rate = _PortBaudRate;
            hComPortConfig.portna  = _PortName;
            
            hTask_Recv = new PRC_Tool.BaseTask(CTCClientCmd.CTCRes_ID.GetID(), 50, res_na +" "+ _PortName+" ���մ�������");
            hTask_Recv.event_Interval += new PRC_Tool.BaseTask.Interval_Event(hTask_Recv_event_Interval);
            hTask_Send = new PRC_Tool.BaseTask(CTCClientCmd.CTCRes_ID.GetID(), 50, res_na + " " + _PortName + " ���ʹ�������");
            hTask_Send.event_Interval += new PRC_Tool.BaseTask.Interval_Event(hTask_Send_event_Interval);

            CTCClientCmd.BasetaskManager.Add(hTask_Send.hTaskResource);
            CTCClientCmd.BasetaskManager.Add(hTask_Recv.hTaskResource);
            /*
            hThread_Recv = new System.Threading.Thread(Thread_ReadPort);
            hThread_Recv.Name = "ComPort Recv Thread";
            hThread_Recv.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Recv.IsBackground = true;
            hThread_Recv.Start();


            hThread_Send = new System.Threading.Thread(Thread_SendPort);
            hThread_Send.Name = "ComPort Send Thread";
            hThread_Send.Priority = System.Threading.ThreadPriority.BelowNormal;
            hThread_Send.IsBackground = true;
            hThread_Send.Start();
            */

        }


        #endregion


        #region ��ʼ������

        /// <summary>
        /// ��ʼ�����ڷ���Ϊ"0"Ϊ�ɹ�
        /// </summary>
        /// <returns></returns>
        public string OpenPort()
        {
            my_Port.PortName = C_PORT_NA_SET;
           // my_Port.BaudRate = 115200;
            my_Port.BaudRate = I_Rate;
            my_Port.Parity = ParityBit;
            my_Port.DataBits = Databit;
            my_Port.StopBits = Stopbit;
            my_Port.WriteTimeout = 20000;

            if (my_Port.PortName == "")
                return "����δ��ȷ��ʼ��";
            if (my_Port.IsOpen)
                return my_Port.PortName + "�Ѿ�����������ʹ��";
            my_Port.ReadBufferSize = ReadBufSize;
            my_Port.WriteBufferSize = WriteBufSize;
            try
            {
                my_Port.Open();
            }
            catch (Exception e)
            {
                return e.Message.ToString();
            }

            ChangeStatus((byte)CTCClientCmd.ComPortConfig.E_PORT_STATUS.I_INIT);
            return "";
        }
        #endregion

        #region �رմ���
        /// <summary>
        /// �رմ���
        /// </summary>
        public void ClosePort()
        {
            ChangeStatus((byte)CTCClientCmd.ComPortConfig.E_PORT_STATUS.I_DISCONNECT);

            if (my_Port != null && my_Port.IsOpen)
                my_Port.Close();
        }
        #endregion


        #region ������ڲ�������
        /// <summary>
        /// ������ͻ���������
        /// </summary>
        private void ClearWriteBuffer()
        {
            if (my_Port == null || my_Port.IsOpen == false) return;
            my_Port.DiscardOutBuffer();
        }
        /// <summary>
        /// ������ջ���������
        /// </summary>
        public void ClearReadBuffer()
        {
            if (my_Port == null || my_Port.IsOpen == false) return;
            my_Port.DiscardInBuffer();
        }
        #endregion

        #region ���������߳�
        /// <summary>
        /// ���������߳�
        /// </summary>
        void hTask_Recv_event_Interval()
        {
            //throw new Exception("The method or operation is not implemented.");
            if (my_Port == null || my_Port.IsOpen == false) return ;
            try
            {
                if (my_Port.BytesToRead <= 0) return ;
                int len = my_Port.BytesToRead;
                byte[] tempbuf = new byte[len];
                my_Port.Read(tempbuf, 0, len);
                Add(tempbuf, len);
            }
            catch
            {
            }
        }
        /*
        /// <summary>
        /// ���ڽ����߳�
        /// </summary>
        private void Thread_ReadPort()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                if (my_Port == null || my_Port.IsOpen == false) continue;
                try
                {
                    if (my_Port.BytesToRead <= 0) continue;
                    int len = my_Port.BytesToRead;
                    byte[] tempbuf = new byte[len];
                    my_Port.Read(tempbuf, 0, len);
                    Add(tempbuf, len);
                }
                catch
                {
                }
            }
        }
         */ 
        #endregion

        #region �����߳�
        /// <summary>
        /// �����߳�
        /// </summary>
        void hTask_Send_event_Interval()
        {
            //throw new Exception("The method or operation is not implemented.");
            if (my_Port == null || my_Port.IsOpen == false) return ;
            try
            {
                if (hQueue_Send.Count <= 0) return ;
                byte[] d = (byte[])hQueue_Send.Dequeue();
                WritePort(d);
            }
            catch
            {
            }
        }
        /*
        /// <summary>
        /// ���ڷ����߳�
        /// </summary>
        private void Thread_SendPort()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(200);
                if (my_Port == null || my_Port.IsOpen == false) continue;
                try
                {
                    if (hQueue_Send.Count  <= 0) continue;
                    byte[] d = (byte[])hQueue_Send.Dequeue();
                    WritePort(d);
                }
                catch
                {
                }
            }
        }
         */ 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        private void WritePort(byte[] d)
        {
            my_Port.Write(d, 0, d.Length);
        }
        #endregion

        #region ���³�ʼ������
        /// <summary>
        /// ���³�ʼ������
        /// </summary>
        public override void Reset()
        {
            try
            {
                ClearReadBuffer();
                ClearWriteBuffer();
                ClosePort();
                hQueue_Cir_Recv.Reset();
            }
            finally
            {
                OpenPort();
            }
            //base.Reset();
        }
        #endregion

        #region �жϴ���״̬
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool bOK()
        {
            if (my_Port == null  ) return false;
            if (my_Port.IsOpen == false) return false;
            if (hTask_Recv==null || hTask_Send==null  ) return false;
            return true;
            //return base.bOK();
        }
        #endregion

        #region �õ��������
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object GetConfig()
        {
            return hComPortConfig;
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
            hComPortConfig.I_SubSys_ID = sysid;
            hComPortConfig.e_ResType = restp;
            hComPortConfig.I_CHID = chid;
        }
        #endregion

        #region �޸���Դ״̬
        /// <summary>
        /// �޸Ĵ�����Դͨ��״̬
        /// </summary>
        /// <param name="b"></param>
        public override void ChangeStatus(byte b)
        {
            //base.ChangeStatus(b);
            //hComPortConfig.ChangeStatus((CTCClientCmd.ComPortConfig.E_PORT_STATUS)b);
        }
        #endregion

        #region ����˿�����
        /// <summary>
        /// ����˿����ݣ����ͻ������ʹ��
        /// </summary>
        /// <param name="d"></param>
        /// <param name="edt"></param>
        public override void PushData(byte [] d, CTCClientCmd.CTCClientCmd.E_DATA_TYPE edt)
        {
            hComPortConfig.Push(d, edt);
            //base.PushData(obj);
        }
        #endregion
    }
}
