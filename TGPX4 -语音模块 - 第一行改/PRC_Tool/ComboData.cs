using System;
using System.Collections.Generic;
using System.Text;

namespace PRC_Tool
{
    /// <summary>
    /// 
    /// </summary>
    public  class ComboData
    {
        /// <summary>
        /// 
        /// </summary>
        public  int id = 0;
        /// <summary>
        /// 
        /// </summary>
        public  string text = "";
        /// <summary>
        /// 
        /// </summary>
        public string name = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public ComboData(int x, string y)
        {
            id = x;
            text = y;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="na"></param>
        /// <param name="y"></param>
        public ComboData(string na, string y)
        {
            name  = na;
            text = y;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //return base.ToString();
            return text;
        }
    }
}
