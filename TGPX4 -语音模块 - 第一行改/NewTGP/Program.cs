using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NewTGP
{
    /*
    public class TGPDispIni
    {
        public TGPDisp TGPdisp;
        public TGPDispIni()
        {
            TGPdisp = new TGPDisp();
        }

    }
    */
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new TGPDisp());
            Process instance = RunningInstance();
            if (instance == null)
                Application.Run(new TGPDisp());
            else
                HandleRunningInstance(instance);


        }
        #region 运行一次
        public static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (process.ProcessName == current.ProcessName)
                        return process;
                }
            }
            return null;
        }
        public static void HandleRunningInstance(Process instance)
        {          
            ShowWindowAsync(instance.MainWindowHandle, 1);
         //   SetForegroundwindow(instance.MainWindowHandle);
            SetWindowPos(instance.MainWindowHandle, -1, 0, 0, 0, 0, 1 | 2);
            
        }
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(System.IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundwindow(System.IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags); 
        #endregion
    }
}
