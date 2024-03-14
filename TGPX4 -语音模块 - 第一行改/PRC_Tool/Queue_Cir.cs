using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace PRC_Tool
{
    /// <summary>
    /// Բ�λ�����
    /// </summary>
    public  class Queue_Cir
    {
        /// <summary>
        /// ��������С
        /// </summary>
        private   int I_SIZE = 10000;
        /// <summary>
        /// ����������
        /// </summary>
        private byte[] Data = new byte[10000];
        /// <summary>
        /// ����������
        /// </summary>
        private byte[] Data_Bak = new byte[10000];
        /// <summary>
        /// ͷָ��
        /// </summary>
        private int I_HEAD = 0;
        /// <summary>
        /// βָ��
        /// </summary>
        private int I_TAIL = 0;
        /// <summary>
        /// ������
        /// </summary>
        private Mutex hMutex = new Mutex();
        /// <summary>
        /// �жϻ������Ƿ��
        /// </summary>
        public bool bEmpty
        {
            get{
                if (I_HEAD == I_TAIL) return true;
                return false;
            }
        }
        /// <summary>
        /// �жϻ������Ƿ���
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
        /// ��ʼ��������
        /// </summary>
        public void Reset()
        {
            Data.Initialize();
            Data_Bak.Initialize();
            I_HEAD = 0;
            I_TAIL = 0;
        }
        /// <summary>
        /// ��������
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
        /// ��ȡ����
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
        /// ���հ�ͷ����β������ȡ
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
                        //�ҵ�������
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
                    //��Bufferû����ʱ�ҵ�����
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
                    //Buffer����û���ҵ����ݣ����������ݡ�
                    if (i_end < 0)
                    {
                        for (int i = 0; i < I_HEAD; i++)
                        {
                            if (Data[i] == d_h)
                                i_start = i;
                            else if (i_end < 0 && i_start >= 0 && Data[i] == d_t)
                            {
                                i_end = i;
                                //�������ݶ���0��
                                if (i_end > i_start)
                                {
                                    res = new byte[i_end - i_start + 1];
                                    Buffer.BlockCopy(Data, i_start, res, 0, res.Length);
                                }
                                else//�������ڽӽ�Buffer��ǰ
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
        /// ����TBZK�շ�Э��ȷ�������ݶ�ȡ����,��ͷ
        /// </summary>
        private static byte[] ar_Head = new byte[] { 0x24};
        /// <summary>
        /// 
        /// </summary>
        private static byte[] ar_Head_tmp = new byte[] { 0xf7, 0xf7, 0xf7 };
        /// <summary>
        /// ��ͷ����
        /// </summary>
        private const ushort I_Head_Len = 20;
        /// <summary>
        /// ���ĳ���λ��
        /// </summary>
        private const int I_TextLen_Pos = 11;
        private const int I_TextLen = 110;
        /// <summary>
        /// CRCУ�鳤��
        /// </summary>
        private const int I_CRC_LEN = 2;
        /// <summary>
        /// ��С������ͷ+CRC
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
                //ͷ����β�����
                if (I_HEAD > I_TAIL)
                {
                    if ((I_HEAD - I_TAIL) < I_MIN_PAK_SIZE )
                        return res;
                    res = new byte[I_MIN_PAK_SIZE ];
                    
                    for (int i = I_TAIL; i <= I_HEAD ; i++)
                    {
                        //��λ��ͷ
                        i_start = PRC_Tool.Tool.LocateBuffer(Data, i,  I_HEAD -I_TAIL, ar_Head);
                        if( i_start <0) continue ;
                        break;
                    }
                    //�Ҳ�����ͷ����
                    if (i_start < 0) return null ;
                    //��λ���ĳ���λ��
                    //ushort txtlen = BitConverter.ToUInt16(Data, i_start + I_TextLen_Pos);
                    ushort txtlen = I_TextLen;
                    if ((i_start + txtlen ) > I_HEAD)
                        return null;
                    res = new byte[I_MIN_PAK_SIZE + txtlen];
                    Buffer.BlockCopy(Data, i_start, res, 0, res.Length);
                    I_TAIL = i_start +res.Length ;
                    //�Ѿ����յ�����£���ʼ��Բ�λ��������ÿգ���0��ʼ
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

                    //��Buff��Ҫ��������� ���õ�Data_Bak�� 
                    Buffer.BlockCopy(Data, I_TAIL, Data_Bak, 0, I_SIZE - I_TAIL);
                    Buffer.BlockCopy(Data, 0, Data_Bak, I_SIZE - I_TAIL, I_HEAD);
                    for (int i = 0; i <= datalen; i++)
                    {
                        //��λ��ͷ
                        i_start = PRC_Tool.Tool.LocateBuffer(Data_Bak, i, datalen -I_MIN_PAK_SIZE, ar_Head);
                        if (i_start < 0) continue;
                        break;
                    }
                    //�Ҳ�����ͷ����
                    if (i_start < 0) return null;
                    //��λ���ĳ���λ��
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
