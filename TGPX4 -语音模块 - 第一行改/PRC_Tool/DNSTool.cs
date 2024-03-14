using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public  class DNSTool
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IPAddress[] GetLocalIP()
        {
            string hn = Dns.GetHostName();
            IPAddress []ips= Dns.GetHostAddresses(hn);
            return ips;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bip"></param>
        /// <returns></returns>
        public static string GetLocalIPString(string bip)
        {
            IPAddress[] ip_a = GetLocalIP();
            if (ip_a == null || ip_a.Length == 0) return "";
            string[] dd = bip.Split('.');
            byte[] v_ip = new byte[4];
            for (int i=0; i<  dd. Length ; i++)
            {
                if(Tool .IsNumber_Only (dd [i]))
                    v_ip[i]=System .Convert .ToByte(dd [i]);

            }

            foreach (IPAddress ia in ip_a )
            {
                byte[] i_ip = ia.GetAddressBytes();
                if (i_ip.Length != 4) continue;
                if ((v_ip[0] == 0 || i_ip[0] == v_ip[0]) &&
                    (v_ip[1] == 0 || v_ip[1] == i_ip[1]) &&
                    (v_ip[2] == 0 || v_ip[2] == i_ip[2]) &&
                    (v_ip[3] == 0 || v_ip[3] == i_ip[3]))
                    return ia.ToString();
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string[] GetLocalIPString()
        {
            IPAddress[] ip_ar = GetLocalIP();
            if (ip_ar == null || ip_ar .Length ==0) return null ;
            string[] ss = new string[ip_ar.Length];
            for (int i = 0; i < ip_ar.Length; i++)
            {
                ss[i] = ip_ar[i].ToString();
            }
            return ss;
        }
    }
}
