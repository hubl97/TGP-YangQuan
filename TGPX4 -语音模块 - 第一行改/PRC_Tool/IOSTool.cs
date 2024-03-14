using System;
using System.Collections.Generic;
using System.Text;

namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public class IOSTool
    {
        #region 得到操作系统名称
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetOperatingSystemName()
        {
            string OSName = null;
            System.OperatingSystem osInfo = System.Environment.OSVersion;
            switch (osInfo.Platform)
            {
                case PlatformID.Unix:
                    break;
                case PlatformID.Win32NT:
                    switch (osInfo.Version.Major)
                    {
                        case 3:
                            OSName = "Windows NT 3.51";
                            break;
                        case 4:
                            OSName = "Windows NT 4.0";
                            break;
                        case 5:
                            switch (osInfo.Version.Minor)
                            {
                                case 0:
                                    OSName = "Windows 2000";
                                    break;
                                case 1:
                                    OSName = "Windows XP";
                                    break;
                                case 2:
                                    OSName = "Windows 2003";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 6:
                            switch (osInfo.Version.Minor)
                            {
                                case 0:
                                    OSName = "Windows Vista";
                                    break;
                                case 1:
                                    OSName = "Windows 7";
                                    break;
                            }
                            break;
                        default:
                            OSName = "Unknown Win32NT Windows";
                            break;
                    }
                    break;
                case PlatformID.Win32S:
                    break;
                case PlatformID.Win32Windows:
                    switch (osInfo.Version.Major)
                    {
                        case 0:
                            OSName = "Windows 95";
                            break;
                        case 10:
                            if (osInfo.Version.Revision.ToString() == "2222A")
                                OSName = "Windows 98 Second Edition";
                            else
                                OSName = "Windows 98";
                            break;
                        case 90:
                            OSName = "Windows ME";
                            break;
                        default:
                            OSName = "Unknown Win32 Windows";
                            break;
                    }
                    break;
                case PlatformID.WinCE:
                    break;
                default:
                    break;
            }
            //if (osInfo.ServicePack != null)
            //    OSName += " " + osInfo.ServicePack;

            return OSName /*+ string.Format(" ({0})", osInfo.VersionString)*/;
        }
        #endregion
    }
}
