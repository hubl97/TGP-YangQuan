using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading ;

namespace PRC_Tool
{
    /// <summary>
    /// ��ȫ�Ͷ���
    /// </summary>
    public  class SafeQueue:Queue
    {
        /// <summary>
        /// 
        /// </summary>
        public int I_MAX_COUNT = 1000;
        /// <summary>
        /// 
        /// </summary>
        private Mutex hMutex = new Mutex();
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="obj"></param>
        public override void Enqueue(object obj)
        {
            if (Count > I_MAX_COUNT) return;
            try
            {
                hMutex.WaitOne();
                base.Enqueue(obj);
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// �Ƴ�����
        /// </summary>
        /// <returns></returns>
        public override object Dequeue()
        {
            if (Count <= 0) return null;
            try
            {
                hMutex.WaitOne();
                return base.Dequeue();
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// �������
        /// </summary>
        public override void Clear()
        {
            try
            {
                hMutex.WaitOne();
                base.Clear();
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <returns></returns>
        public override object Peek()
        {
            try
            {
                hMutex.WaitOne();
                return base.Peek();
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
        }
    }
}
