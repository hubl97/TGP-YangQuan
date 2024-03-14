using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace PRC_Tool
{
    /// <summary>
    /// 圆形缓冲区
    /// </summary>
    public  class Queue_Cir
    {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        private   int I_SIZE = 10000;
        /// <summary>
        /// 缓冲区定义
        /// </summary>
        private byte[] Data = new byte[10000];
        /// <summary>
        /// 缓冲区备份
        /// </summary>
        private byte[] Data_Bak = new byte[10000];
        /// <summary>
        /// 头指针
        /// </summary>
        private int I_HEAD = 0;
        /// <summary>
        /// 尾指针
        /// </summary>
        private int I_TAIL = 0;
        /// <summary>
        /// 处理互斥
        /// </summary>
        private Mutex hMutex = new Mutex();
        /// <summary>
        /// 判断缓冲区是否空
        /// </summary>
        public bool bEmpty
        {
            get{
                if (I_HEAD == I_TAIL) return true;
                return false;
            }
        }
        /// <summary>
        /// 判断缓冲区是否满
        /// </summary>
        public bool bFull
        {
            get
            {
                if ( (I_TAIL == I_HEAD - 1) || (I_HEAD == I_SIZE - 1 && I_TAIL == 0) )
                    return true;
                return false; 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Queue_Cir()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public Queue_Cir(int size)
        {
            I_SIZE = size;
            Data = new byte[I_SIZE];
            Data_Bak = new byte[I_SIZE];
        }
        /// <summary>
        /// 初始化缓冲区
        /// </summary>
        public void Reset()
        {
            Data.Initialize();
            Data_Bak.Initialize();
            I_HEAD = 0;
            I_TAIL = 0;
        }
        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public bool Add(byte[] d)
        {
            if (d == null) return true;
            return  Add(d, d.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public bool Add(byte[] d, int len)
        {
            bool res = true;
            if( len ==0) return res ;
            try
            {
                hMutex.WaitOne();
                for (int i = 0; i < len; i++)
                {

                    Data[I_HEAD] = d[i];
                    I_HEAD++;
                    if (I_HEAD == I_SIZE)
                        I_HEAD = 0;
                    if (I_HEAD == I_TAIL && i<len -1)
                        res = false; ;
                }
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
            return res;
        }
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns></returns>
        public byte[] Read()
        {
            
            byte[] res = new byte[0];
            if (I_TAIL == I_HEAD) return res;
            try
            {
                hMutex.WaitOne();
                if (I_HEAD > I_TAIL)
                {
                    res = new byte[I_HEAD - I_TAIL];
                    Buffer.BlockCopy(Data, I_TAIL, res, 0, res.Length);
                    I_TAIL = I_HEAD;
                }
                else
                {
                    res = new byte[I_SIZE - I_TAIL + I_HEAD];
                    Buffer.BlockCopy(Data, I_TAIL, res, 0, I_SIZE - I_TAIL);
                    Buffer.BlockCopy(Data, 0, res, I_SIZE - I_TAIL, I_HEAD);
                    I_TAIL = I_HEAD;
                }
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
            return res;
        }
        /// <summary>
        /// 按照包头、包尾条件读取
        /// </summary>
        /// <param name="d_h"></param>
        /// <param name="d_t"></param>
        /// <returns></returns>
        public byte[] Read_To(byte d_h, byte d_t)
        {
            byte[] res = new byte[0];
            if (I_TAIL == I_HEAD) return res;
            int i_start = -1;
            int i_end = -1 ;
            try
            {
                hMutex.WaitOne();

                if (I_HEAD > I_TAIL)
                {
                    for (int i = I_TAIL; i < I_HEAD; i++)
                    {
                        //找到完整包
                        if (i_end <0 && i_start  >=0 && Data[i] == d_t)
                        {
                            i_end =i;
                            res = new byte[i_end - i_start + 1];
                            Buffer.BlockCopy(Data, i_start, res, 0, res.Length);
                            I_TAIL = i_end  + 1;
                            break;
                        }
                        if (Data[i] == d_h)
                        {
                            i_start = i;
                        }
                    }
                }
                else
                {
                    //在Buffer没有满时找到数据
                    for (int i = I_TAIL; i < I_SIZE; i++)
                    {
                        if (Data[i] == d_h)
                            i_start = i;
                        else if (i_end < 0 && i_start >= 0 && Data[i] == d_t)
                        {
                            i_end = i;
                            res = new byte[i_end - i_start + 1];
                            Buffer.BlockCopy(Data, i_start, res, 0, res.Length);
                            I_TAIL = i_end + 1;
                            if (I_TAIL == I_SIZE)
                                I_TAIL = 0;
                            break;
                        }
                    }
                    //Buffer满后没有找到数据，继续找数据。
                    if (i_end < 0)
                    {
                        for (int i = 0; i < I_HEAD; i++)
                        {
                            if (Data[i] == d_h)
                                i_start = i;
                            else if (i_end < 0 && i_start >= 0 && Data[i] == d_t)
                            {
                                i_end = i;
                                //所有数据都在0后
                                if (i_end > i_start)
                                {
                                    res = new byte[i_end - i_start + 1];
                                    Buffer.BlockCopy(Data, i_start, res, 0, res.Length);
                                }
                                else//有数据在接近Buffer满前
                                {
                                    res = new byte[I_SIZE - i_start + i_end + 1];
                                    Buffer.BlockCopy(Data, i_start, res, 0, I_SIZE-i_start );
                                    Buffer.BlockCopy(Data, 0, res, I_SIZE - i_start, i_end + 1); 
                                }
                                I_TAIL = i_end + 1;
                                if (I_TAIL == I_SIZE)
                                    I_TAIL = 0;
                                break;
                            }

                        }
                    }
                }
                
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
            return res;
        }
        /// <summary>
        /// 根据TBZK驼峰协议确定的数据读取部分,包头
        /// </summary>
        private static byte[] ar_Head = new byte[] { 0x24};
        /// <summary>
        /// 
        /// </summary>
        private static byte[] ar_Head_tmp = new byte[] { 0xf7, 0xf7, 0xf7 };
        /// <summary>
        /// 包头长度
        /// </summary>
        private const ushort I_Head_Len = 20;
        /// <summary>
        /// 正文长度位置
        /// </summary>
        private const int I_TextLen_Pos = 11;
        private const int I_TextLen = 110;
        /// <summary>
        /// CRC校验长度
        /// </summary>
        private const int I_CRC_LEN = 2;
        /// <summary>
        /// 最小包长包头+CRC
        /// </summary>
        private const int I_MIN_PAK_SIZE = 0;


        /// <summary>
        /// 
        /// </summary>
        public byte[] Read_HumpTBZK()
        {
            byte[] res = new byte[0];
            if (I_TAIL == I_HEAD) return res;
            int i_start = -1;
            try
            {
                hMutex.WaitOne();
                //头大于尾的情况
                if (I_HEAD > I_TAIL)
                {
                    if ((I_HEAD - I_TAIL) < I_MIN_PAK_SIZE )
                        return res;
                    res = new byte[I_MIN_PAK_SIZE ];
                    
                    for (int i = I_TAIL; i <= I_HEAD ; i++)
                    {
                        //定位包头
                        i_start = PRC_Tool.Tool.LocateBuffer(Data, i,  I_HEAD -I_TAIL, ar_Head);
                        if( i_start <0) continue ;
                        break;
                    }
                    //找不到包头返回
                    if (i_start < 0) return null ;
                    //定位正文长度位置
                    //ushort txtlen = BitConverter.ToUInt16(Data, i_start + I_TextLen_Pos);
                    ushort txtlen = I_TextLen;
                    if ((i_start + txtlen ) > I_HEAD)
                        return null;
                    res = new byte[I_MIN_PAK_SIZE + txtlen];
                    Buffer.BlockCopy(Data, i_start, res, 0, res.Length);
                    I_TAIL = i_start +res.Length ;
                    //已经读空的情况下，初始化圆形缓冲区并置空，从0开始
                    if (I_TAIL == I_HEAD)
                    {
                        Data.Initialize();
                        I_TAIL = 0;
                        I_HEAD = 0;
                    }
                }
                else
                {
                    int datalen = I_SIZE - I_TAIL + I_HEAD;
                    if (datalen < I_MIN_PAK_SIZE) return null;

                    //把Buff需要处理的内容 放置到Data_Bak中 
                    Buffer.BlockCopy(Data, I_TAIL, Data_Bak, 0, I_SIZE - I_TAIL);
                    Buffer.BlockCopy(Data, 0, Data_Bak, I_SIZE - I_TAIL, I_HEAD);
                    for (int i = 0; i <= datalen; i++)
                    {
                        //定位包头
                        i_start = PRC_Tool.Tool.LocateBuffer(Data_Bak, i, datalen -I_MIN_PAK_SIZE, ar_Head);
                        if (i_start < 0) continue;
                        break;
                    }
                    //找不到包头返回
                    if (i_start < 0) return null;
                    //定位正文长度位置
                    //ushort txtlen = BitConverter.ToUInt16(Data_Bak, i_start + I_TextLen_Pos);
                    ushort txtlen = I_TextLen;
                    if ((i_start + txtlen + I_MIN_PAK_SIZE) > datalen)
                        return null;
                    res = new byte[I_MIN_PAK_SIZE + txtlen];
                    Buffer.BlockCopy(Data_Bak, i_start, res, 0, res.Length);
                    if (res.Length < I_SIZE - I_TAIL)
                        I_TAIL = I_TAIL + res.Length;
                    else
                        I_TAIL = res.Length - (I_SIZE - I_TAIL);
                }

            }
            finally
            {
                hMutex.ReleaseMutex();
            }
            return res;
        }


    }
}
