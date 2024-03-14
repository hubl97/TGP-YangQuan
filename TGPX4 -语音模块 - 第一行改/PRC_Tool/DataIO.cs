using System;
using System.Collections.Generic;
using System.Text;
using System.Data ;
using System.Data.OleDb;
namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public  class DataIO
    {
        /// <summary>
        /// 
        /// </summary>
        private  string C_DB_Conn = "";
        /// <summary>
        /// 
        /// </summary>
        private  OleDbConnection hOldDBConn = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        public DataIO(string conn)
        {
            C_DB_Conn = conn;// System.Configuration.ConfigurationManager.AppSettings["OleDB"].ToString();
            hOldDBConn = new OleDbConnection(C_DB_Conn);
            hOldDBConn.ConnectionString += ";Connect Timeout=5";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int SaveOneTableDataToDB(DataTable dt)
        {
            if (hOldDBConn == null) return 0;
            if (dt == null || dt.Rows.Count <= 0) return 0;
            int count = 0;
            
            try
            {
                hOldDBConn.Open();
                OleDbCommand selectCommand = new OleDbCommand("select * from " + dt.TableName, hOldDBConn);
                OleDbDataAdapter da = new OleDbDataAdapter(selectCommand);
                OleDbCommandBuilder cb = new OleDbCommandBuilder(da);
                count += da.Update(dt.Select(null, null, DataViewRowState.Deleted));
                count += da.Update(dt.Select(null, null, DataViewRowState.ModifiedCurrent));
                count += da.Update(dt.Select(null, null, DataViewRowState.Added));

            }
            catch(Exception e)
            {
                count = 0;
            }
            finally
            {
                hOldDBConn.Close();
            }

            //DataRow []sss= dt.Select(null, null, DataViewRowState.ModifiedCurrent);
            return count;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public int SaveDataToDB(DataSet  ds)
        {
            if (hOldDBConn == null) return 0;
            if (ds == null || ds.Tables .Count <= 0) return 0;
            int count = 0;
            try
            {
                hOldDBConn.Open();
                foreach (DataTable dt in ds.Tables)
                {
                    OleDbCommand selectCommand = new OleDbCommand("select * from " + dt.TableName, hOldDBConn);
                    OleDbDataAdapter da = new OleDbDataAdapter(selectCommand);
                    OleDbCommandBuilder cb = new OleDbCommandBuilder(da);
                    count += da.Update(dt.Select(null, null, DataViewRowState.Deleted));
                    count += da.Update(dt.Select(null, null, DataViewRowState.ModifiedCurrent));
                    count += da.Update(dt.Select(null, null, DataViewRowState.Added));
                }

            }
            catch
            {
                count = 0;
            }
            finally
            {
                hOldDBConn.Close();
            }
            return count;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        public void Excute_SQLCmd(string sql)
        {
            try
            {
                hOldDBConn.Open();
                OleDbCommand selectCommand = new OleDbCommand(sql, hOldDBConn);
                OleDbDataAdapter da = new OleDbDataAdapter(selectCommand);
                da.SelectCommand.CommandText = sql;
                da.SelectCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                hOldDBConn.Close();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="tablename"></param>
        /// <param name="sql"></param>
        private bool  Excute_SQL(ref DataSet ds, string tablename, string sql)
        {
            if (tablename == null || tablename.Trim() == "") return false;
            bool res=true;
            try
            {
                hOldDBConn.Open();
                OleDbCommand selectCommand = new OleDbCommand(sql, hOldDBConn);
                OleDbDataAdapter da = new OleDbDataAdapter(selectCommand);
                da.FillSchema(ds, SchemaType.Mapped, tablename);
                da.Fill(ds, tablename);
            }
            catch (Exception ex)
            {
                res = false;
                //throw new Exception(ex.Message);
            }
            finally
            {
                hOldDBConn.Close();
               
            }
             return res ;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="tablename"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool GetTable(ref DataSet ds, string tablename, string sql)
        {
            
            if (sql == null || sql .Trim ()=="")
                sql = "select * from " + tablename;
            return Excute_SQL(ref  ds, tablename, sql);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="tables"></param>
        /// <param name="sqls"></param>
        public void GetTables(ref DataSet ds, string[] tables, string[] sqls)
        {
            if (tables == null || tables.Length == 0) return;
            for(int i=0; i< tables .Length ; i++)
            {
                if( tables[i]==null || tables [i].Trim ()=="") continue ;
                string sql = "";
                if (sqls != null && sqls.Length > i && sqls[i].Trim() != "")
                    sql = sqls[i];
                else
                    sql = "select * from " + tables[i];
                Excute_SQL(ref ds, tables[i], sql);
            }
        }
        /// <summary>
        /// 获取数据库表
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DataTable GetTableList(string dbName, string filter)
        {
            DataTable tables = null;
            string sqlCmd = "";
            if (String.IsNullOrEmpty(dbName)) return null;
            if (String.IsNullOrEmpty(filter))
                sqlCmd = "select TABLE_NAME,COLUMN_NAME,ORDINAL_POSITION,DATA_TYPE from " + dbName + ".INFORMATION_SCHEMA.COLUMNS";
            else
                sqlCmd = "select TABLE_NAME,COLUMN_NAME,ORDINAL_POSITION,DATA_TYPE from " + dbName + ".INFORMATION_SCHEMA.COLUMNS" + " where " + filter;
            try
            {
                hOldDBConn.Open();
                OleDbCommand selectCommand = new OleDbCommand(sqlCmd, hOldDBConn);
                OleDbDataAdapter da = new OleDbDataAdapter(selectCommand);
                OleDbDataReader rs = selectCommand.ExecuteReader();
                if (rs.HasRows)
                {
                    tables = new DataTable();
                    DataColumn cl1 = new DataColumn();
                    cl1.ColumnName = "TABLE_NAME";
                    cl1.DataType = typeof(String);
                    DataColumn cl2 = new DataColumn();
                    cl2.ColumnName = "COLUMN_NAME";
                    cl2.DataType = typeof(String);
                    DataColumn cl3 = new DataColumn();
                    cl3.ColumnName = "ORDINAL_POSITION";
                    cl3.DataType = typeof(Int16);
                    DataColumn cl4 = new DataColumn();
                    cl4.ColumnName = "DATA_TYPE";
                    cl4.DataType = typeof(String);
                    tables.Columns.Add(cl1);
                    tables.Columns.Add(cl2);
                    tables.Columns.Add(cl3);
                    tables.Columns.Add(cl4);
                }
                else
                    return tables;
                while (rs.Read())
                {
                    DataRow newrow = tables.NewRow();
                    newrow["TABLE_NAME"] = rs["TABLE_NAME"];
                    newrow["COLUMN_NAME"] = rs["COLUMN_NAME"];
                    newrow["ORDINAL_POSITION"] = rs["ORDINAL_POSITION"];
                    newrow["DATA_TYPE"] = rs["DATA_TYPE"];
                    tables.Rows.Add(newrow);
                }
            }
            catch (Exception ex)
            {
                tables = null;
            }
            finally
            {
                hOldDBConn.Close();

            }
            return tables;
        }
    }
}
