using System;
using System.Collections.Generic;
using System.Text;

namespace CIPS.CTC.NET
{
    interface IComm
    {
        void Add(byte[] a, int len);
        bool Read(ref byte[] data, ref int len);
        bool Read_Blocking(ref  byte[] data, ref int len);
        int Read_To(byte vh, byte vt, ref byte[] data, ref int len);
    }
}
