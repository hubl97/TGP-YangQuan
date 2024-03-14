using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data ;

namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public class DS_DC
    {
        /// <summary>
        /// 
        /// </summary>
        public static TABLE Tables = null;
        /// <summary>
        /// 
        /// </summary>
        public static void Clear()
        {
            Tables.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabna"></param>
        /// <returns></returns>
        public static Int32 NewID(string tabna)
        {
            Int32 id = 1;
            string primarykey="";
            if (Tables[tabna].PrimaryKey.Length > 0)
                primarykey = Tables[tabna].PrimaryKey[0].ColumnName;
            else
                return -1;
            DataRow[] rows = Tables[tabna].Select("", primarykey + " DESC");
            if (rows.Length > 0)
                id = System.Convert.ToInt32(rows[0][primarykey]) + 1;
            return id;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        public DS_DC(string conn)
        {
            Tables = new TABLE(conn);
        }
        /// <summary>
        /// 
        /// </summary>
        public  class TABLE
        {
            /// <summary>
            /// 
            /// </summary>
            private DataIO hDataIO = null;
            /// <summary>
            /// 
            /// </summary>
            private DataSet dataset_DC = new DataSet();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="tab_na"></param>
            /// <returns></returns>
            public DataTable this[string tab_na]
            {
                get
                {
                    if (dataset_DC.Tables[tab_na] == null)
                    {
                        if (!hDataIO.GetTable(ref dataset_DC, tab_na, ""))
                        {
                            return null;
                        }
                    }
                    return dataset_DC.Tables[tab_na];
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="conn"></param>
            public TABLE(string conn)
            {
                hDataIO = new DataIO(conn);
            }
            /// <summary>
            /// 
            /// </summary>
            public void Clear()
            {
                dataset_DC.Tables.Clear();
                dataset_DC.AcceptChanges();
            }
            /// <summary>
            /// 数据表是否变化
            /// </summary>
            /// <returns></returns>
            public bool TableChanged()
            {
                if (dataset_DC.GetChanges() == null)
                    return false;
                else return true;
            }
        }
    }
}
