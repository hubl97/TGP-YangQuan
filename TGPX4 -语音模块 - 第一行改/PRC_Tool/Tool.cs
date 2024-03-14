using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public  class Tool
    {
        /// <summary>
        /// 
        /// </summary>
        public static DateTime dt1;
        /// <summary>
        /// 
        /// </summary>
        public static DateTime dt2;
        /// <summary>
        /// 计算一个数的2的n次方
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int Squar2(int x)
        {
            int res = 1;
            if (x > 32) return 0;
            for (int i = 0; i < x; i++)
            {
                res = res * 2;
            }
            return res;
        }
        /// <summary>
        /// 计算一个数(大于1)整除2的次方
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int Squart2(int x)
        {
            int res = 0;
            int v = x;
            while (v > 1)
            {
                v = (int)(v / 2);
                res++;
            }
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static bool BitAnd(byte v, int bit)
        {
            if ((v & Squar2(bit)) == 0)
                return false;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="bitv"></param>
        /// <returns></returns>
        public static bool BitAndValue(UInt32 v, UInt32 bitv)
        {
            if ((v & bitv) == 0)
                return false;
            return true;
        }
        /// <summary>
        /// 返回字节中指定比特转换的无符号整数
        /// </summary>
        /// <param name="v">8位无符号整数</param>
        /// <param name="bit">起始位</param>
        /// <param name="size">要转换的比特数</param>
        /// <returns></returns>
        public static byte GetBitValue(byte v, int bit, int size)
        {
            byte res = 0;
            bit = bit % 8;
            for (int i = bit; i < bit + size; i++)
            {
                res = (byte)(res | (byte)Squar2(i));
            }
            res = (byte)(res & v);
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="bit0"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte GetDataValue(byte v, int bit0, int size)
        {
            int bit = bit0 % 8;
            if ((bit + size) > 8) return 0;
            byte res = GetBitValue(v, bit, size);
            res = (byte)(res >> bit);
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static byte HIBYTE(ushort v)
        {
            byte x =(byte )( v >> 8);
            return x;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static byte LOBYTE(ushort v)
        {
            byte x = (byte)(v & 0xff);
            return x;
        }
        /// <summary>
        /// 返回字节中指定比特位清0后的无符号整数
        /// </summary>
        /// <param name="v"></param>
        /// <param name="bit"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte MASK_BIT(byte v,int bit, int size)
        {
            if ((bit + size) > 8) return v;
            for (int i = bit; i < bit + size; i++)
            {
                v =(byte ) (v & ~(byte )Squar2(i));
            }
            return v;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        public static void DataRowInit(ref DataRow row)
        {
            if (row == null || row.Table == null) return;
            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                switch (row.Table.Columns[i].DataType.ToString())
                {
                    case "System.String":
                        row[i] = "";
                        break;
                    case "System.DateTime":
                        row[i] = new DateTime(1900, 1, 1, 0, 0, 0); ;
                        break;
                    default:
                        row[i] = 0;
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        public static void DataRowInit(DataRow row)
        {
            if (row == null || row.Table == null) return;
            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                switch (row.Table.Columns[i].DataType.ToString())
                {
                    case "System.String":
                        row[i] = "";
                        break;
                    case "System.DateTime":
                        row[i] = new DateTime(1900, 1, 1, 0, 0, 0); ;
                        break;
                    default:
                        row[i] = 0;
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="big"></param>
        /// <param name="index"></param>
        /// <param name="small"></param>
        /// <returns></returns>
        public static bool bEqual_Buf(byte[] big, int index, byte[] small)
        {
            if (big.Length < small.Length + index) return false;
            for (int i = 0; i < small.Length; i++)
            {
                if (big[index + i] != small[i])
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="big"></param>
        /// <param name="index"></param>
        /// <param name="sz"></param>
        /// <param name="small"></param>
        /// <returns></returns>
        public static int LocateBuffer(byte[] big, int index, int sz, byte[] small)
        {
            for (int i = index; i < index + sz; i++)
            {
                if (bEqual_Buf(big, i, small)) return i;
            }
            return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumber_Only(string s)
        {
            bool res = false;
            try
            {
                Int64 x= System.Convert.ToInt64(s);
                res = true;
            }
            catch
            {
            }
            return res ;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Roma2Char(string s)
        {
            string[] roma = new string[] { "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ" };
            string[] char_str = new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };
            string res = s;
            for (int i = roma.Length - 1; i >= 0; i--)
            {
                if (res.Contains(roma[i]) == false) continue;
                res = res.Replace(roma[i], char_str[i]);
            }
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Char2Roma(string s)
        {
            string[] roma = new string[] { "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ" };
            string[] char_str = new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };
            string res = s;
            for (int i = roma.Length - 1; i >= 0; i--)
            {
                if (res.Contains(char_str[i]) == false) continue;
                res = res.Replace(char_str[i], roma[i]);
            }
            return res;
        }

    }
}
