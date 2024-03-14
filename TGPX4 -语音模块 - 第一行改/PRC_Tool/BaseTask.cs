using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections;


namespace PRC_Tool
{
    /// <summary>
    /// �߳��������
    /// </summary>
    public  class BaseTask
    {
        /// <summary>
        /// ��ȡ������Ƶ
        /// </summary>
        /// <param name="PerformanceFrequency"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);
        /// <summary>
        /// ��ȡƵ�ʼ���
        /// </summary>
        /// <param name="PerformanceCount"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern bool QueryPerformanceCounter(ref long PerformanceCount);

        private static  int I_MaxID=0;
        /// <summary>
        /// ������Դ����
        /// </summary>
        [Serializable]
        public class BaseTaskResource
        {
            
            /// <summary>
            /// ����ID
            /// </summary>
            public int I_ID = 0;
            /// <summary>
            /// ��������
            /// </summary>
            public string C_Name = "";
            /// <summary>
            /// ������������
            /// </summary>
            public int I_Interval = 0;
            /// <summary>
            /// �����������ʱ��
            /// </summary>
            public int I_MaxRunTime = 0;
            /// <summary>
            /// ������С����ʱ��
            /// </summary>
            public int I_MinRunTime = 10000;
            /// <summary>
            /// �����������ʱ��
            /// </summary>
            public DateTime time_Send = DateTime.Now;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <param name="na"></param>
            /// <param name="intv"></param>
            public BaseTaskResource(int id, string na,int intv)
            {
                I_ID = id;
                C_Name = na;
                I_Interval = intv;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] Serialize()
            {
                byte[] res=Convert.Struct2Bytes(this, typeof(BaseTaskResource));
                return res;
            }
        }
        /// <summary>
        /// ������Դʵ��
        /// </summary>
        public BaseTaskResource hTaskResource=null ;
        /// <summary>
        /// ��������ί��
        /// </summary>
        public delegate void Interval_Event();
        /// <summary>
        /// վ�������¼�
        /// </summary>
        public event Interval_Event event_Interval;
        /// <summary>
        /// �õ�����Ƶ��
        /// </summary>
        private static Int64 HZ_FREQ = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="interval"></param>
        /// <param name="taskna"></param>
        public BaseTask(int id, int interval,string taskna)
        {
            hTaskResource = new BaseTaskResource(id,taskna, interval);
            QueryPerformanceFrequency(ref HZ_FREQ);

            Thread TaskThread = new Thread(BaseTaskThread);
            TaskThread.Name = taskna;
            TaskThread.Priority = ThreadPriority.BelowNormal;
            TaskThread.IsBackground = true;
            TaskThread.Start();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int GetID()
        {
            I_MaxID++;
            return I_MaxID;
        }
        /// <summary>
        /// 
        /// </summary>
        private void BaseTaskThread()
        {
            Int64 time_Old = 0; ;
            Int64 time_Now = 0;
            while (true)
            {
                try
                {
                    QueryPerformanceCounter(ref time_Old);
                    hTaskResource.time_Send = DateTime.Now;
                    event_Interval();
                    QueryPerformanceCounter(ref time_Now);
                    int time_exe = GetTime_Delta(time_Now, time_Old);
                    if (time_exe > hTaskResource.I_MaxRunTime)
                        hTaskResource.I_MaxRunTime = time_exe;
                    if (time_exe < hTaskResource.I_MinRunTime || hTaskResource.I_MinRunTime == 0)
                        hTaskResource.I_MinRunTime = time_exe;
                    Thread.Sleep(hTaskResource.I_Interval);
                }
                catch(Exception e)
                {
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        private static int GetTime_Delta(Int64 d1, Int64 d2)
        {
            int time_delta = (int)((double)(d1 - d2) *1000/ (double)(HZ_FREQ) );
            if (time_delta < 0)
                time_delta = (int)((double)(Int64.MaxValue + d1 - d2)*1000 / (double )HZ_FREQ );
            return time_delta;
        }

    }
}
