using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public class Host
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List < string> GetCpuID()
        {
            List<string> ar_cpuinfo = new List<string>();
            try
            {
                //获取CPU序列号代码     
                string cpuInfo = " ";//cpu序列号     
                System.Management.ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                    string rv = mo.Properties["Revision"].Value.ToString();
                    ar_cpuinfo.Add(cpuInfo+rv);

                    //string ss = "";
                    //foreach (PropertyData pd in mo.Properties)
                    //{
                    //    ss = ss + pd.Name + "---" + pd.Value + "\r";
                    //}
                }
                moc = null;
                mc = null;
            }
            catch
            {
            }
            finally
            {
            }
            return ar_cpuinfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List< string> GetMacAddress()
        {
            List<string> ar_macInfo = new List<string>();
            try
            {
                // 获取网卡硬件地址     
                string mac = " ";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        ar_macInfo.Add(mac);
                    }
                }
            }
            catch
            {
            }
            finally
            {
            }
            return ar_macInfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List < string> GetIPAddress()
        {
            List <string > ar_Ip=new List<string> ();
            try
            {
                //获取IP地址     
                string st = " ";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        //st=mo[ "IpAddress "].ToString();     
                        System.Array ar;
                        ar = (System.Array)(mo.Properties["IpAddress"].Value);
                        st = ar.GetValue(0).ToString();
                        ar_Ip.Add(st);
                    }
                }
            }
            catch
            {
            }
            finally
            {
            }
            return ar_Ip;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List < string> GetDiskID()
        {
            List<string> ar_Disk = new List<string>();
            try
            {
                //获取硬盘ID     
                String HDid = " ";
                ManagementClass mc = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    HDid = (string)mo.Properties["Model"].Value;
                    ar_Disk.Add(HDid);
                }
            }
            catch
            {
            }
            finally
            {
            }
            return ar_Disk;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List < string> GetUserName()
        {
            List<string> ar_User = new List<string>();
            try
            {
                string st = " ";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["UserName"].ToString();
                    ar_User.Add(st);
                }
            }
            catch
            {
            }
            finally
            {
            }
            return ar_User;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List <string> GetSystemType()
        {
            List < string > ar_St=new List<string> ();
            try
            {
                string st = " ";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["SystemType"].ToString();
                    ar_St.Add (st );

                }
            }
            catch
            {
            }
            finally
            {
            }
            return ar_St;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List < string> GetTotalPhysicalMemory()
        {
            List<string> ar_memory = new List<string>();
            try
            {

                string st = " ";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["TotalPhysicalMemory"].ToString();
                    ar_memory.Add(st);
                }
            }
            catch
            {
            }
            finally
            {
            }
            return ar_memory;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetComputerName()
        {
            try
            {
                return System.Environment.GetEnvironmentVariable("ComputerName");
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }  
    }


}
