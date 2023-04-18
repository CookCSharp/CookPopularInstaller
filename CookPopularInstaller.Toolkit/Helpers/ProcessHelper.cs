using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：ProcessHelper 
 * Author： Chance.Zheng
 * Create Time: 2023/3/16 14:28:13
 * .Net Version: 6.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Toolkit.Helpers
{
    public static class ProcessHelper
    {
        private static readonly string CmdPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\cmd.exe";
        private static string _command;
        private static StringBuilder _outputData;
        private static StringBuilder _errorData;

        public static Process CurrentProcess { get; private set; }
        public static string CmdHeader { get; private set; }


        public static void CreateFolderRunAsAdmin(string folder)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = CmdPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = $"/c mkdir \"{folder}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
            });

            process.WaitForExit();
            process.Close();
        }

        public static void StartProcess(string fileName, string arguments, bool isAdmin = false)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = fileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = $"{arguments}",
                WindowStyle = ProcessWindowStyle.Hidden,
            });

            if (isAdmin)
            {
                process.StartInfo.Verb = "runas";
            }

            process.WaitForExit();
            process.Close();
        }

        public static void StartProcessByCmd(string arguments, bool isAdmin = false)
        {
            StartProcessByCmd(null, arguments, isAdmin);
        }

        public static void StartProcessByCmd(string exeName, string arguments, bool isAdmin = false)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = CmdPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = $"/c \"{exeName}\" {arguments}",
                WindowStyle = ProcessWindowStyle.Hidden,
            });

            if (string.IsNullOrEmpty(exeName))
                process.StartInfo.Arguments = $"/c \"{arguments}\"";

            if (isAdmin)
            {
                process.StartInfo.Verb = "runas";
            }

            process.WaitForExit();
            process.Close();
        }

        /// <summary>
        /// 一条命令一输出，每次重新开启cmd
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="processOutput"></param>
        /// <param name="hasError"></param>
        /// <param name="isAdmin"></param>
        public static void StartProcessByCmd(string arguments, out StringBuilder processOutput, out bool hasError, bool isAdmin = true)
        {
            _command = arguments;
            _outputData = new StringBuilder();
            _errorData = new StringBuilder();

            var processStartInfo = new ProcessStartInfo()
            {
                FileName = CmdPath,
                CreateNoWindow = true,  //不显示程序窗口
                UseShellExecute = false, //是否使用操作系统shell启动
                RedirectStandardInput = true, //接收来自调用程序的输入信息
                RedirectStandardOutput = true, //由调用程序获取输出信息
                RedirectStandardError = true,  //重定向标准错误输出
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            if (isAdmin)
            {
                processStartInfo.Verb = "runas"; //open、runas、runasuser
            }

            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Exited += Process_Exited;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.StartInfo = processStartInfo;
            process.Start();
            process.PriorityBoostEnabled = true;
            //process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            CurrentProcess = process;

            //process.StandardInput.WriteLine(arguments + " 2>&1" + "&exit");
            process.StandardInput.WriteLine(arguments.Trim().TrimEnd('&') + "&exit");
            process.StandardInput.AutoFlush = true;

            processOutput = new StringBuilder();

            //获取cmd窗口的输出信息
            string receiveData = process.StandardOutput.ReadToEnd();
            int index = receiveData.IndexOf(arguments) + arguments.Length + "&exit".Length;
            string output = receiveData.Substring(index, receiveData.Length - index).Trim();
            processOutput.Append(output);
            if (string.IsNullOrEmpty(_errorData.ToString()))
            {
                hasError = false;
            }
            else
            {
                hasError = true;
                processOutput.Append(_errorData);
            }

            process.StandardInput.Close();
            process.WaitForExit();
            process.Close();

            //int index = _outputData.ToString().IndexOf("&exit") + "&exit".Length;
            //string output = _outputData.ToString().Substring(index).Trim();
            //processOutput.Append(output);
            //processOutput.Append(_errorData);
        }

        private static void GetCmdHeader(string data, string arguments)
        {
            if (string.IsNullOrEmpty(CmdHeader) && data.Contains(arguments))
                CmdHeader = data.Replace(arguments, "").Replace("&exit", "");
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                ////Microsoft Windows [版本 10.0.17763.379]
                //Process.StandardOutput.ReadLine().Trim();
                ////(c)2018 Microsoft Corporation。保留所有权利。
                //Process.StandardOutput.ReadLine().Trim();
                GetCmdHeader(e.Data, _command);
                _outputData.AppendLine($"{e.Data}");
            }
        }

        private static void Process_Exited(object sender, EventArgs e)
        {

        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _errorData.AppendLine($"{e.Data}");
            }
        }
    }
}
