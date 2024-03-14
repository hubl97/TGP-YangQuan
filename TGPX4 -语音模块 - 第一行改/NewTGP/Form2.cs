using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace NewTGP
{
    public partial class TGPConfig : Form
    {


        //private string passWord = "TGPSET";
        public double Longti, Lati;
        public string DisplayOption = "Time"; //默认初始化空闲时显示时间
        public string InputLongti = "交通强国";
        public string InputLati = "铁路先行";

        public bool SpeakState = false;    //语音开关：开启true；关闭false
        public int SpeakContent = 1;      //播报内容：完整1；简短2 
        public int SpeakNum = 1;          //播报次数
        public bool bWriteLog = true;
        public PRC_Tool.SafeQueue LbQueue = new PRC_Tool.SafeQueue();
        public TGPConfig()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
            textBox1.ReadOnly = false;
            textBox2.ReadOnly = false;
            radioButton1.Checked = true; //默认勾选显示时间选项
            comboBox1.SelectedIndex = 0;//默认开启语音
            comboBox2.SelectedIndex = 0;//默认完整播报
            comboBox3.SelectedIndex = 0;//默认播报一次
            radioButton5.Checked = true;//默认勾选默认亮度
            trackBar2.Value = 10;
            ReadConfigFile();
            comboBox2.Enabled = true; //语音按钮  站场不使用时关闭
            button1.Enabled = false;

        }

        

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
          
        }

        private void TGPConfig_Load(object sender, EventArgs e)
        {
            //comboBox1.Items.Add("单次");
            //comboBox1.Items.Add("重复");
            try
            {
                ReadConfigFile();
                textBox3.Text = InputLongti;
                textBox4.Text = InputLati;
            }
            catch
            {
            }
            
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            WriteConfigFile();
        }
        private void WriteConfigFile()
        {
            try
            {
                string config = Longti.ToString() + "," +
                    Lati.ToString() + "," +
                    DisplayOption.ToString() + "," +
                    InputLongti.ToString() + "," +
                    InputLati.ToString() + "," +
                    SpeakState.ToString() + "," +
                    SpeakContent.ToString() + "," +
                    SpeakNum.ToString() + "," +
                    radioButton1.Checked.ToString() + "," +
                    comboBox1.SelectedIndex.ToString() + "," +
                    comboBox2.SelectedIndex.ToString() + "," +
                    comboBox3.SelectedIndex.ToString() + "," +
                    radioButton1.Checked.ToString() + "," +
                    radioButton2.Checked.ToString() + "," +
                    radioButton3.Checked.ToString() + "," +
                    radioButton4.Checked.ToString() + "," +
                    radioButton5.Checked.ToString() + "," +
                    radioButton6.Checked.ToString() + "," +
                    textBox1.Text.ToString() + "," +
                    textBox2.Text.ToString() + "," +
                    trackBar1.Value.ToString() + "," +
                    trackBar2.Value.ToString() + "," + "0";
                string path = Application.StartupPath;
                //StreamWriter sw = File.AppendText(path + @"\" +"postion.txt");
                StreamWriter sw = new StreamWriter(path + @"\" + "config.txt", false);

                sw.WriteLine(config);
                sw.Close();
            }
            catch
            {
            }
        }
        private void ReadConfigFile()
        {
            string path = Application.StartupPath + @"\" + "config.txt";
            if (File.Exists(path))
            {
                try
                {
                    StreamReader rd = new StreamReader(path, Encoding.UTF8);
                    string str = rd.ReadToEnd();
                    string[] config = str.Split(new char[] { '\u0009', '\u0020', ',' });

                    Longti = Convert.ToDouble(config[0]); textBox3.Text = Longti.ToString();
                    Lati = Convert.ToDouble(config[1]); textBox4.Text = Lati.ToString();
                    DisplayOption = config[2];
                    InputLongti = config[3];
                    InputLati = config[4];
                    SpeakState = Convert.ToBoolean(config[5]);
                    SpeakContent = Convert.ToInt32(config[6]);
                    SpeakNum = Convert.ToInt32(config[7]);
                    radioButton1.Checked = Convert.ToBoolean(config[8]);
                    comboBox1.SelectedIndex = Convert.ToInt32(config[9]);
                    comboBox2.SelectedIndex = Convert.ToInt32(config[10]);
                    comboBox3.SelectedIndex = Convert.ToInt32(config[11]);
                    radioButton1.Checked = Convert.ToBoolean(config[12]);
                    radioButton2.Checked = Convert.ToBoolean(config[13]);
                    radioButton3.Checked = Convert.ToBoolean(config[14]);
                    radioButton4.Checked = Convert.ToBoolean(config[15]);
                    radioButton5.Checked = Convert.ToBoolean(config[16]);
                    radioButton6.Checked = Convert.ToBoolean(config[17]);
                    textBox1.Text = config[18];
                    textBox2.Text = config[19];
                    trackBar1.Value = Convert.ToInt32(config[20]);
                    trackBar2.Value = Convert.ToInt32(config[21]);
                    rd.Close();
                   
                }
                catch (Exception e)
                {
                    
                }
            }
            
        }
        ///<summary>
        ///亮度调节
        ///<summary>
        private void button1_Click(object sender, EventArgs e)  //亮度调节应用按钮
        {
            try
            {
                if (radioButton4.Checked == true) //自动调节（根据白天黑夜自动调节亮度）
                {
                    if (textBox3.Text == "" || textBox4.Text == "")
                    {
                        MessageBox.Show("请输入双精度浮点型站点经纬度数值");
                    }
                    else
                    {
                        InputLongti = textBox3.Text;
                        InputLati = textBox4.Text;
                        try
                        {
                            Longti = Convert.ToDouble(InputLongti);
                            Lati = Convert.ToDouble(InputLati);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("程序错误: " + ex.Message + "\n\n\n请重新输入双精度浮点型经纬度数值");
                        }
                        if (Longti < 0 || Longti > 180 || Lati < 0 || Lati > 90)
                        {
                            MessageBox.Show("提示：经纬度超出合理范围，可能会导致结果误差");
                        }

                        SunTimeResult sunTimeResult = SunTimes.GetSunTime(DateTime.Now.Date, Longti, Lati);
                        DateTime sunrise = sunTimeResult.SunriseTime; //日出日期及时间
                        DateTime sunset = sunTimeResult.SunsetTime;  //日落日期及时间
                        textBox6.Text = sunrise.TimeOfDay.ToString();
                        textBox7.Text = sunset.TimeOfDay.ToString();
                        DateTime CurruntDateTime = DateTime.Now; // 当前日期及时间
                        if (CurruntDateTime < sunrise || CurruntDateTime > sunset) //夜晚亮度
                        {
                            if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 1)
                            {
                                BrightnessMethod.SetBrightness(3, bWriteLog);
                            }
                            if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 2)
                            {
                                BrightnessMethod.SetBrightness2(3, bWriteLog);
                            }
                        }
                        if (CurruntDateTime > sunrise && CurruntDateTime < sunset) //白天亮度
                        {
                            if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 1)
                            {
                                BrightnessMethod.SetBrightness(11, bWriteLog);
                            }
                            if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 2)
                            {
                                BrightnessMethod.SetBrightness2(11, bWriteLog);
                            }
                        }
                        //MessageBox.Show("自动调节已开启");
                        listBox1.Items.Add(DateTime.Now.ToString("G") + "  " + "亮度调节：自动模式" + "\n");
                        //LbQueue.Enqueue(DateTime.Now.ToString("G") + "  " + "亮度调节：自动模式" + "\n");
                        Thread AutoBrightnessThread = new Thread(AutoBrightnessThreadMethod);
                        AutoBrightnessThread.Priority = ThreadPriority.Lowest;
                        AutoBrightnessThread.IsBackground = true;
                        AutoBrightnessThread.Start();
                    }

                }

                if (radioButton5.Checked == true)  //默认亮度
                {
                    if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 1)
                    {
                        BrightnessMethod.SetBrightness(7, bWriteLog); //默认亮度
                    }
                    if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 2)
                    {
                        BrightnessMethod.SetBrightness2(7, bWriteLog);
                    }
                    //MessageBox.Show("已切换至默认亮度");
                    listBox1.Items.Add(DateTime.Now.ToString("G") + "  " + "亮度调节：默认亮度" + "\n");
                    //LbQueue.Enqueue(DateTime.Now.ToString("G") + "  " + "亮度调节：默认亮度" + "\n");
                }

                if (radioButton6.Checked == true) //自定义调节
                {
                    if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 1)
                    {
                        BrightnessMethod.SetBrightness(trackBar1.Value, bWriteLog);
                    }
                    if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 2)
                    {
                        BrightnessMethod.SetBrightness2(trackBar1.Value, bWriteLog);
                    }
                    listBox1.Items.Add(DateTime.Now.ToString("G") + "  " + "亮度调节：" + "自定义亮度为 " + trackBar1.Value + "\n");
                    //LbQueue.Enqueue(DateTime.Now.ToString("G") + "  " + "亮度调节：" + "自定义亮度为 " + trackBar1.Value + "\n");

                }
            }
            catch(Exception ex)
            { }
        }

        private void AutoBrightnessThreadMethod() //自动亮度调节更新线程方法
        {
            while(true)
            {
                Thread.Sleep(7200000); //两小时7200000ms更新一次
                Longti = Convert.ToDouble(InputLongti);
                Lati = Convert.ToDouble(InputLati);
                SunTimeResult sunTimeResult = SunTimes.GetSunTime(DateTime.Now.Date, Longti, Lati);
                DateTime sunrise = sunTimeResult.SunriseTime; //日出日期及时间
                DateTime sunset = sunTimeResult.SunsetTime;  //日落日期及时间
                textBox6.Text = sunrise.TimeOfDay.ToString();
                textBox7.Text = sunset.TimeOfDay.ToString();
                DateTime CurruntDateTime = DateTime.Now; // 当前日期及时间
                if (CurruntDateTime < sunrise || CurruntDateTime > sunset) //夜晚亮度
                {
                    if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 1)
                    {
                        BrightnessMethod.SetBrightness(3, bWriteLog);
                    }
                    if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 2)
                    {
                        BrightnessMethod.SetBrightness2(3, bWriteLog);
                    }
                }
                if (CurruntDateTime > sunrise && CurruntDateTime < sunset) //白天亮度
                {
                    if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 1)
                    {
                        BrightnessMethod.SetBrightness(11, bWriteLog);
                    }
                    if (TGPDisp.StationScreen == 0 || TGPDisp.StationScreen == 2)
                    {
                        BrightnessMethod.SetBrightness2(11, bWriteLog);
                    }
                }
                ///MessageBox.Show("自动调节已刷新");
                listBox1.Items.Add(DateTime.Now.ToString("G") + "  " + "亮度调节：自动调节刷新成功" + "\n");
                //LbQueue.Enqueue(DateTime.Now.ToString("G") + "  " + "亮度调节：自动调节刷新成功" + "\n");
            }
           
        }


        /// <summary>
        /// 显示内容设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)  //显示内容应用按钮
        {
            try
            {
                if (radioButton1.Checked == true) //显示时间
                {
                    DisplayOption = "Time";
                    listBox1.Items.Add(DateTime.Now.ToString("G") + "  " + "空闲时显示内容：时间" + "\n");
                    //LbQueue.Enqueue(DateTime.Now.ToString("G") + "  " + "空闲时显示内容：时间" + "\n");
                }
                if (radioButton2.Checked == true) //显示时间和标语
                {
                    textBox1.ReadOnly = false;
                    textBox2.ReadOnly = false;
                    DisplayOption = "TimeSlogan";
                    listBox1.Items.Add(DateTime.Now.ToString("G") + "  " + "空闲时显示内容：时间和标语" + "\n");
                    //LbQueue.Enqueue(DateTime.Now.ToString("G") + "  " + "空闲时显示内容：时间和标语" + "\n");

                }
                if (radioButton3.Checked == true) //灭屏
                {
                    DisplayOption = "Off";
                    listBox1.Items.Add(DateTime.Now.ToString("G") + "  " + "空闲时显示内容：灭屏" + "\n");
                    //LbQueue.Enqueue(DateTime.Now.ToString("G") + "  " + "空闲时显示内容：灭屏" + "\n");
                }
            }
           catch(Exception ex)
            { }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)   ///经度输入限制
        {
            if (e.KeyChar == 0x20) e.KeyChar = (char)0;  //禁止空格键  
            if ((e.KeyChar == 0x2D) && (((TextBox)sender).Text.Length == 0)) return;   //处理负数  
            if (e.KeyChar > 0x20)
            {
                try
                {
                    double.Parse(((TextBox)sender).Text + e.KeyChar.ToString());
                }
                catch
                {
                    e.KeyChar = (char)0;   //处理非法字符  
                }
            }
        }  

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)  ///纬度输入限制
        {
            if (e.KeyChar == 0x20) e.KeyChar = (char)0;  //禁止空格键  
            if ((e.KeyChar == 0x2D) && (((TextBox)sender).Text.Length == 0)) return;   //处理负数  
            if (e.KeyChar > 0x20)
            {
                try
                {
                    double.Parse(((TextBox)sender).Text + e.KeyChar.ToString());
                }
                catch
                {
                    e.KeyChar = (char)0;   //处理非法字符  
                }
            }
        }  

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 语音设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)  //语音设置应用按钮
        {
            if(comboBox2.Text =="开启")
            {
                SpeakState = true;
                //MessageBox.Show("语音功能开启\n当前模式：" + comboBox3.Text + ", " + comboBox1.Text + "播报");
                //Speech_SDK.Speech.ChangeVolume((byte)trackBar2.Value);
                //Byte a = (byte)trackBar2.Value;
                //Speech_SDK.Speech.SpeakSyn("语音开启");
                
                listBox1.Items.Add(DateTime.Now.ToString("G") + "  " + "语音功能开启, 当前模式：" + comboBox3.Text + "内容, " + comboBox1.Text + "播报, " + "当前音量: " + Convert.ToInt32((Convert.ToDouble(trackBar2.Value) / 10) * 100) + "%" + "\n");
                //LbQueue.Enqueue(DateTime.Now.ToString("G") + "  " + "语音功能开启, 当前模式：" + comboBox3.Text + "内容, " + comboBox1.Text + "播报, " + "当前音量: " + Convert.ToInt32((Convert.ToDouble(trackBar2.Value) / 255) * 100) + "%" + "\n");
                
                
            }
            if(comboBox2.Text == "关闭")
            {
                SpeakState = false;
                //MessageBox.Show("语音功能关闭");
                //Speech_SDK.Speech.SpeakSyn("语音关闭");
                listBox1.Items.Add(DateTime.Now.ToString("G") + "  " + "语音功能关闭" + "\n");
                //LbQueue.Enqueue(DateTime.Now.ToString("G") + "  " + "语音功能关闭" + "\n");
            }
            if(comboBox3.Text == "完整")
            {
                SpeakContent = 1;
            }
            if(comboBox3.Text == "简明")
            {
                SpeakContent = 2;
            }
            if(comboBox1.Text == "单次")
            {
                SpeakNum = 1;
            }
            if(comboBox1.Text == "重复")
            {
                SpeakNum = 2;
            }

            if(TGPDisp.StationAudio == 0 || TGPDisp.StationAudio == 1) //语音模块调节音量
            {
                TGPDisp.AudioQueue.Enqueue("[v"+ trackBar2.Value.ToString() +"]");  //例#[v08] 音量调节功能需要在播报内容前加[v05]
            }
            if(TGPDisp.StationAudio == 0 || TGPDisp.StationAudio == 2)
            {
                TGPDisp.AudioQueue2.Enqueue("[v" + trackBar2.Value.ToString() + "]");
            }

        }

        private void buttonClearSpace_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否清空设备存储？\r\n(该功能会导致设备重启,并且设备开始播放默认节目)", 
                "注意", 
                MessageBoxButtons.OKCancel, 
                MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                Thread ClearDeviceThread = new Thread(new ThreadStart(TGPDisp.XClearDeviceSpace));
                ClearDeviceThread.Name = "ClearDeviceThread";
                ClearDeviceThread.IsBackground = true;
                ClearDeviceThread.Start();
            }   
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
        }

        
    }


    




    /// <summary>
    /// 亮度调节方法
    /// </summary>
    public static class BrightnessMethod
    {
        public static void SetBrightness(int BrightnessValue,bool bWriteLog)  
        {
            int nResult;
            LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示
            CommunicationInfo.LEDType = 1;  //串口LEDType选1

            if (TGPDisp.CommunicationType == 1) //网络通信
            {
                CommunicationInfo.LEDType = 3; //网口LEDType选3
                                               //TCP通讯********************************************************************************
                CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                CommunicationInfo.IpStr = TGPDisp.P1IP;//给IpStr赋值LED控制卡的IP
                CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                                                //广播通讯********************************************************************************
                                                //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                                                //串口通讯********************************************************************************
                                                //CommunicationInfo.SendType=2;//串口通讯
                                                //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                                                //CommunicationInfo.Baud=9600;//波特率
                                                //CommunicationInfo.LedNumber=1;
            }
            if (TGPDisp.CommunicationType == 2) //串口
            {
                //串口通讯********************************************************************************
                CommunicationInfo.SendType = 2;//串口通讯
                CommunicationInfo.Commport = TGPDisp.ComP1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                CommunicationInfo.Baud = 115200;//波特率
                CommunicationInfo.LedNumber = 1;
            }


            //LedDll.LV_SetBrightness(ref CommunicationInfo, BrightnessValue);
            nResult = LedDll.LV_SetBrightness(ref CommunicationInfo, BrightnessValue);
            if (nResult != 0)
            {
                string ErrStr;
                ErrStr = LedDll.LS_GetError(nResult);
                MessageBox.Show("屏1亮度调节错误: "+ ErrStr);
                if (bWriteLog)
                    Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏1亮度调节错误：");
                return;
            }
        }
        public static void SetBrightness2(int BrightnessValue,bool bWriteLog)
        {
            int nResult;
            LedDll.COMMUNICATIONINFO CommunicationInfo = new LedDll.COMMUNICATIONINFO();//定义一通讯参数结构体变量用于对设定的LED通讯，具体对此结构体元素赋值说明见COMMUNICATIONINFO结构体定义部份注示
            CommunicationInfo.LEDType = 1;

            if (TGPDisp.CommunicationType == 1) //网络通信
            {
                CommunicationInfo.LEDType = 3;
                //TCP通讯********************************************************************************
                CommunicationInfo.SendType = 0;//设为固定IP通讯模式，即TCP通讯
                CommunicationInfo.IpStr = TGPDisp.P2IP;//给IpStr赋值LED控制卡的IP
                CommunicationInfo.LedNumber = 1;//LED屏号为1，注意socket通讯和232通讯不识别屏号，默认赋1就行了，485必需根据屏的实际屏号进行赋值
                                                //广播通讯********************************************************************************
                                                //CommunicationInfo.SendType=1;//设为单机直连，即广播通讯无需设LED控制器的IP地址
                                                //串口通讯********************************************************************************
                                                //CommunicationInfo.SendType=2;//串口通讯
                                                //CommunicationInfo.Commport=1;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                                                //CommunicationInfo.Baud=9600;//波特率
                                                //CommunicationInfo.LedNumber=1;
            }
            if (TGPDisp.CommunicationType == 2) //串口
            {
                //串口通讯********************************************************************************
                CommunicationInfo.SendType = 2;//串口通讯
                CommunicationInfo.Commport = TGPDisp.ComP2;//串口的编号，如设备管理器里显示为 COM3 则此处赋值 3
                CommunicationInfo.Baud = 115200;//波特率
                CommunicationInfo.LedNumber = 1;
            }


            //LedDll.LV_SetBrightness(ref CommunicationInfo, BrightnessValue);
            nResult = LedDll.LV_SetBrightness(ref CommunicationInfo, BrightnessValue);
            if (nResult != 0)
            {
                string ErrStr;
                ErrStr = LedDll.LS_GetError(nResult);
                MessageBox.Show("屏2亮度调节错误: " + ErrStr);
                if (bWriteLog) 
                Tool.WriteDataLog.m_instance.WriteLog(Tool.E_DATE_IOTYPE.no, Tool.E_LOG_TYPE.Log, "屏2亮度调节错误：");
                return;
            }
        }
        

    }


    public static class SunTimes
    {
        #region 公共方法
        /// <summary>
        /// 计算日长
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>日长</returns>
        /// <remarks>
        /// 注：日期最小为2000.1.1号
        /// </remarks>
        public static double GetDayLength(DateTime date, double longitude, double latitude)
        {
            double result = DayLen(date.Year, date.Month, date.Day, longitude, latitude, -35.0 / 60.0, 1);
            return result;
        }

        /// <summary>
        /// 计算日出日没时间
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>日落日出时间</returns>
        /// <remarks>
        /// 注：日期最小为2000.1.1号
        /// </remarks>
        public static SunTimeResult GetSunTime(DateTime date, double longitude, double latitude)
        {
            double start = 0;
            double end = 0;
            SunRiset(date.Year, date.Month, date.Day, longitude, latitude, -35.0 / 60.0, 1, ref start, ref end);
            DateTime sunrise = ToLocalTime(date, start);
            DateTime sunset = ToLocalTime(date, end);
            return new SunTimeResult(sunrise, sunset);
        }
        #endregion

        #region 私有方法

        #region 时间转换
        private static DateTime ToLocalTime(DateTime time, double utTime)
        {
            int hour = Convert.ToInt32(Math.Floor(utTime));
            double temp = utTime - hour;
            hour += 8;//转换为东8区北京时间
            temp = temp * 60;
            int minute = Convert.ToInt32(Math.Floor(temp));
            try
            {
                return new DateTime(time.Year, time.Month, time.Day, hour, minute, 0);
            }
            catch
            {
                return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            }
        }
        #endregion

        #region 与日出日落时间相关计算
        private static double DayLen(int year, int month, int day, double lon, double lat,
            double altit, int upper_limb)
        {
            double d,  /* Days since 2000 Jan 0.0 (negative before) */
                obl_ecl,    /* Obliquity (inclination) of Earth's axis */
                //黄赤交角，在2000.0历元下国际规定为23度26分21.448秒，但有很小的时间演化。

                sr,         /* Solar distance, astronomical units */
                slon,       /* True solar longitude */
                sin_sdecl,  /* Sine of Sun's declination */
                //太阳赤纬的正弦值。
                cos_sdecl,  /* Cosine of Sun's declination */
                sradius,    /* Sun's apparent radius */
                t;          /* Diurnal arc */

            /* Compute d of 12h local mean solar time */
            d = Days_since_2000_Jan_0(year, month, day) + 0.5 - lon / 360.0;

            /* Compute obliquity of ecliptic (inclination of Earth's axis) */
            obl_ecl = 23.4393 - 3.563E-7 * d;
            //这个黄赤交角时变公式来历复杂，很大程度是经验性的，不必追究。

            /* Compute Sun's position */
            slon = 0.0;
            sr = 0.0;
            Sunpos(d, ref slon, ref sr);

            /* Compute sine and cosine of Sun's declination */
            sin_sdecl = Sind(obl_ecl) * Sind(slon);
            cos_sdecl = Math.Sqrt(1.0 - sin_sdecl * sin_sdecl);
            //用球面三角学公式计算太阳赤纬。

            /* Compute the Sun's apparent radius, degrees */
            sradius = 0.2666 / sr;
            //视半径，同前。

            /* Do correction to upper limb, if necessary */
            if (upper_limb != 0)
                altit -= sradius;

            /* Compute the diurnal arc that the Sun traverses to reach */
            /* the specified altitide altit: */
            //根据设定的地平高度判据计算周日弧长。
            double cost;
            cost = (Sind(altit) - Sind(lat) * sin_sdecl) /
                (Cosd(lat) * cos_sdecl);
            if (cost >= 1.0)
                t = 0.0;                      /* Sun always below altit */
            //极夜。
            else if (cost <= -1.0)
                t = 24.0;                     /* Sun always above altit */
            //极昼。
            else t = (2.0 / 15.0) * Acosd(cost); /* The diurnal arc, hours */
            //周日弧换算成小时计。
            return t;

        }

        private static void Sunpos(double d, ref double lon, ref double r)
        {
            double M,//太阳的平均近点角，从太阳观察到的地球（=从地球看到太阳的）距近日点（近地点）的角度。
                w, //近日点的平均黄道经度。
                e, //地球椭圆公转轨道离心率。
                E, //太阳的偏近点角。计算公式见下面。

                x, y,
                v;  //真近点角，太阳在任意时刻的真实近点角。


            M = Revolution(356.0470 + 0.9856002585 * d);//自变量的组成：2000.0时刻太阳黄经为356.0470度,此后每天约推进一度（360度/365天
            w = 282.9404 + 4.70935E-5 * d;//近日点的平均黄经。

            e = 0.016709 - 1.151E-9 * d;//地球公转椭圆轨道离心率的时间演化。以上公式和黄赤交角公式一样，不必深究。

            E = M + e * Radge * Sind(M) * (1.0 + e * Cosd(M));
            x = Cosd(E) - e;
            y = Math.Sqrt(1.0 - e * e) * Sind(E);
            r = Math.Sqrt(x * x + y * y);
            v = Atan2d(y, x);
            lon = v + w;
            if (lon >= 360.0)
                lon -= 360.0;
        }

        private static void Sun_RA_dec(double d, ref double RA, ref double dec, ref double r)
        {
            double lon, obl_ecl, x, y, z;
            lon = 0.0;

            Sunpos(d, ref lon, ref r);
            //计算太阳的黄道坐标。

            x = r * Cosd(lon);
            y = r * Sind(lon);
            //计算太阳的直角坐标。

            obl_ecl = 23.4393 - 3.563E-7 * d;
            //黄赤交角，同前。

            z = y * Sind(obl_ecl);
            y = y * Cosd(obl_ecl);
            //把太阳的黄道坐标转换成赤道坐标（暂改用直角坐标）。

            RA = Atan2d(y, x);
            dec = Atan2d(z, Math.Sqrt(x * x + y * y));
            //最后转成赤道坐标。显然太阳的位置是由黄道坐标方便地直接确定的，但必须转换到赤
            //道坐标里才能结合地球的自转确定我们需要的白昼长度。

        }
        /// <summary>
        /// 日出没时刻计算
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="altit"></param>
        /// <param name="upper_limb"></param>
        /// <param name="trise">日出时刻</param>
        /// <param name="tset">日没时刻</param>
        /// <returns>太阳有出没现象，返回0 极昼，返回+1 极夜，返回-1</returns>
        private static int SunRiset(int year, int month, int day, double lon, double lat,
            double altit, int upper_limb, ref double trise, ref double tset)
        {
            double d,  /* Days since 2000 Jan 0.0 (negative before) */
                //以历元2000.0起算的日数。

                sr,         /* Solar distance, astronomical units */
                //太阳距离，以天文单位计算（约1.5亿公里）。      

                sRA,        /* Sun's Right Ascension */
                //同前，太阳赤经。

                sdec,       /* Sun's declination */
                //太阳赤纬。

                sradius,    /* Sun's apparent radius */
                //太阳视半径，约16分（受日地距离、大气折射等诸多影响）

                t,          /* Diurnal arc */
                //周日弧，太阳一天在天上走过的弧长。

                tsouth,     /* Time when Sun is at south */
                sidtime;    /* Local sidereal time */
            //当地恒星时，即地球的真实自转周期。比平均太阳日（日常时间）长3分56秒。      

            int rc = 0; /* Return cde from function - usually 0 */

            /* Compute d of 12h local mean solar time */
            d = Days_since_2000_Jan_0(year, month, day) + 0.5 - lon / 360.0;
            //计算观测地当日中午时刻对应2000.0起算的日数。

            /* Compute local sideral time of this moment */
            sidtime = Revolution(GMST0(d) + 180.0 + lon);
            //计算同时刻的当地恒星时（以角度为单位）。以格林尼治为基准，用经度差校正。

            /* Compute Sun's RA + Decl at this moment */
            sRA = 0.0;
            sdec = 0.0;
            sr = 0.0;
            Sun_RA_dec(d, ref sRA, ref sdec, ref sr);
            //计算同时刻太阳赤经赤纬。

            /* Compute time when Sun is at south - in hours UT */
            tsouth = 12.0 - Rev180(sidtime - sRA) / 15.0;
            //计算太阳日的正午时刻，以世界时（格林尼治平太阳时）的小时计。

            /* Compute the Sun's apparent radius, degrees */
            sradius = 0.2666 / sr;
            //太阳视半径。0.2666是一天文单位处的太阳视半径（角度）。

            /* Do correction to upper limb, if necessary */
            if (upper_limb != 0)
                altit -= sradius;
            //如果要用上边缘，就要扣除一个视半径。

            /* Compute the diurnal arc that the Sun traverses to reach */
            //计算周日弧。直接利用球面三角公式。如果碰到极昼极夜问题，同前处理。
            /* the specified altitide altit: */

            double cost;
            cost = (Sind(altit) - Sind(lat) * Sind(sdec)) /
                (Cosd(lat) * Cosd(sdec));
            if (cost >= 1.0)
            {
                rc = -1;
                t = 0.0;
            }
            else
            {
                if (cost <= -1.0)
                {
                    rc = +1;
                    t = 12.0;      /* Sun always above altit */
                }
                else
                    t = Acosd(cost) / 15.0;   /* The diurnal arc, hours */
            }


            /* Store rise and set times - in hours UT */
            trise = tsouth - t;
            tset = tsouth + t;

            return rc;
        }
        #endregion

        #region 辅助函数
        /// <summary>
        /// 历元2000.0，即以2000年第一天开端为计日起始（天文学以第一天为0日而非1日）。
        /// 它与UT（就是世界时，格林尼治平均太阳时）1999年末重合。 
        /// </summary>
        /// <param name="y"></param>
        /// <param name="m"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static long Days_since_2000_Jan_0(int y, int m, int d)
        {
            return (367L * (y) - ((7 * ((y) + (((m) + 9) / 12))) / 4) + ((275 * (m)) / 9) + (d) - 730530L);
        }

        private static double Revolution(double x)
        {
            return (x - 360.0 * Math.Floor(x * Inv360));
        }

        private static double Rev180(double x)
        {
            return (x - 360.0 * Math.Floor(x * Inv360 + 0.5));
        }

        private static double GMST0(double d)
        {
            double sidtim0;
            sidtim0 = Revolution((180.0 + 356.0470 + 282.9404) +
                (0.9856002585 + 4.70935E-5) * d);
            return sidtim0;
        }


        private static double Inv360 = 1.0 / 360.0;
        #endregion

        #region 度与弧度转换系数，为球面三角计算作准备
        private static double Sind(double x)
        {
            return Math.Sin(x * Degrad);
        }

        private static double Cosd(double x)
        {
            return Math.Cos(x * Degrad);
        }

        private static double Tand(double x)
        {
            return Math.Tan(x * Degrad);

        }

        private static double Atand(double x)
        {
            return Radge * Math.Atan(x);
        }

        private static double Asind(double x)
        {
            return Radge * Math.Asin(x);
        }

        private static double Acosd(double x)
        {
            return Radge * Math.Acos(x);
        }

        private static double Atan2d(double y, double x)
        {
            return Radge * Math.Atan2(y, x);

        }

        private static double Radge = 180.0 / Math.PI;
        private static double Degrad = Math.PI / 180.0;

        #endregion

        #endregion
    }

    /// <summary>
    /// 日出日落时间结果
    /// </summary>
    public class SunTimeResult
    {
        #region 构造与析构
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sunrise">日出时间</param>
        /// <param name="sunset">日落时间</param>
        public SunTimeResult(DateTime sunrise, DateTime sunset)
        {
            sunriseTime = sunrise;
            sunsetTime = sunset;
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 获取日出时间
        /// </summary>
        public DateTime SunriseTime
        {
            get
            {
                return sunriseTime;
            }
        }

        /// <summary>
        /// 获取日落时间
        /// </summary>
        public DateTime SunsetTime
        {
            get
            {
                return sunsetTime;
            }
        }
        #endregion


        #region 私成员
        private DateTime sunriseTime;//日出时间
        private DateTime sunsetTime;//日落时间
        #endregion
    }
}
