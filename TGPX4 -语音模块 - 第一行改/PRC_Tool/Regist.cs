using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public class Regist
    {
        /// <summary>
        /// 
        /// </summary>
        public enum RegistryType:byte
        {
            /// <summary>
            /// 
            /// </summary>
            LocalMachine=0,
            /// <summary>
            /// 
            /// </summary>
            CurrentUser,
            /// <summary>
            /// 
            /// </summary>
            CurrentConfig
        }
        /// <summary>
        /// 
        /// </summary>
        public static RegistryType kt = RegistryType.LocalMachine;
        /// <summary>
        /// 
        /// </summary>
        private static RegistryKey reg_Key
        {
            get
            {
                switch (kt)
                {
                    case RegistryType.LocalMachine:
                        return Registry.LocalMachine;
                    case RegistryType.CurrentUser:
                        return Registry.CurrentUser;
                    case RegistryType.CurrentConfig:
                        return Registry.CurrentConfig;
                    default:
                        return null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rk"></param>
        /// <param name="subkeyname"></param>
        /// <returns></returns>
        public static RegistryKey Open_SubKey(RegistryKey rk, string subkeyname)
        {
            if (rk == null)
                rk = reg_Key;
            RegistryKey r= rk.OpenSubKey(subkeyname);
            return r;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rk"></param>
        /// <param name="subkeyname"></param>
        /// <returns></returns>
        public static RegistryKey Create_SubKey(RegistryKey rk, string subkeyname)
        {
            if (rk == null)
                rk = reg_Key;
            RegistryKey  r= rk.CreateSubKey(subkeyname);
            return r;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rk"></param>
        /// <param name="subkeyname"></param>
        public static void Delete_SubKey(RegistryKey rk, string subkeyname)
        {
            if (rk == null)
                rk = reg_Key;
            rk.DeleteSubKeyTree(subkeyname);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rk"></param>
        public static void Close_Key(RegistryKey rk)
        {
            if (rk != null)
                rk = reg_Key;
            rk.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rk"></param>
        /// <param name="name"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool SetValue(RegistryKey rk, string name,string v)
        {
            if (rk == null)
                return false;
            rk.SetValue(name, v);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rk"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool DeleteValue(RegistryKey rk, string name)
        {
            if (rk == null)
                return false;
            rk.DeleteValue(name);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rk"></param>
        /// <param name="valuename"></param>
        /// <returns></returns>
        public static bool bValueExist(RegistryKey rk, string valuename)
        {
            if (rk == null)
                rk = reg_Key;
            string[] s = rk.GetValueNames();
            if (s == null)
                return false;
            foreach (string a in s)
            {
                if (a == valuename)
                    return true;
            }
            return false;
        }
    }
}
