using System;
using System.Collections.Generic;
using System.Text;

namespace CIPS.CTC.NET
{
    public class E_DATARECV_TYPE
    {
        /// <summary>
        /// ���ݽ��ճɹ�
        /// </summary>
        public static int SUCC = 0;
        /// <summary>
        /// ͨ�Ŷ˿ڹ���
        /// </summary>
        public static int ERR = 1;
        /// <summary>
        /// ���ݳ���
        /// </summary>
        public static int OVERRIDE = 2;
        /// <summary>
        /// ���ճ�ʱ
        /// </summary>
        public static int TIMEOUT = 3;
        /// <summary>
        /// CRC ����
        /// </summary>
        public static int CRC_ERR = 4;
        /// <summary>
        /// BCC����
        /// </summary>
        public static int BCC_ERR = 5;
        /// <summary>
        /// ������
        /// </summary>
        public static int NULL = 6;
    }
}
