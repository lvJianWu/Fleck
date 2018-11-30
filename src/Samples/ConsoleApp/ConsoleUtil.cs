/***************************************************************************
 * Copyright (c) 2018 All Rights Reserved.
 * CLR版本： 4.0.30319.42000
 *机器名称：LVJW-PC
 *公司名称：北京华誉维诚技术服务有限公司
 *命名空间：Trusit.TableWrite
 *文件名：  ConsoleUtil
 *唯一标识：f1ebcf90-4555-4849-99b8-8e61665f8462
 *当前的用户域：CORP
 *创建人：  lvjw
 *创建时间：2018-11-29 12:33:18
 *描述：
 *
 *=====================================================================*/
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trusit.TableWrite
{
    public class windowUtilities
    {
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>
        /// 设置窗体隐藏
        /// </summary>
        /// <param name="consoleTitle">窗体的Title</param>
        public static void WindowHide(string consoleTitle)
        {
            IntPtr a = FindWindow("ConsoleWindowClass", consoleTitle);
            if (a != IntPtr.Zero)
                ShowWindow(a, 0);//隐藏窗口
            else
                throw new Exception("can't hide console window");
        }
        /// <summary>
        /// 设置程序开机自启动
        /// </summary>
        public static void SetAutoStart()
        {
            string R_startPath = Application.ExecutablePath;
            RegistryKey rk_local = Registry.LocalMachine;
            RegistryKey rk_run = rk_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            //rk_run.DeleteSubKey("ConsoleApp_yh", false);

            rk_run.SetValue("ConsoleApp_yh", R_startPath);
            rk_run.Close();
            rk_local.Close();
        }
    }
}
