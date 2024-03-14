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
        /// 串口实例
        /// </summary>
        private System.IO.Ports.SerialPort my_Port = new System.IO.Ports.SerialPort();
        /// <summary>
        /// 串口的配置管理，用于客户端显示
        /// </summary>
        private CTCClientCmd.ComPortConfig hComPortConfig;
        /// <summary>
        /// 接收缓冲区大小
        /// </summary>
        private int ReadBufSize = 3000;
        /// <summary>
        /// 发送缓冲区大小
        /// </summary>
        private int WriteBufSize = 3000;
        /// 串口名称
        /// </summary>
        private string _PortName
        {
            get
            {
                return my_Port.PortName;
            }
        }
        /// <summary>
        /// 串口波特率
        /// </summary>
        private int _PortBaudRate
        {
            get
            {
                return my_Port.BaudRate;
            }
        }
        /// <summary>
        /// 校验
        /// </summary>
        private string _PortParity
        {
            get
            {
                if (my_Port.Parity == System.IO.Ports.Parity.Even)
                    return "偶校验";
                else if (my_Port.Parity == System.IO.Ports.Parity.Odd)
                    return "奇校验";
                else if (my_Port.Parity == System.IO.Ports.Parity.Space)
                    return "校验位为常0";
                else if (my_Port.Parity == System.IO.Ports.Parity.Mark)
                    return "校验位为常1";
                else return "无校验";

            }
        }

        /// <summary>
        /// 数据位
        /// </summary>
        private int _PortDataBit
        {
            get
            {
                return my_Port.DataBits;
            }
        }
        /// <summary>
        /// 停止位
        /// </summary>
        private string _PortStopBit
        {
            get
            {
                if (my_Port.StopBits == System.IO.Ports.StopBits.Two)
                    return "2位停止位";
                else if (my_Port.StopBits == System.IO.Ports.StopBits.OnePointFive)
                    return "1.5位停止位";
                else return "1位停止位";
            }
        }
        /// <summary>
        /// 读取的互斥
        /// </summary>
        private Mutex hMutex = new Mutex();
        /// <summary>
        /// 串口接收线程
        /// </summary>
        //private System.Threading.Thread hThread_Recv = null;
        /// <summary>
        /// 串口发送线程
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
        /// 判断串口状态
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
        #region 构造函数
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portname">名称</param>
        /// <param name="res_na"></param>
        /// <param name="res"></param>
        /// <param name="rate">波特路</param>
        /// <param name="parity">校验位</param>
        /// <param name="databit">数据位</param>
        /// <param name="stop">停止位</param>
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
            
            hTask_Recv = new PRC_Tool.BaseTask(CTCClientCmd.CTCRes_ID.GetID(), 50, res_na +" "+ _PortName+" 接收处理任务");
            hTask_Recv.event_Interval += new PRC_Tool.BaseTask.Interval_Event(hTask_Recv_event_Interval);
            hTask_Send = new PRC_Tool.BaseTask(CTCClientCmd.CTCRes_ID.GetID(), 50, res_na + " " + _PortName + " 发送处理任务");
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


        #region 初始化串口

        /// <summary>
        /// 初始化串口返回为"0"为成功
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
                return "串口未正确初始化";
            if (my_Port.IsOpen)
                return my_Port.PortName + "已经被其它任务使用";
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

        #region 关闭串口
        /// <summary>
        /// 关闭串口
        /// </summary>
        public void ClosePort()
        {
            ChangeStatus((byte)CTCClientCmd.ComPortConfig.E_PORT_STATUS.I_DISCONNECT);

            if (my_Port != null && my_Port.IsOpen)
                my_Port.Close();
        }
        #endregion


        #region 清除串口残留内容
        /// <summary>
        /// 清除发送缓冲区内容
        /// </summary>
        private void ClearWriteBuffer()
        {
            if (my_Port == null || my_Port.IsOpen == false) return;
            my_Port.DiscardOutBuffer();
        }
        /// <summary>
        /// 清除接收缓冲区内容
        /// </summary>
        public void ClearReadBuffer()
        {
            if (my_Port == null || my_Port.IsOpen == false) return;
            my_Port.DiscardInBuffer();
        }
        #endregion

        #region 接收数据线程
        /// <summary>
        /// 接收数据线程
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
        /// 串口接收线程
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

        #region 发送线程
        /// <summary>
        /// 发送线程
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
        /// 串口发送线程
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

        #region 重新初始化串口
        /// <summary>
        /// 重新初始化串口
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

        #region 判断串口状态
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

        #region 得到配置情况
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

        #region 修改资源状态
        /// <summary>
        /// 修改串口资源通信状态
        /// </summary>
        /// <param name="b"></param>
        public override void ChangeStatus(byte b)
        {
            //base.ChangeStatus(b);
            //hComPortConfig.ChangeStatus((CTCClientCmd.ComPortConfig.E_PORT_STATUS)b);
        }
        #endregion

        #region 保存端口数据
        /// <summary>
        /// 保存端口数据，给客户端诊断使用
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
