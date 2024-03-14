using System;
using System.Collections.Generic;
using System.Text;
using System.Collections ;
using System.IO;
using System.Data ;

namespace PRC_Tool
{
    /// <summary>
    /// 集控数据数据库存储处理
    /// </summary>
    public class CTCLogData
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime time;
        /// <summary>
        /// 
        /// </summary>
        public string cmd;
        /// <summary>
        /// 
        /// </summary>
        public string oper;
        /// <summary>
        /// 
        /// </summary>
        public string remark;
        /// <summary>
        /// 
        /// </summary>
        public string clientip = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="c"></param>
        /// <param name="op"></param>
        /// <param name="re"></param>
        public CTCLogData(DateTime tm, string c, string op, string re)
        {
            time = tm;
            cmd = c;
            oper = op;
            remark = re;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="c"></param>
        /// <param name="op"></param>
        /// <param name="re"></param>
        /// <param name="ip"></param>
        public CTCLogData(DateTime tm, string c, string op, string re,string ip)
        {
            time = tm;
            cmd = c;
            oper = op;
            remark = re;
            clientip = ip;
        }
    }
    /// <summary>
    /// 通用PRC的数据库存储处理
    /// </summary>
    public class PRCLogData
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime time;
        /// <summary>
        /// 
        /// </summary>
        public string cmd;
        /// <summary>
        /// 
        /// </summary>
        public Int64 routes_id;
        /// <summary>
        /// 
        /// </summary>
        public ushort event_id;
        /// <summary>
        /// 
        /// </summary>
        public ushort fault_id;
        /// <summary>
        /// 
        /// </summary>
        public string remark;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="cm"></param>
        /// <param name="rtid"></param>
        /// <param name="evid"></param>
        /// <param name="faid"></param>
        /// <param name="rem"></param>
        public PRCLogData(DateTime tm, string cm, Int64 rtid, ushort evid, ushort faid, string rem)
        {
            time = tm;
            cmd = cm;
            event_id = evid;
            routes_id = rtid;
            fault_id = faid;
            remark = rem;
        }
    }
    /// <summary>
    /// 一般文本文件记录存储处理
    /// </summary>
    public class File_LOG
    {
        /// <summary>
        /// 
        /// </summary>
        private static SafeArrayList ar_CTCData = new SafeArrayList();
        /// <summary>
        /// 
        /// </summary>
        private static SafeArrayList ar_PRCData = new SafeArrayList();
        /// <summary>
        /// 
        /// </summary>
        private static SafeArrayList  ar_Data = new SafeArrayList ();
        /// <summary>
        /// 
        /// </summary>
        private static BaseTask hBT = null;
        /// <summary>
        /// 保存数据的表
        /// </summary>
        private static DataTable dt_Data = null;
        /// <summary>
        /// 
        /// </summary>
        private static DataTable dt_PRCData=null ;
        /// <summary>
        /// 
        /// </summary>
        private static DataIO H_WEB_DB = null;
        /// <summary>
        /// 增加日志记录，文本记录方式，以文本文件存储
        /// </summary>
        /// <param name="s"></param>
        public static void WriteLog(string s)
        {
            
            if (hBT == null)
            {
                hBT = new BaseTask(1000, 20000, "日志保存任务");
                hBT.event_Interval += new BaseTask.Interval_Event(hBT_event_Interval);
            }
            try
            {
                if (ar_Data.Count > 1000)
                    ar_Data.Clear();
                ar_Data.Add(s+" ["+DateTime .Now .ToString("HH:mm:ss")+"]");
            }
            finally
            {
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c_cmd"></param>
        /// <param name="c_operate"></param>
        /// <param name="c_remark"></param>
        public static void WriteCTC_Log(string c_cmd, string c_operate, string c_remark)
        {
            WriteCTC_Log(c_cmd, c_operate, c_remark, "");
        }
        /// <summary>
        /// 记录集控操作和事件
        /// </summary>
        /// <param name="c_cmd"></param>
        /// <param name="c_operate"></param>
        /// <param name="c_remark"></param>
        /// <param name="ip"></param>
        public static void WriteCTC_Log(string c_cmd, string c_operate, string c_remark,string ip)
        {
            if (hBT == null)
            {
                hBT = new BaseTask(1000, 20000, "日志保存任务");
                hBT.event_Interval += new BaseTask.Interval_Event(hBT_event_Interval);
            }
            if (H_WEB_DB == null)
            {
                //string url = System.Configuration.ConfigurationManager.AppSettings["H_WEB_DC_GRAPH"].ToString();
                //H_WEB_DB = new Web_Client.DataBase(url);
                string oledb = System.Configuration.ConfigurationManager.AppSettings["OleDB"].ToString();
                H_WEB_DB = new DataIO(oledb);
            }
            if (dt_Data == null)
            {
                dt_Data = new DataTable("DR_LOG_CTC");
                dt_Data.Columns.Add("D_TIME",typeof (DateTime ));
                dt_Data.Columns.Add("C_CMD",typeof (string ));
                dt_Data.Columns.Add("C_OPERATOR", typeof(string));
                dt_Data.Columns.Add("C_REMARK", typeof(string));
                //dt_Data.Columns.Add("C_IP", typeof(string));
            }
            try
            {
                ar_CTCData.Add(new CTCLogData(DateTime.Now, c_cmd, c_operate, c_remark,ip ));
                if (ar_CTCData.Count > 3000)
                    ar_CTCData.Clear();
            }
            catch
            {
            }
        }
        /// <summary>
        /// 写PRC的操作日志
        /// </summary>
        public static void WritePRC_Log(string c_cmd,Int64 i_routes_id,Int16 e_event_id, Int16  e_fault_id,string c_remark)
        {
            if (hBT == null)
            {
                hBT = new BaseTask(1000, 20000, "日志保存任务");
                hBT.event_Interval += new BaseTask.Interval_Event(hBT_event_Interval);
            }
            if (H_WEB_DB == null)
            {
                //string url = System.Configuration.ConfigurationManager.AppSettings["H_WEB_DC_GRAPH"].ToString();
                //H_WEB_DB = new Web_Client.DataBase(url);
                string oledb = System.Configuration.ConfigurationManager.AppSettings["OleDB"].ToString();
                H_WEB_DB = new DataIO(oledb);
            }

            if (dt_PRCData == null)
            {
                dt_PRCData = new DataTable("DR_LOG_PRC");
                dt_PRCData.Columns.Add("D_TIME", typeof(DateTime));
                dt_PRCData.Columns.Add("C_CMD", typeof(string));
                dt_PRCData.Columns.Add("I_ROUTES_ID", typeof(Int64));
                dt_PRCData.Columns .Add ("E_EVENT_ID",typeof (Int16 ));
                dt_PRCData.Columns.Add("E_FAULT_ID", typeof(Int16));
                dt_PRCData.Columns.Add("C_REMARK", typeof(string));
            }
            try
            {
                ar_PRCData.Add(new PRCLogData(DateTime.Now, c_cmd, i_routes_id,(ushort ) e_event_id,(ushort ) e_fault_id, c_remark));
                if (ar_PRCData.Count > 3000)
                    ar_PRCData.Clear();
            }
            catch
            {
            }
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        private static  void SaveFileLog()
        {
            try
            {
                StreamWriter errFile = null;
                if (ar_Data.Count > 0)
                {
                    try
                    {
                        
                        string fn = AppDomain.CurrentDomain .BaseDirectory  + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                        errFile = File.AppendText(fn);
                        while (ar_Data.Count > 0)
                        {
                            string s = ar_Data[0].ToString();
                            errFile.WriteLine(s);
                            ar_Data.RemoveAt(0);
                        }
                        errFile.Flush();
                    }
                    catch(Exception e)
                    {
                    }
                    finally
                    {
                        errFile.Close();
                    }
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// 保存集控操作到数据库
        /// </summary>
        private static  void SaveCTCLog()
        {
            if( dt_Data ==null || ar_CTCData.Count ==0) return ;
            try
            {
                while (ar_CTCData.Count > 0)
                {
                    CTCLogData cld = ar_CTCData[0] as CTCLogData;
                    DataRow row = dt_Data.NewRow();
                    row["D_TIME"] = cld .time ;
                    row["C_CMD"] = cld .cmd;
                    row["C_OPERATOR"] = cld .oper;
                    row["C_REMARK"] = cld .remark ;
                    //row["C_IP"] = cld.clientip;
                    dt_Data.Rows.Add(row);
                    ar_CTCData.RemoveAt(0);

                }
                DataTable dt=dt_Data.GetChanges();
                if (dt != null && dt.Rows.Count > 0)
                {
                    //DataSet ds = new DataSet();
                    //ds.Tables.Add(dt);
                    //int count = H_WEB_DB.SaveOneTableDataToDB(ds, dt.TableName);
                    //ds.Tables.Remove(dt);
                    //ds.Dispose();
                    int count= H_WEB_DB.SaveOneTableDataToDB(dt);
                    if (count > 0)
                    {
                        dt_Data.Clear();
                        dt_Data.AcceptChanges();
                    }
                    else 
                    {
                        foreach (DataRow row in dt_Data.Rows)
                        {
                            string ss =System .Convert .ToDateTime (  row["D_TIME"]).ToString("HH:mm:ss") + "," + row["C_CMD"].ToString() + "," + row["C_OPERATOR"].ToString() + "," + row["C_REMARK"].ToString();
                            WriteLog(ss);
                        }
                        dt_Data.Clear();
                        dt_Data.AcceptChanges();
                    }
                }
            }
            finally
            {
                if (dt_Data != null && dt_Data.Rows.Count > 2000)
                {
                    dt_Data.Clear();
                    dt_Data.AcceptChanges();
                }
            }
        }
        /// <summary>
        /// 保存PRC操作到数据库
        /// </summary>
        private static void SavePRCLog()
        {
            if (dt_PRCData  == null ||  ar_PRCData. Count == 0) return;
            try
            {
                while (ar_PRCData.Count > 0)
                {
                    PRCLogData cld = ar_PRCData[0] as PRCLogData;
                    DataRow row = dt_PRCData.NewRow();
                    row["D_TIME"] = cld.time;
                    row["C_CMD"] = cld.cmd;
                    row["I_ROUTES_ID"] = cld.routes_id;
                    row["E_EVENT_ID"] = cld.event_id ;
                    row["E_FAULT_ID"] = cld.fault_id ;
                    row["C_REMARK"] = cld.remark;
                    dt_PRCData.Rows.Add(row);
                    ar_PRCData.RemoveAt(0);
                }

                DataTable dt = dt_PRCData. GetChanges();
                if (dt != null && dt.Rows.Count > 0)
                {
                    //DataSet ds = new DataSet();
                    //ds.Tables.Add(dt);
                    //int count = H_WEB_DB.SaveOneTableDataToDB(ds, dt.TableName);
                    //ds.Tables.Remove(dt);
                    //ds.Dispose();
                    int count = H_WEB_DB.SaveOneTableDataToDB(dt);
                    if (count > 0)
                    {
                        dt_PRCData.Clear();
                        dt_PRCData.AcceptChanges();
                    }
                    else
                    {
                        foreach (DataRow row in dt_PRCData.Rows)
                        {
                            string ss = System.Convert.ToDateTime(row["D_TIME"]).ToString("HH:mm:ss") + "," + row["C_CMD"].ToString() + "," + row["I_ROUTES_ID"].ToString() + "," + row["E_EVENT_ID"].ToString() + "," + row["E_FAULT_ID"].ToString() + "," + row["C_REMARK"].ToString();
                            WriteLog(ss);
                        }
                        dt_Data.Clear();
                        dt_Data.AcceptChanges();
                    }

                }
            }
            finally
            {
                if (dt_PRCData != null &&  dt_PRCData.Rows .Count  > 2000)
                {
                    dt_PRCData.Clear();
                    dt_PRCData.AcceptChanges();
                }
            }

        }
        /// <summary>
        /// 删除较早前文件
        /// </summary>
        private static  void DeleteFile()
        {
            //删除月前数据
            try
            {
                //删除文件
                string[] files = Directory.GetFiles(System.Environment.CurrentDirectory);
                foreach (string s in files)
                {
                    if (s.Length <= 4) return;
                    if (s.Substring(s.Length - 4, 4) == ".log")
                    {
                        DateTime time_last = File.GetLastWriteTime(s);
                        TimeSpan ts = DateTime.Now - time_last;
                        if (ts.TotalDays > 10)
                        {
                            try
                            {
                                File.Delete(s);
                            }
                            catch
                            {
                            }
                        }
                    }
                }

            }
            catch
            {
            }

        }
        /// <summary>
        /// 删除较早前数据库数据
        /// </summary>
        private static void DeleteTable()
        {
            //删除表
            if (dt_Data != null && H_WEB_DB != null)
            {
                //H_WEB_DB.Excute_SQLCmd("DELETE FROM " + dt_Data.TableName + " WHERE D_TIME <'" + DateTime.Now.AddDays(-10) + "'");
                try
                {
                    H_WEB_DB.Excute_SQLCmd("DELETE FROM " + dt_Data.TableName + " WHERE D_TIME <'" + DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss") + "'");
                }
                catch
                {
                }
            }
            if (dt_PRCData != null && H_WEB_DB != null)
            {
                //H_WEB_DB.Excute_SQLCmd("DELETE FROM " + dt_Data.TableName + " WHERE D_TIME <'" + DateTime.Now.AddDays(-10) + "'");
                try
                {
                    H_WEB_DB.Excute_SQLCmd("DELETE FROM " + dt_PRCData.TableName + " WHERE D_TIME <'" + DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss") + "'");
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        static void hBT_event_Interval()
        {
            SaveFileLog();
            SaveCTCLog();
            SavePRCLog();
            if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second < 30)
            {
                DeleteFile();
                DeleteTable();
            }
            //throw new Exception("The method or operation is not implemented.");
        }

    }
}
