using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;





namespace NewTGP
{

    public partial class TGPDisp : Form
    {
        public Label Lab1;


        public TGPDisp()  //构造函数
        {
            InitializeComponent();
            //"中国通号\n\通达天下\n\r号令四方";
            //Speech_SDK.Speech.Resume();
            //Speech_SDK.Speech.SpeakSyn("欢迎使用YRS-YS驼峰溜放信息综合显示系统");

        }

        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filepath); //用来读取配置文件的aip函数
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returnvalue, int buffersize, string filepath);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(string section, string key, string def, char[] returnvalue, int buffersize, string filepath);

        public CIPS.CTC.NET.CommResource cr = null;  //comresource 串口资源
        public CIPS.CTC.NET.CommResource cli = null; //峰1语音模块TCP客户端
        public CIPS.CTC.NET.CommResource cli2 = null;//峰2语音模块TCP客户端
        public PRC_Tool.SafeArrayList DisplayCon = new PRC_Tool.SafeArrayList(); //安全型列表
        //private System.Windows.Forms.Timer timer_TimeDisplay2 = new System.Windows.Forms.Timer();   // Timer
        List<System.Windows.Forms.Label> LabelList = new List<System.Windows.Forms.Label>();
        DateTime timeScreen = DateTime.Now;
        DateTime timeScreen2 = DateTime.Now.AddSeconds(5);
        DateTime ScreenOffTime = DateTime.Now;
        DateTime ScreenOffTime2 = DateTime.Now;
        DateTime timeSound = DateTime.Now;
        //public string slogan = "";
        public static int StationScreen = 0; //站场屏配置  0：双屏都使用  1：只使用屏1   2：只使用屏2
        public static int StationAudio = 0; //站场语音模块配置 0：双峰都使用  1：只使用峰1   2：只使用峰2  3.关闭语音模块
        public static int CommunicationType = 1; //通信方式 1为网口 2为串口    设置为Public 都可调用
        public static int ComP1 = 3;  //屏1 COM口号
        public static int ComP2 = 4;  //屏2 COM口号
        public static string P1IP = "192.168.1.199";  //屏1IP地址
        public static string P2IP = "192.168.1.198";  //屏2IP地址
        public static string P1AudioIP = "192.168.1.197"; //屏1音频IP
        public static string P2AudioIP = "192.168.1.196"; //屏2音频IP
        public static string P1Name = "Led001";
        public static string P2Name = "Led002";
        public static int AreaNum = 5; //屏幕区域数
        public int RenewCarNum = 3; //翻钩辆数
        public static string Slogan1 = "交通强国";
        public static string Slogan2 = "铁路先行";
        public static TGPConfig TGPconfig = new TGPConfig();  //实例化设置界面(静态字段方便子窗体调用)
        public static PSWPage PSWpage = new PSWPage(); //实例化密码界面
        public static bool bWriteLog = true;
        public bool bTimeDisplay = false; //TimerDisplay是否使用
        public bool bTimeDisplay2 = false;//TimerDisplay是否使用
        public int TimerCount = 0;   // 用来计数，同步显示时间或标语
        public int TimerCount2 = 0;   // 用来计数，同步显示时间或标语
        public static string DoMarkLiu = "-";  //溜放作业符
        public static string DoMarkDan = "D";  //手动单溜作业符
        public static string DoMarkGua = "+";  //挂车作业符
        public static string DoMarkJin = "#";  //禁溜作业符
        public static string DoMarkXia = "X";  //下峰作业符
        public static string DomarkShang = "S";//上峰作业符
        public static int ScreenTop = 0; //屏幕第一行显示配置  0：车次+驼信   1：车次    2：驼信
        /// <summary>
        /// 队列定义
        /// </summary>
        public static PRC_Tool.SafeQueue AudioQueue = new PRC_Tool.SafeQueue(); //峰1语音模块音频队列
        public static PRC_Tool.SafeQueue AudioQueue2 = new PRC_Tool.SafeQueue();//峰2语音模块音频队列
        PRC_Tool.SafeQueue SentAudioQueue = new PRC_Tool.SafeQueue();//峰1语音模块已被发送的音频队列
        PRC_Tool.SafeQueue SentAudioQueue2 = new PRC_Tool.SafeQueue();//峰2语音模块已被发送的音频队列
        PRC_Tool.SafeQueue recQueue = new PRC_Tool.SafeQueue();  //信息窗接收队列
        PRC_Tool.SafeQueue sndQueue = new PRC_Tool.SafeQueue();  //信息窗发送队列
        PRC_Tool.SafeQueue RecieveToProcessQueue = new PRC_Tool.SafeQueue();  //屏1接收到处理队列
        PRC_Tool.SafeQueue RecieveToProcessQueue2 = new PRC_Tool.SafeQueue(); //屏2接收到处理队列
        PRC_Tool.SafeQueue SendDataQueue = new PRC_Tool.SafeQueue(); //屏1发送数据队列
        PRC_Tool.SafeQueue SendDataQueue2 = new PRC_Tool.SafeQueue();//屏2发送数据队列
        PRC_Tool.SafeQueue SendDataTimeQueue = new PRC_Tool.SafeQueue();//屏1发送时间标语队列
        PRC_Tool.SafeQueue SendDataTimeQueue2 = new PRC_Tool.SafeQueue();//屏2发送时间标语队列

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            ListenDll.LV_InitDllEx(1);//X4-75的初始化

            Line1Lab.Text = Line3Lab.Text = Line4Lab.Text = Line5Lab.Text = Line6Lab.Text = Line7Lab.Text = "";
            Line1Labb.Text = Line3Labb.Text = Line4Labb.Text = Line5Labb.Text = Line6Labb.Text = Line7Labb.Text = "";
            Line2Lab.Visible = Line2Labb.Visible = false;


            string ComPort = "com1";   //com口
            int Speed = 9600, DataBit = 1;  //  波特率
            string IniFilePath, ReadValue;
            Parity ParityBit = Parity.None;  //奇偶校验位
            StopBits StopBit = StopBits.One;  //停止位数



            IniFilePath = Application.StartupPath + "\\Run.ini";
            StringBuilder stringBuilder = new StringBuilder();

            GetPrivateProfileString("COMCONFIG", "ComPort", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "") ComPort = ReadValue;
            else ComPort = "com1";

            GetPrivateProfileString("COMCONFIG", "Slogan1", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue == null)
            {
                Slogan1 = "交通强国";
            }
            else
            {
                Slogan1 = ReadValue;
            }

            GetPrivateProfileString("COMCONFIG", "Slogan2", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue == null)
            {
                Slogan2 = "铁路先行";
            }
            else
            {
                Slogan2 = ReadValue;
            }

            TGPDisp.TGPconfig.textBox1.Text = Slogan1;
            TGPDisp.TGPconfig.textBox2.Text = Slogan2;
            GetPrivateProfileString("COMCONFIG", "Speed", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (!Int32.TryParse(ReadValue, out Speed)) Speed = 9600;

            GetPrivateProfileString("COMCONFIG", "bWriteLog", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            try
            {
                if (Convert.ToInt32(ReadValue) == 1)
                    bWriteLog = true;
                else
                    bWriteLog = false;
                TGPconfig.bWriteLog = bWriteLog;
            }
            catch { }

            GetPrivateProfileString("COMCONFIG", "DataBit", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "")
            {
                if (!Int32.TryParse(ReadValue, out DataBit)) DataBit = 8;
            }
            //MessageBox.Show(DataBit.ToString());

            GetPrivateProfileString("COMCONFIG", "StopBit", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "")
            {
                //StopBits.None,StopBits.One,StopBits.OnePointFive,StopBits.Two
                if (ReadValue == "1") StopBit = StopBits.One;
                if (ReadValue == "2") StopBit = StopBits.Two;
                if (ReadValue == "N") StopBit = StopBits.None;
                if (ReadValue == "1.5") StopBit = StopBits.OnePointFive;
            }
            else StopBit = StopBits.One;

            GetPrivateProfileString("COMCONFIG", "Parity", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            switch (ReadValue)
            {
                case "E":
                    ParityBit = Parity.Even;
                    break;
                case "M":
                    ParityBit = Parity.Mark;
                    break;
                case "N":
                    ParityBit = Parity.None;
                    break;
                case "O":
                    ParityBit = Parity.Odd;
                    break;
                case "S":
                    ParityBit = Parity.Space;
                    break;
                default:
                    ParityBit = Parity.None;
                    break;
            }
            GetPrivateProfileString("COMCONFIG", "StationScreen", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "")
            {
                if (ReadValue == "0") StationScreen = 0;
                else if (ReadValue == "1") StationScreen = 1;
                else if (ReadValue == "2") StationScreen = 2;
                else { MessageBox.Show("StationScreen配置错误"); StationScreen = 0; }
            }
            else StationScreen = 0;
            
            GetPrivateProfileString("COMCONFIG", "StationAudio", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "")
            {
                if (ReadValue == "0") StationAudio = 0;
                else if (ReadValue == "1") StationAudio = 1;
                else if (ReadValue == "2") StationAudio = 2;
                else if (ReadValue == "3") StationAudio = 3;
                else { MessageBox.Show("StationAudio配置错误"); StationAudio = 3; }
            }
            else StationAudio = 3;

            GetPrivateProfileString("COMCONFIG", "CommunicationType", "", stringBuilder, 1024, IniFilePath);  //配置文件中读取通信方式 网络或串口 
            ReadValue = stringBuilder.ToString();
            if (ReadValue == "1") CommunicationType = 1;  //网络
            else if (ReadValue == "2") CommunicationType = 2;   //串口
            else CommunicationType = 1;

            GetPrivateProfileString("COMCONFIG", "ComP1", "", stringBuilder, 1024, IniFilePath);  // 屏1 COM口 配置文件读取
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "")
            {
                try
                {
                    ComP1 = Convert.ToInt32(ReadValue);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ComP1配置错误: \n" + ex);
                }
                //ComPort = ReadValue;
            }
            else ComP1 = 3;

            GetPrivateProfileString("COMCONFIG", "ComP2", "", stringBuilder, 1024, IniFilePath); //屏 COM口 配置文件读取
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "")
            {
                try
                {
                    ComP2 = Convert.ToInt32(ReadValue);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ComP2配置错误: \n" + ex);
                }
                //ComPort = ReadValue;
            }
            else ComP2 = 4;

            GetPrivateProfileString("COMCONFIG", "RenewCarNum", "", stringBuilder, 1024, IniFilePath);  //配置文件中读取翻钩辆数（压入且辆数少于等于RenewCarNum时 屏幕翻钩）
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "")
            {
                try
                {
                    RenewCarNum = Convert.ToInt32(ReadValue);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("RenewCarNum配置错误：\n" + ex);
                }
            }
            else RenewCarNum = 3;

            GetPrivateProfileString("COMCONFIG", "DoMarkLiu", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "") DoMarkLiu = ReadValue;
            else DoMarkLiu = "-";

            GetPrivateProfileString("COMCONFIG", "DoMarkDan", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "") DoMarkDan = ReadValue;
            else DoMarkDan = "D";

            GetPrivateProfileString("COMCONFIG", "DoMarkGua", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "") DoMarkGua = ReadValue;
            else DoMarkGua = "+";

            GetPrivateProfileString("COMCONFIG", "DoMarkJin", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "") DoMarkJin = ReadValue;
            else DoMarkJin = "#";

            GetPrivateProfileString("COMCONFIG", "DoMarkXia", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "") DoMarkXia = ReadValue;
            else DoMarkXia = "X";

            GetPrivateProfileString("COMCONFIG", "DomarkShang", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "") DomarkShang = ReadValue;
            else DomarkShang = "S";

            GetPrivateProfileString("COMCONFIG", "ScreenTop", "", stringBuilder, 1024, IniFilePath);
            ReadValue = stringBuilder.ToString();
            if (ReadValue != "")
            {
                if (ReadValue == "0") ScreenTop = 0;    //屏幕第一行显示配置  0：车次+驼信   1：车次    2：驼信
                else if (ReadValue == "1") ScreenTop = 1;
                else if (ReadValue == "2") ScreenTop = 2;
                else if (ReadValue == "3") ScreenTop = 3;
                else { MessageBox.Show("ScreenTop配置错误"); ScreenTop = 0; }
            }
            else ScreenTop = 3;

            label4.Text = Slogan1 + "\n\r" + Slogan2;
            label5.Text = Slogan1 + "\n\r" + Slogan2;
            label4.Visible = label5.Visible = false;

            string res;
            cr = new CIPS.CTC.NET.ComPort(ComPort, "tgp", out res, Speed, ParityBit, DataBit, StopBit);
            if (res != "")
            {
                MessageBox.Show(res);
            }

            Thread CIPSThread = new Thread(CommRecThread);           //定时读取CIPS数据线程
            CIPSThread.Name = "CIPSReadThread";
            CIPSThread.Priority = ThreadPriority.BelowNormal;
            CIPSThread.IsBackground = true;
            CIPSThread.Start();

            //语音模块配置
            if (StationAudio == 0 || StationAudio == 1)
            {
                cli = new CIPS.CTC.NET.TCPClient(P1AudioIP, 50000); //峰1语音模块客户端
                Thread ClientThread = new Thread(ClientThreadMethod);
                ClientThread.Name = "ClientThread";
                ClientThread.Priority = ThreadPriority.BelowNormal;
                ClientThread.IsBackground = true;
                ClientThread.Start();

                Thread ClientReplyThread = new Thread(ClientReplyThreadMethod);
                ClientReplyThread.Name = "ClientReplyThread";
                ClientReplyThread.Priority = ThreadPriority.BelowNormal;
                ClientReplyThread.IsBackground = true;
                ClientReplyThread.Start();
            }  //双峰或峰1有语音：启动峰1语音模块
            if (StationAudio == 0 || StationAudio == 2)
            {
                cli2 = new CIPS.CTC.NET.TCPClient(P2AudioIP, 50000); //峰2语音模块客户端
                Thread ClientThread2 = new Thread(ClientThreadMethod2);
                ClientThread2.Name = "ClientThread2";
                ClientThread2.Priority = ThreadPriority.BelowNormal;
                ClientThread2.IsBackground = true;
                ClientThread2.Start();

                Thread ClientReplyThread2 = new Thread(ClientReplyThreadMethod2);
                ClientReplyThread2.Name = "ClientReplyThread2";
                ClientReplyThread2.Priority = ThreadPriority.BelowNormal;
                ClientReplyThread2.IsBackground = true;
                ClientReplyThread2.Start();
            }  //双峰或峰2有语音：启动峰2语音模块
            /*AudioQueue2.Enqueue("语音播报");
            AudioQueue2.Enqueue("8一五四语音语音速度会撒谎覅赛u带u的撒大哈哈");
            AudioQueue2.Enqueue("你好");
            AudioQueue2.Enqueue("水水水水水水");*/


            if (StationScreen == 0 || StationScreen == 1)
            {
                Thread SendThread = new Thread(SendScreenThread);        //发送屏幕1线程
                SendThread.Name = "SendThread";
                SendThread.Priority = ThreadPriority.BelowNormal;
                SendThread.IsBackground = true;
                SendThread.Start();
            }
            if (StationScreen == 0 || StationScreen == 2)
            {
                Thread Send2Thread = new Thread(SendScreen2Thread);      //发送屏幕2线程
                Send2Thread.Name = "Send2Thread";
                Send2Thread.Priority = ThreadPriority.BelowNormal;
                Send2Thread.IsBackground = true;
                Send2Thread.Start();
            }

            timer_Checker.Interval = 1000;  //一直在运行的Timer
            //timer_TimeDisplay2.Interval = 1000;  //接收到峰2空包时发送时间的Timer
            //timer_TimeDisplay2.Tick += new EventHandler(timer_TimeDisplay2_Tick); 
            //timer_TimeDisplay.Interval = 1000; //接收到峰1空包时发送时间的Timer
            timer_Checker.Start();



            if (StationScreen == 0 || StationScreen == 1)
            {
                //SetTGP();
                List<string> IniList = new List<string>();
                IniList.Add("");
                IniList.Add("");
                IniList.Add("");
                IniList.Add("");
                IniList.Add("");
                XStaticInit(IniList, false);  // 屏1初始化创建五个静态文本区域
            }
            if (StationScreen == 0 || StationScreen == 2)
            {
                //SetTGP2();
                List<string> IniList2 = new List<string>();
                IniList2.Add("");
                IniList2.Add("");
                IniList2.Add("");
                IniList2.Add("");
                IniList2.Add("");
                XStaticInit2(IniList2, false);  //屏2初始化创建五个静态文本区域
            }
            TGPconfig.radioButton1.Checked = true;
            Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "程序启动");


        }



        /// <summary>
        /// Part2：Timer线程
        /// </summary> 
        public string temp1 = "", temp3 = "", temp4 = "", temp5 = "", temp6 = "", temp7 = "";
        //public string tempFirst = "";
        public string temp1b = "", temp3b = "", temp4b = "", temp5b = "", temp6b = "", temp7b = "";
        //public string tempFirstb = "";
        public bool P1State = false; //P1 状态 False为空包
        public bool P2State = false; //P2 状态 False为空包 
        public string tempFirstDC = "";
        private void timer_Checker_Tick(object sender, EventArgs e)
        {
            try
            {
                //峰1有数据
                if (RecieveToProcessQueue.Count > 0) //峰1有数据
                {
                    PRC_Tool.SafeArrayList TempList = RecieveToProcessQueue.Dequeue() as PRC_Tool.SafeArrayList;

                    if (TempList.Count >= 6)
                    {
                        //峰1空包
                        if (TempList[1] as string == "      本场    0          0                    " &&
                           TempList[2] as string == "      本场    0          0                    " &&
                           TempList[3] as string == "      本场    0          0                    " &&
                           TempList[4] as string == "      本场    0          0                    " &&
                           TempList[5] as string == "      本场    0          0                    ") //峰1空包
                        {
                            P1State = false; // P1 状态为空包
                        }
                        //峰1有效
                        else
                        {
                            P1State = true;  //P1 状态为  数据有效
                            //MessageBox.Show("峰1有效");

                            //WinForm中Label显示驼峰信息
                            Line1Lab.Visible = true;
                            Line2Lab.Visible = true;
                            Line3Lab.Visible = true;
                            Line4Lab.Visible = true;
                            Line5Lab.Visible = true;
                            Line6Lab.Visible = true;
                            Line7Lab.Visible = true;
                            label4.Visible = false;

                            //5钩数据分割字符串为数组并删除空字符
                            string[] ScreenArr1 = TempList[1].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr2 = TempList[2].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr3 = TempList[3].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr4 = TempList[4].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr5 = TempList[5].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //提取车次号,驼信
                            string[] ScreenArr0 = TempList[0].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string CarNum = ScreenArr0[1].Substring(3);   //车次号
                            string HumpSig = ScreenArr0[2].Substring(3);   //驼峰信号
                            string FirstDC = ScreenArr0[3].Substring(4);  //一分岔
                            //CarNum = "5171234";    //测试用J17-36183
                            string CarNumToSend = "";   //要发送的车次号
                            string HumpSigToSend = ""; //要发送的驼峰信号
                            /*//有-就分割，没-就取前三位
                            if (CarNum.Contains("-"))          //有-
                            {
                                string[] CarNumArray = CarNum.Split('-');
                                CarNumToSend = CarNumArray[0];
                            }
                            else if (CarNum.Contains("—"))     //是—
                            {
                                string[] CarNumArray = CarNum.Split('—');
                                CarNumToSend = CarNumArray[0];
                            }
                            else
                            {
                                if (CarNum.Length >= 3)
                                {
                                    CarNumToSend = CarNum.Substring(0, 3);
                                }
                            }*/
                            if (ScreenTop == 0)    //屏幕第一行显示配置0：车次+驼信   此时车次号不超过五位    
                            {
                                if (CarNum.Length >= 6)    //车次号大于六位留六位
                                {
                                    CarNumToSend = CarNum.Substring(0, 6);
                                }
                                else if (CarNum == "") //无车次号空7格 使驼信在右边
                                {
                                    CarNumToSend = "       ";
                                }
                                else
                                {
                                    CarNumToSend = CarNum;
                                }
                            }
                            else if (ScreenTop == 1)  //屏幕第一行显示配置1： 只有车次  此时车次号不超过八位
                            {
                                if (CarNum.Length >= 6)    //车次号大于6位留6位
                                {
                                    CarNumToSend = CarNum.Substring(0, 6);
                                }
                                else
                                {
                                    CarNumToSend = CarNum;
                                }
                            }
                            HumpSigToSend = HumpSig;

                            //赋值给UI界面
                            Line1Lab.Text = TempList[0] as string; //第一行 车次 一分岔等信息 
                            Line2Lab.Text = " 钩序 场别  股道 方式 辆数 状态 开口号";
                            //钩信息元素大于5才赋值给界面
                            if (ScreenArr1.Count() > 5)
                            {
                                Line3Lab.Text = TempList[1] as string;
                                if (ScreenArr2.Count() > 5)
                                {
                                    Line4Lab.Text = TempList[2] as string;
                                    if (ScreenArr3.Count() > 5)
                                    {
                                        Line5Lab.Text = TempList[3] as string;
                                        if (ScreenArr4.Count() > 5)
                                        {
                                            Line6Lab.Text = TempList[4] as string;
                                            if (ScreenArr5.Count() > 5)
                                            {
                                                Line7Lab.Text = TempList[5] as string;
                                            }
                                            else Line7Lab.Text = "";
                                        }
                                        else Line6Lab.Text = Line7Lab.Text = "";
                                    }
                                    else Line5Lab.Text = Line6Lab.Text = Line7Lab.Text = "";
                                }
                                else Line4Lab.Text = Line5Lab.Text = Line6Lab.Text = Line7Lab.Text = "";
                            }
                            else Line3Lab.Text = Line4Lab.Text = Line5Lab.Text = Line6Lab.Text = Line7Lab.Text = "";
                            
                            //LineLab颜色变化
                            if (DoMarkLiu == "-")
                            {
                                if (Line3Lab.Text.Contains(DoMarkLiu)) { Line3Lab.ForeColor = Color.Lime; } else if (Line3Lab.Text.Contains(DoMarkDan)) { Line3Lab.ForeColor = Color.Yellow; } else Line3Lab.ForeColor = Color.Red;
                                if (Line4Lab.Text.Contains(DoMarkLiu)) { Line4Lab.ForeColor = Color.Lime; } else if (Line4Lab.Text.Contains(DoMarkDan)) { Line4Lab.ForeColor = Color.Yellow; } else Line4Lab.ForeColor = Color.Red;
                                if (Line5Lab.Text.Contains(DoMarkLiu)) { Line5Lab.ForeColor = Color.Lime; } else if (Line5Lab.Text.Contains(DoMarkDan)) { Line5Lab.ForeColor = Color.Yellow; } else Line5Lab.ForeColor = Color.Red;
                                if (Line6Lab.Text.Contains(DoMarkLiu)) { Line6Lab.ForeColor = Color.Lime; } else if (Line6Lab.Text.Contains(DoMarkDan)) { Line6Lab.ForeColor = Color.Yellow; } else Line6Lab.ForeColor = Color.Red;
                                if (Line7Lab.Text.Contains(DoMarkLiu)) { Line7Lab.ForeColor = Color.Lime; } else if (Line7Lab.Text.Contains(DoMarkDan)) { Line7Lab.ForeColor = Color.Yellow; } else Line7Lab.ForeColor = Color.Red;
                            }
                            else if (DoMarkLiu == "X")
                            {
                                if (Line3Lab.Text.Contains(DoMarkGua) || Line3Lab.Text.Contains(DoMarkJin) || Line3Lab.Text.Contains(DoMarkXia) || Line3Lab.Text.Contains(DomarkShang)) { Line3Lab.ForeColor = Color.Red; } else if (Line3Lab.Text.Contains(DoMarkDan)) { Line3Lab.ForeColor = Color.Yellow; } else Line3Lab.ForeColor = Color.Lime;
                                if (Line4Lab.Text.Contains(DoMarkGua) || Line4Lab.Text.Contains(DoMarkJin) || Line4Lab.Text.Contains(DoMarkXia) || Line4Lab.Text.Contains(DomarkShang)) { Line4Lab.ForeColor = Color.Red; } else if (Line4Lab.Text.Contains(DoMarkDan)) { Line4Lab.ForeColor = Color.Yellow; } else Line4Lab.ForeColor = Color.Lime;
                                if (Line5Lab.Text.Contains(DoMarkGua) || Line5Lab.Text.Contains(DoMarkJin) || Line5Lab.Text.Contains(DoMarkXia) || Line5Lab.Text.Contains(DomarkShang)) { Line5Lab.ForeColor = Color.Red; } else if (Line5Lab.Text.Contains(DoMarkDan)) { Line5Lab.ForeColor = Color.Yellow; } else Line5Lab.ForeColor = Color.Lime;
                                if (Line6Lab.Text.Contains(DoMarkGua) || Line6Lab.Text.Contains(DoMarkJin) || Line6Lab.Text.Contains(DoMarkXia) || Line6Lab.Text.Contains(DomarkShang)) { Line6Lab.ForeColor = Color.Red; } else if (Line6Lab.Text.Contains(DoMarkDan)) { Line6Lab.ForeColor = Color.Yellow; } else Line6Lab.ForeColor = Color.Lime;
                                if (Line7Lab.Text.Contains(DoMarkGua) || Line7Lab.Text.Contains(DoMarkJin) || Line7Lab.Text.Contains(DoMarkXia) || Line7Lab.Text.Contains(DomarkShang)) { Line7Lab.ForeColor = Color.Red; } else if (Line7Lab.Text.Contains(DoMarkDan)) { Line7Lab.ForeColor = Color.Yellow; } else Line7Lab.ForeColor = Color.Lime;
                            }
                            else
                            {
                                if (Line3Lab.Text.Contains(DoMarkLiu)) { Line3Lab.ForeColor = Color.Lime; } else if (Line3Lab.Text.Contains(DoMarkDan)) { Line3Lab.ForeColor = Color.Yellow; } else Line3Lab.ForeColor = Color.Red;
                                if (Line4Lab.Text.Contains(DoMarkLiu)) { Line4Lab.ForeColor = Color.Lime; } else if (Line4Lab.Text.Contains(DoMarkDan)) { Line4Lab.ForeColor = Color.Yellow; } else Line4Lab.ForeColor = Color.Red;
                                if (Line5Lab.Text.Contains(DoMarkLiu)) { Line5Lab.ForeColor = Color.Lime; } else if (Line5Lab.Text.Contains(DoMarkDan)) { Line5Lab.ForeColor = Color.Yellow; } else Line5Lab.ForeColor = Color.Red;
                                if (Line6Lab.Text.Contains(DoMarkLiu)) { Line6Lab.ForeColor = Color.Lime; } else if (Line6Lab.Text.Contains(DoMarkDan)) { Line6Lab.ForeColor = Color.Yellow; } else Line6Lab.ForeColor = Color.Red;
                                if (Line7Lab.Text.Contains(DoMarkLiu)) { Line7Lab.ForeColor = Color.Lime; } else if (Line7Lab.Text.Contains(DoMarkDan)) { Line7Lab.ForeColor = Color.Yellow; } else Line7Lab.ForeColor = Color.Red;

                            }
                            /*if (ScreenArr1[3].Contains(DoMarkLiu)) { Line3Lab.ForeColor = Color.Lime; } else if (ScreenArr1[3].Contains(DoMarkDan)) { Line3Lab.ForeColor = Color.Yellow; } else Line3Lab.ForeColor = Color.Red;
                            if (ScreenArr2[3].Contains(DoMarkLiu)) { Line4Lab.ForeColor = Color.Lime; } else if (ScreenArr2[3].Contains(DoMarkDan)) { Line4Lab.ForeColor = Color.Yellow; } else Line4Lab.ForeColor = Color.Red;
                            if (ScreenArr3[3].Contains(DoMarkLiu)) { Line5Lab.ForeColor = Color.Lime; } else if (ScreenArr3[3].Contains(DoMarkDan)) { Line5Lab.ForeColor = Color.Yellow; } else Line5Lab.ForeColor = Color.Red;
                            if (ScreenArr4[3].Contains(DoMarkLiu)) { Line6Lab.ForeColor = Color.Lime; } else if (ScreenArr4[3].Contains(DoMarkDan)) { Line6Lab.ForeColor = Color.Yellow; } else Line6Lab.ForeColor = Color.Red;
                            if (ScreenArr5[3].Contains(DoMarkLiu)) { Line7Lab.ForeColor = Color.Lime; } else if (ScreenArr5[3].Contains(DoMarkDan)) { Line7Lab.ForeColor = Color.Yellow; } else Line7Lab.ForeColor = Color.Red;*/
                            //处理数据准备发送给屏幕队列
                            List<string> screen = new List<string>();
                            bool No1VoiceShow = true; //第一行语音是否显示 (语音随翻钩变化)
                            //确保每行至少五个元素时才能发送屏幕
                            int row = 0; //行数
                            try
                            {
                                if (ScreenTop == 0)
                                {
                                    screen.Add("  " + CarNumToSend + " " + HumpSigToSend);  //加上车次号和主体信号
                                    //screen.Add(" " + 2345678 + " " + HumpSigToSend);  //加上车次号和主体信号
                                    row++;
                                }
                                else if (ScreenTop == 1)
                                {
                                    screen.Add(CarNumToSend);      //只有车次号
                                    row++;
                                }
                                else if(ScreenTop == 2)
                                {
                                    screen.Add("       " + HumpSigToSend);    //只有驼信
                                    row++;
                                }

                                if (ScreenArr1.Count() > 5 && (row < AreaNum)) //第一钩
                                {
                                    bool bshow = true;  //true 第一钩显示    false 不显示
                                    if ((!TempList[0].ToString().Contains("未压入")) && (Convert.ToInt32(ScreenArr1[4]) <= RenewCarNum)) //压入且车辆小于4略过第一钩显示第二钩（提前翻钩）
                                    {
                                        bshow = false;
                                        No1VoiceShow = false;
                                    }
                                    if (bshow)
                                    {
                                        if ((Convert.ToInt32(ScreenArr1[0]) > 100)) //三位数前两组之间没有空格
                                        {
                                            //screen.Add(ScreenArr1[0] + ScreenArr1[3] + " " + ScreenArr1[4] + " " + ScreenArr1[5]);
                                            screen.Add(" "+ ScreenArr1[0].Substring(1) + "  " + ScreenArr1[3] + "  " + ScreenArr1[4] + "  " + ScreenArr1[5]);
                                        }
                                        else                                        //两位数删掉勾序第一个字符
                                        {
                                            screen.Add(" " + ScreenArr1[0].Substring(0) + "  " + ScreenArr1[3] + "  " + ScreenArr1[4] + "  " + ScreenArr1[5]);
                                        }
                                        row++;
                                        if (ScreenArr1.Count() >= 7 && (row < AreaNum))  //有开口号
                                        {
                                            screen.Add(ScreenArr1[6]);
                                            row++;
                                        }
                                    }
                                    //screen.Add(ScreenArr3[2] + " " + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);

                                    if (ScreenArr2.Count() > 5 && (row < AreaNum))  //第二钩
                                    {
                                        if ((Convert.ToInt32(ScreenArr2[0]) > 100))
                                        {
                                            //screen.Add(ScreenArr2[0] + ScreenArr2[3] + " " + ScreenArr2[4] + " " + ScreenArr2[5]);
                                            screen.Add(" " + ScreenArr2[0].Substring(1) + "  " + ScreenArr2[3] + "  " + ScreenArr2[4] + "  " + ScreenArr2[5]);
                                        }
                                        else
                                        {
                                            screen.Add(" " + ScreenArr2[0].Substring(0) + "  " + ScreenArr2[3] + "  " + ScreenArr2[4] + "  " + ScreenArr2[5]);
                                        }
                                        row++;
                                        if (ScreenArr2.Count() >= 7 && (row < AreaNum))
                                        {
                                            screen.Add(ScreenArr2[6]);
                                            row++;
                                        }

                                        if (ScreenArr3.Count() > 5 && (row < AreaNum)) //第三钩
                                        {
                                            if ((Convert.ToInt32(ScreenArr3[0]) > 100))
                                            {
                                                //screen.Add(ScreenArr3[0] + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);
                                                screen.Add(" " + ScreenArr3[0].Substring(1) + "  " + ScreenArr3[3] + "  " + ScreenArr3[4] + "  " + ScreenArr3[5]);
                                            }
                                            else
                                            {
                                                screen.Add(" " + ScreenArr3[0].Substring(0) + "  " + ScreenArr3[3] + "  " + ScreenArr3[4] + "  " + ScreenArr3[5]);
                                            }
                                            row++;
                                            if (ScreenArr3.Count() >= 7 && (row < AreaNum))
                                            {
                                                screen.Add(ScreenArr3[6]);
                                                row++;
                                            }

                                            if (ScreenArr4.Count() > 5 && (row < AreaNum)) //第四钩
                                            {
                                                if ((Convert.ToInt32(ScreenArr4[0]) > 100))
                                                {
                                                    //screen.Add(ScreenArr4[0] + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                                    screen.Add(" " + ScreenArr4[0].Substring(1) + "  " + ScreenArr4[3] + "  " + ScreenArr4[4] + "  " + ScreenArr4[5]);
                                                }
                                                else
                                                {
                                                    screen.Add(" " + ScreenArr4[0].Substring(0) + "  " + ScreenArr4[3] + "  " + ScreenArr4[4] + "  " + ScreenArr4[5]);
                                                }
                                                row++;
                                                if (ScreenArr4.Count() >= 7 && (row < AreaNum))
                                                {
                                                    screen.Add(ScreenArr4[6]);
                                                    row++;
                                                }

                                                if (ScreenArr5.Count() > 5 && (row < AreaNum)) //第五钩
                                                {
                                                    if ((Convert.ToInt32(ScreenArr5[0]) > 100))
                                                    {
                                                        //screen.Add(ScreenArr5[0] + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                                        screen.Add(" " + ScreenArr5[0].Substring(1) + "  " + ScreenArr5[3] + "  " + ScreenArr5[4] + "  " + ScreenArr5[5]);
                                                    }
                                                    else
                                                    {
                                                        screen.Add(" " + ScreenArr5[0].Substring(0) + "  " + ScreenArr5[3] + "  " + ScreenArr5[4] + "  " + ScreenArr5[5]);
                                                    }
                                                    row++;

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }


                            //发屏幕判定：如果数据变化，发送给屏幕，防止屏幕刷新过快;时间超过15s不刷新也会刷新一次
                            TimeSpan tsScreen = DateTime.Now - timeScreen;
                            if (((Line1Lab.Text != temp1 || Line3Lab.Text != temp3 || Line4Lab.Text != temp4 || Line5Lab.Text != temp5 || Line6Lab.Text != temp6 || Line7Lab.Text != temp7)) ||
                                tsScreen.TotalSeconds > 15)

                            {
                                timeScreen = DateTime.Now;
                                temp1 = Line1Lab.Text;
                                temp3 = Line3Lab.Text;
                                temp4 = Line4Lab.Text;
                                temp5 = Line5Lab.Text;
                                temp6 = Line6Lab.Text;
                                temp7 = Line7Lab.Text;


                                //无语音情况发送队列
                                if (TGPconfig.SpeakState == false)
                                {
                                    SendDataQueue.Enqueue(new DataOfSend(screen, false));
                                }
                                //有语音情况发送队列
                                else if (TGPconfig.SpeakState == true)
                                {
                                    //准备语音内容
                                    string VoiceStr = "";
                                    //第一钩内容
                                    if (No1VoiceShow == true && ScreenArr1.Count() > 5)
                                    {
                                        //完整播报内容
                                        if (TGPconfig.SpeakContent == 1)
                                        {
                                            VoiceStr = ScreenArr1[0] + "钩, "
                                                //+ ScreenArr1[1] + ", "
                                                + ScreenArr1[2] + "道, "
                                                + ScreenArr1[3].Replace(DoMarkLiu, "溜放钩").Replace(DoMarkDan, "手动单溜钩").Replace(DoMarkGua, "挂车").Replace(DoMarkJin, "禁溜车").Replace(DoMarkXia, "机车下峰").Replace(DomarkShang, "机车上峰") + ", "
                                                + toChinese(Convert.ToInt32(ScreenArr1[4])) + "辆, ";
                                                //+ ScreenArr1[5].Replace("K", "空车").Replace("H", "空重混编车").Replace("Z", "重车").Replace("X", "超重车").Replace("！", "注意溜放车");
                                            if (ScreenArr1.Count() >= 7)
                                            {
                                                VoiceStr = VoiceStr + ", " + "开口号" + ScreenArr1[6];
                                            }
                                        }
                                        //简明播报内容
                                        else if (TGPconfig.SpeakContent == 2)
                                        {
                                            VoiceStr = ScreenArr1[0] + "钩, "
                                                + toChinese(Convert.ToInt32(ScreenArr1[4])) + "辆, ";
                                        }
                                    }
                                    //翻钩内容（第二钩）
                                    else if (No1VoiceShow == false && ScreenArr2.Count() > 5)
                                    {
                                        //完整播报内容
                                        if (TGPconfig.SpeakContent == 1)
                                        {
                                            VoiceStr = ScreenArr2[0] + "钩, "
                                            //+ ScreenArr2[1] + ", "
                                            + ScreenArr2[2] + "道, "
                                            + ScreenArr2[3].Replace(DoMarkLiu, "溜放钩").Replace(DoMarkDan, "手动单溜钩").Replace(DoMarkGua, "挂车").Replace(DoMarkJin, "禁溜车").Replace(DoMarkXia, "机车下峰").Replace(DomarkShang, "机车上峰") + ", "
                                            + toChinese(Convert.ToInt32(ScreenArr2[4])) + "辆, ";
                                            //+ ScreenArr2[5].Replace("K", "空车").Replace("H", "空重混编车").Replace("Z", "重车").Replace("X", "超重车").Replace("！", "注意溜放车");
                                            if (ScreenArr2.Count() >= 7)
                                            {
                                                VoiceStr = VoiceStr + "  " + "开口号" + ScreenArr2[6];
                                            }
                                        }
                                        //简明播报内容
                                        else if (TGPconfig.SpeakContent == 2)
                                        {
                                            VoiceStr = ScreenArr2[0] + "钩, "
                                                + toChinese(Convert.ToInt32(ScreenArr2[4])) + "辆, ";
                                        }
                                    }
                                    SendDataQueue.Enqueue(new DataOfSend(screen, false, VoiceStr));
                                    //配置语音模块情况下直接加入语音队列
                                    if(StationAudio == 0 || StationAudio ==1)
                                    {
                                        if (TGPconfig.SpeakNum == 1)
                                        {
                                            AudioQueue.Enqueue(VoiceStr);
                                        }
                                        else if(TGPconfig.SpeakNum == 2)
                                        {
                                            AudioQueue.Enqueue(VoiceStr + "[p1500]" + VoiceStr);  //两遍中停顿1500ms
                                        }
                                        else
                                        {
                                            AudioQueue.Enqueue(VoiceStr);
                                        }
                                    }
                                }

                            }

                            //一分岔信息播报(峰1)
                            if(TGPconfig.SpeakState == true) //语音功能开启
                            {
                                //准备一分岔语音内容
                                string DCVoiceStr = ""; 
                                if (FirstDC != tempFirstDC) 
                                {
                                    if (FirstDC == "未压入")
                                    {
                                        //DCVoiceStr = "一分岔" + FirstDC.Replace("未压入", "出清");
                                        DCVoiceStr = "一分岔出清";
                                    }
                                    else
                                    {
                                        //DCVoiceStr = "一分岔" + FirstDC;  //语音内容赋值
                                        DCVoiceStr = "一分岔压入";
                                    }
                                    tempFirstDC = FirstDC;
                                    if (StationAudio == 0 || StationAudio == 1) //配置相应语音模块情况下加入语音队列
                                    {
                                        AudioQueue.Enqueue(DCVoiceStr);
                                    }
                                }
                            }
                        }
                    }
                }
                //峰2有数据
                if (RecieveToProcessQueue2.Count > 0)//峰2有数据
                {
                    PRC_Tool.SafeArrayList TempList = RecieveToProcessQueue2.Dequeue() as PRC_Tool.SafeArrayList;

                    if (TempList.Count >= 6)
                    {
                        //峰2空包
                        if (TempList[1] as string == "      本场    0          0                    " &&
                           TempList[2] as string == "      本场    0          0                    " &&
                           TempList[3] as string == "      本场    0          0                    " &&
                           TempList[4] as string == "      本场    0          0                    " &&
                           TempList[5] as string == "      本场    0          0                    ") //峰2空包
                        {
                            P2State = false; // P2 状态为空包
                            //MessageBox.Show("峰2空包");
                        }
                        //峰2有效
                        else
                        {
                            P2State = true;  //P2 状态为  数据有效
                            //MessageBox.Show("峰2有效");

                            //WinForm中Label显示驼峰信息
                            Line1Labb.Visible = true;
                            Line2Labb.Visible = true;
                            Line3Labb.Visible = true;
                            Line4Labb.Visible = true;
                            Line5Labb.Visible = true;
                            Line6Labb.Visible = true;
                            Line7Labb.Visible = true;
                            label5.Visible = false;

                            //5钩数据分割字符串为数组并删除空字符
                            string[] ScreenArr1 = TempList[1].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr2 = TempList[2].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr3 = TempList[3].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr4 = TempList[4].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr5 = TempList[5].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //提取车次号,驼信
                            string[] ScreenArr0 = TempList[0].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string CarNum = ScreenArr0[1].Substring(3);   //车次号
                            string HumpSig = ScreenArr0[2].Substring(3);   //驼峰信号
                            string FirstDC = ScreenArr0[3].Substring(4);  //一分岔
                            //CarNum = "123456789";    //测试用J17-36183
                            string CarNumToSend = "";   //要发送的车次号
                            string HumpSigToSend = ""; //要发送的驼峰信号
                            /*//有-就分割，没-就取前三位
                            if (CarNum.Contains("-"))          //有-
                            {
                                string[] CarNumArray = CarNum.Split('-');
                                CarNumToSend = CarNumArray[0];
                            }
                            else if (CarNum.Contains("—"))     //是—
                            {
                                string[] CarNumArray = CarNum.Split('—');
                                CarNumToSend = CarNumArray[0];
                            }
                            else
                            {
                                if (CarNum.Length >= 3)
                                {
                                    CarNumToSend = CarNum.Substring(0, 3);
                                }
                            }*/
                            if (ScreenTop == 0)    //屏幕第一行显示配置0：车次+驼信   此时车次号不超过五位    
                            {
                                if (CarNum.Length >= 6)    //车次号大于六位留六位
                                {
                                    CarNumToSend = CarNum.Substring(0, 6);
                                }
                                else if (CarNum == "") //无车次号空7格 使驼信在右边
                                {
                                    CarNumToSend = "       ";
                                }
                                else
                                {
                                    CarNumToSend = CarNum;
                                }
                            }
                            else if (ScreenTop == 1)  //屏幕第一行显示配置1： 只有车次  此时车次号不超过八位
                            {
                                if (CarNum.Length >= 6)    //车次号大于6位留6位
                                {
                                    CarNumToSend = CarNum.Substring(0, 6);
                                }
                                else
                                {
                                    CarNumToSend = CarNum;
                                }
                            }
                            HumpSigToSend = HumpSig;

                            //赋值给UI界面
                            Line1Labb.Text = TempList[0] as string; //第一行 车次 一分岔等信息 
                            Line2Labb.Text = " 钩序 场别  股道 方式 辆数 状态 开口号";
                            //钩信息元素大于5才赋值给界面
                            if (ScreenArr1.Count() > 5)
                            {
                                Line3Labb.Text = TempList[1] as string;
                                if (ScreenArr2.Count() > 5)
                                {
                                    Line4Labb.Text = TempList[2] as string;
                                    if (ScreenArr3.Count() > 5)
                                    {
                                        Line5Labb.Text = TempList[3] as string;
                                        if (ScreenArr4.Count() > 5)
                                        {
                                            Line6Labb.Text = TempList[4] as string;
                                            if (ScreenArr5.Count() > 5)
                                            {
                                                Line7Labb.Text = TempList[5] as string;
                                            }
                                            else Line7Labb.Text = "";
                                        }
                                        else Line6Labb.Text = Line7Labb.Text = "";
                                    }
                                    else Line5Labb.Text = Line6Labb.Text = Line7Labb.Text = "";
                                }
                                else Line4Labb.Text = Line5Labb.Text = Line6Labb.Text = Line7Labb.Text = "";
                            }
                            else Line3Labb.Text = Line4Labb.Text = Line5Labb.Text = Line6Labb.Text = Line7Labb.Text = "";
                            //LineLabb颜色变化
                            if (DoMarkLiu == "-")
                            {
                                if (Line3Labb.Text.Contains(DoMarkLiu)) { Line3Labb.ForeColor = Color.Lime; } else if (Line3Labb.Text.Contains(DoMarkDan)) { Line3Labb.ForeColor = Color.Yellow; } else Line3Labb.ForeColor = Color.Red;
                                if (Line4Labb.Text.Contains(DoMarkLiu)) { Line4Labb.ForeColor = Color.Lime; } else if (Line4Labb.Text.Contains(DoMarkDan)) { Line4Labb.ForeColor = Color.Yellow; } else Line4Labb.ForeColor = Color.Red;
                                if (Line5Labb.Text.Contains(DoMarkLiu)) { Line5Labb.ForeColor = Color.Lime; } else if (Line5Labb.Text.Contains(DoMarkDan)) { Line5Labb.ForeColor = Color.Yellow; } else Line5Labb.ForeColor = Color.Red;
                                if (Line6Labb.Text.Contains(DoMarkLiu)) { Line6Labb.ForeColor = Color.Lime; } else if (Line6Labb.Text.Contains(DoMarkDan)) { Line6Labb.ForeColor = Color.Yellow; } else Line6Labb.ForeColor = Color.Red;
                                if (Line7Labb.Text.Contains(DoMarkLiu)) { Line7Labb.ForeColor = Color.Lime; } else if (Line7Labb.Text.Contains(DoMarkDan)) { Line7Labb.ForeColor = Color.Yellow; } else Line7Labb.ForeColor = Color.Red;
                            }
                            else if (DoMarkLiu == "X")
                            {
                                if (Line3Labb.Text.Contains(DoMarkGua) || Line3Labb.Text.Contains(DoMarkJin) || Line3Labb.Text.Contains(DoMarkXia) || Line3Labb.Text.Contains(DomarkShang)) { Line3Labb.ForeColor = Color.Red; } else if (Line3Labb.Text.Contains(DoMarkDan)) { Line3Labb.ForeColor = Color.Yellow; } else Line3Labb.ForeColor = Color.Lime;
                                if (Line4Labb.Text.Contains(DoMarkGua) || Line4Labb.Text.Contains(DoMarkJin) || Line4Labb.Text.Contains(DoMarkXia) || Line4Labb.Text.Contains(DomarkShang)) { Line4Labb.ForeColor = Color.Red; } else if (Line4Labb.Text.Contains(DoMarkDan)) { Line4Labb.ForeColor = Color.Yellow; } else Line4Labb.ForeColor = Color.Lime;
                                if (Line5Labb.Text.Contains(DoMarkGua) || Line5Labb.Text.Contains(DoMarkJin) || Line5Labb.Text.Contains(DoMarkXia) || Line5Labb.Text.Contains(DomarkShang)) { Line5Labb.ForeColor = Color.Red; } else if (Line5Labb.Text.Contains(DoMarkDan)) { Line5Labb.ForeColor = Color.Yellow; } else Line5Labb.ForeColor = Color.Lime;
                                if (Line6Labb.Text.Contains(DoMarkGua) || Line6Labb.Text.Contains(DoMarkJin) || Line6Labb.Text.Contains(DoMarkXia) || Line6Labb.Text.Contains(DomarkShang)) { Line6Labb.ForeColor = Color.Red; } else if (Line6Labb.Text.Contains(DoMarkDan)) { Line6Labb.ForeColor = Color.Yellow; } else Line6Labb.ForeColor = Color.Lime;
                                if (Line7Labb.Text.Contains(DoMarkGua) || Line7Labb.Text.Contains(DoMarkJin) || Line7Labb.Text.Contains(DoMarkXia) || Line7Labb.Text.Contains(DomarkShang)) { Line7Labb.ForeColor = Color.Red; } else if (Line7Labb.Text.Contains(DoMarkDan)) { Line7Labb.ForeColor = Color.Yellow; } else Line7Labb.ForeColor = Color.Lime;
                            }
                            else
                            {
                                if (Line3Labb.Text.Contains(DoMarkLiu)) { Line3Labb.ForeColor = Color.Lime; } else if (Line3Labb.Text.Contains(DoMarkDan)) { Line3Labb.ForeColor = Color.Yellow; } else Line3Labb.ForeColor = Color.Red;
                                if (Line4Labb.Text.Contains(DoMarkLiu)) { Line4Labb.ForeColor = Color.Lime; } else if (Line4Labb.Text.Contains(DoMarkDan)) { Line4Labb.ForeColor = Color.Yellow; } else Line4Labb.ForeColor = Color.Red;
                                if (Line5Labb.Text.Contains(DoMarkLiu)) { Line5Labb.ForeColor = Color.Lime; } else if (Line5Labb.Text.Contains(DoMarkDan)) { Line5Labb.ForeColor = Color.Yellow; } else Line5Labb.ForeColor = Color.Red;
                                if (Line6Labb.Text.Contains(DoMarkLiu)) { Line6Labb.ForeColor = Color.Lime; } else if (Line6Labb.Text.Contains(DoMarkDan)) { Line6Labb.ForeColor = Color.Yellow; } else Line6Labb.ForeColor = Color.Red;
                                if (Line7Labb.Text.Contains(DoMarkLiu)) { Line7Labb.ForeColor = Color.Lime; } else if (Line7Labb.Text.Contains(DoMarkDan)) { Line7Labb.ForeColor = Color.Yellow; } else Line7Labb.ForeColor = Color.Red;
                            }
/*
                            if (ScreenArr1[3].Contains(DoMarkLiu)) { Line3Labb.ForeColor = Color.Lime; } else if (ScreenArr1[3].Contains(DoMarkDan)) { Line3Labb.ForeColor = Color.Yellow; } else Line3Labb.ForeColor = Color.Red;
                            if (ScreenArr2[3].Contains(DoMarkLiu)) { Line4Labb.ForeColor = Color.Lime; } else if (ScreenArr2[3].Contains(DoMarkDan)) { Line4Labb.ForeColor = Color.Yellow; } else Line4Labb.ForeColor = Color.Red;
                            if (ScreenArr3[3].Contains(DoMarkLiu)) { Line5Labb.ForeColor = Color.Lime; } else if (ScreenArr3[3].Contains(DoMarkDan)) { Line5Labb.ForeColor = Color.Yellow; } else Line5Labb.ForeColor = Color.Red;
                            if (ScreenArr4[3].Contains(DoMarkLiu)) { Line6Labb.ForeColor = Color.Lime; } else if (ScreenArr4[3].Contains(DoMarkDan)) { Line6Labb.ForeColor = Color.Yellow; } else Line6Labb.ForeColor = Color.Red;
                            if (ScreenArr5[3].Contains(DoMarkLiu)) { Line7Labb.ForeColor = Color.Lime; } else if (ScreenArr5[3].Contains(DoMarkDan)) { Line7Labb.ForeColor = Color.Yellow; } else Line7Labb.ForeColor = Color.Red;
*/
                            //处理数据准备发送给屏幕队列
                            List<string> screen = new List<string>();
                            bool No1VoiceShow = true; //第一行语音是否显示 (语音随翻钩变化)
                            //确保每行至少五个元素时才能发送屏幕
                            int row = 0; //行数
                            try
                            {
                                if (ScreenTop == 0)
                                {
                                    screen.Add(CarNumToSend + " " + HumpSigToSend);  //加上车次号和主体信号
                                    row++;
                                }
                                else if (ScreenTop == 1)
                                {
                                    screen.Add(CarNumToSend);      //只有车次号
                                    row++;
                                }
                                else if (ScreenTop == 2)
                                {
                                    screen.Add("       " + HumpSigToSend);    //只有驼信
                                    row++;
                                }

                                if (ScreenArr1.Count() > 5 && (row < AreaNum)) //第一钩
                                {
                                    bool bshow = true;  //true 第一钩显示    false 不显示
                                    if ((!TempList[0].ToString().Contains("未压入")) && (Convert.ToInt32(ScreenArr1[4]) <= RenewCarNum)) //压入且车辆小于4略过第一钩显示第二钩（提前翻钩）
                                    {
                                        bshow = false;
                                        No1VoiceShow = false;
                                    }
                                    if (bshow)
                                    {
                                        if ((Convert.ToInt32(ScreenArr1[0]) > 100)) //三位数前两组之间没有空格
                                        {
                                            //screen.Add(ScreenArr1[0] + ScreenArr1[3] + " " + ScreenArr1[4] + " " + ScreenArr1[5]);
                                            screen.Add(ScreenArr1[0].Substring(1) + " " + ScreenArr1[3] + " " + ScreenArr1[4] + " " + ScreenArr1[5]);
                                        }
                                        else                                        //两位数删掉勾序第一个字符
                                        {
                                            screen.Add(ScreenArr1[0].Substring(0) + " " + ScreenArr1[3] + " " + ScreenArr1[4] + " " + ScreenArr1[5]);
                                        }
                                        row++;
                                        if (ScreenArr1.Count() >= 7 && (row < AreaNum))  //有开口号
                                        {
                                            screen.Add("  " + ScreenArr1[6]);
                                            row++;
                                        }
                                    }
                                    //screen.Add(ScreenArr3[2] + " " + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);

                                    if (ScreenArr2.Count() > 5 && (row < AreaNum))  //第二钩
                                    {
                                        if ((Convert.ToInt32(ScreenArr2[0]) > 100))
                                        {
                                            //screen.Add(ScreenArr2[0] + ScreenArr2[3] + " " + ScreenArr2[4] + " " + ScreenArr2[5]);
                                            screen.Add(ScreenArr2[0].Substring(1) + " " + ScreenArr2[3] + " " + ScreenArr2[4] + " " + ScreenArr2[5]);
                                        }
                                        else
                                        {
                                            screen.Add(ScreenArr2[0].Substring(0) + " " + ScreenArr2[3] + " " + ScreenArr2[4] + " " + ScreenArr2[5]);
                                        }
                                        row++;
                                        if (ScreenArr2.Count() >= 7 && (row < AreaNum))
                                        {
                                            screen.Add("  " + ScreenArr2[6]);
                                            row++;
                                        }

                                        if (ScreenArr3.Count() > 5 && (row < AreaNum)) //第三钩
                                        {
                                            if ((Convert.ToInt32(ScreenArr3[0]) > 100))
                                            {
                                                //screen.Add(ScreenArr3[0] + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);
                                                screen.Add(ScreenArr3[0].Substring(1) + " " + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);
                                            }
                                            else
                                            {
                                                screen.Add(ScreenArr3[0].Substring(0) + " " + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);
                                            }
                                            row++;
                                            if (ScreenArr3.Count() >= 7 && (row < AreaNum))
                                            {
                                                screen.Add("  " + ScreenArr3[6]);
                                                row++;
                                            }

                                            if (ScreenArr4.Count() > 5 && (row < AreaNum)) //第四钩
                                            {
                                                if ((Convert.ToInt32(ScreenArr4[0]) > 100))
                                                {
                                                    //screen.Add(ScreenArr4[0] + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                                    screen.Add(ScreenArr4[0].Substring(1) + " " + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                                }
                                                else
                                                {
                                                    screen.Add(ScreenArr4[0].Substring(0) + " " + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                                }
                                                row++;
                                                if (ScreenArr4.Count() >= 7 && (row < AreaNum))
                                                {
                                                    screen.Add("  " + ScreenArr4[6]);
                                                    row++;
                                                }

                                                if (ScreenArr5.Count() > 5 && (row < AreaNum)) //第五钩
                                                {
                                                    if ((Convert.ToInt32(ScreenArr5[0]) > 100))
                                                    {
                                                        //screen.Add(ScreenArr5[0] + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                                        screen.Add(ScreenArr5[0].Substring(1) + " " + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                                    }
                                                    else
                                                    {
                                                        screen.Add(ScreenArr5[0].Substring(0) + " " + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                                    }
                                                    row++;

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }


                            //发屏幕判定：如果数据变化，发送给屏幕，防止屏幕刷新过快;时间超过15s不刷新也会刷新一次
                            TimeSpan tsScreen = DateTime.Now - timeScreen2;
                            if (((Line1Labb.Text != temp1b || Line3Labb.Text != temp3b || Line4Labb.Text != temp4b || Line5Labb.Text != temp5b || Line6Labb.Text != temp6b || Line7Labb.Text != temp7b)) ||
                                tsScreen.TotalSeconds > 15)

                            {
                                timeScreen = DateTime.Now;
                                temp1b = Line1Labb.Text;
                                temp3b = Line3Labb.Text;
                                temp4b = Line4Labb.Text;
                                temp5b = Line5Labb.Text;
                                temp6b = Line6Labb.Text;
                                temp7b = Line7Labb.Text;

                                if (TGPconfig.SpeakState == false)
                                {
                                    SendDataQueue2.Enqueue(new DataOfSend(screen, false));
                                    //SendData(screen, false);
                                }
                                else if (TGPconfig.SpeakState == true)
                                {
                                    //准备语音内容
                                    string VoiceStr = "";
                                    //第一钩内容
                                    if (No1VoiceShow == true && ScreenArr1.Count() > 5)
                                    {
                                        //完整播报内容
                                        if (TGPconfig.SpeakContent == 1)
                                        {
                                            VoiceStr = ScreenArr1[0] + "钩, "
                                            //+ ScreenArr1[1] + ", "
                                            + ScreenArr1[2] + "道, "
                                            + ScreenArr1[3].Replace(DoMarkLiu, "溜放钩").Replace(DoMarkDan, "手动单溜钩").Replace(DoMarkGua, "挂车").Replace(DoMarkJin, "禁溜车").Replace(DoMarkXia, "机车下峰").Replace(DomarkShang, "机车上峰") + ", "
                                            + toChinese(Convert.ToInt32(ScreenArr1[4])) + "辆, ";
                                            //+ ScreenArr1[5].Replace("K", "空车").Replace("H", "空重混编车").Replace("Z", "重车").Replace("X", "超重车").Replace("！", "注意溜放车");
                                            if (ScreenArr1.Count() >= 7)
                                            {
                                                VoiceStr = VoiceStr + ", " + "开口号" + ScreenArr1[6];
                                            }
                                        }
                                        //简明播报内容
                                        else if (TGPconfig.SpeakContent == 2)
                                        {
                                            VoiceStr = ScreenArr1[0] + "钩, "
                                                + toChinese(Convert.ToInt32(ScreenArr1[4])) + "辆, ";
                                        }
                                    }
                                    //翻钩内容（第二钩）
                                    else if (No1VoiceShow == false && ScreenArr2.Count() > 5)
                                    {
                                        //完整播报内容
                                        if (TGPconfig.SpeakContent == 1)
                                        {
                                            VoiceStr = ScreenArr2[0] + "钩, "
                                            //+ ScreenArr2[1] + ", "
                                            + ScreenArr2[2] + "道, "
                                            + ScreenArr2[3].Replace(DoMarkLiu, "溜放钩").Replace(DoMarkDan, "手动单溜钩").Replace(DoMarkGua, "挂车").Replace(DoMarkJin, "禁溜车").Replace(DoMarkXia, "机车下峰").Replace(DomarkShang, "机车上峰") + ", "
                                            + toChinese(Convert.ToInt32(ScreenArr2[4])) + "辆, ";
                                            //+ ScreenArr2[5].Replace("K", "空车").Replace("H", "空重混编车").Replace("Z", "重车").Replace("X", "超重车").Replace("！", "注意溜放车");
                                            if (ScreenArr2.Count() >= 7)
                                            {
                                                VoiceStr = VoiceStr + "  " + "开口号" + ScreenArr2[6];
                                            }
                                        }
                                        //简明播报内容
                                        else if (TGPconfig.SpeakContent == 2)
                                        {
                                            VoiceStr = ScreenArr2[0] + "钩, "
                                                + toChinese(Convert.ToInt32(ScreenArr2[4])) + "辆, ";
                                        }
                                    }
                                    SendDataQueue2.Enqueue(new DataOfSend(screen, false, VoiceStr));
                                    //配置语音模块情况下直接加入语音队列
                                    if (StationAudio == 0 || StationAudio == 2)
                                    {
                                        if (TGPconfig.SpeakNum == 1)
                                        {
                                            AudioQueue2.Enqueue(VoiceStr);
                                        }
                                        else if (TGPconfig.SpeakNum == 2)
                                        {
                                            AudioQueue2.Enqueue(VoiceStr + "[p1500]" + VoiceStr);  //两遍中停顿1500ms
                                        }
                                        else
                                        {
                                            AudioQueue2.Enqueue(VoiceStr);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }

                #region
                /*#region//原程序UI界面及发送室外屏幕刷新
                //UI界面及发送室外屏幕刷新
                if (DisplayCon.Count >= 7)
                {
                    //如果是峰1信息
                    if (DisplayCon[0].ToString()[3] == '1')
                    {
                        //如果峰1信息是空包
                        if (DisplayCon.Count >= 7 &&
                            DisplayCon[2] as string == "      本场    0          0                    " &&

                            DisplayCon[3] as string == "      本场    0          0                    " &&
                            DisplayCon[4] as string == "      本场    0          0                    " &&
                            DisplayCon[5] as string == "      本场    0          0                    " &&
                            DisplayCon[6] as string == "      本场    0          0                    ")
                        {
                            
                            P1State = false; // P1 状态为空包
                        }
                        //如果峰1信息有效
                        else
                        {
                            P1State = true; //P1 状态为 数据有效
                            
                            //WinForm中Label显示驼峰信息
                            Line1Lab.Visible = true;
                            Line2Lab.Visible = true;
                            Line3Lab.Visible = true;
                            Line4Lab.Visible = true;
                            Line5Lab.Visible = true;
                            Line6Lab.Visible = true;
                            Line7Lab.Visible = true;
                            label4.Visible = false;


                            //准备发送给屏幕  
                            //分割字符串为数组并删除空字符
                            string[] ScreenArr3 = DisplayCon[2].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr4 = DisplayCon[3].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr5 = DisplayCon[4].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr6 = DisplayCon[5].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr7 = DisplayCon[6].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            

                            //判断不是空包才赋值给UI界面
                            Line1Lab.Text = DisplayCon[0] as string;
                            Line2Lab.Text = DisplayCon[1] as string;
                            Line3Lab.Text = DisplayCon[2] as string;
                            //if (DisplayCon[3] as string != "      本场    0          0              ")
                            if (ScreenArr4.Count() > 5)
                            {
                                Line4Lab.Text = DisplayCon[3] as string;
                                if (ScreenArr5.Count() > 5)
                                {
                                    Line5Lab.Text = DisplayCon[4] as string;
                                    if (ScreenArr6.Count() > 5)
                                    {
                                        Line6Lab.Text = DisplayCon[5] as string;
                                        if (ScreenArr7.Count() > 5)
                                        {
                                            Line7Lab.Text = DisplayCon[6] as string;
                                        }
                                        else Line7Lab.Text = "";
                                    }
                                    else Line6Lab.Text = Line7Lab.Text = "";
                                }
                                else Line5Lab.Text = Line6Lab.Text = Line7Lab.Text = "";
                            }
                            else Line4Lab.Text = Line5Lab.Text = Line6Lab.Text = Line7Lab.Text = "";

                            //LineLab颜色变化
                            if (Line3Lab.Text.Contains("-")) { Line3Lab.ForeColor = Color.Lime; } else if (Line3Lab.Text.Contains("D")) { Line3Lab.ForeColor = Color.Yellow; } else Line3Lab.ForeColor = Color.Red;
                            if (Line4Lab.Text.Contains("-")) { Line4Lab.ForeColor = Color.Lime; } else if (Line4Lab.Text.Contains("D")) { Line4Lab.ForeColor = Color.Yellow; } else Line4Lab.ForeColor = Color.Red;
                            if (Line5Lab.Text.Contains("-")) { Line5Lab.ForeColor = Color.Lime; } else if (Line5Lab.Text.Contains("D")) { Line5Lab.ForeColor = Color.Yellow; } else Line5Lab.ForeColor = Color.Red;
                            if (Line6Lab.Text.Contains("-")) { Line6Lab.ForeColor = Color.Lime; } else if (Line6Lab.Text.Contains("D")) { Line6Lab.ForeColor = Color.Yellow; } else Line6Lab.ForeColor = Color.Red;
                            if (Line7Lab.Text.Contains("-")) { Line7Lab.ForeColor = Color.Lime; } else if (Line7Lab.Text.Contains("D")) { Line7Lab.ForeColor = Color.Yellow; } else Line7Lab.ForeColor = Color.Red;



                            

                            List<string> screen = new List<string>();
                            
                            //确保每行至少五个元素时才能发送屏幕
                            int row = 0; //行数
                            try
                            {
                                if (ScreenArr3.Count() > 5 && (row < 4))
                                {
                                    bool bshow = true;
                                    if ((!DisplayCon[0].ToString().Contains("未压入")) && (Convert.ToInt32(ScreenArr3[4]) < 4))
                                    {
                                        bshow = false;
                                    }
                                    if (bshow)
                                    {
                                        if ((Convert.ToInt32(ScreenArr3[0]) > 100)) //三位数前两组之间没有空格
                                        {
                                            screen.Add(ScreenArr3[0] + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);
                                        }
                                        else                                        //两位数删掉勾序第一个字符
                                        {
                                            screen.Add(ScreenArr3[0].Substring(0) + " " + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);
                                        }
                                        row++;
                                        if (ScreenArr3.Count() >= 7 && (row < 4))
                                        {
                                            screen.Add("  " + ScreenArr3[6]);
                                            row++;
                                        }
                                    }
                                    //screen.Add(ScreenArr3[2] + " " + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);

                                    if (ScreenArr4.Count() > 5 && (row < 4))
                                    {
                                        if ((Convert.ToInt32(ScreenArr4[0]) > 100))
                                        {
                                            screen.Add(ScreenArr4[0] + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                        }
                                        else
                                        {
                                            screen.Add(ScreenArr4[0].Substring(0) + " " + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                        }
                                        row++;
                                        if (ScreenArr4.Count() >= 7 && (row < 4))
                                        {
                                            screen.Add("  " + ScreenArr4[6]);
                                            row++;
                                        }
                                        //screen.Add(ScreenArr4[2] + " " + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                        if (ScreenArr5.Count() > 5 && (row < 4))
                                        {
                                            if ((Convert.ToInt32(ScreenArr5[0]) > 100))
                                            {
                                                screen.Add(ScreenArr5[0] + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                            }
                                            else
                                            {
                                                screen.Add(ScreenArr5[0].Substring(0) + " " + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                            }
                                            row++;
                                            if (ScreenArr5.Count() >= 7 && (row < 4))
                                            {
                                                screen.Add("  " + ScreenArr5[6]);
                                                row++;
                                            }
                                            //screen.Add(ScreenArr5[2] + " " + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                            if (ScreenArr6.Count() > 5 && (row < 4))
                                            {
                                                if ((Convert.ToInt32(ScreenArr6[0]) > 100))
                                                {
                                                    screen.Add(ScreenArr6[0] + ScreenArr6[3] + " " + ScreenArr6[4] + " " + ScreenArr6[5]);
                                                }
                                                else
                                                {
                                                    screen.Add(ScreenArr6[0].Substring(0) + " " + ScreenArr6[3] + " " + ScreenArr6[4] + " " + ScreenArr6[5]);
                                                }
                                                row++;
                                                if (ScreenArr6.Count() >= 7 && (row < 4))
                                                {
                                                    screen.Add("  " + ScreenArr6[6]);
                                                    row++;
                                                }
                                                //screen.Add(ScreenArr6[2] + " " + ScreenArr6[3] + " " + ScreenArr6[4] + " " + ScreenArr6[5]);
                                                if (ScreenArr7.Count() > 5 && (row < 4))
                                                {
                                                    if ((Convert.ToInt32(ScreenArr7[0]) > 100))
                                                    {
                                                        screen.Add(ScreenArr7[0] + ScreenArr7[3] + " " + ScreenArr7[4] + " " + ScreenArr7[5]);
                                                    }
                                                    else
                                                    {
                                                        screen.Add(ScreenArr7[0].Substring(0) + " " + ScreenArr7[3] + " " + ScreenArr7[4] + " " + ScreenArr7[5]);
                                                    }
                                                    row++;
                                                    //screen.Add(ScreenArr7[2] + " " + ScreenArr7[3] + " " + ScreenArr7[4] + " " + ScreenArr7[5]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }


                            //发屏幕判定：如果数据变化，发送给屏幕，防止屏幕刷新过快;时间超过60s不刷新也会刷新一次
                            TimeSpan tsScreen = DateTime.Now - timeScreen;
                            if (((Line3Lab.Text != temp3 || Line4Lab.Text != temp4 || Line5Lab.Text != temp5 || Line6Lab.Text != temp6 || Line7Lab.Text != temp7)) ||
                                tsScreen.TotalSeconds > 15)

                            {
                                timeScreen = DateTime.Now;
                                temp3 = Line3Lab.Text;
                                temp4 = Line4Lab.Text;
                                temp5 = Line5Lab.Text;
                                temp6 = Line6Lab.Text;
                                temp7 = Line7Lab.Text;


                                SendData(screen, false);

                            }

                            *//*
                            //发声音判定：如果第一行变化，发送一个语音内容字符到VQueue，40s没有改变也会发一个
                            TimeSpan tsSound = DateTime.Now - timeSound;
                            if (Line3Lab.Text != tempFirst || tsSound.TotalSeconds > 40)
                            {
                                timeSound = DateTime.Now; //获取现在时间

                                //例：原Line3Lab.Text = "01    本场    04     -    01      X       " 据此进行处理转化成将要发送的音频字符	
                                string[] SoundArr = Line3Lab.Text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); //分割字符串且去除空字符组


                                if (SoundArr.Count() >= 5) ///确保是有效信息 分割后的字符组元素大于等于5（无效信息只有三个元素，不进行处理）
                                {
                                    string SoundStr = SoundArr[0] + "勾, " + SoundArr[1] + ", " + SoundArr[2] + "股道, " + SoundArr[3].Replace("-", "溜放勾").Replace("D", "手动单溜勾").Replace("+", "挂车").Replace("#", "禁溜车").Replace("X", "机车下峰").Replace("S", "机车上峰") + ", " + SoundArr[4] + "辆, " + SoundArr[5].Replace("K", "空车").Replace("H", "空重混编车").Replace("Z", "重车").Replace("X", "超重车").Replace("！", "注意溜放车").Replace("!", "注意溜放车");


                                    if (TGPconfig.SpeakContent == 2) //如果是简短内容，语音字符串更改
                                    {
                                        SoundStr = SoundArr[2] + "股道, " + SoundArr[4] + "辆";
                                    }
                                    if (TGPconfig.SpeakState == true) //设置界面语音开启的情况下
                                    {
                                        VQueue.Enqueue(SoundStr);  //发送语音字符到声音队列			例SoundStr	"01勾, 本场, 04股道, 溜放勾, 01辆, 超重车"	string
                                    }

                                }

                                tempFirst = Line3Lab.Text;    // 储存此刻信息用作下次判断是否变化
                            }
                            *//*
                        }
                    }


                    //如果是峰2信息
                    if (DisplayCon[0].ToString()[3] == '2')
                    {
                        //如果峰2信息是空包
                        //if (DisplayCon.Count >= 7 && DisplayCon[2] as string == "      本场    0          0              " && DisplayCon[3] as string == "      本场    0          0              " && DisplayCon[4] as string == "      本场    0          0              " && DisplayCon[5] as string == "      本场    0          0              " && DisplayCon[6] as string == "      本场    0          0              ")
                        if (DisplayCon.Count >= 7 &&
                            DisplayCon[2] as string == "      本场    0          0                    " &&

                            DisplayCon[3] as string == "      本场    0          0                    " &&
                            DisplayCon[4] as string == "      本场    0          0                    " &&
                            DisplayCon[5] as string == "      本场    0          0                    " &&
                            DisplayCon[6] as string == "      本场    0          0                    ")

                        {
                            *//*
                            if (!bTimeDisplay2)
                            {
                                bTimeDisplay2 = true;
                                timer_TimeDisplay2.Start();
                            }
                            *//*
                            P2State = false; //P2状态为空包
                        }
                        //如果峰2信息有效
                        else
                        {
                            P2State = true; //P2状态为数据有效
                            *//*
                            if (bTimeDisplay2)
                            {
                                bTimeDisplay2 = false;
                                timer_TimeDisplay2.Stop();
                            }
                            *//*
                            //WinForm中Label显示驼峰信息
                            Line1Labb.Visible = true;
                            Line2Labb.Visible = true;
                            Line3Labb.Visible = true;
                            Line4Labb.Visible = true;
                            Line5Labb.Visible = true;
                            Line6Labb.Visible = true;
                            Line7Labb.Visible = true;
                            label5.Visible = false;

                            //准备发送给屏幕  
                            //分割字符串为数组并删除空字符
                            string[] ScreenArr3 = DisplayCon[2].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr4 = DisplayCon[3].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr5 = DisplayCon[4].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr6 = DisplayCon[5].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr7 = DisplayCon[6].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            *//*
                            //赋值给UI界面
                            Line1Labb.Text = DisplayCon[0] as string;
                            Line2Labb.Text = DisplayCon[1] as string;
                            Line3Labb.Text = DisplayCon[2] as string;
                            Line4Labb.Text = DisplayCon[3] as string;
                            Line5Labb.Text = DisplayCon[4] as string;
                            Line6Labb.Text = DisplayCon[5] as string;
                            Line7Labb.Text = DisplayCon[6] as string;
                            *//*

                            //判断不是空包才赋值给UI界面
                            Line1Labb.Text = DisplayCon[0] as string;
                            Line2Labb.Text = DisplayCon[1] as string;
                            Line3Labb.Text = DisplayCon[2] as string;
                            //if (DisplayCon[3] as string != "      本场    0          0              ")
                            if (ScreenArr4.Count() > 5)
                            {
                                Line4Labb.Text = DisplayCon[3] as string;
                                if (ScreenArr5.Count() > 5)
                                {
                                    Line5Labb.Text = DisplayCon[4] as string;
                                    if (ScreenArr6.Count() > 5)
                                    {
                                        Line6Labb.Text = DisplayCon[5] as string;
                                        if (ScreenArr7.Count() > 5)
                                        {
                                            Line7Labb.Text = DisplayCon[6] as string;
                                        }
                                        else Line7Labb.Text = "";
                                    }
                                    else Line6Labb.Text = Line7Labb.Text = "";
                                }
                                else Line5Labb.Text = Line6Labb.Text = Line7Labb.Text = "";
                            }
                            else Line4Labb.Text = Line5Labb.Text = Line6Labb.Text = Line7Labb.Text = "";

                            //LineLab颜色变化
                            if (Line3Labb.Text.Contains("-")) { Line3Labb.ForeColor = Color.Lime; } else if (Line3Labb.Text.Contains("D")) { Line3Labb.ForeColor = Color.Yellow; } else Line3Labb.ForeColor = Color.Red;
                            if (Line4Labb.Text.Contains("-")) { Line4Labb.ForeColor = Color.Lime; } else if (Line4Labb.Text.Contains("D")) { Line4Labb.ForeColor = Color.Yellow; } else Line4Labb.ForeColor = Color.Red;
                            if (Line5Labb.Text.Contains("-")) { Line5Labb.ForeColor = Color.Lime; } else if (Line5Labb.Text.Contains("D")) { Line5Labb.ForeColor = Color.Yellow; } else Line5Labb.ForeColor = Color.Red;
                            if (Line6Labb.Text.Contains("-")) { Line6Labb.ForeColor = Color.Lime; } else if (Line6Labb.Text.Contains("D")) { Line6Labb.ForeColor = Color.Yellow; } else Line6Labb.ForeColor = Color.Red;
                            if (Line7Labb.Text.Contains("-")) { Line7Labb.ForeColor = Color.Lime; } else if (Line7Labb.Text.Contains("D")) { Line7Labb.ForeColor = Color.Yellow; } else Line7Labb.ForeColor = Color.Red;



                            *//*
                            //准备发送给屏幕  
                            //分割字符串为数组并删除空字符
                            string[] ScreenArr3 = Line3Labb.Text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr4 = Line4Labb.Text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr5 = Line5Labb.Text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr6 = Line6Labb.Text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] ScreenArr7 = Line7Labb.Text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            *//*

                            List<string> screen = new List<string>();
                            //增加if确保有五个元素时才发送屏幕（空数据包时只有3个元素 会因为Timer时间差导致无法切换到另一个timer   因超过索引范围卡住）
                            *//*
                            if (ScreenArr3.Count() >= 5 || ScreenArr4.Count() >= 5 || ScreenArr5.Count() >= 5 || ScreenArr6.Count() >= 5 || ScreenArr7.Count() >= 5)
                            {
                                //string strrr = ScreenArr3[0] + "  " + ScreenArr3[1] + "  " + ScreenArr3[2] + "  " + ScreenArr3[3] + "  " + ScreenArr3[4] + "  " + ScreenArr3[5];	
                                //上一行用来设断点调试 strrr	"01  本场  04  -  01  X"	string
                                screen.Add(ScreenArr3[2] + " " + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);
                                screen.Add(ScreenArr4[2] + " " + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                screen.Add(ScreenArr5[2] + " " + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                screen.Add(ScreenArr6[2] + " " + ScreenArr6[3] + " " + ScreenArr6[4] + " " + ScreenArr6[5]);
                                screen.Add(ScreenArr7[2] + " " + ScreenArr7[3] + " " + ScreenArr7[4] + " " + ScreenArr7[5]);
                            }
                            *//*
                            //确保每行至少五个元素时才能发送屏幕
                            int row = 0; //行数
                            try
                            {
                                if (ScreenArr3.Count() > 5 && (row < 4))
                                {
                                    bool bshow = true;
                                    if ((!DisplayCon[0].ToString().Contains("未压入")) && (Convert.ToInt32(ScreenArr3[4]) < 4))
                                    {
                                        bshow = false;
                                    }
                                    if (bshow)
                                    {
                                        if ((Convert.ToInt32(ScreenArr3[0]) > 100)) //三位数前两组之间没有空格
                                        {
                                            screen.Add(ScreenArr3[0] + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);
                                        }
                                        else                                        //两位数删掉勾序第一个字符
                                        {
                                            screen.Add(ScreenArr3[0].Substring(0) + " " + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);
                                        }
                                        row++;
                                        if (ScreenArr3.Count() >= 7 && (row < 4))
                                        {
                                            screen.Add("  " + ScreenArr3[6]);
                                            row++;
                                        }
                                    }
                                    //screen.Add(ScreenArr3[2] + " " + ScreenArr3[3] + " " + ScreenArr3[4] + " " + ScreenArr3[5]);

                                    if (ScreenArr4.Count() > 5 && (row < 4))
                                    {
                                        if ((Convert.ToInt32(ScreenArr4[0]) > 100))
                                        {
                                            screen.Add(ScreenArr4[0] + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                        }
                                        else
                                        {
                                            screen.Add(ScreenArr4[0].Substring(0) + " " + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                        }
                                        row++;
                                        if (ScreenArr4.Count() >= 7 && (row < 4))
                                        {
                                            screen.Add("  " + ScreenArr4[6]);
                                            row++;
                                        }
                                        //screen.Add(ScreenArr4[2] + " " + ScreenArr4[3] + " " + ScreenArr4[4] + " " + ScreenArr4[5]);
                                        if (ScreenArr5.Count() > 5 && (row < 4))
                                        {
                                            if ((Convert.ToInt32(ScreenArr5[0]) > 100))
                                            {
                                                screen.Add(ScreenArr5[0] + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                            }
                                            else
                                            {
                                                screen.Add(ScreenArr5[0].Substring(0) + " " + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                            }
                                            row++;
                                            if (ScreenArr5.Count() >= 7 && (row < 4))
                                            {
                                                screen.Add("  " + ScreenArr5[6]);
                                                row++;
                                            }
                                            //screen.Add(ScreenArr5[2] + " " + ScreenArr5[3] + " " + ScreenArr5[4] + " " + ScreenArr5[5]);
                                            if (ScreenArr6.Count() > 5 && (row < 4))
                                            {
                                                if ((Convert.ToInt32(ScreenArr6[0]) > 100))
                                                {
                                                    screen.Add(ScreenArr6[0] + ScreenArr6[3] + " " + ScreenArr6[4] + " " + ScreenArr6[5]);
                                                }
                                                else
                                                {
                                                    screen.Add(ScreenArr6[0].Substring(0) + " " + ScreenArr6[3] + " " + ScreenArr6[4] + " " + ScreenArr6[5]);
                                                }
                                                row++;
                                                if (ScreenArr6.Count() >= 7 && (row < 4))
                                                {
                                                    screen.Add("  " + ScreenArr6[6]);
                                                    row++;
                                                }
                                                //screen.Add(ScreenArr6[2] + " " + ScreenArr6[3] + " " + ScreenArr6[4] + " " + ScreenArr6[5]);
                                                if (ScreenArr7.Count() > 5 && (row < 4))
                                                {
                                                    if ((Convert.ToInt32(ScreenArr7[0]) > 100))
                                                    {
                                                        screen.Add(ScreenArr7[0] + ScreenArr7[3] + " " + ScreenArr7[4] + " " + ScreenArr7[5]);
                                                    }
                                                    else
                                                    {
                                                        screen.Add(ScreenArr7[0].Substring(0) + " " + ScreenArr7[3] + " " + ScreenArr7[4] + " " + ScreenArr7[5]);
                                                    }
                                                    row++;
                                                    //screen.Add(ScreenArr7[2] + " " + ScreenArr7[3] + " " + ScreenArr7[4] + " " + ScreenArr7[5]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }


                            //发屏幕判定：如果数据变化，发送给屏幕，防止屏幕刷新过快;时间超过60s不刷新也会刷新一次
                            TimeSpan tsScreen = DateTime.Now - timeScreen2;
                            if (((Line3Labb.Text != temp3b || Line4Labb.Text != temp4b || Line5Labb.Text != temp5b || Line6Labb.Text != temp6b || Line7Labb.Text != temp7b)) ||
                                tsScreen.TotalSeconds > 15)

                            {
                                timeScreen2 = DateTime.Now;
                                temp3b = Line3Labb.Text;
                                temp4b = Line4Labb.Text;
                                temp5b = Line5Labb.Text;
                                temp6b = Line6Labb.Text;
                                temp7b = Line7Labb.Text;

                                SendData2(screen, false);

                            }

                            *//*
                            //发声音判定：如果第一行变化，发送一个语音内容字符到VQueue，40s没有改变也会发一个
                            TimeSpan tsSound = DateTime.Now - timeSound;
                            if (Line3Labb.Text != tempFirstb || tsSound.TotalSeconds > 40)
                            {
                                timeSound = DateTime.Now; //获取现在时间

                                //例：原Line3Lab.Text = "01    本场    04     -    01      X       " 据此进行处理转化成将要发送的音频字符	
                                string[] SoundArr = Line3Labb.Text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); //分割字符串且去除空字符组


                                if (SoundArr.Count() >= 5) ///确保是有效信息 分割后的字符组元素大于等于5（无效信息只有三个元素，不进行处理）
                                {
                                    string SoundStr = SoundArr[0] + "勾, " + SoundArr[1] + ", " + SoundArr[2] + "股道, " + SoundArr[3].Replace("-", "溜放勾").Replace("D", "手动单溜勾").Replace("+", "挂车").Replace("#", "禁溜车").Replace("X", "机车下峰").Replace("S", "机车上峰") + ", " + SoundArr[4] + "辆, " + SoundArr[5].Replace("K", "空车").Replace("H", "空重混编车").Replace("Z", "重车").Replace("X", "超重车").Replace("！", "注意溜放车").Replace("!", "注意溜放车");


                                    if (TGPconfig.SpeakContent == 2) //如果是简短内容，语音字符串更改
                                    {
                                        SoundStr = SoundArr[2] + "股道, " + SoundArr[4] + "辆";
                                    }
                                    if (TGPconfig.SpeakState == true) //设置界面语音开启的情况下
                                    {
                                        VQueue.Enqueue(SoundStr);  //发送语音字符到声音队列			例SoundStr	"01勾, 本场, 04股道, 溜放勾, 01辆, 超重车"	string
                                    }

                                }

                                tempFirstb = Line3Labb.Text;    // 储存此刻信息用作下次判断是否变化
                            }
                            *//*
                        }

                    }

                }
                #endregion*/
                #endregion

                //判断P1 P2状态并且决定是否发时间标语函数
                if (P1State == false) //峰1收到空包
                {
                    if (TGPconfig.DisplayOption == "Time") //提钩屏设置显示内容选择为时间
                    {

                        Line1Lab.Visible = false;
                        Line2Lab.Visible = false;
                        Line3Lab.Visible = false;
                        Line4Lab.Visible = false;
                        Line5Lab.Visible = false;
                        Line6Lab.Visible = false;
                        Line7Lab.Visible = false;
                        label4.Text = DateTime.Now.ToString();
                        label4.Visible = true;

                        if (StationScreen == 0 || StationScreen == 1)
                        {
                            //C E用
                            /*List<string> temp = new List<string>();
                            temp.Add(DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd"));    //日期
                            temp.Add(" " + DateTime.Now.ToString("HH") + ":" + DateTime.Now.ToString("mm"));    //时间
                            if ((DateTime.Now.Second % 5) == 0)
                                SendDataTimeQueue.Enqueue(new DataOfSend(temp, true));*/
                            //X卡
                            List<string> Xtemp = new List<string>();
                            Xtemp.Add("");
                            Xtemp.Add("   "+ DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd"));
                            Xtemp.Add("      " + DateTime.Now.ToString("dddd"));
                            Xtemp.Add("   " + DateTime.Now.ToString("HH") + "时" + DateTime.Now.ToString("mm")+"分");
                            Xtemp.Add("");
                            //Xtemp.Add("");
                            if ((DateTime.Now.Second % 5) == 0)
                                SendDataTimeQueue.Enqueue(new DataOfSend(Xtemp, false));

                        }
                    }
                    if (TGPconfig.DisplayOption == "TimeSlogan") //提钩屏设置显示内容选择为标语和时间
                    {
                        TimerCount += 1;

                        if (TimerCount == 1 || TimerCount == 6) //前10s显示标语 TimerCount<=10
                        {

                            Line1Lab.Visible = false;
                            Line2Lab.Visible = false;
                            Line3Lab.Visible = false;
                            Line4Lab.Visible = false;
                            Line5Lab.Visible = false;
                            Line6Lab.Visible = false;
                            Line7Lab.Visible = false;
                            label4.Text = TGPconfig.textBox1.Text + "\n" + TGPconfig.textBox2.Text;
                            label4.Visible = true;

                            if (StationScreen == 0 || StationScreen == 1)
                            {
                                //C E用
                                /*List<string> temp = new List<string>();
                                temp.Add(TGPconfig.textBox1.Text); //标语第一行
                                temp.Add(TGPconfig.textBox2.Text); //标语第二行
                                SendDataQueue.Enqueue(new DataOfSend(temp, true));*/
                                //X卡
                                List<string> Xtemp = new List<string>();
                                Xtemp.Add("");
                                Xtemp.Add("    " + TGPconfig.textBox1.Text);
                                Xtemp.Add("");
                                Xtemp.Add("    " + TGPconfig.textBox2.Text);
                                Xtemp.Add("");
                                //Xtemp.Add("");
                                SendDataTimeQueue.Enqueue(new DataOfSend(Xtemp, false));
                            }
                        }
                        else if (TimerCount < 20 && TimerCount > 10) //后10s显示时间
                        {

                            Line1Lab.Visible = false;
                            Line2Lab.Visible = false;
                            Line3Lab.Visible = false;
                            Line4Lab.Visible = false;
                            Line5Lab.Visible = false;
                            Line6Lab.Visible = false;
                            Line7Lab.Visible = false;
                            label4.Text = DateTime.Now.ToString();
                            label4.Visible = true;

                            if (StationScreen == 0 || StationScreen == 1)
                            {
                                //C E用
                                /*List<string> temp = new List<string>();
                                temp.Add(DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd"));    //日期
                                temp.Add(" " + DateTime.Now.ToString("HH") + ":" + DateTime.Now.ToString("mm"));    //时间
                                if ((DateTime.Now.Second % 5) == 0)
                                    SendDataTimeQueue.Enqueue(new DataOfSend(temp, true));*/
                                //X卡
                                List<string> Xtemp = new List<string>();
                                Xtemp.Add("");
                                Xtemp.Add("   " + DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd"));
                                Xtemp.Add("      " + DateTime.Now.ToString("dddd"));
                                Xtemp.Add("   " + DateTime.Now.ToString("HH") + "时" + DateTime.Now.ToString("mm") + "分");
                                Xtemp.Add("");
                                //Xtemp.Add("");
                                if ((DateTime.Now.Second % 5) == 0)
                                    SendDataTimeQueue.Enqueue(new DataOfSend(Xtemp, false));

                            }
                        }
                        else if (TimerCount == 20) //20s依然显示时间 timerCount归零
                        {

                            Line1Lab.Visible = false;
                            Line2Lab.Visible = false;
                            Line3Lab.Visible = false;
                            Line4Lab.Visible = false;
                            Line5Lab.Visible = false;
                            Line6Lab.Visible = false;
                            Line7Lab.Visible = false;
                            label4.Text = DateTime.Now.ToString();
                            label4.Visible = true;

                            /*   List<string> temp = new List<string>();
                               temp.Add(DateTime.Now.ToString("yy") + "-" + DateTime.Now.ToString("MM") + "-" + DateTime.Now.ToString("dd"));    //日期
                               temp.Add("  " + DateTime.Now.ToString("HH") + ":" + DateTime.Now.ToString("mm"));    //时间
                               if ((DateTime.Now.Second % 5) == 0)
                                   SendDataTime(temp, true);   //发送屏幕
                               */
                            //richTextBox1.Text += DateTime.Now.ToString("G") + "  时间发送至屏幕" + "\n";
                            TimerCount = 0;
                        }

                    }
                    if (TGPconfig.DisplayOption == "Off")  //发送空字符息屏
                    {
                        Line1Lab.Visible = false;
                        Line2Lab.Visible = false;
                        Line3Lab.Visible = false;
                        Line4Lab.Visible = false;
                        Line5Lab.Visible = false;
                        Line6Lab.Visible = false;
                        Line7Lab.Visible = false;
                        label4.Text = "\n屏幕已熄灭";
                        label4.Visible = true;

                        if (StationScreen == 0 || StationScreen == 1)
                        {
                            TimeSpan ts = DateTime.Now - ScreenOffTime;
                            if (ts.TotalSeconds > 30)
                            {
                                ScreenOffTime = DateTime.Now;
                                List<string> temp = new List<string>();
                                temp.Add("");
                                //SendData(temp, true);
                                SendDataQueue.Enqueue(new DataOfSend(temp, true));
                                //richTextBox1.Text += DateTime.Now.ToString("G") + "  灭屏" + "\n";
                            }
                        }
                    }
                }
                if (P2State == false) //峰2收到空包
                {
                    if (TGPconfig.DisplayOption == "Time") //提钩屏设置显示内容选择为时间
                    {

                        Line1Labb.Visible = false;
                        Line2Labb.Visible = false;
                        Line3Labb.Visible = false;
                        Line4Labb.Visible = false;
                        Line5Labb.Visible = false;
                        Line6Labb.Visible = false;
                        Line7Labb.Visible = false;
                        label5.Text = DateTime.Now.ToString();
                        label5.Visible = true;

                        if (StationScreen == 0 || StationScreen == 2)
                        {
                            //C E用
                            /*List<string> temp = new List<string>();
                            //temp.Add(DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd"));    //日期
                            temp.Add(DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd"));    //日期
                            temp.Add(" " + DateTime.Now.ToString("HH") + ":" + DateTime.Now.ToString("mm"));    //时间
                            if ((DateTime.Now.Second % 5) == 3)
                                SendDataTimeQueue2.Enqueue(new DataOfSend(temp, true));*/
                            //X卡
                            List<string> Xtemp = new List<string>();
                            Xtemp.Add("");
                            Xtemp.Add(DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd"));
                            Xtemp.Add("");
                            Xtemp.Add("   " + DateTime.Now.ToString("HH") + ":" + DateTime.Now.ToString("mm"));
                            Xtemp.Add("");
                            if ((DateTime.Now.Second % 5) == 3)
                                SendDataTimeQueue2.Enqueue(new DataOfSend(Xtemp, false));

                        }

                    }
                    if (TGPconfig.DisplayOption == "TimeSlogan") //提钩屏设置显示内容选择为标语和时间
                    {
                        TimerCount2 += 1;

                        if (TimerCount2 == 1 || TimerCount2 == 6) //前10s显示标语 TimerCount<=10
                        {

                            Line1Labb.Visible = false;
                            Line2Labb.Visible = false;
                            Line3Labb.Visible = false;
                            Line4Labb.Visible = false;
                            Line5Labb.Visible = false;
                            Line6Labb.Visible = false;
                            Line7Labb.Visible = false;
                            label5.Text = TGPconfig.textBox1.Text + "\n" + TGPconfig.textBox2.Text;
                            label5.Visible = true;

                            if (StationScreen == 0 || StationScreen == 2)
                            {
                                //C E卡用
                                /*List<string> temp = new List<string>();
                                temp.Add(TGPconfig.textBox1.Text); //标语第一行
                                temp.Add(TGPconfig.textBox2.Text); //标语第二行
                                SendDataQueue2.Enqueue(new DataOfSend(temp, true));*/
                                //X卡用
                                List<string> Xtemp = new List<string>();
                                Xtemp.Add("");
                                Xtemp.Add(" " + TGPconfig.textBox1.Text);
                                Xtemp.Add("");
                                Xtemp.Add(" " + TGPconfig.textBox2.Text);
                                Xtemp.Add("");
                                SendDataTimeQueue2.Enqueue(new DataOfSend(Xtemp, false));

                            }
                        }
                        else if (TimerCount2 < 20 && TimerCount2 > 10) //后10s显示时间
                        {

                            Line1Labb.Visible = false;
                            Line2Labb.Visible = false;
                            Line3Labb.Visible = false;
                            Line4Labb.Visible = false;
                            Line5Labb.Visible = false;
                            Line6Labb.Visible = false;
                            Line7Labb.Visible = false;
                            label5.Text = DateTime.Now.ToString();
                            label5.Visible = true;

                            if (StationScreen == 0 || StationScreen == 2)
                            {
                                //C E卡专用
                                /*List<string> temp = new List<string>();
                                temp.Add(DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd"));    //日期
                                temp.Add(" " + DateTime.Now.ToString("HH") + ":" + DateTime.Now.ToString("mm"));    //时间
                                if ((DateTime.Now.Second % 5) == 3)
                                    SendDataTimeQueue2.Enqueue(new DataOfSend(temp, true));*/
                                //X卡
                                List<string> Xtemp = new List<string>();
                                Xtemp.Add("");
                                Xtemp.Add(DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "/" + DateTime.Now.ToString("dd"));
                                Xtemp.Add("");
                                Xtemp.Add("   " + DateTime.Now.ToString("HH") + ":" + DateTime.Now.ToString("mm"));
                                Xtemp.Add("");
                                if ((DateTime.Now.Second % 5) == 3)
                                    SendDataTimeQueue2.Enqueue(new DataOfSend(Xtemp, false));
                            }
                        }
                        else if (TimerCount2 == 20) //20s依然显示时间 timerCount归零
                        {

                            Line1Labb.Visible = false;
                            Line2Labb.Visible = false;
                            Line3Labb.Visible = false;
                            Line4Labb.Visible = false;
                            Line5Labb.Visible = false;
                            Line6Labb.Visible = false;
                            Line7Labb.Visible = false;
                            label5.Text = DateTime.Now.ToString();
                            label5.Visible = true;

                            /* List<string> temp = new List<string>();
                             temp.Add(DateTime.Now.ToString("yy") + "-" + DateTime.Now.ToString("MM") + "-" + DateTime.Now.ToString("dd"));    //日期
                             temp.Add("  " + DateTime.Now.ToString("HH") + ":" + DateTime.Now.ToString("mm"));    //时间
                             if ((DateTime.Now.Second % 5) == 0)
                                 SendDataTime2(temp, true);   //发送屏幕
                             */
                            //richTextBox1.Text += DateTime.Now.ToString("G") + "  时间发送至屏幕" + "\n";
                            TimerCount2 = 0;
                        }

                    }
                    if (TGPconfig.DisplayOption == "Off")  //发送空字符息屏
                    {
                        Line1Labb.Visible = false;
                        Line2Labb.Visible = false;
                        Line3Labb.Visible = false;
                        Line4Labb.Visible = false;
                        Line5Labb.Visible = false;
                        Line6Labb.Visible = false;
                        Line7Labb.Visible = false;
                        label5.Text = "\n屏幕已熄灭";
                        label5.Visible = true;
                        if (StationScreen == 0 || StationScreen == 2)
                        {
                            TimeSpan ts = DateTime.Now - ScreenOffTime2;
                            if (ts.TotalSeconds > 30)
                            {
                                ScreenOffTime2 = DateTime.Now;
                                List<string> temp = new List<string>();
                                temp.Add("");
                                //SendData2(temp, true);
                                SendDataQueue2.Enqueue(new DataOfSend(temp, true));
                                //richTextBox1.Text += DateTime.Now.ToString("G") + "  灭屏" + "\n";
                            }
                        }
                    }
                }


                //其他需要刷新的功能
                if (recQueue.Count > 0)  //接收信息窗队列有东西的时候
                {
                    while (true)
                    {
                        string str = recQueue.Dequeue() as string;
                        if (str == null)
                            break;
                        else
                            listBox1.Items.Add(str);
                        listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    }
                }

                if (listBox1.Items.Count > 12) //接收信息窗内容超过12行
                {
                    listBox1.Items.RemoveAt(0);
                }

                if (sndQueue.Count > 0)  //发送信息窗队列有东西的时候
                {
                    while (true)
                    {
                        string str = sndQueue.Dequeue() as string;
                        if (str == null)
                            break;
                        else
                            listBox2.Items.Add(str);
                        listBox2.SelectedIndex = listBox2.Items.Count - 1;
                    }
                }

                if (listBox2.Items.Count > 12) //发送信息窗队列有东西的时候
                {
                    listBox2.Items.RemoveAt(0);
                }

                if (TGPconfig.listBox1.Items.Count > 11)//设置界面信息窗内容超过11行
                {
                    TGPconfig.listBox1.Items.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "timer_Checker_Tick错误：" + ex);
            }
        }



        /// <summary>
        /// Part1：串口接收线程
        /// </summary>
        private void CommRecThread()
        {
            byte[] recByte = new byte[5000];
            int len = 0;
            int ErrorCount = 0;
            bool bComError = false;
            byte[] LastDcdByte = new byte[5000];
            byte[] LastDcdByte2 = new byte[5000];
            while (true)
            {
                try
                {
                    Thread.Sleep(50);
                    if (cr.Read_HumpTBZK(ref recByte, ref len))
                    {
                        ErrorCount = 0;
                        if (bComError)
                            bComError = false;
                        //计算校验值并比较
                        DCDInfo DcdInfo = new DCDInfo(recByte);
                        UInt16 calCRCval = DcdInfo.GenerateCrcCode(recByte, len - 4);
                        //string calCRCstr = calCRCval.ToString();
                        //MessageBox.Show(recCRCval.ToString()+","+calCRCval.ToString());
                        byte[] recCRCbyte = new byte[4];
                        //InfoLen = 110;
                        recCRCbyte[0] = recByte[len - 4];
                        recCRCbyte[1] = recByte[len - 3];
                        recCRCbyte[2] = recByte[len - 2];
                        recCRCbyte[3] = recByte[len - 1];
                        string recCRCstr = System.Text.Encoding.Default.GetString(recCRCbyte);
                        //MessageBox.Show("rev="+recCRCstr);
                        UInt16 recCRCval;
                        //UInt16 recCRCval = Convert.ToUInt16(recCRCstr, 16);
                        if (!UInt16.TryParse(recCRCstr, System.Globalization.NumberStyles.AllowHexSpecifier, null, out recCRCval))
                        {
                            //MessageBox.Show("ok");
                            recCRCval = 0;
                            continue;
                        }
                        //校验比较
                        if (recCRCval != calCRCval)
                        {
                            //recStr = "";
                            //fm.label1.Text = recStr;
                            //break;
                            //return;//校验不成功返回
                            continue;
                        }


                        /// <summary>
                        /// 判断数据来自哪个峰位，并且是否与上一包一样,并且处理数据发给相应队列
                        /// </summary>
                        //---------------------------峰1---------------------------
                        if (StationScreen == 0 || StationScreen == 1)  //屏1确认为启用状态
                        {
                            bool bSame = true;  //数据是否一样布尔值，一样true 不一样false
                            for (int i = 0; i < DcdInfo.Info.Length && i < LastDcdByte.Length; i++)   //byte[]要一个一个进行比对
                            {
                                if (DcdInfo.Info[i] != LastDcdByte[i])
                                {
                                    bSame = false;
                                    break;
                                }
                            }
                            //收到峰1数据且与上一包不同
                            if (DcdInfo.Info[1] == 49 && (!bSame))  // 1峰位信息 HEX 31 = DEC 49
                            {
                                recQueue.Enqueue(DateTime.Now.ToString("G") + "  接收到峰1数据更新" + "\n");
                                Array.Copy(DcdInfo.Info, LastDcdByte, DcdInfo.Info.Length); //此时数据赋值给上一包（byte[]不能直接“=”， 指针一样，用Array.Copy()函数）
                                                                                            //进行处理
                                DcdInfo.InfoPross();
                                string strTitle, str1, str2, str3, str4, str5;
                                strTitle = "峰位:" + DcdInfo.HumpNumberStr + "  车次:" + DcdInfo.TrainNumberStr + "  驼信:" + DcdInfo.HumpSigStr + "  一分岔:" + DcdInfo.FirstDCOnStr;

                                int j = 0;
                                str1 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                                j = 1;
                                str2 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                                j = 2;
                                str3 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                                j = 3;
                                str4 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                                j = 4;
                                str5 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];


                                //public PRC_Tool.SafeArrayList DisplayCon = new PRC_Tool.SafeArrayList();
                                PRC_Tool.SafeArrayList TempList = new PRC_Tool.SafeArrayList();
                                TempList.Clear();
                                TempList.Add(strTitle);
                                TempList.Add(str1);
                                TempList.Add(str2);
                                TempList.Add(str3);
                                TempList.Add(str4);
                                TempList.Add(str5);
                                RecieveToProcessQueue.Enqueue(TempList);

                            }
                        }

                        //---------------------------峰2---------------------------
                        if (StationScreen == 0 || StationScreen == 2) //屏2确认为启用状态
                        {
                            bool bSame2 = true;  //数据是否一样布尔值，一样true 不一样false
                            for (int i = 0; i < DcdInfo.Info.Length && i < LastDcdByte2.Length; i++)   //byte[]要一个一个进行比对
                            {
                                if (DcdInfo.Info[i] != LastDcdByte2[i])
                                {
                                    bSame2 = false;
                                    break;
                                }
                            }
                            //收到峰2数据且与上一包不同
                            if (DcdInfo.Info[1] == 50 && (!bSame2))  // 2峰位信息 HEX 32 = DEC 50
                                                                     //else if(DcdInfo.Info[1] == 50)
                            {
                                recQueue.Enqueue(DateTime.Now.ToString("G") + "  接收到峰2数据更新" + "\n");
                                Array.Copy(DcdInfo.Info, LastDcdByte2, DcdInfo.Info.Length);
                                //进行处理
                                DcdInfo.InfoPross();
                                string strTitle, str1, str2, str3, str4, str5;
                                strTitle = "峰位:" + DcdInfo.HumpNumberStr + "  车次:" + DcdInfo.TrainNumberStr + "  驼信:" + DcdInfo.HumpSigStr + "  一分岔:" + DcdInfo.FirstDCOnStr;

                                int j = 0;
                                str1 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                                j = 1;
                                str2 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                                j = 2;
                                str3 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                                j = 3;
                                str4 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                                j = 4;
                                str5 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];


                                //public PRC_Tool.SafeArrayList DisplayCon = new PRC_Tool.SafeArrayList();
                                PRC_Tool.SafeArrayList TempList = new PRC_Tool.SafeArrayList();
                                TempList.Clear();
                                TempList.Add(strTitle);
                                TempList.Add(str1);
                                TempList.Add(str2);
                                TempList.Add(str3);
                                TempList.Add(str4);
                                TempList.Add(str5);
                                RecieveToProcessQueue2.Enqueue(TempList);
                            }
                        }
                        #region
                        /*
                        //有效信息处理
                        DcdInfo.InfoPross();

                        //显示
                        string str1, str2, str3, str4, str5, str6, str7;
                        str1 = "峰位:" + DcdInfo.HumpNumberStr + "  车次:" + DcdInfo.TrainNumberStr + "  驼信:" + DcdInfo.HumpSigStr + "  一分岔:" + DcdInfo.FirstDCOnStr;
                        str2 = "勾序 " + "  场别" + "   股道" + "  方式" + "  辆数" + "   状态" + "   开口号";

                        int j = 0;
                        str3 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                        j = 1;
                        str4 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                        j = 2;
                        str5 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                        j = 3;
                        str6 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];
                        j = 4;
                        str7 = DcdInfo.HookNumStr[j] + "    " + DcdInfo.YardNumStr[j] + "    " + DcdInfo.GDNumberStr[j] + "     " + DcdInfo.DoMarkStr[j] + "    " + DcdInfo.CarNumStr[j] + "      " + DcdInfo.CarStateStr[j] + "      " + DcdInfo.OpenNumberStr[j];

                        DisplayCon.Clear();
                        DisplayCon.Add(str1);  //Displaycon[0]:峰位 车次等信息
                        DisplayCon.Add(str2);  //Displaycon[1]:勾序 场别 股道等目录
                        DisplayCon.Add(str3);  //Displaycon[2-6]: 五行作业单
                        DisplayCon.Add(str4);
                        DisplayCon.Add(str5);
                        DisplayCon.Add(str6);
                        DisplayCon.Add(str7);
                        recQueue.Enqueue(DateTime.Now.ToString("G") + "  接收到峰" + DcdInfo.HumpNumberStr + "数据" + "\n");
                        */
                        #endregion

                    }
                    else
                    {

                        if (ErrorCount >= 200 && bComError == false)
                        {
                            recQueue.Enqueue(DateTime.Now.ToString("G") + "  接收串口中断" + "\n");    //超过200 未接收到数据？
                            bComError = true;
                        }
                        else if (ErrorCount <= 200)
                            ErrorCount++;
                    }
                }
                catch (Exception ex)
                {
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "CommRecThread-While错误：" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 语音模块TCP客户端线程
        /// </summary>
        public int CountReply = 0;
        private void ClientThreadMethod()
        {
            try
            {
                string temp = "";
                while (true)
                {
                    Thread.Sleep(50);
                    if (AudioQueue.Count > 0 && SentAudioQueue.Count == 0)     //有数据且上一包发完了就发
                    {
                        temp = AudioQueue.Dequeue() as string;
                        cli.Write("#" + temp);
                        SentAudioQueue.Enqueue(temp);
                    }
                    if (SentAudioQueue.Count != 0)
                    {
                        CountReply += 1;
                    }
                    if (CountReply > 300)  //长时间未收到回复
                    {
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "峰1语音模块15秒未收到播报完成回复, 内容：" + SentAudioQueue.Dequeue() as string);
                        SentAudioQueue.Clear();
                        CountReply = 0;
                        continue;
                    }
                }
            }
            catch(Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "ClientThread错误：" + ex.Message);
            }
        }

        private void ClientReplyThreadMethod()
        {
            try
            {
                byte[] recByte = new byte[5000];
                int len = 0;
                string res = "";
                SentAudioQueue.Clear();
                while (true)
                {
                    //Thread.Sleep(50);
                    if (SentAudioQueue.Count > 0)
                    {
                        if (cli.Read(ref recByte, ref len)) //成功读取再往下走，里面可能存储其他数据，导致误判
                        {
                            res = System.Text.Encoding.Default.GetString(recByte);
                            if (res.IndexOf("OL") == 0)  //判断第一行
                            {
                                SentAudioQueue.Clear();
                                CountReply = 0;
                                res = "";
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "ClientReplyThread错误：" + ex.Message);
            }
        }

        public int CountReply2 = 0;
        private void ClientThreadMethod2()
        {
            try
            {
                string temp = "";
                while (true)
                {
                    Thread.Sleep(50);
                    if (AudioQueue2.Count > 0 && SentAudioQueue2.Count == 0)     //有数据且上一包发完了就发
                    {
                        temp = AudioQueue2.Dequeue() as string;
                        cli2.Write("#" + temp);
                        SentAudioQueue2.Enqueue(temp);
                    }
                    if (SentAudioQueue2.Count != 0)
                    {
                        CountReply2 += 1;
                    }
                    if (CountReply2 > 300)  //长时间未收到回复
                    {
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "峰2语音模块15秒未收到播报完成回复, 内容：" + SentAudioQueue2.Dequeue() as string);
                        SentAudioQueue2.Clear();
                        CountReply2 = 0;
                        continue;
                    }
                }
            }
            catch(Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "ClientThread2错误：" + ex.Message);
            }
        }
        private void ClientReplyThreadMethod2()
        {
            try
            {
                byte[] recByte = new byte[5000];
                int len = 0;
                string res = "";
                SentAudioQueue2.Clear();
                while (true)
                {
                    //Thread.Sleep(50);
                    if (SentAudioQueue2.Count > 0)
                    {
                        if (cli2.Read(ref recByte, ref len)) //成功读取再往下走，里面可能存储其他数据，导致误判
                        {
                            res = System.Text.Encoding.Default.GetString(recByte);
                            if (res.IndexOf("OL") == 0)  //判断第一行
                            {
                                SentAudioQueue2.Clear();
                                CountReply2 = 0;
                                res = "";
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "ClientReplyThread2错误：" + ex.Message);
            }
        }
        /// <summary>
        /// Part3：发送屏幕线程
        /// </summary>
        private void SendScreenThread()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(10);
                    //发送屏1数据
                    if (SendDataQueue.Count > 0)
                    {
                        //排出除最新数据以外队列里所有旧的元素，保证发送的是最新的一包。
                        int n = SendDataQueue.Count;
                        for (int i = 1; i < n; i++)
                        {
                            SendDataQueue.Dequeue();
                        }
                        //最新的一包
                        DataOfSend dt = SendDataQueue.Dequeue() as DataOfSend;
                        //发送数据
                        if (dt.SpeechState == false)  //无语音
                        {
                            //SendData(dt.Info, dt.BigFont);   //C E卡
                            XRefresh(dt.Info, dt.BigFont);     //X卡
                        }
                        else if (dt.SpeechState == true)  //有语音
                        {
                            //SendData(dt.Info, dt.BigFont, dt.SpeechStr); //C E卡
                            XRefresh(dt.Info, dt.BigFont);     //X卡 配语音模块
                        }
                    }
                    //发送屏1时间
                    if (SendDataTimeQueue.Count > 0)
                    {
                        int n = SendDataTimeQueue.Count;
                        for (int i = 1; i < n; i++)
                        {
                            SendDataTimeQueue.Dequeue();
                        }
                        DataOfSend dt = SendDataTimeQueue.Dequeue() as DataOfSend;
                        //SendDataTime(dt.Info, dt.BigFont);
                        //XStaticInit(dt.Info, dt.BigFont);
                        XRefreshTm(dt.Info, dt.BigFont);

                    }

                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "SendScreenThread错误：" + ex.Message);
            }
        }
        private void SendScreen2Thread()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(10);
                    //发送屏2数据
                    if (SendDataQueue2.Count > 0)
                    {
                        //排出除最新数据以外队列里所有旧的元素，保证发送的是最新的一包。
                        int n = SendDataQueue2.Count;
                        for (int i = 1; i < n; i++)
                        {
                            SendDataQueue2.Dequeue();
                        }
                        //最新的一包
                        DataOfSend dt = SendDataQueue2.Dequeue() as DataOfSend;
                        //发送数据
                        if (dt.SpeechState == false) //无语音
                        {
                            //SendData2(dt.Info, dt.BigFont);// C E卡
                            XRefresh2(dt.Info, dt.BigFont); //X卡
                        }
                        else if (dt.SpeechState == true) //有语音
                        {
                            //SendData2(dt.Info, dt.BigFont, dt.SpeechStr); //C E卡
                            XRefresh2(dt.Info, dt.BigFont); //X卡
                        }
                    }
                    //发送屏2时间
                    if (SendDataTimeQueue2.Count > 0)
                    {
                        int n = SendDataTimeQueue2.Count;
                        for (int i = 1; i < n; i++)
                        {
                            SendDataTimeQueue2.Dequeue();
                        }
                        DataOfSend dt = SendDataTimeQueue2.Dequeue() as DataOfSend;
                        //SendDataTime2(dt.Info, dt.BigFont);
                        XRefreshTm2(dt.Info, dt.BigFont);
                    }

                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "SendScreen2Thread错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 设置按钮
        /// </summary>
        private void TGPConfigMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PSWpage.ShowDialog();
                if (PSWpage.bPass)
                {

                    TGPDisp.TGPconfig.ShowDialog();

                }
            }
            catch (Exception)
            { }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void TGPDisp_FormClosed(object sender, FormClosedEventArgs e)
        {
            Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "程序退出");
            System.Environment.Exit(System.Environment.ExitCode);
            this.Dispose();
            this.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }




        /// <summary>
        /// C4M,E卡初始化设置屏幕函数
        /// </summary>
        //设置屏参（注意：只需根据屏的宽高点数的颜色设置一次，发送节目时无需设置）
        private void SetTGP()  //屏1设置
        {
            int nResult;
            LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示


            CommunicationInfo.LEDType = 1;  //串口E卡选1   网口C卡选3

            if (CommunicationType == 1) //网络通信
            {
                CommunicationInfo.LEDType = 3;
                //TCP通讯********************************************************************************
                CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                CommunicationInfo.IpStr = P1IP;//给IpStr赋值LED控制卡的IP
                CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                //广播通讯********************************************************************************
                //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                //串口通讯********************************************************************************
                //CommunicationInfo.SendType=2;//串口通讯
                //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                //CommunicationInfo.Baud=9600;//波特率
                //CommunicationInfo.LedNumber=1;
            }
            if (CommunicationType == 2) //串口
            {
                //串口通讯********************************************************************************
                CommunicationInfo.SendType = 2;//串口通讯
                CommunicationInfo.Commport = ComP1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                CommunicationInfo.Baud = 115200;//波特率
                CommunicationInfo.LedNumber = 1;
            }


            nResult = LedDll.LV_SetBasicInfoEx(ref CommunicationInfo, 3, 0, 128, 128);//设置屏参，屏的颜色为2即为双基色，64为屏宽点数，32为屏高点数，具体函数参数说明见函数声明注示
            if (CommunicationType == 1)
            {
                nResult = LedDll.LV_SetBasicInfoEx(ref CommunicationInfo, 3, 5, 128, 128);
            }
            if (nResult != 0)//如果失败则可以调用LV_GetError获取中文错误信息
            {
                string ErrStr;
                ErrStr = LedDll.LS_GetError(nResult);
                //  MessageBox.Show("屏1错误：" + ErrStr);
                recQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1设置错误：" + ErrStr + "\n");
                if (bWriteLog)
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, " 屏1设置错误：" + ErrStr);
            }
            else
            {
                if (CommunicationType == 1)
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1设置成功, IP: " + P1IP + "\n");
                }
                else if (CommunicationType == 2)
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1设置成功, COM号: " + ComP1 + "\n");
                }
            }
            if (StationScreen == 0 || StationScreen == 1)
            {
                List<string> temp = new List<string>();
                temp.Add(Slogan1);
                temp.Add(Slogan2);
                //SendData(temp,true,"一二三四五六七八九十");
                SendDataQueue.Enqueue(new DataOfSend(temp, true));
            }
        }
        private void SetTGP2() //屏2设置
        {
            int nResult;
            LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示


            CommunicationInfo.LEDType = 1;  //串口E卡选1   网口C卡选3

            if (CommunicationType == 1) //网络通信
            {
                CommunicationInfo.LEDType = 3;
                //TCP通讯********************************************************************************
                CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                CommunicationInfo.IpStr = P2IP;//给IpStr赋值LED控制卡的IP
                CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                //广播通讯********************************************************************************
                //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                //串口通讯********************************************************************************
                //CommunicationInfo.SendType=2;//串口通讯
                //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                //CommunicationInfo.Baud=9600;//波特率
                //CommunicationInfo.LedNumber=1;
            }
            if (CommunicationType == 2) //串口
            {
                //串口通讯********************************************************************************
                CommunicationInfo.SendType = 2;//串口通讯
                CommunicationInfo.Commport = ComP2;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                CommunicationInfo.Baud = 115200;//波特率
                CommunicationInfo.LedNumber = 1;
            }


            nResult = LedDll.LV_SetBasicInfoEx(ref CommunicationInfo, 3, 0, 128, 128);//设置屏参，屏的颜色为2即为双基色，64为屏宽点数，32为屏高点数，具体函数参数说明见函数声明注示
            if (CommunicationType == 1)
            {
                nResult = LedDll.LV_SetBasicInfoEx(ref CommunicationInfo, 3, 5, 128, 128);
            }
            if (nResult != 0)//如果失败则可以调用LV_GetError获取中文错误信息
            {
                string ErrStr;
                ErrStr = LedDll.LS_GetError(nResult);
                //MessageBox.Show("屏2错误：" + ErrStr);
                recQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2设置错误：" + ErrStr + "\n");
                if (bWriteLog)
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, " 屏2设置错误：" + ErrStr);
            }
            else
            {
                if (CommunicationType == 1)
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2设置成功, IP: " + P2IP + "\n");
                }
                else if (CommunicationType == 2)
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2设置成功, COM号: " + ComP2 + "\n");
                }
            }
            if (StationScreen == 0 || StationScreen == 2)
            {
                List<string> temp = new List<string>();
                temp.Add(Slogan1);
                temp.Add(Slogan2);
                //SendData2(temp, true);
                SendDataQueue2.Enqueue(new DataOfSend(temp, true));
            }
        }


        /// <summary>
        /// C4M,E卡发送屏幕函数
        /// </summary>
        //两个节目下各有一个单行文本区和一个数字时钟区(多节目通过此方法添加)
        private void SendData(List<string> Lstring, bool bBigFont)  //发送屏1
        {
            try
            {
                int nResult;
                LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示
                CommunicationInfo.LEDType = 1;  //串口LEDType选1

                if (CommunicationType == 1) //网络通信
                {
                    CommunicationInfo.LEDType = 3; //网口LEDType选3
                    //TCP通讯********************************************************************************
                    CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                    CommunicationInfo.IpStr = P1IP;//给IpStr赋值LED控制卡的IP
                    CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                                                    //广播通讯********************************************************************************
                                                    //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                                                    //串口通讯********************************************************************************
                                                    //CommunicationInfo.SendType=2;//串口通讯
                                                    //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                                                    //CommunicationInfo.Baud=9600;//波特率
                                                    //CommunicationInfo.LedNumber=1;
                }
                if (CommunicationType == 2) //串口
                {
                    //串口通讯********************************************************************************
                    CommunicationInfo.SendType = 2;//串口通讯
                    CommunicationInfo.Commport = ComP1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                    CommunicationInfo.Baud = 115200;//波特率
                    CommunicationInfo.LedNumber = 1;
                }

                IntPtr hProgram;//节目句柄

                hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 0, 0);//根据传的参数创建节目句柄，64是屏宽点数，32是屏高点数，2是屏的颜色，注意此处屏宽高及颜色参数必需与设置屏参的屏宽高及颜色一致，否则发送时会提示错误
                                                                        //此处可自行判断有未创建成功，hProgram返回NULL失败，非NULL成功,一般不会失败

                if (CommunicationType == 1)
                {
                    hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 5, 0); //网口句柄参数要改变
                }
                nResult = LedDll.LV_AddProgram(hProgram, 0, 0, 1);//添加一个节目，参数说明见函数声明注示
                if (nResult != 0)
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    //MessageBox.Show(ErrStr);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + " 屏1添加节目错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, " 屏1添加节目错误：" + ErrStr);
                    return;
                }
                int High = 30;     //原25
                int count = 4;     //原5
                if (bBigFont)
                {
                    High = 50;
                    count = 2;
                }
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    LedDll.AREARECT AreaRect = new LedDll.AREARECT();//区域坐标属性结构体变量
                    if (bBigFont)
                    {
                        AreaRect.left = 8;
                        AreaRect.top = High * i + 20;
                        AreaRect.width = 128 - 8;
                        AreaRect.height = High;
                    }
                    else
                    {
                        AreaRect.left = 0;
                        AreaRect.top = High * i;
                        AreaRect.width = 128;
                        AreaRect.height = High;
                    }



                    LedDll.LV_AddImageTextArea(hProgram, 0, i + 1, ref AreaRect, 0);

                    LedDll.FONTPROP FontProp = new LedDll.FONTPROP();//文字属性
                    FontProp.FontName = "黑体";
                    FontProp.FontSize = 21;    //原18
                    if (DoMarkLiu == "-")
                    {
                        if (Lstring[i].Contains("-"))
                            FontProp.FontColor = LedDll.COLOR_GREEN;
                        else if (Lstring[i].Contains("D"))
                            FontProp.FontColor = LedDll.COLOR_YELLOW;
                        else
                            FontProp.FontColor = LedDll.COLOR_RED;
                    }
                    else if (DoMarkLiu == "X")
                    {
                        if (Lstring[i].Contains(DoMarkGua) || Lstring[i].Contains(DoMarkJin) || Lstring[i].Contains(DoMarkXia) || Lstring[i].Contains(DomarkShang))
                            FontProp.FontColor = LedDll.COLOR_RED;
                        else if (Lstring[i].Contains("D"))
                            FontProp.FontColor = LedDll.COLOR_YELLOW;
                        else
                            FontProp.FontColor = LedDll.COLOR_GREEN;
                    }
                    if (bBigFont)
                    {
                        FontProp.FontColor = LedDll.COLOR_GREEN;
                    }
                    FontProp.FontBold = 0;

                    LedDll.PLAYPROP PlayProp = new LedDll.PLAYPROP();
                    PlayProp.InStyle = 0;
                    PlayProp.DelayTime = 6555;
                    PlayProp.Speed = 3;
                    if (CommunicationType == 1)
                    {
                        //PlayProp.Speed = 6555;
                    }
                    nResult = LedDll.LV_AddMultiLineTextToImageTextArea(hProgram, 0, i + 1, LedDll.ADDTYPE_STRING, Lstring[i], ref FontProp, ref PlayProp, 0, 0);//通过字符串添加一个多行文本到图文区，参数说明见声明注示


                }


                nResult = LedDll.LV_Send(ref CommunicationInfo, hProgram);//发送，见函数声明注示
                LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                                                  //LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                if (nResult != 0)//如果失败则可以调用LV_GetError获取中文错误信息
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏1数据错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "发送屏1数据错误：" + ErrStr);
                    //MessageBox.Show(ErrStr);
                }
                else
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏1数据成功" + "\n");
                    //   MessageBox.Show("发送成功");
                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "Sendata错误：" + ex.Message);
            }
        }
        private void SendData(List<string> Lstring, bool bBigFont, string VoiceStr)  //发送屏1,有语音
        {
            try
            {
                int nResult;
                LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示
                CommunicationInfo.LEDType = 1;  //串口LEDType选1

                if (CommunicationType == 1) //网络通信
                {
                    CommunicationInfo.LEDType = 3; //网口LEDType选3
                    //TCP通讯********************************************************************************
                    CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                    CommunicationInfo.IpStr = P1IP;//给IpStr赋值LED控制卡的IP
                    CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                                                    //广播通讯********************************************************************************
                                                    //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                                                    //串口通讯********************************************************************************
                                                    //CommunicationInfo.SendType=2;//串口通讯
                                                    //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                                                    //CommunicationInfo.Baud=9600;//波特率
                                                    //CommunicationInfo.LedNumber=1;
                }
                if (CommunicationType == 2) //串口
                {
                    //串口通讯********************************************************************************
                    CommunicationInfo.SendType = 2;//串口通讯
                    CommunicationInfo.Commport = ComP1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                    CommunicationInfo.Baud = 115200;//波特率
                    CommunicationInfo.LedNumber = 1;
                }

                IntPtr hProgram;//节目句柄

                hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 0, 0);//根据传的参数创建节目句柄，64是屏宽点数，32是屏高点数，2是屏的颜色，注意此处屏宽高及颜色参数必需与设置屏参的屏宽高及颜色一致，否则发送时会提示错误
                                                                        //此处可自行判断有未创建成功，hProgram返回NULL失败，非NULL成功,一般不会失败

                if (CommunicationType == 1)
                {
                    hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 5, 0); //网口句柄参数要改变
                }
                nResult = LedDll.LV_AddProgram(hProgram, 0, 0, 1);//添加一个节目，参数说明见函数声明注示
                if (nResult != 0)
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    //MessageBox.Show(ErrStr);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + " 屏1添加节目错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, " 屏1添加节目错误：" + ErrStr);
                    return;
                }
                int High = 30;     //原25
                int count = 4;     //原5
                if (bBigFont)
                {
                    High = 50;
                    count = 2;
                }
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    LedDll.AREARECT AreaRect = new LedDll.AREARECT();//区域坐标属性结构体变量
                    if (bBigFont)
                    {
                        AreaRect.left = 8;
                        AreaRect.top = High * i + 20;
                        AreaRect.width = 128 - 8;
                        AreaRect.height = High;
                    }
                    else
                    {
                        AreaRect.left = 0;
                        AreaRect.top = High * i;
                        AreaRect.width = 128;
                        AreaRect.height = High;
                    }



                    LedDll.LV_AddImageTextArea(hProgram, 0, i + 1, ref AreaRect, 0);

                    LedDll.FONTPROP FontProp = new LedDll.FONTPROP();//文字属性
                    FontProp.FontName = "黑体";
                    FontProp.FontSize = 21;    //原18
                    if (DoMarkLiu == "-")
                    {
                        if (Lstring[i].Contains("-"))
                            FontProp.FontColor = LedDll.COLOR_GREEN;
                        else if (Lstring[i].Contains("D"))
                            FontProp.FontColor = LedDll.COLOR_YELLOW;
                        else
                            FontProp.FontColor = LedDll.COLOR_RED;
                    }
                    else if (DoMarkLiu == "X")
                    {
                        if (Lstring[i].Contains(DoMarkGua) || Lstring[i].Contains(DoMarkJin) || Lstring[i].Contains(DoMarkXia) || Lstring[i].Contains(DomarkShang))
                            FontProp.FontColor = LedDll.COLOR_RED;
                        else if (Lstring[i].Contains("D"))
                            FontProp.FontColor = LedDll.COLOR_YELLOW;
                        else
                            FontProp.FontColor = LedDll.COLOR_GREEN;
                    }
                    if (bBigFont)
                    {
                        FontProp.FontColor = LedDll.COLOR_GREEN;
                    }
                    FontProp.FontBold = 0;

                    LedDll.PLAYPROP PlayProp = new LedDll.PLAYPROP();
                    PlayProp.InStyle = 0;
                    PlayProp.DelayTime = 6555;
                    PlayProp.Speed = 3;
                    if (CommunicationType == 1)
                    {
                        //PlayProp.Speed = 6555;
                    }
                    nResult = LedDll.LV_AddMultiLineTextToImageTextArea(hProgram, 0, i + 1, LedDll.ADDTYPE_STRING, Lstring[i], ref FontProp, ref PlayProp, 0, 0);//通过字符串添加一个多行文本到图文区，参数说明见声明注示


                }

                //语音
                LedDll.VOICEAREAINFO voicearea = new LedDll.VOICEAREAINFO();
                voicearea.VoiceStr = VoiceStr;
                voicearea.DelayTime = 5;
                voicearea.PlayCount = 2;


                nResult = LedDll.LV_AddVoiceArea(hProgram, 0, 100, ref voicearea);
                /*if (nResult != 0)
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    MessageBox.Show(ErrStr);
                    return;
                }*/

                nResult = LedDll.LV_Send(ref CommunicationInfo, hProgram);//发送，见函数声明注示
                LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                                                  //LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                if (nResult != 0)//如果失败则可以调用LV_GetError获取中文错误信息
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏1数据及语音错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "发送屏1数据及语音错误：" + ErrStr);
                    //MessageBox.Show(ErrStr);
                }
                else
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏1数据及语音成功" + "\n");
                    //   MessageBox.Show("发送成功");
                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "Sendata（语音重载）错误：" + ex.Message);
            }
        }
        private void SendData2(List<string> Lstring, bool bBigFont)  //发送屏2
        {
            try
            {
                int nResult;
                LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示
                CommunicationInfo.LEDType = 1;

                if (CommunicationType == 1) //网络通信
                {
                    CommunicationInfo.LEDType = 3;
                    //TCP通讯********************************************************************************
                    CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                    CommunicationInfo.IpStr = P2IP;//给IpStr赋值LED控制卡的IP
                    CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                                                    //广播通讯********************************************************************************
                                                    //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                                                    //串口通讯********************************************************************************
                                                    //CommunicationInfo.SendType=2;//串口通讯
                                                    //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                                                    //CommunicationInfo.Baud=9600;//波特率
                                                    //CommunicationInfo.LedNumber=1;
                }
                if (CommunicationType == 2) //串口
                {
                    //串口通讯********************************************************************************
                    CommunicationInfo.SendType = 2;//串口通讯
                    CommunicationInfo.Commport = ComP2;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                    CommunicationInfo.Baud = 115200;//波特率
                    CommunicationInfo.LedNumber = 1;
                }

                IntPtr hProgram;//节目句柄
                hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 0, 0);//根据传的参数创建节目句柄，64是屏宽点数，32是屏高点数，2是屏的颜色，注意此处屏宽高及颜色参数必需与设置屏参的屏宽高及颜色一致，否则发送时会提示错误
                                                                        //此处可自行判断有未创建成功，hProgram返回NULL失败，非NULL成功,一般不会失败

                if (CommunicationType == 1)
                {
                    hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 5, 0); //网口句柄参数要改变
                }
                nResult = LedDll.LV_AddProgram(hProgram, 0, 0, 1);//添加一个节目，参数说明见函数声明注示
                if (nResult != 0)
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    //MessageBox.Show(ErrStr);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + " 屏2添加节目错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, " 屏2添加节目错误：" + ErrStr);
                    return;
                }
                int High = 30;     //原25
                int count = 4;     //原5
                if (bBigFont)
                {
                    High = 50;
                    count = 2;
                }
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    LedDll.AREARECT AreaRect = new LedDll.AREARECT();//区域坐标属性结构体变量
                    if (bBigFont)
                    {
                        AreaRect.left = 8;
                        AreaRect.top = High * i + 20;
                        AreaRect.width = 128 - 8;
                        AreaRect.height = High;
                    }
                    else
                    {
                        AreaRect.left = 0;
                        AreaRect.top = High * i;
                        AreaRect.width = 128;
                        AreaRect.height = High;
                    }



                    LedDll.LV_AddImageTextArea(hProgram, 0, i + 1, ref AreaRect, 0);

                    LedDll.FONTPROP FontProp = new LedDll.FONTPROP();//文字属性
                    FontProp.FontName = "黑体";
                    FontProp.FontSize = 21;    //原18
                    if (DoMarkLiu == "-")
                    {
                        if (Lstring[i].Contains("-"))
                            FontProp.FontColor = LedDll.COLOR_GREEN;
                        else if (Lstring[i].Contains("D"))
                            FontProp.FontColor = LedDll.COLOR_YELLOW;
                        else
                            FontProp.FontColor = LedDll.COLOR_RED;
                    }
                    else if (DoMarkLiu == "X")
                    {
                        if (Lstring[i].Contains(DoMarkGua) || Lstring[i].Contains(DoMarkJin) || Lstring[i].Contains(DoMarkXia) || Lstring[i].Contains(DomarkShang))
                            FontProp.FontColor = LedDll.COLOR_RED;
                        else if (Lstring[i].Contains("D"))
                            FontProp.FontColor = LedDll.COLOR_YELLOW;
                        else
                            FontProp.FontColor = LedDll.COLOR_GREEN;
                    }
                    if (bBigFont)
                    {
                        FontProp.FontColor = LedDll.COLOR_GREEN;
                    }
                    FontProp.FontBold = 0;

                    LedDll.PLAYPROP PlayProp = new LedDll.PLAYPROP();
                    PlayProp.InStyle = 0;
                    PlayProp.DelayTime = 6555;
                    PlayProp.Speed = 3;
                    nResult = LedDll.LV_AddMultiLineTextToImageTextArea(hProgram, 0, i + 1, LedDll.ADDTYPE_STRING, Lstring[i], ref FontProp, ref PlayProp, 0, 0);//通过字符串添加一个多行文本到图文区，参数说明见声明注示


                }


                nResult = LedDll.LV_Send(ref CommunicationInfo, hProgram);//发送，见函数声明注示
                LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                                                  //LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                if (nResult != 0)//如果失败则可以调用LV_GetError获取中文错误信息
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏2数据错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "发送屏2数据错误：" + ErrStr);
                    //MessageBox.Show(ErrStr);
                }
                else
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏2数据成功" + "\n");
                    //   MessageBox.Show("发送成功");
                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "Sendata2错误：" + ex.Message);
            }
        }
        private void SendData2(List<string> Lstring, bool bBigFont, string VoiceStr)  //发送屏2,有语音
        {
            try
            {
                int nResult;
                LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示
                CommunicationInfo.LEDType = 1;

                if (CommunicationType == 1) //网络通信
                {
                    CommunicationInfo.LEDType = 3;
                    //TCP通讯********************************************************************************
                    CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                    CommunicationInfo.IpStr = P2IP;//给IpStr赋值LED控制卡的IP
                    CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                                                    //广播通讯********************************************************************************
                                                    //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                                                    //串口通讯********************************************************************************
                                                    //CommunicationInfo.SendType=2;//串口通讯
                                                    //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                                                    //CommunicationInfo.Baud=9600;//波特率
                                                    //CommunicationInfo.LedNumber=1;
                }
                if (CommunicationType == 2) //串口
                {
                    //串口通讯********************************************************************************
                    CommunicationInfo.SendType = 2;//串口通讯
                    CommunicationInfo.Commport = ComP2;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                    CommunicationInfo.Baud = 115200;//波特率
                    CommunicationInfo.LedNumber = 1;
                }

                IntPtr hProgram;//节目句柄
                hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 0, 0);//根据传的参数创建节目句柄，64是屏宽点数，32是屏高点数，2是屏的颜色，注意此处屏宽高及颜色参数必需与设置屏参的屏宽高及颜色一致，否则发送时会提示错误
                                                                        //此处可自行判断有未创建成功，hProgram返回NULL失败，非NULL成功,一般不会失败

                if (CommunicationType == 1)
                {
                    hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 5, 0); //网口句柄参数要改变
                }
                nResult = LedDll.LV_AddProgram(hProgram, 0, 0, 1);//添加一个节目，参数说明见函数声明注示
                if (nResult != 0)
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    //MessageBox.Show(ErrStr);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + " 屏2添加节目错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, " 屏2添加节目错误：" + ErrStr);
                    return;
                }
                int High = 30;     //原25
                int count = 4;     //原5
                if (bBigFont)
                {
                    High = 50;
                    count = 2;
                }
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    LedDll.AREARECT AreaRect = new LedDll.AREARECT();//区域坐标属性结构体变量
                    if (bBigFont)
                    {
                        AreaRect.left = 8;
                        AreaRect.top = High * i + 20;
                        AreaRect.width = 128 - 8;
                        AreaRect.height = High;
                    }
                    else
                    {
                        AreaRect.left = 0;
                        AreaRect.top = High * i;
                        AreaRect.width = 128;
                        AreaRect.height = High;
                    }



                    LedDll.LV_AddImageTextArea(hProgram, 0, i + 1, ref AreaRect, 0);

                    LedDll.FONTPROP FontProp = new LedDll.FONTPROP();//文字属性
                    FontProp.FontName = "黑体";
                    FontProp.FontSize = 21;    //原18
                    if (DoMarkLiu == "-")
                    {
                        if (Lstring[i].Contains("-"))
                            FontProp.FontColor = LedDll.COLOR_GREEN;
                        else if (Lstring[i].Contains("D"))
                            FontProp.FontColor = LedDll.COLOR_YELLOW;
                        else
                            FontProp.FontColor = LedDll.COLOR_RED;
                    }
                    else if (DoMarkLiu == "X")
                    {
                        if (Lstring[i].Contains(DoMarkGua) || Lstring[i].Contains(DoMarkJin) || Lstring[i].Contains(DoMarkXia) || Lstring[i].Contains(DomarkShang))
                            FontProp.FontColor = LedDll.COLOR_RED;
                        else if (Lstring[i].Contains("D"))
                            FontProp.FontColor = LedDll.COLOR_YELLOW;
                        else
                            FontProp.FontColor = LedDll.COLOR_GREEN;
                    }

                    if (bBigFont)
                    {
                        FontProp.FontColor = LedDll.COLOR_GREEN;
                    }
                    FontProp.FontBold = 0;

                    LedDll.PLAYPROP PlayProp = new LedDll.PLAYPROP();
                    PlayProp.InStyle = 0;
                    PlayProp.DelayTime = 6555;
                    PlayProp.Speed = 3;
                    nResult = LedDll.LV_AddMultiLineTextToImageTextArea(hProgram, 0, i + 1, LedDll.ADDTYPE_STRING, Lstring[i], ref FontProp, ref PlayProp, 0, 0);//通过字符串添加一个多行文本到图文区，参数说明见声明注示


                }

                //语音
                LedDll.VOICEAREAINFO voicearea = new LedDll.VOICEAREAINFO();
                voicearea.VoiceStr = VoiceStr;
                voicearea.DelayTime = 5;
                voicearea.PlayCount = 2;


                nResult = LedDll.LV_AddVoiceArea(hProgram, 0, 100, ref voicearea);
                /*if (nResult != 0)
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    MessageBox.Show(ErrStr);
                    return;
                }*/
                nResult = LedDll.LV_Send(ref CommunicationInfo, hProgram);//发送，见函数声明注示
                LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                                                  //LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                if (nResult != 0)//如果失败则可以调用LV_GetError获取中文错误信息
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏2数据及语音错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "发送屏2数据及语音错误：" + ErrStr);
                    //MessageBox.Show(ErrStr);
                }
                else
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏2数据及语音成功" + "\n");
                    //   MessageBox.Show("发送成功");
                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "Sendata2（语音重载）错误：" + ex.Message);
            }
        }

        private void SendDataTime(List<string> Lstring, bool bBigFont)  //发送屏1时间
        {
            try
            {

                int nResult;
                LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示
                CommunicationInfo.LEDType = 1;

                if (CommunicationType == 1) //网络通信
                {
                    CommunicationInfo.LEDType = 3;
                    //TCP通讯********************************************************************************
                    CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                    CommunicationInfo.IpStr = P1IP;//给IpStr赋值LED控制卡的IP
                    CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                                                    //广播通讯********************************************************************************
                                                    //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                                                    //串口通讯********************************************************************************
                                                    //CommunicationInfo.SendType=2;//串口通讯
                                                    //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                                                    //CommunicationInfo.Baud=9600;//波特率
                                                    //CommunicationInfo.LedNumber=1;
                }
                if (CommunicationType == 2) //串口
                {
                    //串口通讯********************************************************************************
                    CommunicationInfo.SendType = 2;//串口通讯
                    CommunicationInfo.Commport = ComP1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                    CommunicationInfo.Baud = 115200;//波特率
                    CommunicationInfo.LedNumber = 1;
                }

                IntPtr hProgram;//节目句柄
                hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 0, 0);//根据传的参数创建节目句柄，64是屏宽点数，32是屏高点数，2是屏的颜色，注意此处屏宽高及颜色参数必需与设置屏参的屏宽高及颜色一致，否则发送时会提示错误
                                                                        //此处可自行判断有未创建成功，hProgram返回NULL失败，非NULL成功,一般不会失败

                if (CommunicationType == 1)
                {
                    hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 5, 0); //网口句柄参数要改变
                }
                nResult = LedDll.LV_AddProgram(hProgram, 0, 0, 1);//添加一个节目，参数说明见函数声明注示
                if (nResult != 0)
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    //MessageBox.Show(ErrStr);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + " 屏1添加节目(Time)错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, " 屏1添加节目(Time)错误：" + ErrStr);
                    return;
                }
                int High = 25;     //原25
                int count = 5;
                if (bBigFont)
                {
                    High = 50;
                    count = 2;
                }
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    LedDll.AREARECT AreaRect = new LedDll.AREARECT();//区域坐标属性结构体变量
                    if (bBigFont)
                    {
                        AreaRect.left = 10;
                        AreaRect.top = High * i + 20;  //距离屏幕上方距离
                        AreaRect.width = 128 - 10;
                        AreaRect.height = High;
                    }
                    else
                    {
                        AreaRect.left = 0;
                        AreaRect.top = High * i;
                        AreaRect.width = 128;
                        AreaRect.height = High;
                    }



                    LedDll.LV_AddImageTextArea(hProgram, 0, i + 1, ref AreaRect, 0);

                    LedDll.FONTPROP FontProp = new LedDll.FONTPROP();//文字属性
                    FontProp.FontName = "黑体";
                    FontProp.FontSize = 21;    //原18
                    if (Lstring[i].Contains("-"))
                        FontProp.FontColor = LedDll.COLOR_GREEN;
                    else if (Lstring[i].Contains("D"))
                        FontProp.FontColor = LedDll.COLOR_YELLOW;
                    else
                        FontProp.FontColor = LedDll.COLOR_RED;
                    if (bBigFont)
                    {
                        FontProp.FontColor = LedDll.COLOR_GREEN;
                    }
                    FontProp.FontBold = 0;

                    LedDll.PLAYPROP PlayProp = new LedDll.PLAYPROP();
                    PlayProp.InStyle = 0;
                    PlayProp.DelayTime = 6555;
                    PlayProp.Speed = 3;
                    nResult = LedDll.LV_AddMultiLineTextToImageTextArea(hProgram, 0, i + 1, LedDll.ADDTYPE_STRING, Lstring[i], ref FontProp, ref PlayProp, 0, 0);//通过字符串添加一个多行文本到图文区，参数说明见声明注示


                }


                nResult = LedDll.LV_Send(ref CommunicationInfo, hProgram);//发送，见函数声明注示
                LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                                                  //LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                if (nResult != 0)//如果失败则可以调用LV_GetError获取中文错误信息
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏1时间数据错误" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "发送屏1时间数据错误：" + ErrStr);
                    //MessageBox.Show(ErrStr);
                }
                else
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏1时间数据成功" + "\n");
                    //   MessageBox.Show("发送成功");
                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "SendDataTime错误：" + ex.Message);
            }
        }
        private void SendDataTime2(List<string> Lstring, bool bBigFont)  //发送屏2时间
        {
            try
            {

                int nResult;
                LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示
                CommunicationInfo.LEDType = 1;

                if (CommunicationType == 1) //网络通信
                {
                    CommunicationInfo.LEDType = 3;
                    //TCP通讯********************************************************************************
                    CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                    CommunicationInfo.IpStr = P2IP;//给IpStr赋值LED控制卡的IP
                    CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                                                    //广播通讯********************************************************************************
                                                    //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                                                    //串口通讯********************************************************************************
                                                    //CommunicationInfo.SendType=2;//串口通讯
                                                    //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                                                    //CommunicationInfo.Baud=9600;//波特率
                                                    //CommunicationInfo.LedNumber=1;
                }
                if (CommunicationType == 2) //串口
                {
                    //串口通讯********************************************************************************
                    CommunicationInfo.SendType = 2;//串口通讯
                    CommunicationInfo.Commport = ComP2;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                    CommunicationInfo.Baud = 115200;//波特率
                    CommunicationInfo.LedNumber = 1;
                }

                IntPtr hProgram;//节目句柄
                hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 0, 0);//根据传的参数创建节目句柄，64是屏宽点数，32是屏高点数，2是屏的颜色，注意此处屏宽高及颜色参数必需与设置屏参的屏宽高及颜色一致，否则发送时会提示错误
                                                                        //此处可自行判断有未创建成功，hProgram返回NULL失败，非NULL成功,一般不会失败

                if (CommunicationType == 1)
                {
                    hProgram = LedDll.LV_CreateProgramEx(128, 128, 3, 5, 0); //网口句柄参数要改变
                }
                nResult = LedDll.LV_AddProgram(hProgram, 0, 0, 1);//添加一个节目，参数说明见函数声明注示
                if (nResult != 0)
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    // MessageBox.Show(ErrStr);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + " 屏2添加节目(Time)错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, " 屏2添加节目(Time)错误：" + ErrStr);
                    return;
                }
                int High = 25;     //原25
                int count = 5;
                if (bBigFont)
                {
                    High = 50;
                    count = 2;
                }
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    LedDll.AREARECT AreaRect = new LedDll.AREARECT();//区域坐标属性结构体变量
                    if (bBigFont)
                    {
                        AreaRect.left = 10;
                        AreaRect.top = High * i + 20;  //距离屏幕上方距离
                        AreaRect.width = 128 - 10;
                        AreaRect.height = High;
                    }
                    else
                    {
                        AreaRect.left = 0;
                        AreaRect.top = High * i;
                        AreaRect.width = 128;
                        AreaRect.height = High;
                    }



                    LedDll.LV_AddImageTextArea(hProgram, 0, i + 1, ref AreaRect, 0);

                    LedDll.FONTPROP FontProp = new LedDll.FONTPROP();//文字属性
                    FontProp.FontName = "黑体";
                    FontProp.FontSize = 21;    //原18
                    if (Lstring[i].Contains("-"))
                        FontProp.FontColor = LedDll.COLOR_GREEN;
                    else if (Lstring[i].Contains("D"))
                        FontProp.FontColor = LedDll.COLOR_YELLOW;
                    else
                        FontProp.FontColor = LedDll.COLOR_RED;
                    if (bBigFont)
                    {
                        FontProp.FontColor = LedDll.COLOR_GREEN;
                    }
                    FontProp.FontBold = 0;

                    LedDll.PLAYPROP PlayProp = new LedDll.PLAYPROP();
                    PlayProp.InStyle = 0;
                    PlayProp.DelayTime = 6555;
                    PlayProp.Speed = 3;
                    nResult = LedDll.LV_AddMultiLineTextToImageTextArea(hProgram, 0, i + 1, LedDll.ADDTYPE_STRING, Lstring[i], ref FontProp, ref PlayProp, 0, 0);//通过字符串添加一个多行文本到图文区，参数说明见声明注示


                }


                nResult = LedDll.LV_Send(ref CommunicationInfo, hProgram);//发送，见函数声明注示
                LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                                                  //LedDll.LV_DeleteProgram(hProgram);//删除节目内存对象，详见函数声明注示
                if (nResult != 0)//如果失败则可以调用LV_GetError获取中文错误信息
                {
                    string ErrStr;
                    ErrStr = LedDll.LS_GetError(nResult);
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏2时间数据错误：" + ErrStr + "\n");
                    if (bWriteLog)
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "发送屏2时间数据错误：" + ErrStr);
                    //MessageBox.Show(ErrStr);
                }
                else
                {
                    recQueue.Enqueue(DateTime.Now.ToString("G") + "  发送屏2时间数据成功" + "\n");
                    //   MessageBox.Show("发送成功");
                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "SendDataTime2错误：" + ex.Message);
            }
        }




        /// <summary>
        /// X卡发送屏幕函数
        /// </summary>
        // 只有初始化中的bool bBigFont用上了，所有刷新都不能改变字体大小
        private void XStaticInit(List<string> Lstring, bool bBigFont)  //屏1发送静态文本 （开辟静态文本区，发最初的静态文本。相当于初始化，这个函数用了之后才能刷新静态文本区域）
        {
            try
            {

                //-----------------------------------------创建屏幕-----------------------------------------Part0
                //(1.屏幕名字P1Name变为byScreenName格式  2.用byScreenName创建屏幕)
                byte[] byScreenName = System.Text.Encoding.UTF8.GetBytes(P1Name);
                int nRet = ListenDll.LV_CreateScreen(byScreenName);
                //MessageBox.Show(nRet == 0 ? "创建屏幕成功\r\nCreate screen successfully" : "创建屏幕失败\r\nCreate screen failed");
                if (nRet != 0)
                {
                    //MessageBox.Show("创建屏幕失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1创建屏幕失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1创建屏幕失败" + XGetLastError());
                }

                //Thread.Sleep(2000);


                //-----------------------------------------添加页面-----------------------------------------part1
                //（1.自定义节目ID变为byProgramId格式  2.配置pageInfo  3.用byScreenName byProgramId pageInfo添加页面到节目）
                string ProgramId = "1111111";        //节目ID  自定义
                byte[] byScreenName1 = System.Text.Encoding.UTF8.GetBytes(P1Name);
                byte[] byProgramId1 = System.Text.Encoding.UTF8.GetBytes(ProgramId);

                page pageInfo = new page();
                pageInfo.pageId = 0;    //页面 id,同一个节目中,页面 id 唯一
                pageInfo.resolution = "1920X1080";  //页面的分辨率,一般与设备一致 如”1024X768”        ********************************可能会改 默认"1920X1080"
                pageInfo.bgColor = "#000000";  //背景颜色,16 进制的 RRGGBB 如:“#000000”
                string strBgFileName = "";//本地的一个图片文件
                pageInfo.bgImg = strBgFileName;  //背景图片, 图片所在的本地路径,可为空表示不加载图片
                pageInfo.eqType = "Q5";  //设备类型 可为””                                            ********************************可能会改 默认"Q5"
                pageInfo.pageTime = 20;  //页面播放停留时间,单位秒(s).单页面时,该值无效,最小 10
                pageInfo.guid = Guid.NewGuid().ToString();  //页面标识,新建页面,建议重新生成该标识

                string strPageInfo = JsonExtension.ToJSON(pageInfo);
                byte[] byPageInfo = System.Text.Encoding.UTF8.GetBytes(strPageInfo);

                int nRet1 = ListenDll.LV_AddPageToProgram(byScreenName1, byProgramId1, byPageInfo);
                //MessageBox.Show(nRet1 == 0 ? "添加页面成功\r\nAdd page successfully" : "添加页面失败\r\nAdd page failed");
                if (nRet1 != 0)
                {
                    //MessageBox.Show("添加页面失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1添加页面失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1添加页面失败" + XGetLastError());
                }

                //Thread.Sleep(2000);


                //-----------------------------------------添加静态文本-----------------------------------------part2
                int High1 = 22;//第一行高度
                int High = 22; //后续区域高度
                int UpSpace = 0; //第一个区域上面余留的高度
                int count = AreaNum; //区域数量  （有几个区域）
                if (bBigFont)  //大字体区域大小参数
                {
                    High = 50;
                    UpSpace = 20;
                    count = 2;
                }

                //设置第一行区域（新添加    目的：字体设小一点， 20改到16  能放下车次+主体信号）
                StaticTextArea textArea0 = new StaticTextArea();

                textArea0.id = 0; //区域id    后面刷新是根据区域id来的
                textArea0.type = 200;//区域素材类型，200: 静态文本区域  不能改

                //大字体区域大小
                if (bBigFont)
                {
                    textArea0.left = 0;                  //区域 x 坐标点            ********************************可能会改
                    textArea0.top = 0 * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                    textArea0.width = 96;               //区域宽度                 ********************************可能会改 看点数
                    textArea0.height = High;             //区域高度                 ********************************可能会改
                }
                //默认区域大小
                else
                {
                    textArea0.left = 0;                  //区域 x 坐标点            ********************************可能会改
                    textArea0.top = 0 * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                    textArea0.width = 112;               //区域宽度                 ********************************可能会改 看点数
                    textArea0.height = High1;             //区域高度                 ********************************可能会改
                }

                //根据溜放方式更改字体颜色
                textArea0.background = "#000000";  //背景黑色
                                                  //textArea.fontColor = "#ff0000";

                textArea0.fontColor = "#ffffff"; //白色
                
                if (bBigFont)
                {
                    textArea0.fontColor = "#00ff00";  //绿色
                }

                textArea0.fontFamily = "Arial";//当前只支持默认字体.字体需要设备端有对应字库才能支持
                if (bBigFont)
                {
                    textArea0.fontSize = 23;  //字体大小
                }
                else
                {
                    textArea0.fontSize = 16;  //第一行字体大小  20改16
                }
                textArea0.text = Lstring[0] as string;  //字幕内容,不允许为空.UTF - 8 编码;

                textArea0.zIndex = 1;
                string strTextjson0 = JsonExtension.ToJSON(textArea0);//对象转为Json

                byte[] byTextInfo0 = System.Text.Encoding.UTF8.GetBytes(strTextjson0);


                byte[] byScreenName20 = System.Text.Encoding.UTF8.GetBytes(P1Name);
                byte[] byProgramId20 = System.Text.Encoding.UTF8.GetBytes(ProgramId);

                int nRet20 = ListenDll.LV_AddArea(byScreenName20, byProgramId20, 0, byTextInfo0);
                //MessageBox.Show(nRet2 == 0 ? "添加静态区域成功\r\nAdd static area successfully" : "添加静态区域失败\r\nFailed to add static zone");
                if (nRet20 != 0)
                {
                    //MessageBox.Show("添加静态区域失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1添加静态区域失败" + "AreaID:" + 0 + " " + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1添加静态区域失败" + "AreaID:" + 0 + " " + XGetLastError());
                }
                //循环设置每个区域（2-4行）
                for (int i = 1; i < count && i < Lstring.Count; i++)      //i从0改成1， 循环变为第二行到第五行
                {
                    StaticTextArea textArea = new StaticTextArea();

                    textArea.id = i; //区域id    后面刷新是根据区域id来的
                    textArea.type = 200;//区域素材类型，200: 静态文本区域  不能改

                    //大字体区域大小
                    if (bBigFont)
                    {
                        textArea.left = 0;                  //区域 x 坐标点            ********************************可能会改
                        textArea.top = i * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                        textArea.width = 112;               //区域宽度                 ********************************可能会改 看点数
                        textArea.height = High;             //区域高度                 ********************************可能会改
                    }
                    //默认区域大小
                    else
                    {
                        textArea.left = 0;                  //区域 x 坐标点            ********************************可能会改
                        //textArea.top = i * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                        textArea.top = High1 + (i-1) * High + UpSpace;  //区域 y 坐标点
                        textArea.width = 112;               //区域宽度                 ********************************可能会改 看点数
                        textArea.height = High;             //区域高度                 ********************************可能会改
                    }

                    //根据溜放方式更改字体颜色
                    textArea.background = "#000000";  //背景黑色
                                                      //textArea.fontColor = "#ff0000";
                    if (Lstring[i].Contains("-"))
                    {
                        textArea.fontColor = "#00ff00";  //绿色
                    }
                    else if (Lstring[i].Contains("D"))
                    {
                        textArea.fontColor = "#ffff00";  //黄色
                    }
                    else
                    {
                        textArea.fontColor = "#ff0000";  //红色
                    }

                    if (bBigFont)
                    {
                        textArea.fontColor = "#00ff00";  //绿色
                    }

                    textArea.fontFamily = "Arial";//当前只支持默认字体.字体需要设备端有对应字库才能支持
                    if (bBigFont)
                    {
                        textArea.fontSize = 23;  //字体大小
                    }
                    else
                    {
                        textArea.fontSize = 20;  //后四行字体大小 选20/21号
                    }
                    textArea.text = Lstring[i] as string;  //字幕内容,不允许为空.UTF - 8 编码;

                    textArea.zIndex = 1;
                    string strTextjson = JsonExtension.ToJSON(textArea);//对象转为Json

                    byte[] byTextInfo = System.Text.Encoding.UTF8.GetBytes(strTextjson);


                    byte[] byScreenName2 = System.Text.Encoding.UTF8.GetBytes(P1Name);
                    byte[] byProgramId2 = System.Text.Encoding.UTF8.GetBytes(ProgramId);

                    int nRet2 = ListenDll.LV_AddArea(byScreenName2, byProgramId2, 0, byTextInfo);
                    //MessageBox.Show(nRet2 == 0 ? "添加静态区域成功\r\nAdd static area successfully" : "添加静态区域失败\r\nFailed to add static zone");
                    if (nRet2 != 0)
                    {
                        //MessageBox.Show("添加静态区域失败\r\n" + XGetLastError());
                        sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1添加静态区域失败" + "AreaID:" + i + " " + XGetLastError() + "\n");
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1添加静态区域失败" + "AreaID:" + i + " " + XGetLastError());
                    }
                }



                //Thread.Sleep(2000);

                //-----------------------------------------发送节目-----------------------------------------part3
                string ProgramName = "test_program";
                byte[] byScreenName3 = System.Text.Encoding.UTF8.GetBytes(P1Name);
                byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P1IP);
                byte[] byProgramId3 = System.Text.Encoding.UTF8.GetBytes(ProgramId);
                byte[] byProgramName3 = System.Text.Encoding.UTF8.GetBytes(ProgramName);

                byte[] byPassword = System.Text.Encoding.UTF8.GetBytes("");

                int nRet3 = ListenDll.LV_SendProgramToDevice(byScreenName3, byIp, byProgramId3, byProgramName3, byPassword);
                //MessageBox.Show(nRet3 == 0 ? "发送节目成功\r\nSend the program successfully" : "发送节目失败\r\nSend the program Failed");
                if (nRet3 != 0)
                {
                    //MessageBox.Show("发送节目失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1发送节目失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1发送节目失败" + XGetLastError());
                }

                //-----------------------------------------清理缓存-----------------------------------------part4
                int nRet4 = ListenDll.LV_CleanProgramTemporary();
                //MessageBox.Show(nRet4 == 0 ? "清理缓存成功\r\nSuccess" : "清理缓存失败\r\nFail");
                if (nRet4 != 0)
                {
                    //MessageBox.Show("清理缓存失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1清理缓存失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1清理缓存失败" + XGetLastError());
                }

                //---------------------------------------发送节目计划---------------------------------------part5
                List<ProgramPlan> listPlan = new List<ProgramPlan>();

                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

                ProgramPlan plan1 = new ProgramPlan();
                plan1.createDate = Convert.ToInt64(ts.TotalMilliseconds);  //节目创建时间,13 位时间戳
                plan1.programId = ProgramId;
                plan1.programName = ProgramName;
                plan1.programType = 1;  //节目类型 1:轮播节目(该类型节目最多只能1 个) 2:插播节目 3:定时节目 。播放的优先级为 插播> 定时 > 轮播
                plan1.updateDate = Convert.ToInt64(ts.TotalMilliseconds); //节目更新时间,13 位时间戳
                plan1.date = new List<ProgramPlan.Date>();  //定时和插播节目中的日期段设置,轮播节目该值的元素为空集
                plan1.planWeekDay = new List<int>();
                plan1.time = new List<ProgramPlan.Time>();
                plan1.isDefault = 0;  //是否为默认节目,仅支持将轮播节目设置成默认节目 1:是, 0:否。

                listPlan.Add(plan1);


                string strPlanJson = JsonExtension.ToJSON(listPlan);//对象转为Json
                byte[] byPlanInfo = System.Text.Encoding.UTF8.GetBytes(strPlanJson);
                byte[] byIp5 = System.Text.Encoding.UTF8.GetBytes(P1IP);
                string strDefVideo = "";//如果是默认节目,同时默认节目中存在视频文件,该字符串才不为空字符串
                byte[] byDefVideo = System.Text.Encoding.UTF8.GetBytes(strDefVideo);

                int nRet5 = ListenDll.LV_SendProgramPlan(byIp5, byPlanInfo, byDefVideo);
                //MessageBox.Show(nRet5 == 0 ? "发送节目计划成功\r\nSuccessfully sent the program plan" : "发送节目计划失败\r\nFailed to send program plan");
                if (nRet5 != 0)
                {
                    //MessageBox.Show("发送节目计划失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1发送节目计划失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1发送节目计划失败" + XGetLastError());
                    MessageBox.Show("屏1初始化失败,请修复故障后重启程序" + "\r\n" + XGetLastError());
                }
                if (nRet5 == 0)
                {
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1初始化完成" + "（已添加" + AreaNum + "个静态文本区）" + "\n");
                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "XStaticInit错误：" + ex.Message);
            }
        }
        private void XStaticInit2(List<string> Lstring, bool bBigFont)  //屏2发送静态文本 
        {
            try
            {

                //-----------------------------------------创建屏幕-----------------------------------------Part0
                //(1.屏幕名字P2Name变为byScreenName格式  2.用byScreenName创建屏幕)
                byte[] byScreenName = System.Text.Encoding.UTF8.GetBytes(P2Name);
                int nRet = ListenDll.LV_CreateScreen(byScreenName);
                //MessageBox.Show(nRet == 0 ? "创建屏幕成功\r\nCreate screen successfully" : "创建屏幕失败\r\nCreate screen failed");
                if (nRet != 0)
                {
                    //MessageBox.Show("创建屏幕失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2创建屏幕失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2创建屏幕失败" + XGetLastError());
                }

                //Thread.Sleep(2000);


                //-----------------------------------------添加页面-----------------------------------------part1
                //（1.自定义节目ID变为byProgramId格式  2.配置pageInfo  3.用byScreenName byProgramId pageInfo添加页面到节目）
                string ProgramId = "1111111";        //节目ID  自定义
                byte[] byScreenName1 = System.Text.Encoding.UTF8.GetBytes(P2Name);
                byte[] byProgramId1 = System.Text.Encoding.UTF8.GetBytes(ProgramId);

                page pageInfo = new page();
                pageInfo.pageId = 0;    //页面 id,同一个节目中,页面 id 唯一
                pageInfo.resolution = "1920X1080";  //页面的分辨率,一般与设备一致 如”1024X768”        
                pageInfo.bgColor = "#000000";  //背景颜色,16 进制的 RRGGBB 如:“#000000”
                string strBgFileName = "";//本地的一个图片文件
                pageInfo.bgImg = strBgFileName;  //背景图片, 图片所在的本地路径,可为空表示不加载图片
                pageInfo.eqType = "Q5";  //设备类型 可为””                                            
                pageInfo.pageTime = 20;  //页面播放停留时间,单位秒(s).单页面时,该值无效,最小 10
                pageInfo.guid = Guid.NewGuid().ToString();  //页面标识,新建页面,建议重新生成该标识

                string strPageInfo = JsonExtension.ToJSON(pageInfo);
                byte[] byPageInfo = System.Text.Encoding.UTF8.GetBytes(strPageInfo);

                int nRet1 = ListenDll.LV_AddPageToProgram(byScreenName1, byProgramId1, byPageInfo);
                //MessageBox.Show(nRet1 == 0 ? "添加页面成功\r\nAdd page successfully" : "添加页面失败\r\nAdd page failed");
                if (nRet1 != 0)
                {
                    //MessageBox.Show("添加页面失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2添加页面失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2添加页面失败" + XGetLastError());
                }

                //Thread.Sleep(2000);


                //-----------------------------------------添加静态文本-----------------------------------------part2
                int High = 25; //区域高度
                int UpSpace = 3; //第一个区域上面余留的高度
                int count = AreaNum; //区域数量  （有几个区域）
                if (bBigFont)  //大字体区域大小参数
                {
                    High = 50;
                    UpSpace = 20;
                    count = 2;
                }

                //设置第一行区域（新添加    目的：字体设小一点， 20改到16  能放下车次+主体信号）
                StaticTextArea textArea0 = new StaticTextArea();

                textArea0.id = 0; //区域id    后面刷新是根据区域id来的
                textArea0.type = 200;//区域素材类型，200: 静态文本区域  不能改

                //大字体区域大小
                if (bBigFont)
                {
                    textArea0.left = 0;                  //区域 x 坐标点            ********************************可能会改
                    textArea0.top = 0 * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                    textArea0.width = 96;               //区域宽度                 ********************************可能会改 看点数
                    textArea0.height = High;             //区域高度                 ********************************可能会改
                }
                //默认区域大小
                else
                {
                    textArea0.left = 0;                  //区域 x 坐标点            ********************************可能会改
                    textArea0.top = 0 * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                    textArea0.width = 96;               //区域宽度                 ********************************可能会改 看点数
                    textArea0.height = High;             //区域高度                 ********************************可能会改
                }

                //根据溜放方式更改字体颜色
                textArea0.background = "#000000";  //背景黑色
                                                   //textArea.fontColor = "#ff0000";

                textArea0.fontColor = "#ffffff"; //白色

                if (bBigFont)
                {
                    textArea0.fontColor = "#00ff00";  //绿色
                }

                textArea0.fontFamily = "Arial";//当前只支持默认字体.字体需要设备端有对应字库才能支持
                if (bBigFont)
                {
                    textArea0.fontSize = 23;  //字体大小
                }
                else
                {
                    textArea0.fontSize = 16;  //字体大小  20改16
                }
                textArea0.text = Lstring[0] as string;  //字幕内容,不允许为空.UTF - 8 编码;

                textArea0.zIndex = 1;
                string strTextjson0 = JsonExtension.ToJSON(textArea0);//对象转为Json

                byte[] byTextInfo0 = System.Text.Encoding.UTF8.GetBytes(strTextjson0);


                byte[] byScreenName20 = System.Text.Encoding.UTF8.GetBytes(P2Name);
                byte[] byProgramId20 = System.Text.Encoding.UTF8.GetBytes(ProgramId);

                int nRet20 = ListenDll.LV_AddArea(byScreenName20, byProgramId20, 0, byTextInfo0);
                //MessageBox.Show(nRet2 == 0 ? "添加静态区域成功\r\nAdd static area successfully" : "添加静态区域失败\r\nFailed to add static zone");
                if (nRet20 != 0)
                {
                    //MessageBox.Show("添加静态区域失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1添加静态区域失败" + "AreaID:" + 0 + " " + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1添加静态区域失败" + "AreaID:" + 0 + " " + XGetLastError());
                }

                //循环设置2-4行
                for (int i = 1; i < count && i < Lstring.Count; i++)    //i从0改为1
                {
                    StaticTextArea textArea = new StaticTextArea();

                    textArea.id = i; //区域id    后面刷新是根据区域id来的
                    textArea.type = 200;//区域素材类型，200: 静态文本区域  不能改

                    //大字体区域大小
                    if (bBigFont)
                    {
                        textArea.left = 0;                  //区域 x 坐标点            ********************************可能会改
                        textArea.top = i * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                        textArea.width = 96;               //区域宽度                 ********************************可能会改 看点数
                        textArea.height = High;             //区域高度                 ********************************可能会改
                    }
                    //默认区域大小
                    else
                    {
                        textArea.left = 0;                  //区域 x 坐标点            ********************************可能会改
                        textArea.top = i * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                        textArea.width = 96;               //区域宽度                 ********************************可能会改 看点数
                        textArea.height = High;             //区域高度                 ********************************可能会改
                    }

                    //根据溜放方式更改字体颜色
                    textArea.background = "#000000";  //背景黑色
                                                      //textArea.fontColor = "#ff0000";
                    if (Lstring[i].Contains("-"))
                    {
                        textArea.fontColor = "#00ff00";  //绿色
                    }
                    else if (Lstring[i].Contains("D"))
                    {
                        textArea.fontColor = "#ffff00";  //黄色
                    }
                    else
                    {
                        textArea.fontColor = "#ff0000";  //红色
                    }

                    if (bBigFont)
                    {
                        textArea.fontColor = "#00ff00";  //绿色
                    }

                    textArea.fontFamily = "Arial";//当前只支持默认字体.字体需要设备端有对应字库才能支持
                    if (bBigFont)
                    {
                        textArea.fontSize = 23;  //字体大小
                    }
                    else
                    {
                        textArea.fontSize = 20;  //字体大小
                    }
                    textArea.text = Lstring[i] as string;  //字幕内容,不允许为空.UTF - 8 编码;

                    textArea.zIndex = 1;
                    string strTextjson = JsonExtension.ToJSON(textArea);//对象转为Json

                    byte[] byTextInfo = System.Text.Encoding.UTF8.GetBytes(strTextjson);


                    byte[] byScreenName2 = System.Text.Encoding.UTF8.GetBytes(P2Name);
                    byte[] byProgramId2 = System.Text.Encoding.UTF8.GetBytes(ProgramId);

                    int nRet2 = ListenDll.LV_AddArea(byScreenName2, byProgramId2, 0, byTextInfo);
                    //MessageBox.Show(nRet2 == 0 ? "添加静态区域成功\r\nAdd static area successfully" : "添加静态区域失败\r\nFailed to add static zone");
                    if (nRet2 != 0)
                    {
                        //MessageBox.Show("添加静态区域失败\r\n" + XGetLastError());
                        sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2添加静态区域失败" + "AreaID:" + i + " " + XGetLastError() + "\n");
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2添加静态区域失败" + "AreaID:" + i + " " + XGetLastError());
                    }
                }



                //Thread.Sleep(2000);

                //-----------------------------------------发送节目-----------------------------------------part3
                string ProgramName = "test_program";
                byte[] byScreenName3 = System.Text.Encoding.UTF8.GetBytes(P2Name);
                byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P2IP);
                byte[] byProgramId3 = System.Text.Encoding.UTF8.GetBytes(ProgramId);
                byte[] byProgramName3 = System.Text.Encoding.UTF8.GetBytes(ProgramName);

                byte[] byPassword = System.Text.Encoding.UTF8.GetBytes("");

                int nRet3 = ListenDll.LV_SendProgramToDevice(byScreenName3, byIp, byProgramId3, byProgramName3, byPassword);
                //MessageBox.Show(nRet3 == 0 ? "发送节目成功\r\nSend the program successfully" : "发送节目失败\r\nSend the program Failed");
                if (nRet3 != 0)
                {
                    //MessageBox.Show("发送节目失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2发送节目失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2发送节目失败" + XGetLastError());
                }

                //-----------------------------------------清理缓存-----------------------------------------part4
                int nRet4 = ListenDll.LV_CleanProgramTemporary();
                //MessageBox.Show(nRet4 == 0 ? "清理缓存成功\r\nSuccess" : "清理缓存失败\r\nFail");
                if (nRet4 != 0)
                {
                    //MessageBox.Show("清理缓存失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2清理缓存失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2清理缓存失败" + XGetLastError());
                }

                //---------------------------------------发送节目计划---------------------------------------part5
                List<ProgramPlan> listPlan = new List<ProgramPlan>();

                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

                ProgramPlan plan1 = new ProgramPlan();
                plan1.createDate = Convert.ToInt64(ts.TotalMilliseconds);  //节目创建时间,13 位时间戳
                plan1.programId = ProgramId;
                plan1.programName = ProgramName;
                plan1.programType = 1;  //节目类型 1:轮播节目(该类型节目最多只能1 个) 2:插播节目 3:定时节目 。播放的优先级为 插播> 定时 > 轮播
                plan1.updateDate = Convert.ToInt64(ts.TotalMilliseconds); //节目更新时间,13 位时间戳
                plan1.date = new List<ProgramPlan.Date>();  //定时和插播节目中的日期段设置,轮播节目该值的元素为空集
                plan1.planWeekDay = new List<int>();
                plan1.time = new List<ProgramPlan.Time>();
                plan1.isDefault = 0;  //是否为默认节目,仅支持将轮播节目设置成默认节目 1:是, 0:否。

                listPlan.Add(plan1);


                string strPlanJson = JsonExtension.ToJSON(listPlan);//对象转为Json
                byte[] byPlanInfo = System.Text.Encoding.UTF8.GetBytes(strPlanJson);
                byte[] byIp5 = System.Text.Encoding.UTF8.GetBytes(P2IP);
                string strDefVideo = "";//如果是默认节目,同时默认节目中存在视频文件,该字符串才不为空字符串
                byte[] byDefVideo = System.Text.Encoding.UTF8.GetBytes(strDefVideo);

                int nRet5 = ListenDll.LV_SendProgramPlan(byIp5, byPlanInfo, byDefVideo);
                //MessageBox.Show(nRet5 == 0 ? "发送节目计划成功\r\nSuccessfully sent the program plan" : "发送节目计划失败\r\nFailed to send program plan");
                if (nRet5 != 0)
                {
                    //MessageBox.Show("发送节目计划失败\r\n" + XGetLastError());
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2发送节目计划失败" + XGetLastError() + "\n");
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2发送节目计划失败" + XGetLastError());
                    MessageBox.Show("屏2初始化失败,请修复故障后重启程序" + "\r\n" + XGetLastError());
                }
                if (nRet5 == 0)
                {
                    sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2初始化完成" + "（已添加" + AreaNum + "个静态文本区）" + "\n");
                }
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "XStaticInit2错误：" + ex.Message);
            }

        }

        private void XRefresh(List<string> Lstring, bool bBigFont) //屏1刷新静态文本区域 注意：区域id要和初始化的对上 
        {
            try
            {
                int count = AreaNum;
                //循环设置每个区域
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    RefreshData refreshData = new RefreshData();
                    refreshData.areaId = i;//区域ID必须与节目中的静态文本区域ID一致才能刷新
                    refreshData.text = Lstring[i];
                    //refreshData.fontColor = "#00ff00";
                    //根据溜放方式更改字体颜色
                    /*if (Lstring[i].Contains(DoMarkLiu))
                    {
                        refreshData.fontColor = "#00ff00";  //绿色
                    }
                    else if (Lstring[i].Contains(DoMarkDan))
                    {
                        refreshData.fontColor = "#ffff00";  //黄色
                    }
                    else
                    {
                        refreshData.fontColor = "#ff0000";  //红色
                    }*/
                    
                    string[] DoMarkk = Lstring[i].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (DoMarkk.Count() >= 4)
                    {
                        if (DoMarkk[1].Contains(DoMarkLiu))
                        {
                            refreshData.fontColor = "#00ff00";  //绿色
                        }
                        else if (DoMarkk[1].Contains(DoMarkDan))
                        {
                            refreshData.fontColor = "#ffff00";  //黄色
                        }
                        else
                        {
                            refreshData.fontColor = "#ff0000";  //红色
                        }
                    }
                    else
                        refreshData.fontColor = "#ff0000";

                    if (Lstring[i].Contains("红") || Lstring[i].Contains("黄") || Lstring[i].Contains("绿") || Lstring[i].Contains("白"))    //第一行车次＋驼信   识别到汉字改成白色。
                    {
                        refreshData.fontColor = "#ffffff";
                    }

                    refreshData.refreshTimes = 0; //闪烁次数 0 为不闪烁,如果 json 中没有该 key,也是不闪烁
                    refreshData.base64 = 0; //刷新数据传输的数据是否为 base64 编码 0:否 1:是(一般使用的语言文字不需要 base64 来编码)

                    List<RefreshData> list = new List<RefreshData>();
                    list.Add(refreshData);

                    string strRefreshData = JsonExtension.ToJSON(list);//对象转为Json

                    byte[] byRefreshData = System.Text.Encoding.UTF8.GetBytes(strRefreshData);
                    byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P1IP);
                    int nRet = ListenDll.LV_RefreshFixedArea(byIp, byRefreshData);
                    //MessageBox.Show(nRet == 0 ? "刷新成功\r\nRefresh successfully" : "刷新失败\r\nRefresh failed");
                    if (nRet != 0)
                    {
                        //MessageBox.Show("AreaID" + i + " 刷新失败\r\n" + XGetLastError());
                        sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1刷新失败:" + "AreaID:" + i + "  " + XGetLastError() + "\n");
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1刷新失败" + "AreaID:" + i + "  " + XGetLastError());
                    }
                }
                //把没刷新的区域刷成空字符
                if (Lstring.Count < count)
                {
                    for (int i = Lstring.Count; i < count; i++)
                    {
                        RefreshData refreshData = new RefreshData();
                        refreshData.areaId = i;//区域ID必须与节目中的静态文本区域ID一致才能刷新
                        refreshData.text = "";  //发空字符！！！！！！！
                                                //refreshData.fontColor = "#00ff00";
                                                //空字符不改颜色
                        refreshData.refreshTimes = 0; //闪烁次数 0 为不闪烁,如果 json 中没有该 key,也是不闪烁
                        refreshData.base64 = 0; //刷新数据传输的数据是否为 base64 编码 0:否 1:是(一般使用的语言文字不需要 base64 来编码)

                        List<RefreshData> list = new List<RefreshData>();
                        list.Add(refreshData);

                        string strRefreshData = JsonExtension.ToJSON(list);//对象转为Json

                        byte[] byRefreshData = System.Text.Encoding.UTF8.GetBytes(strRefreshData);
                        byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P1IP);
                        int nRet = ListenDll.LV_RefreshFixedArea(byIp, byRefreshData);
                        //MessageBox.Show(nRet == 0 ? "刷新成功\r\nRefresh successfully" : "刷新失败\r\nRefresh failed");
                        if (nRet != 0)
                        {
                            //MessageBox.Show("AreaID" + i + " 刷新失败\r\n" + XGetLastError());
                            sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1刷新失败:" + "AreaID:" + i + "  " + XGetLastError() + "\n");
                            Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1刷新失败" + "AreaID:" + i + "  " + XGetLastError());
                        }
                    }
                }
                sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1刷新" + "\n");
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "XRefresh错误：" + ex.Message);
            }
        }
        private void XRefresh2(List<string> Lstring, bool bBigFont) //屏2刷新静态文本区域 
        {
            try
            {
                int count = AreaNum;
                //循环设置每个区域
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    RefreshData refreshData = new RefreshData();
                    refreshData.areaId = i;//区域ID必须与节目中的静态文本区域ID一致才能刷新
                    refreshData.text = Lstring[i];
                    //refreshData.fontColor = "#00ff00";
                    //根据溜放方式更改字体颜色
                    /*if (Lstring[i].Contains(DoMarkLiu))
                    {
                        refreshData.fontColor = "#00ff00";  //绿色
                    }
                    else if (Lstring[i].Contains(DoMarkDan))
                    {
                        refreshData.fontColor = "#ffff00";  //黄色
                    }
                    else
                    {
                        refreshData.fontColor = "#ff0000";  //红色
                    }
*/
                    string[] DoMarkk = Lstring[i].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (DoMarkk.Count() >= 4)
                    {
                        if (DoMarkk[1].Contains(DoMarkLiu))
                        {
                            refreshData.fontColor = "#00ff00";  //绿色
                        }
                        else if (DoMarkk[1].Contains(DoMarkDan))
                        {
                            refreshData.fontColor = "#ffff00";  //黄色
                        }
                        else
                        {
                            refreshData.fontColor = "#ff0000";  //红色
                        }
                    }
                    else
                        refreshData.fontColor = "#ff0000";  //红色

                    if (Lstring[i].Contains("红") || Lstring[i].Contains("黄") || Lstring[i].Contains("绿") || Lstring[i].Contains("白"))    //第一行车次＋驼信   识别到汉字改成白色。
                    {
                        refreshData.fontColor = "#ffffff";
                    }


                    refreshData.refreshTimes = 0; //闪烁次数 0 为不闪烁,如果 json 中没有该 key,也是不闪烁
                    refreshData.base64 = 0; //刷新数据传输的数据是否为 base64 编码 0:否 1:是(一般使用的语言文字不需要 base64 来编码)

                    List<RefreshData> list = new List<RefreshData>();
                    list.Add(refreshData);

                    string strRefreshData = JsonExtension.ToJSON(list);//对象转为Json

                    byte[] byRefreshData = System.Text.Encoding.UTF8.GetBytes(strRefreshData);
                    byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P2IP);
                    int nRet = ListenDll.LV_RefreshFixedArea(byIp, byRefreshData);
                    //MessageBox.Show(nRet == 0 ? "刷新成功\r\nRefresh successfully" : "刷新失败\r\nRefresh failed");
                    if (nRet != 0)
                    {
                        //MessageBox.Show("AreaID" + i + " 刷新失败\r\n" + XGetLastError());
                        sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2刷新失败:" + "AreaID:" + i + "  " + XGetLastError() + "\n");
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2刷新失败" + "AreaID:" + i + "  " + XGetLastError());
                    }
                }
                //把没刷新的区域刷成空字符
                if (Lstring.Count < count)
                {
                    for (int i = Lstring.Count; i < count; i++)
                    {
                        RefreshData refreshData = new RefreshData();
                        refreshData.areaId = i;//区域ID必须与节目中的静态文本区域ID一致才能刷新
                        refreshData.text = "";  //发空字符！！！！！！！
                                                //refreshData.fontColor = "#00ff00";
                                                //空字符不改颜色
                        refreshData.refreshTimes = 0; //闪烁次数 0 为不闪烁,如果 json 中没有该 key,也是不闪烁
                        refreshData.base64 = 0; //刷新数据传输的数据是否为 base64 编码 0:否 1:是(一般使用的语言文字不需要 base64 来编码)

                        List<RefreshData> list = new List<RefreshData>();
                        list.Add(refreshData);

                        string strRefreshData = JsonExtension.ToJSON(list);//对象转为Json

                        byte[] byRefreshData = System.Text.Encoding.UTF8.GetBytes(strRefreshData);
                        byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P2IP);
                        int nRet = ListenDll.LV_RefreshFixedArea(byIp, byRefreshData);
                        //MessageBox.Show(nRet == 0 ? "刷新成功\r\nRefresh successfully" : "刷新失败\r\nRefresh failed");
                        if (nRet != 0)
                        {
                            //MessageBox.Show("AreaID" + i + " 刷新失败\r\n" + XGetLastError());
                            sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2刷新失败:" + "AreaID:" + i + "  " + XGetLastError() + "\n");
                            Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2刷新失败" + "AreaID:" + i + "  " + XGetLastError());
                        }
                    }
                }
                sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2刷新" + "\n");
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "XRefresh2错误：" + ex.Message);
            }
        }

        private void XRefreshTm(List<string> Lstring, bool bBigFont) //屏1刷新静态文本（时间标语专用）
        {
            try
            {
                int count = AreaNum;
                //循环设置每个区域
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    RefreshData refreshData = new RefreshData();
                    refreshData.areaId = i;//区域ID必须与节目中的静态文本区域ID一致才能刷新
                    refreshData.text = Lstring[i];
                    refreshData.fontColor = "#00ff00";  //时间页面都是绿色
                    refreshData.refreshTimes = 0; //闪烁次数 0 为不闪烁,如果 json 中没有该 key,也是不闪烁
                    refreshData.base64 = 0; //刷新数据传输的数据是否为 base64 编码 0:否 1:是(一般使用的语言文字不需要 base64 来编码)

                    List<RefreshData> list = new List<RefreshData>();
                    list.Add(refreshData);

                    string strRefreshData = JsonExtension.ToJSON(list);//对象转为Json

                    byte[] byRefreshData = System.Text.Encoding.UTF8.GetBytes(strRefreshData);
                    byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P1IP);
                    int nRet = ListenDll.LV_RefreshFixedArea(byIp, byRefreshData);
                    //MessageBox.Show(nRet == 0 ? "刷新成功\r\nRefresh successfully" : "刷新失败\r\nRefresh failed");
                    if (nRet != 0)
                    {
                        //MessageBox.Show("AreaID" + i + " 刷新失败\r\n" + XGetLastError());
                        sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1时间/标语刷新失败:" + "AreaID:" + i + "  " + XGetLastError() + "\n");
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1时间/标语刷新失败" + "AreaID:" + i + "  " + XGetLastError());
                    }
                }
                //把没刷新的区域刷成空字符
                if (Lstring.Count < count)
                {
                    for (int i = Lstring.Count; i < count; i++)
                    {
                        RefreshData refreshData = new RefreshData();
                        refreshData.areaId = i;//区域ID必须与节目中的静态文本区域ID一致才能刷新
                        refreshData.text = "";  //发空字符！！！！！！！
                        refreshData.fontColor = "#00ff00";
                        //空字符不改颜色
                        refreshData.refreshTimes = 0; //闪烁次数 0 为不闪烁,如果 json 中没有该 key,也是不闪烁
                        refreshData.base64 = 0; //刷新数据传输的数据是否为 base64 编码 0:否 1:是(一般使用的语言文字不需要 base64 来编码)

                        List<RefreshData> list = new List<RefreshData>();
                        list.Add(refreshData);

                        string strRefreshData = JsonExtension.ToJSON(list);//对象转为Json

                        byte[] byRefreshData = System.Text.Encoding.UTF8.GetBytes(strRefreshData);
                        byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P1IP);
                        int nRet = ListenDll.LV_RefreshFixedArea(byIp, byRefreshData);
                        //MessageBox.Show(nRet == 0 ? "刷新成功\r\nRefresh successfully" : "刷新失败\r\nRefresh failed");
                        if (nRet != 0)
                        {
                            //MessageBox.Show("AreaID" + i + " 刷新失败\r\n" + XGetLastError());
                            sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1时间/标语刷新失败:" + "AreaID:" + i + "  " + XGetLastError() + "\n");
                            Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1时间/标语刷新失败" + "AreaID:" + i + "  " + XGetLastError());
                        }
                    }
                }
                sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏1时间/标语刷新" + "\n");
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "XRefreshTm错误：" + ex.Message);
            }
        }
        private void XRefreshTm2(List<string> Lstring, bool bBigFont) //屏1刷新静态文本（时间标语专用）
        {
            try
            {
                int count = AreaNum;
                //循环设置每个区域
                for (int i = 0; i < count && i < Lstring.Count; i++)
                {
                    RefreshData refreshData = new RefreshData();
                    refreshData.areaId = i;//区域ID必须与节目中的静态文本区域ID一致才能刷新
                    refreshData.text = Lstring[i];
                    refreshData.fontColor = "#00ff00";  //时间页面都是绿色
                    refreshData.refreshTimes = 0; //闪烁次数 0 为不闪烁,如果 json 中没有该 key,也是不闪烁
                    refreshData.base64 = 0; //刷新数据传输的数据是否为 base64 编码 0:否 1:是(一般使用的语言文字不需要 base64 来编码)

                    List<RefreshData> list = new List<RefreshData>();
                    list.Add(refreshData);

                    string strRefreshData = JsonExtension.ToJSON(list);//对象转为Json

                    byte[] byRefreshData = System.Text.Encoding.UTF8.GetBytes(strRefreshData);
                    byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P2IP);
                    int nRet = ListenDll.LV_RefreshFixedArea(byIp, byRefreshData);
                    //MessageBox.Show(nRet == 0 ? "刷新成功\r\nRefresh successfully" : "刷新失败\r\nRefresh failed");
                    if (nRet != 0)
                    {
                        //MessageBox.Show("AreaID" + i + " 刷新失败\r\n" + XGetLastError());
                        sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2时间/标语刷新失败:" + "AreaID:" + i + "  " + XGetLastError() + "\n");
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2时间/标语刷新失败" + "AreaID:" + i + "  " + XGetLastError());
                    }
                }
                //把没刷新的区域刷成空字符
                if (Lstring.Count < count)
                {
                    for (int i = Lstring.Count; i < count; i++)
                    {
                        RefreshData refreshData = new RefreshData();
                        refreshData.areaId = i;//区域ID必须与节目中的静态文本区域ID一致才能刷新
                        refreshData.text = "";  //发空字符！！！！！！！
                        refreshData.fontColor = "#00ff00";
                        //空字符不改颜色
                        refreshData.refreshTimes = 0; //闪烁次数 0 为不闪烁,如果 json 中没有该 key,也是不闪烁
                        refreshData.base64 = 0; //刷新数据传输的数据是否为 base64 编码 0:否 1:是(一般使用的语言文字不需要 base64 来编码)

                        List<RefreshData> list = new List<RefreshData>();
                        list.Add(refreshData);

                        string strRefreshData = JsonExtension.ToJSON(list);//对象转为Json

                        byte[] byRefreshData = System.Text.Encoding.UTF8.GetBytes(strRefreshData);
                        byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P2IP);
                        int nRet = ListenDll.LV_RefreshFixedArea(byIp, byRefreshData);
                        //MessageBox.Show(nRet == 0 ? "刷新成功\r\nRefresh successfully" : "刷新失败\r\nRefresh failed");
                        if (nRet != 0)
                        {
                            //MessageBox.Show("AreaID" + i + " 刷新失败\r\n" + XGetLastError());
                            sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2时间/标语刷新失败:" + "AreaID:" + i + "  " + XGetLastError() + "\n");
                            Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2时间/标语刷新失败" + "AreaID:" + i + "  " + XGetLastError());
                        }
                    }
                }
                sndQueue.Enqueue(DateTime.Now.ToString("G") + "  屏2时间/标语刷新" + "\n");
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "XRefreshTm2错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 发送字幕给屏幕方法《X4-75卡》   刷新慢 未使用
        /// </summary>
        private void XSendText(List<string> Lstring, bool bBigFont)
        {

            //-----------------------------------------创建屏幕-----------------------------------------Part0
            //(1.屏幕名字P1Name变为byScreenName格式  2.用byScreenName创建屏幕)
            byte[] byScreenName = System.Text.Encoding.UTF8.GetBytes(P1Name);
            int nRet = ListenDll.LV_CreateScreen(byScreenName);
            //MessageBox.Show(nRet == 0 ? "创建屏幕成功\r\nCreate screen successfully" : "创建屏幕失败\r\nCreate screen failed");
            if (nRet != 0)
            {
                XGetLastError();
            }

            //Thread.Sleep(2000);


            //-----------------------------------------添加页面-----------------------------------------part1
            //（1.自定义节目ID变为byProgramId格式  2.配置pageInfo  3.用byScreenName byProgramId pageInfo添加页面到节目）
            string ProgramId = "1111111";        //节目ID  自定义
            byte[] byScreenName1 = System.Text.Encoding.UTF8.GetBytes(P1Name);
            byte[] byProgramId1 = System.Text.Encoding.UTF8.GetBytes(ProgramId);

            page pageInfo = new page();
            pageInfo.pageId = 0;    //页面 id,同一个节目中,页面 id 唯一
            pageInfo.resolution = "1920X1080";  //页面的分辨率,一般与设备一致 如”1024X768”        ********************************可能会改 默认"1920X1080"
            pageInfo.bgColor = "#000000";  //背景颜色,16 进制的 RRGGBB 如:“#000000”
            string strBgFileName = "";//本地的一个图片文件
            pageInfo.bgImg = strBgFileName;  //背景图片, 图片所在的本地路径,可为空表示不加载图片
            pageInfo.eqType = "Q5";  //设备类型 可为””                                            ********************************可能会改 默认"Q5"
            pageInfo.pageTime = 20;  //页面播放停留时间,单位秒(s).单页面时,该值无效,最小 10
            pageInfo.guid = Guid.NewGuid().ToString();  //页面标识,新建页面,建议重新生成该标识

            string strPageInfo = JsonExtension.ToJSON(pageInfo);
            byte[] byPageInfo = System.Text.Encoding.UTF8.GetBytes(strPageInfo);

            int nRet1 = ListenDll.LV_AddPageToProgram(byScreenName1, byProgramId1, byPageInfo);
            //MessageBox.Show(nRet1 == 0 ? "添加页面成功\r\nAdd page successfully" : "添加页面失败\r\nAdd page failed");
            if (nRet1 != 0)
            {
                XGetLastError();
            }

            //Thread.Sleep(2000);


            //-----------------------------------------添加字幕-----------------------------------------part2
            int High = 25; //区域高度
            int UpSpace = 5; //第一个区域上面余留的高度
            int count = 5; //区域数量  （有几个区域）
            if (bBigFont)  //大字体区域大小参数
            {
                High = 50;
                UpSpace = 20;
                count = 2;
            }
            //循环设置每个区域
            for (int i = 0; i < count && i < Lstring.Count; i++)
            {
                TextArea textArea = new TextArea();
                textArea.id = 2 + i;  //区域 id(节目文件中,区域 ID 唯一)            ********************************可能会改 默认为2 id一样还是不一样？
                textArea.type = 2; //区域素材类型，对应的类型为:2 
                textArea.borderEffect = 0; //区域边框特效 0:静止 1:旋转 2:闪烁
                textArea.borderSW = 0;  //区域边框开关 1:开 0:关
                textArea.borderSpeed = 0;  //区域边框速度 0~4
                textArea.borderType = 0;  //区域边框类型 0~9

                //大字体区域大小
                if (bBigFont)
                {
                    textArea.left = 0;                  //区域 x 坐标点            ********************************可能会改
                    textArea.top = i * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                    textArea.width = 256;               //区域宽度                 ********************************可能会改 看点数
                    textArea.height = High;             //区域高度                 ********************************可能会改
                }
                //默认区域大小
                else
                {
                    textArea.left = 0;                  //区域 x 坐标点            ********************************可能会改
                    textArea.top = i * High + UpSpace;  //区域 y 坐标点            ********************************可能会改
                    textArea.width = 256;               //区域宽度                 ********************************可能会改 看点数
                    textArea.height = High;             //区域高度                 ********************************可能会改
                }

                //根据溜放方式更改字体颜色
                textArea.background = "#000000"; //背景颜色,如:”#000000”,16 进制的 RRGGBB 
                //textArea.fontColor = "#ff0000";  //文字颜色, 如:”#ffffff”,16 进制的 RRGGBB
                if (Lstring[i].Contains("-"))
                {
                    textArea.fontColor = "#00ff00";  //绿色
                }
                else if (Lstring[i].Contains("D"))
                {
                    textArea.fontColor = "#ffff00";  //黄色
                }
                else
                {
                    textArea.fontColor = "#ff0000";  //红色
                }
                textArea.fontFamily = "黑体";  //文字字体,如”宋体”  原：宋体
                textArea.fontSize = 21;  //字体大小 12~128 ,单位 px   原：36
                textArea.italic = 0;  //是否斜体 1:是 0:否
                textArea.bold = 0;  //是否粗体 1:是 0:否
                textArea.textLine = 0;  //文字线 0:无 1:下划线 2:删除线
                textArea.lineHeight = 12; //行间距,多行时有效                     ********************************一个区域只有一行 应该不用设置  默认12 
                textArea.pauseTime = 255;  //停留时间,1~255      原5
                textArea.scrollSpeed = 14; //字幕滚动速度 0~14
                textArea.siderType = 0;  //滚动方式  0为静止
                textArea.textAlign = 0;  //对齐方式 0:左,1:中间 2:右              ********************************
                textArea.colorEffect = 0; //颜色特效 0:无 1:水平渐变 2:垂直渐变 3:斜角渐变
                textArea.textTop = 1;  //文字是否垂直方向置顶 1:是 0:否  （区域内）
                textArea.rotation = 0;  //多行显示时有效;  旋转角度 0: 0° 1:90° 2:180° 3: 270°
                textArea.text = Lstring[i] as string;  //字幕内容,不允许为空.UTF - 8 编码
                textArea.textShow = 1;//字幕显示方式 1:单行 0:多行(多行时,自动换行的行首不支持空格显示)

                textArea.zIndex = 1; //层级
                string strTextjson = JsonExtension.ToJSON(textArea);//对象转为Json

                byte[] byTextInfo = System.Text.Encoding.UTF8.GetBytes(strTextjson);

                byte[] byScreenName2 = System.Text.Encoding.UTF8.GetBytes(P1Name);
                byte[] byProgramId2 = System.Text.Encoding.UTF8.GetBytes(ProgramId);
                int nRet2 = ListenDll.LV_AddArea(byScreenName2, byProgramId2, 0, byTextInfo);  //原第三个参数为0  ************可能会改
                //MessageBox.Show(nRet2 == 0 ? "添加字幕成功\r\n Text added successfully" : "添加字幕失败\r\nText added failed");
                if (nRet2 != 0)
                {
                    XGetLastError();
                }
            }

            //Thread.Sleep(2000);

            //-----------------------------------------发送节目-----------------------------------------part3
            string ProgramName = "test_program";
            byte[] byScreenName3 = System.Text.Encoding.UTF8.GetBytes(P1Name);
            byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P1IP);
            byte[] byProgramId3 = System.Text.Encoding.UTF8.GetBytes(ProgramId);
            byte[] byProgramName3 = System.Text.Encoding.UTF8.GetBytes(ProgramName);

            byte[] byPassword = System.Text.Encoding.UTF8.GetBytes("");

            int nRet3 = ListenDll.LV_SendProgramToDevice(byScreenName3, byIp, byProgramId3, byProgramName3, byPassword);
            MessageBox.Show(nRet3 == 0 ? "发送节目成功\r\nSend the program successfully" : "发送节目失败\r\nSend the program Failed");
            if (nRet3 != 0)
            {
                XGetLastError();
            }

            //-----------------------------------------清理缓存-----------------------------------------part4
            int nRet4 = ListenDll.LV_CleanProgramTemporary();
            MessageBox.Show(nRet4 == 0 ? "清理缓存成功\r\nSuccess" : "清理缓存失败\r\nFail");
            if (nRet4 != 0)
            {
                XGetLastError();
            }

            //---------------------------------------发送节目计划---------------------------------------part5
            List<ProgramPlan> listPlan = new List<ProgramPlan>();

            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

            ProgramPlan plan1 = new ProgramPlan();
            plan1.createDate = Convert.ToInt64(ts.TotalMilliseconds);  //节目创建时间,13 位时间戳
            plan1.programId = ProgramId;
            plan1.programName = ProgramName;
            plan1.programType = 1;  //节目类型 1:轮播节目(该类型节目最多只能1 个) 2:插播节目 3:定时节目 。播放的优先级为 插播> 定时 > 轮播
            plan1.updateDate = Convert.ToInt64(ts.TotalMilliseconds); //节目更新时间,13 位时间戳
            plan1.date = new List<ProgramPlan.Date>();  //定时和插播节目中的日期段设置,轮播节目该值的元素为空集
            plan1.planWeekDay = new List<int>();
            plan1.time = new List<ProgramPlan.Time>();
            plan1.isDefault = 0;  //是否为默认节目,仅支持将轮播节目设置成默认节目 1:是, 0:否。

            listPlan.Add(plan1);


            string strPlanJson = JsonExtension.ToJSON(listPlan);//对象转为Json
            byte[] byPlanInfo = System.Text.Encoding.UTF8.GetBytes(strPlanJson);
            byte[] byIp5 = System.Text.Encoding.UTF8.GetBytes(P1IP);
            string strDefVideo = "";//如果是默认节目,同时默认节目中存在视频文件,该字符串才不为空字符串
            byte[] byDefVideo = System.Text.Encoding.UTF8.GetBytes(strDefVideo);

            int nRet5 = ListenDll.LV_SendProgramPlan(byIp5, byPlanInfo, byDefVideo);
            MessageBox.Show(nRet5 == 0 ? "发送节目计划成功\r\nSuccessfully sent the program plan" : "发送节目计划失败\r\nFailed to send program plan");
            if (nRet5 != 0)
            {
                XGetLastError();
            }
        }

        /// <summary>
        /// X4卡demo源码，用来测试联通
        /// </summary>
        private void X4Test()
        {
            string ProgramID = "1111111";
            btnClearPrograms_Click();
            btnAddPage_Click();
            btnAddText_Click();
            btnSendProgram_Click();
            btnSendPlan_Click();
            //创建屏幕
            void btnClearPrograms_Click()
            {
                byte[] byScreenName = System.Text.Encoding.UTF8.GetBytes(P1Name);

                int nRet = ListenDll.LV_CreateScreen(byScreenName);
                MessageBox.Show(nRet == 0 ? "创建屏幕成功\r\nCreate screen successfully" : "创建屏幕失败\r\nCreate screen failed");
                if (nRet != 0)
                {
                    XGetLastError();
                }
            }
            //添加页面
            void btnAddPage_Click()
            {

                byte[] byScreenName = System.Text.Encoding.UTF8.GetBytes(P1Name);
                byte[] byProgramId = System.Text.Encoding.UTF8.GetBytes(ProgramID);

                page pageInfo = new page();
                pageInfo.pageId = 0;
                pageInfo.resolution = "1920X1080";
                pageInfo.bgColor = "#000000";
                string strBgFileName = "";//本地的一个图片文件
                pageInfo.bgImg = null;
                pageInfo.eqType = "Q5";
                pageInfo.pageTime = 20;
                pageInfo.guid = Guid.NewGuid().ToString();

                string strPageInfo = JsonExtension.ToJSON(pageInfo);
                byte[] byPageInfo = System.Text.Encoding.UTF8.GetBytes(strPageInfo);

                int nRet = ListenDll.LV_AddPageToProgram(byScreenName, byProgramId, byPageInfo);
                MessageBox.Show(nRet == 0 ? "添加页面成功\r\nAdd page successfully" : "添加页面失败\r\nAdd page failed");
                if (nRet != 0)
                {
                    XGetLastError();
                }
            }
            //添加字幕
            void btnAddText_Click()
            {
                TextArea textArea = new TextArea();

                textArea.id = 2;
                textArea.type = 2;
                textArea.borderEffect = 0;
                textArea.borderSW = 0;
                textArea.borderSpeed = 0;
                textArea.borderType = 0;
                textArea.left = 0;
                textArea.top = 0;
                textArea.width = 256;
                textArea.height = 64;

                textArea.background = "#000000";
                textArea.fontColor = "#00ff00";
                textArea.fontFamily = "黑体";
                textArea.fontSize = 21;
                textArea.italic = 0;
                textArea.bold = 0;
                textArea.textLine = 0;
                textArea.lineHeight = 12;
                textArea.pauseTime = 5;
                textArea.scrollSpeed = 10;
                textArea.siderType = 0;
                textArea.textAlign = 0;
                textArea.colorEffect = 0;
                textArea.textTop = 1;
                textArea.rotation = 0;
                textArea.text = "测试测试102223";
                textArea.textShow = 0;//0: 多行 1:单行

                textArea.zIndex = 1;
                string strTextjson = JsonExtension.ToJSON(textArea);//对象转为Json

                byte[] byTextInfo = System.Text.Encoding.UTF8.GetBytes(strTextjson);


                byte[] byScreenName = System.Text.Encoding.UTF8.GetBytes(P1Name);
                byte[] byProgramId = System.Text.Encoding.UTF8.GetBytes(ProgramID);

                int nRet = ListenDll.LV_AddArea(byScreenName, byProgramId, 0, byTextInfo);
                MessageBox.Show(nRet == 0 ? "添加字幕成功\r\n Text added successfully" : "添加字幕失败\r\nText added failed");
                if (nRet != 0)
                {
                    XGetLastError();
                }
            }
            //发送节目
            void btnSendProgram_Click()
            {
                string ProgramName = "test_program";
                byte[] byScreenName = System.Text.Encoding.UTF8.GetBytes(P1Name);
                byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P1IP);
                byte[] byProgramId = System.Text.Encoding.UTF8.GetBytes(ProgramID);
                byte[] byProgramName = System.Text.Encoding.UTF8.GetBytes(ProgramName);

                byte[] byPassword = System.Text.Encoding.UTF8.GetBytes("");

                int nRet = ListenDll.LV_SendProgramToDevice(byScreenName, byIp, byProgramId, byProgramName, byPassword);
                MessageBox.Show(nRet == 0 ? "发送节目成功\r\nSend the program successfully" : "发送节目失败\r\nSend the program Failed");
                if (nRet != 0)
                {
                    XGetLastError();
                }
            }
            //发送节目计划
            void btnSendPlan_Click()
            {

                List<ProgramPlan> listPlan = new List<ProgramPlan>();

                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

                ProgramPlan plan1 = new ProgramPlan();
                plan1.createDate = Convert.ToInt64(ts.TotalMilliseconds);
                plan1.programId = ProgramID;
                plan1.programName = "test_program";
                plan1.programType = 1;
                plan1.updateDate = Convert.ToInt64(ts.TotalMilliseconds);
                plan1.date = new List<ProgramPlan.Date>();
                plan1.planWeekDay = new List<int>();
                plan1.time = new List<ProgramPlan.Time>();
                plan1.isDefault = 0;

                listPlan.Add(plan1);


                string strPlanJson = JsonExtension.ToJSON(listPlan);//对象转为Json
                byte[] byPlanInfo = System.Text.Encoding.UTF8.GetBytes(strPlanJson);
                byte[] byIp = System.Text.Encoding.UTF8.GetBytes(P1IP);
                string strDefVideo = "";//如果是默认节目,同时默认节目中存在视频文件,该字符串才不为空字符串
                byte[] byDefVideo = System.Text.Encoding.UTF8.GetBytes(strDefVideo);

                int nRet = ListenDll.LV_SendProgramPlan(byIp, byPlanInfo, byDefVideo);
                MessageBox.Show(nRet == 0 ? "发送节目计划成功\r\nSuccessfully sent the program plan" : "发送节目计划失败\r\nFailed to send program plan");
                if (nRet != 0)
                {
                    XGetLastError();
                }
            }
        }

        /// <summary>
        /// X4卡获取最近一次失败操作的错误信息
        /// </summary>
        private static string XGetLastError()
        {
            byte[] byErrInfo = new byte[1024];
            int nRet = ListenDll.LV_GetLastErrInfo(byErrInfo, 1024);
            if (nRet > 0)
            {
                string strTitle = Convert.ToString(nRet);
                string strMsg = System.Text.Encoding.UTF8.GetString(byErrInfo);
                //MessageBox.Show("发送错误：" + strMsg, "错误码：" + strTitle);
                return strMsg.Trim().Replace("\0", "") + "(" + strTitle.Trim() + ")";
            }
            else
            {
                string strTitle = Convert.ToString(nRet);
                return "当前 dll 的错误列表记录为 0 " + "(" + strTitle + ")";
            }
        }

        /// <summary>
        /// 清除X卡设备存储(该功能会导致设备重启,并且设备开始播放默认节目,注意重新搜索设备,IP 不变的情况不需要搜索)
        /// </summary>
        public static void XClearDeviceSpace()
        {
            try
            {
                if (StationScreen == 0 || StationScreen == 1)
                {
                    int nRet = ListenDll.LV_ClearDeviceSpace(System.Text.Encoding.UTF8.GetBytes(P1IP));
                    if (nRet != 0)
                    {
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "LV_ClearDeviceSpace(P1IP)错误：" + XGetLastError());
                    }
                    else
                    {
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "LV_ClearDeviceSpace(P1IP)成功，已清除P1设备存储");
                    }
                }
                if (StationScreen == 0 || StationScreen == 2)
                {
                    int nRet = ListenDll.LV_ClearDeviceSpace(System.Text.Encoding.UTF8.GetBytes(P2IP));
                    if (nRet != 0)
                    {
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "LV_ClearDeviceSpace(P2IP)错误：" + XGetLastError());
                    }
                    else
                    {
                        Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "LV_ClearDeviceSpace(P2IP)成功，已清除P2设备存储");
                    }
                }
                MessageBox.Show("已清空设备存储，该功能会导致设备重启\r\n请关闭并重启此程序");
            }
            catch (Exception ex)
            {
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "XClearDeviceSpace()错误：" + ex.Message);
            }
        }

        private void TGPDisp_FormClosing(object sender, FormClosingEventArgs e)
        {
            Quit quit = new Quit();
            quit.StartPosition = FormStartPosition.CenterParent;
            quit.ShowDialog(this);
            if (quit.select == Quit.cancel)
            {
                e.Cancel = true;
            }
            else if (quit.select == Quit.hide)
            {
                this.WindowState = FormWindowState.Minimized;
                e.Cancel = true;
            }
            ListenDll.LV_ReleaseDllEx(); // X4卡dll资源释放
        }

        static string toChinese(int num)
        {
            string sb = "";
            string[] unit = { "", "十", "百", "千" };
            char[] chineseNum = { '零', '一', '二', '三', '四', '五', '六', '七', '八', '九' };
            string chars = num.ToString();
            int a = chars.Length;
            int count = 0;

            if (a == 2 && chars[0] == '1')//如12，13
            {
                sb += unit[1];
                if (chars[1] != '0')//10，20
                {
                    // 将字符转为对应的中文 例如： '1' -> 一 ;
                    sb += chineseNum[chars[1] - 48];
                }
            }
            else if (a <= 4)//三到四位
            {
                for (int i = 0; i < chars.Length; i++)//1011，1010
                {
                    if (chars[i] == '0')
                    {
                        count++;
                        continue;
                    }
                    if (count >= 1)//有零
                    {
                        sb += chineseNum[0];
                        count = -4;//防止多个零叠在一起
                    }
                    sb += chineseNum[chars[i] - 48];
                    sb += unit[a - 1 - i];//权重
                }
            }
            else if (a <= 8)
            {
                //截取
                int index = chars.Length - 4;//万的部分
                string wan = chars.Substring(0, chars.Length - 4);
                string qita = chars.Substring(chars.Length - 4, 4);
                if (chars[a - 4] == '0')
                {
                    sb = toChinese(int.Parse(wan)) + "万" + "零" + toChinese(int.Parse(qita));
                }
                else
                {
                    sb = toChinese(int.Parse(wan)) + "万" + toChinese(int.Parse(qita));
                }
            }
            else
            {
                sb = "超出范围！";
            }
            return sb;
        }
    }


    public class DataOfSend
    {
        public List<string> Info;   // 被发送的语音信息  对应SendData函数里的参数类型List<string>
        public bool BigFont;        // 是否大字体 同样对应SendData函数里的参数
        public string SpeechStr;    //  语音内容
        public bool SpeechState;     //  语音是否被开启

        //两种构造函数
        //没有语音参数的SpeechState为false
        public DataOfSend(List<string> info, bool bBigFont)
        {
            Info = info;
            BigFont = bBigFont;
            this.SpeechState = false;
        }
        //有语音参数的SpeechState为true
        public DataOfSend(List<string> info, bool bBigFont, string speechStr)
        {
            Info = info;
            BigFont = bBigFont;
            SpeechStr = speechStr;
            this.SpeechState = true;
        }


    }

    /// <summary>
    /// 调车单类
    /// </summary>
    public class DCDInfo
    {
        public byte[] Info;
        public string HumpNumberStr;//峰位
        public string TrainNumberStr; //车次
        public string HumpSigStr; //驼峰主体信号
        public string FirstDCOnStr; //第一分路道岔占用

        public string[] HookNumStr = new string[5];//勾序
        public string[] YardNumStr = new string[5];//场别
        public string[] GDNumberStr = new string[5];//股道号
        public string[] DoMarkStr = new string[5];//作业符
        public string[] CarNumStr = new string[5];//辆数
        public string[] CarStateStr = new string[5];//状态
        public string[] OpenNumberStr = new string[5];//状态

        public DCDInfo(byte[] info)
        {
            Info = info;
            //MessageBox.Show(Info.Length.ToString());  
        }
        public void InfoPross()
        {

            byte[] tempByte;
            string tempStr;

            tempByte = new Byte[100];
            tempByte[0] = Info[1];
            tempStr = System.Text.Encoding.Default.GetString(tempByte);
            HumpNumberStr = tempStr.Replace("\0", "");//峰位

            tempByte = new Byte[100];
            for (int j = 2; j < 14; j++)
            {
                tempByte[j - 2] = Info[j];
            }
            tempStr = System.Text.Encoding.Default.GetString(tempByte);
            TrainNumberStr = tempStr.Replace("\0", "");//车次
            TrainNumberStr = TrainNumberStr.Trim();
            //int TrainNumber; //
            //TrainNumber = Convert.ToInt32(tempStr);//字符串转换为数字
            //TrainNumberStr = TrainNumber.ToString("D7");//格式化字符串
            //MessageBox.Show("TrainNumberStr=" + TrainNumberStr);

            tempByte = new Byte[100];
            tempByte[0] = Info[14];
            tempStr = System.Text.Encoding.Default.GetString(tempByte);
            HumpSigStr = tempStr.Replace("\0", "");////驼峰主体信号

            if (HumpSigStr.CompareTo("0") == 0) HumpSigStr = "红灯";
            if (HumpSigStr.CompareTo("1") == 0) HumpSigStr = "白灯";
            if (HumpSigStr.CompareTo("2") == 0) HumpSigStr = "黄闪";
            if (HumpSigStr.CompareTo("3") == 0) HumpSigStr = "绿灯";
            if (HumpSigStr.CompareTo("4") == 0) HumpSigStr = "白闪";
            if (HumpSigStr.CompareTo("5") == 0) HumpSigStr = "红闪";
            if (HumpSigStr.CompareTo("6") == 0) HumpSigStr = "绿闪";
            if (HumpSigStr.CompareTo("8") == 0) HumpSigStr = "黄灯";
            //MessageBox.Show("HumpSigStr=" + HumpSigStr);

            tempByte = new Byte[100];
            tempByte[0] = Info[15];
            tempStr = System.Text.Encoding.Default.GetString(tempByte);
            FirstDCOnStr = tempStr.Replace("\0", "");//第一分路道岔占用
            if (FirstDCOnStr.CompareTo("0") == 0) FirstDCOnStr = "压入";
            if (FirstDCOnStr.CompareTo("1") == 0) FirstDCOnStr = "未压入";
            //MessageBox.Show("FirstDCOnStr=" + FirstDCOnStr);

            int InfoByteNumber = 0;
            int BeginByteNumber = 16;  //勾序信息开始数组字节号
            for (int i = 0; i < 5; i++)
            {
                tempByte = new Byte[100];
                InfoByteNumber = 0;
                for (int j = 0; j < 3; j++) //3为本项内容字节数,以下含义相同
                {
                    tempByte[j] = Info[i * 18 + BeginByteNumber + InfoByteNumber + j]; //18为每勾数据长度，以下相同
                }
                tempStr = System.Text.Encoding.Default.GetString(tempByte);
                //MessageBox.Show(tempStr);
                int HookNum; //勾序
                //HookNum = Convert.ToInt32(tempStr.Replace("\0", ""));//字符串转换为数字
                bool h = Int32.TryParse(tempStr.Replace("\0", ""), out HookNum);
                if (h) HookNumStr[i] = HookNum.ToString("D2");//格式化字符串
                else HookNumStr[i] = "  ";
                //MessageBox.Show("HookNum=" + HookNumStr[i]);


                tempByte = new Byte[100];
                InfoByteNumber += 3;
                for (int j = 0; j < 2; j++)
                {
                    tempByte[j] = Info[i * 18 + BeginByteNumber + InfoByteNumber + j];
                }
                tempStr = System.Text.Encoding.Default.GetString(tempByte);
                YardNumStr[i] = tempStr.Replace("\0", ""); //场别
                if (YardNumStr[i].Length < 2) YardNumStr[i] = " " + YardNumStr[i];
                if (YardNumStr[i] == "  ") YardNumStr[i] = "本场";
                //MessageBox.Show("YardNumStr="+YardNumStr[i]);


                tempByte = new Byte[100];
                InfoByteNumber += 2;
                for (int j = 0; j < 2; j++)
                {
                    tempByte[j] = Info[i * 18 + BeginByteNumber + InfoByteNumber + j];
                }
                tempStr = System.Text.Encoding.Default.GetString(tempByte);
                GDNumberStr[i] = tempStr.Replace("\0", ""); //股道号
                if (GDNumberStr[i].Trim().Length < 2) GDNumberStr[i] = "0" + GDNumberStr[i].Trim();
                //MessageBox.Show("GDdNumberStr="+GDNumberStr[i]);

                tempByte = new Byte[100];
                InfoByteNumber += 2;
                for (int j = 0; j < 1; j++)
                {
                    tempByte[j] = Info[i * 18 + BeginByteNumber + InfoByteNumber + j];
                }
                tempStr = System.Text.Encoding.Default.GetString(tempByte);
                //DoMarkStr[i] = tempStr.Replace("\0", ""); //作业符
                //作业符换成客户需求的符号（注意替换先后顺序！）
                DoMarkStr[i] = tempStr.Replace("\0", "")
                    .Replace("X", TGPDisp.DoMarkXia)
                    .Replace("-", TGPDisp.DoMarkLiu)
                    .Replace("D", TGPDisp.DoMarkDan)
                    .Replace("+", TGPDisp.DoMarkGua)
                    .Replace("#", TGPDisp.DoMarkJin)
                    .Replace("S", TGPDisp.DomarkShang);
                //MessageBox.Show("DoMarkStr="+DoMarkStr[i]);    


                tempByte = new Byte[100];
                InfoByteNumber += 1;
                for (int j = 0; j < 2; j++)
                {
                    tempByte[j] = Info[i * 18 + BeginByteNumber + InfoByteNumber + j];
                }
                tempStr = System.Text.Encoding.Default.GetString(tempByte);
                CarNumStr[i] = tempStr.Replace("\0", ""); //辆数
                if (CarNumStr[i].Trim().Length < 2) CarNumStr[i] = "0" + CarNumStr[i].Trim();
                //MessageBox.Show("CarNumStr="+CarNumStr[i]);

                tempByte = new Byte[100];
                InfoByteNumber += 2;
                for (int j = 0; j < 1; j++)
                {
                    tempByte[j] = Info[i * 18 + BeginByteNumber + InfoByteNumber + j];
                }
                tempStr = System.Text.Encoding.Default.GetString(tempByte);
                CarStateStr[i] = tempStr.Replace("\0", ""); //状态
                //MessageBox.Show("CarStateStr="+CarStateStr[i]);    


                tempByte = new Byte[100];
                InfoByteNumber += 1;
                for (int j = 0; j < 7; j++)
                {
                    tempByte[j] = Info[i * 18 + BeginByteNumber + InfoByteNumber + j];
                }
                tempStr = System.Text.Encoding.Default.GetString(tempByte);
                OpenNumberStr[i] = tempStr.Replace("\0", ""); //七位开口号
                //MessageBox.Show("OpenNumberStr=" + OpenNumberStr[i]);
                //InfoByteNumber += 8;

            }//i勾序数=5勾

        }



        // CRC计算 
        public UInt16 GenerateCrcCode(byte[] pData, int DataLength)
        {

            // CRC余式表
            UInt16[] crc_ta =
            {
        0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7,
        0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef,
        0x1231, 0x0210, 0x3273, 0x2252, 0x52b5, 0x4294, 0x72f7, 0x62d6,
        0x9339, 0x8318, 0xb37b, 0xa35a, 0xd3bd, 0xc39c, 0xf3ff, 0xe3de,
        0x2462, 0x3443, 0x0420, 0x1401, 0x64e6, 0x74c7, 0x44a4, 0x5485,
        0xa56a, 0xb54b, 0x8528, 0x9509, 0xe5ee, 0xf5cf, 0xc5ac, 0xd58d,
        0x3653, 0x2672, 0x1611, 0x0630, 0x76d7, 0x66f6, 0x5695, 0x46b4,
        0xb75b, 0xa77a, 0x9719, 0x8738, 0xf7df, 0xe7fe, 0xd79d, 0xc7bc,
        0x48c4, 0x58e5, 0x6886, 0x78a7, 0x0840, 0x1861, 0x2802, 0x3823,
        0xc9cc, 0xd9ed, 0xe98e, 0xf9af, 0x8948, 0x9969, 0xa90a, 0xb92b,
        0x5af5, 0x4ad4, 0x7ab7, 0x6a96, 0x1a71, 0x0a50, 0x3a33, 0x2a12,
        0xdbfd, 0xcbdc, 0xfbbf, 0xeb9e, 0x9b79, 0x8b58, 0xbb3b, 0xab1a,
        0x6ca6, 0x7c87, 0x4ce4, 0x5cc5, 0x2c22, 0x3c03, 0x0c60, 0x1c41,
        0xedae, 0xfd8f, 0xcdec, 0xddcd, 0xad2a, 0xbd0b, 0x8d68, 0x9d49,
        0x7e97, 0x6eb6, 0x5ed5, 0x4ef4, 0x3e13, 0x2e32, 0x1e51, 0x0e70,
        0xff9f, 0xefbe, 0xdfdd, 0xcffc, 0xbf1b, 0xaf3a, 0x9f59, 0x8f78,
        0x9188, 0x81a9, 0xb1ca, 0xa1eb, 0xd10c, 0xc12d, 0xf14e, 0xe16f,
        0x1080, 0x00a1, 0x30c2, 0x20e3, 0x5004, 0x4025, 0x7046, 0x6067,
        0x83b9, 0x9398, 0xa3fb, 0xb3da, 0xc33d, 0xd31c, 0xe37f, 0xf35e,
        0x02b1, 0x1290, 0x22f3, 0x32d2, 0x4235, 0x5214, 0x6277, 0x7256,
        0xb5ea, 0xa5cb, 0x95a8, 0x8589, 0xf56e, 0xe54f, 0xd52c, 0xc50d,
        0x34e2, 0x24c3, 0x14a0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
        0xa7db, 0xb7fa, 0x8799, 0x97b8, 0xe75f, 0xf77e, 0xc71d, 0xd73c,
        0x26d3, 0x36f2, 0x0691, 0x16b0, 0x6657, 0x7676, 0x4615, 0x5634,
        0xd94c, 0xc96d, 0xf90e, 0xe92f, 0x99c8, 0x89e9, 0xb98a, 0xa9ab,
        0x5844, 0x4865, 0x7806, 0x6827, 0x18c0, 0x08e1, 0x3882, 0x28a3,
        0xcb7d, 0xdb5c, 0xeb3f, 0xfb1e, 0x8bf9, 0x9bd8, 0xabbb, 0xbb9a,
        0x4a75, 0x5a54, 0x6a37, 0x7a16, 0x0af1, 0x1ad0, 0x2ab3, 0x3a92,
        0xfd2e, 0xed0f, 0xdd6c, 0xcd4d, 0xbdaa, 0xad8b, 0x9de8, 0x8dc9,
        0x7c26, 0x6c07, 0x5c64, 0x4c45, 0x3ca2, 0x2c83, 0x1ce0, 0x0cc1,
        0xef1f, 0xff3e, 0xcf5d, 0xdf7c, 0xaf9b, 0xbfba, 0x8fd9, 0x9ff8,
        0x6e17, 0x7e36, 0x4e55, 0x5e74, 0x2e93, 0x3eb2, 0x0ed1, 0x1ef0
        };

            int i;
            UInt16 result = (UInt16)0U;

            if (pData.Length != 0)
            {
                for (i = 0; i < DataLength; i++)
                {
                    result = (UInt16)((result << (UInt16)8U) ^ crc_ta[(result >> 8) ^ (pData[i])]);
                }

            }
            return result;

        }//CRC 


    }//DCDInfo

    public class TCPMethod
    {
        
    }
        

}



