using System;
using System.Collections.Generic;
using System.Text;
using System.Data ;
namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public  class SaveDB
    {
        /// <summary>
        /// 
        /// </summary>
        private List<DataTable> ar_tables = new List<DataTable>();
        /// <summary>
        /// 
        /// </summary>
        private DataIO hDataIO = null;
        /// <summary>
        /// 
        /// </summary>
        private int I_Interval = 10000;
        /// <summary>
        /// 
        /// </summary>
        private PRC_Tool.BaseTask task_Save = null;
        /// <summary>
        /// 
        /// </summary>
        private System.Threading.Mutex hmutex = new System.Threading.Mutex();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dio"></param>
        public SaveDB(DataIO dio)
        {
            hDataIO = dio;
            Init();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dio"></param>
        /// <param name="interval"></param>
        public SaveDB(DataIO dio,int interval)
        {
            hDataIO = dio;
            I_Interval = interval;
            Init();
        }
        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            task_Save = new BaseTask(1001, I_Interval, "savedb");
            task_Save.event_Interval += task_Save_event_Interval;
        }
        /// <summary>
        /// 
        /// </summary>
        void task_Save_event_Interval()
        {
            if (hDataIO == null) return;
            while (ar_tables.Count > 0)
            {
                int res = 0;
                try
                {
                    res = hDataIO.SaveOneTableDataToDB(ar_tables[0]);
                }
                catch
                {
                    res = 0;
                }
                if (res <= 0)
                    break;
                else
                {
                    hmutex.WaitOne();
                    try
                    {
                        ar_tables.RemoveAt(0);
                    }
                    finally
                    {
                        hmutex.ReleaseMutex();
                    }
                }
            }
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public bool Add(DataTable dt)
        {
            if (ar_tables.Count > 1000) return false;
            if (dt == null) return false ;
            DataTable dt_c = dt.GetChanges();
            if (dt_c == null) return false;
            hmutex .WaitOne ();
            try
            {
                ar_tables.Add(dt_c);
            }
            finally
            {
                hmutex.ReleaseMutex();
            }
            return true;
        }

    }
}
