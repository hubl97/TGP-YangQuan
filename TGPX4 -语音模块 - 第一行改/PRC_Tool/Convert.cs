using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Data;
namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public  class Convert
    {
        /// <summary>
        /// 把Bytes[]转换为结构
        /// </summary>
        /// <param name="data"></param>
        /// <param name="off"></param>
        /// <param name="str_type"></param>
        /// <returns></returns>
        public static object Bytes2Struct(byte[] data, int off, Type str_type)
        {
            //CIPS_PLAN cp;
            int size_plan = Marshal.SizeOf(str_type);
            IntPtr buf = Marshal.AllocHGlobal(size_plan);
            try
            {
                Marshal.Copy(data, off, buf, size_plan);
                return (object)Marshal.PtrToStructure(buf, str_type);
            }
            catch
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }
        /// <summary>
        /// 字符串转换为Byte[]
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] String2Bytes(string s)
        {
            return System.Text.Encoding.Default.GetBytes(s);
        }
        /// <summary>
        /// 结构序列化为连续的Byte[]
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="str_type"></param>
        /// <returns></returns>
        public static byte[] Struct2Bytes(object obj, Type str_type)
        {
            //CIPS_PLAN cp;
            int sz = Marshal.SizeOf(str_type);
            IntPtr buf = Marshal.AllocHGlobal(sz);
            byte[] data = new byte[sz];
            try
            {
                Marshal.StructureToPtr(obj, buf, true);
                Marshal.Copy(buf, data, 0, sz);
            }
            catch
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
            return data;
        }

        /// <summary>
        /// 微软的字节流序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="send"></param>
        public static void BinarySerializeObj(object obj, out byte[] send)
        {
            send = null;
            MemoryStream tempms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                formatter.Serialize(tempms, obj);
                send = new byte[tempms.Length];
                tempms.Seek(0, SeekOrigin.Begin);
                tempms.Read(send, 0, (int)tempms.Length);
            }
            catch(Exception e)
            {
            }
            finally
            {
                tempms.Close();
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="send"></param>
        /// <param name="obj"></param>
        public static void BinaryDesrializeObj(byte[] send, out object obj)
        {
            MemoryStream tempms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            obj = null;
            try
            {
                tempms.Write(send, 0, send.Length);
                tempms.Seek(0, SeekOrigin.Begin);
                obj = formatter.Deserialize(tempms);
            }
            catch
            {
            }
            finally
            {
                tempms.Close();
            }
        }
        /// <summary>
        /// 把二进制字节流转换为字符串
        /// </summary>
        /// <param name="b"></param>
        /// <param name="ss"></param>
        public static void ConvertBinary_String(byte [] b,ref string ss)
        {
            ss = "";
            if (b == null) return;
            for (int i = 0; i < b.Length ; i++)
            {
                string s = ConvertByte_String(b[i]);
                ss += s;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="len"></param>
        /// <param name="ss"></param>
        public static void ConvertBinary_String(byte[] b,int len, ref string ss)
        {
            ss = "";
            if (b == null) return;
            if (len > b.Length)
                len = b.Length;
            for (int i = 0; i < len ; i++)
            {
                string s = ConvertByte_String(b[i]);
                ss += s;
            }
        }
        /// <summary>
        /// 把一个Byte转换为以16进制显示的字符串
        /// </summary>
        /// <param name="b"></param>
        public static string  ConvertByte_String(byte b)
        {
            string s = Uri.HexEscape((char)b);
            s = s.Replace('%', ' ');
            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="filena"></param>
        public static  void Read(ref DataSet ds,string filena)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filena, FileMode.Open, FileAccess.Read, FileShare.Read);
            ds = (DataSet)formatter.Deserialize(stream);
            stream.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="filena"></param>
        public static  void Write(DataSet ds,string filena)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(filena, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, ds);
                stream.Close();
            }
            catch
            {
            }
        }

    }
}
