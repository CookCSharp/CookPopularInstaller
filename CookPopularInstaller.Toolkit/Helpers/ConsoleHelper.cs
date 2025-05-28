/*
 * Description：ConsoleHelper 
 * Author： Chance.Zheng
 * Create Time: 2023/7/12 10:05:24
 * .Net Version: 2.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2023 All Rights Reserved.
 */


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CookPopularInstaller.Toolkit.Helpers
{
    public static class ConsoleHelper
    {
        /// <summary>
        /// 获取窗口句柄
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 设置窗体的显示与隐藏
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        public static void ShowOrHideConsole(bool isShowConsole = true, string consoleTitle = "")
        {
            consoleTitle = string.IsNullOrEmpty(consoleTitle) ? Console.Title : consoleTitle;
            IntPtr hWnd = FindWindow("ConsoleWindowClass", consoleTitle);
            if (hWnd != IntPtr.Zero)
            {
                if (isShowConsole)
                    ShowWindow(hWnd, 1);
                else
                    ShowWindow(hWnd, 0);
            }
        }
    }
}