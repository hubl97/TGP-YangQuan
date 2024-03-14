using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Tool
{
    /// <summary>
    /// 
    /// </summary>
    public class E_LOG_TYPE
    {
        public const int CIPS = 1;
        public const int TW = 2;        
        public const int Log = 3;
        public const int CIPSBack = 6;
    }
    /// <summary>
    /// 输入输出标
    /// </summary>
    public class E_DATE_IOTYPE
    {
        public const int no = 0;
        public const int I_DATA_OUT = 1;
        public const int I_DATA_IN = 2;
    }
    public class WriteDataLog
    {
        static string path ;
        //static DirectoryInfo PathParent;
        static string PathParent;
        private WriteDataLog()
        {
            cmdWriteQueue = new CIPS.MQ.CmdQueue();
            Thread thread = new Thread(new ThreadStart(WriteQueueProc));
            thread.Name = "WriteLogThread";
            thread.IsBackground = true;
            thread.Start();
            path = Application.StartupPath;
            PathParent = path;//Directory.GetParent(path);
            System.Timers.Timer tm = new System.Timers.Timer();
            tm.Interval = 60 * 1000 * 60;
            tm.Elapsed += new System.Timers.ElapsedEventHandler(tm_Elapsed);
            tm.Start();
        }

        void tm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string path = PathParent + @"\LogFile\"; ;
                DateTime dTime = DateTime.Now.AddDays(-30);
                DateTime dTime1 = DateTime.Now.AddDays(-31);
                string fileNa = dTime.ToString("MMdd");
                string fileNa1 = dTime1.ToString("MMdd");
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] fi = di.GetFiles();
                for (int i = 0; i < fi.Length; i++)
                {
                    if (fi[i].Name.IndexOf(fileNa) != -1 || fi[i].Name.IndexOf(fileNa1) != -1)
                    {
                        fi[i].Delete();
                    }
                }
                string path2 = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo di2 = new DirectoryInfo(path);
                FileInfo[] fi2 = di.GetFiles();
                for (int i = 0; i < fi2.Length; i++)
                {
                    if (fi2[i].Name.IndexOf(fileNa) != -1 || fi2[i].Name.IndexOf(fileNa1) != -1)
                    {
                        fi2[i].Delete();
                    }
                }
            }
            finally
            {
            }
        }
        public static readonly WriteDataLog m_instance = new WriteDataLog();
        CIPS.MQ.CmdQueue cmdWriteQueue;
        
        private void WriteQueueProc()
        {
            while (true)
            {
                cmdWriteQueue.MessageArrivedEvent.WaitOne();
                CIPS.MQ.CmdQueue.Message m = cmdWriteQueue.PopData();
                if (m != null)
                {
                    try
                    {
                        WriteLog(m.data);
                    }
                    catch 
                    { }
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iotype">输入输出标E_DATE_IOTYPE</param>
        /// <param name="type">E_LOG_TYPE</param>
        /// <param name="ob">数据</param>
        public void WriteLog(int iotype,int type, object ob)
        {
            CIPS.MQ.CmdQueue.Message m = new CIPS.MQ.CmdQueue.Message();
            object[] objs = new object[3];
            objs[0] = iotype;
            objs[1] = type;
            objs[2] = ob;
            m.data = objs;
            cmdWriteQueue.PushData(m);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void WriteLog(object obj)
        {
            try
            {
                object[] objs = (object[])obj;
                int iotype = (int)objs[0];
                int type = (int)objs[1];
                object ob = objs[2];
                string path = PathParent+@"\LogFile\";
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
                string hour = DateTime.Now.ToString("MMddHH");
                string day = DateTime.Now.ToString("MMdd");
                switch (type)
                {
                    case E_LOG_TYPE.CIPS:
                        WriteData(ob, path, iotype, "CIPS主" + hour);
                        break;
                    case E_LOG_TYPE.TW:
                        WriteData(ob, path, iotype, "TW" + hour);
                        break;
                    case E_LOG_TYPE.Log:
                        WriteLog(ob, path, iotype, "LOG" + day);
                        break;    
                    case E_LOG_TYPE.CIPSBack:
                        WriteData(ob, path, iotype, "CIPS备" + hour);
                        break;
                    default:
                        break;
                }
            }
            catch
            { }
        }

        /// <summary>
        /// 记录通道数据
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        private void WriteData(object obj, string path,int type,string fileName)
        {
            try
            {
                StreamWriter sw = File.AppendText(path + fileName + ".txt");            
                byte[] by = (byte[])obj;
                string str = "";
                string ar = "";
                if (type == E_DATE_IOTYPE.I_DATA_OUT)
                {
                    str = "< ";
                }
                else if (type == E_DATE_IOTYPE.I_DATA_IN)
                {
                    str = "> ";
                }
                else
                {
                    str = "-- ";
                }
                str = str + "  :";
                ConvertBinary_String(by, ref ar);
                str = str + ar;
                string tm = DateTime.Now.ToString("HH:mm:ss");
                str = " [" + tm + "]"+str + "\t";
                sw.WriteLine(str);              
                sw.Close();
            }
            catch (Exception ee) 
            { 

            }
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        private void WriteLog(object obj, string path, int type, string fileName)
        {
            try
            {
                StreamWriter sw = File.AppendText(path + fileName + ".txt");
                string log = (string)obj;
                string str = "";
                if (type == E_DATE_IOTYPE.I_DATA_OUT)
                {
                    str = "< ";
                }
                else if (type == E_DATE_IOTYPE.I_DATA_IN)
                {
                    str = "> ";
                }
                else
                {
                    str = "-- ";
                }
                str = str + " :";
                str = str + log;
                string tm = DateTime.Now.ToString("HH:mm:ss");
                str = " [" + tm + "]" +str +  "\t";
                sw.WriteLine(str);
                sw.Close();
            }
            catch (Exception ee)
            {

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="by"></param>
        /// <param name="ss"></param>
        public static void ConvertBinary_String(byte[] by, ref string ss)
        {
            ss = "";
            if (by == null) return;
            for (int i = 0; i < by.Length; i++)
            {
                string s = ConvertByte_String(by[i]);
                ss += s;
            }
        }
        /// <summary>
        /// 把一个Byte转换为以16进制显示的字符串
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public static string ConvertByte_String(byte by)
        {
            string s = Uri.HexEscape((char)by);
            s = s.Replace('%', ' ');
            return s;
        }
    }
}
