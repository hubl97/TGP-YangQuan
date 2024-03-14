using System;
using System.Collections.Generic;
using System.Text;

namespace CIPS.CTC.NET
{
    public class E_DATARECV_TYPE
    {
        /// <summary>
        /// 数据接收成功
        /// </summary>
        public static int SUCC = 0;
        /// <summary>
        /// 通信端口故障
        /// </summary>
        public static int ERR = 1;
        /// <summary>
        /// 数据超长
        /// </summary>
        public static int OVERRIDE = 2;
        /// <summary>
        /// 接收超时
        /// </summary>
        public static int TIMEOUT = 3;
        /// <summary>
        /// CRC 错误
        /// </summary>
        public static int CRC_ERR = 4;
        /// <summary>
        /// BCC错误
        /// </summary>
        public static int BCC_ERR = 5;
        /// <summary>
        /// 无数据
        /// </summary>
        public static int NULL = 6;
    }
}
