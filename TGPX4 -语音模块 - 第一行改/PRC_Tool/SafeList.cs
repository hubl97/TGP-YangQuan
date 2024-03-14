using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

namespace PRC_Tool
{
    /// <summary>
    /// 安全型链表
    /// </summary>
    public  class SafeArrayList:ArrayList
    {
        /// <summary>
        /// 
        /// </summary>
        private Mutex hMutex = new Mutex();
        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override int Add(object value)
        {
            try
            {
                hMutex.WaitOne();
                return base.Add(value);
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
            
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="obj"></param>
        public override void Remove(object obj)
        {
            try
            {
                hMutex.WaitOne();
                base.Remove(obj);
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// 指定位置移除
        /// </summary>
        /// <param name="index"></param>
        public override void RemoveAt(int index)
        {
            try
            {
                hMutex.WaitOne();
                base.RemoveAt(index);
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// 数据排序
        /// </summary>
        public override void Sort()
        {
            try
            {
                hMutex.WaitOne();
                base.Sort();
            }
            finally
            {
                hMutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// 数据清空
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
    }
}
