using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NewTGP
{
    public static class ListenDll
    {
        /// <summary>
        /// 初始化DLL
        /// <param name="nlog">是否生成log日志文件</para>
        /// </summary>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
       public extern static void LV_InitDllEx(int nlog);


        /// <summary>
        /// 初始化DLL
        /// <param name="nlog">是否生成log日志文件</para>
        /// <param name="pWorkPath">软件的工作目录,用来存储节目文件,临时文件</param>
        /// </summary>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static void LV_InitDllEx2(int nlog, byte[] pWorkPath);

        /// <summary>
        /// 释放dll
        /// </summary>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static void LV_ReleaseDllEx();

        /// <summary>
        /// 获取dll的版本号
        /// </summary>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static UInt32 LV_DllVersion();
        /// <summary>
        /// 搜索设备
        /// </summary>
        /// <param name="pDeviceInfo">设备数据的数组</param>
        /// <param name="nBufLen">数组的长度</param>
        /// <returns>搜索到的设备数量</returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_SearchDeviceInfoEx(byte[] pDeviceInfo, int nBufLen); 

        /// <summary>
        /// 添加一个节目页面
        /// </summary>
        /// <param name="pScreenName">屏幕名称</param>
        /// <param name="pProgramId">节目id</param>
        /// <param name="pPageJson">页面数据的json</param>
        /// <returns></returns>

        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_AddPageToProgram(byte[] pScreenName, byte[] pProgramId, byte[] pPageJson);
        /// <summary>
        /// 给节目添加一个区域信息
        /// </summary>
        /// <param name="pScreenName">屏幕名称</param>
        /// <param name="pProgramId">节目id</param>
        /// <param name="nPageId">页面</param>
        /// <param name="pAreaInfoJson">区域json</param>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_AddArea(byte[] pScreenName, byte[] pProgramId, int nPageId, byte[] pAreaInfoJson);
        /// <summary>
        /// 发送对应IP的节目 与LV_SendProgramToDevice2的区别详见接口文档
        /// </summary>
        /// <param name="pIp">设备ip</param>
        /// <param name="pProgramId">节目id</param>
        /// <param name="pProgramName">节目名</param>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_SendProgramToDevice(byte[] pScreenName, byte[] pIp, byte[] pProgramId, byte[] pProgramName,byte[] pPassword);

        /// <summary>
        /// 发送对应IP的节目 与LV_SendProgramToDevice的区别详见接口文档
        /// </summary>
        /// <param name="pIp">设备ip</param>
        /// <param name="pProgramId">节目id</param>
        /// <param name="pProgramName">节目名</param>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_SendProgramToDevice2(byte[] pScreenName, byte[] pIp, byte[] pProgramId, byte[] pProgramName, byte[] pPassword);

        /// <summary>
        /// 发送对应IP的节目计划
        /// </summary>
        /// <param name="pIp">设备ip</param>
        /// <param name="pPlanData">计划数据</param>
        /// <param name="pDefResVideoFile">默认节目中使用的视频资源组成的json.如果没有默认节目,或默认节目中没有使用视频,则该值为空字符串</param>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_SendProgramPlan(byte[] pIp, byte[] pPlanData, byte[] pDefResVideoFile);

        /// <summary>
        /// 发送对应IP的节目计划2
        /// </summary>
        /// <param name="pIp">设备ip</param>
        /// <param name="pPlanData">计划数据</param>
        /// <param name="pDefResVideoFile">默认节目中使用的视频资源组成的json.如果没有默认节目,或默认节目中没有使用视频,则该值为空字符串</param>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_SendProgramPlan2(byte[] pIp, byte[] pPlanData, byte[] pDefResVideoFile);

        /// <summary>
        /// 用来清除节目生成的临时文件和临时数据	
        /// </summary>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_CleanProgramTemporary();

        /// <summary>
        /// 开关屏
        /// </summary>
        /// <param name="pIp"></param>
        /// <param name="isOn"></param>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_SwitchLed(byte[] pIp, int isOn);


        /// <summary>
        /// 创建一个屏幕,如果之前存在这个屏幕数据,那么之前的数据将会被删除
        /// </summary>
        /// <param name="pScreenName">屏幕名称</param>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_CreateScreen(byte[] pScreenName);

        /// <summary>
        /// 刷新固定区域(静态文本区域)
        /// </summary>
        /// <param name="pIp">发送的设备的ip地址</param>
        /// <param name="pInfo">参数;详见接口说明文档</param>
        /// <returns>0:成功 -1:失败,可调用LV_GetLastErrInfo()接口获取错误信息</returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_RefreshFixedArea(byte[] pIp, byte[] pInfo);

        /// <summary>
        /// 清理设备存储
        /// </summary>
        /// <param name="pIp">发送的设备的ip地址</param>
        /// <returns>0:成功 -1:失败,可调用LV_GetLastErrInfo()接口获取错误信息</returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_ClearDeviceSpace(byte[] pIp);

        /// <summary>
        /// <para name="pErrInfo">错误信息数据的数组</para>
        /// <param name="nBufLen">数组长度</param>
        /// </summary>
        /// <returns></returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_GetLastErrInfo(byte[] pErrInfo, int nBufLen);

        /// <summary>
        /// 刷新图片区域的显示
        /// </summary>
        /// <param name="pIp">发送的设备的ip地址</param>
        /// <param name="pInfo">参数;详见接口说明文档</param>
        /// <returns>0:成功 -1:失败,可调用LV_GetLastErrInfo()接口获取错误信息</returns>
        [DllImport(@"ListenLedX3.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static int LV_RefreshPictureArea(byte[] pIp, byte[] pInfo);
    }


    public static class JsonExtension
    {
        /// <summary>
        /// 把对象转换为JSON字符串
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(this object o)
        {
            if (o == null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(o);
        }
        /// <summary>
        /// 把Json文本转为实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T FromJSON<T>(this string input)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(input);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
    }
     public class DeviceInfo
     {
        public  List<Device> device;       

        public class Device
         {
             private string _ip;

             public string ip
             {
                 get { return _ip; }
                 set { _ip = value; }
             }
             private string _mac;

             public string mac
             {
                 get { return _mac; }
                 set { _mac = value; }
             }
             private string _name;

             public string name
             {
                 get { return _name; }
                 set { _name = value; }
             }
             private string _pattern;

             public string pattern
             {
                 get { return _pattern; }
                 set { _pattern = value; }
             }
             private string _width;

             public string width
             {
                 get { return _width; }
                 set { _width = value; }
             }
             private string _height;

             public string height
             {
                 get { return _height; }
                 set { _height = value; }
             }
             private string _systemVersion;

             public string systemVersion
             {
                 get { return _systemVersion; }
                 set { _systemVersion = value; }
             }
             private string _cardMemory;

             public string cardMemory
             {
                 get { return _cardMemory; }
                 set { _cardMemory = value; }
             }
         }
         public DeviceInfo()
        {
            device = new List<Device>();
        }
     }
}
