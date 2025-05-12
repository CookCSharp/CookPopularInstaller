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
 * Copyright (c) NCATest 2018-2023 All Rights Reserved.
 */
namespace NCATestInstaller.Toolkit.Helpers
{
    public static class ProcessHelper
    {
        private static readonly string CmdPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\cmd.exe";
        private static string _command;
        private static StringBuilder _outputData;
        private static StringBuilder _errorData;

        public static Process CurrentProcess { get; private set; }
        public static string CmdHeader { get; private set; }


        public static void CreateFolderRunAsAdmin(string folderPath)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = CmdPath,
                CreateNoWindow = true,
                UseShellExecute = true,
                Arguments = $"/c mkdir \"{folderPath}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
            });

            process.WaitForExit();
            process.Close();
        }

        public static void StartProcess(string fileName, string arguments, bool isAdmin = false)
        {
            var process = Process.Start(new ProcessStartInfo(fileName, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = isAdmin ? "runas" : "",
            });

            process.WaitForExit();
            process.Close();
        }

        public static void StartProcessByCmd(string arguments, bool isAdmin = false)
        {
            StartProcessByCmd(null, arguments, isAdmin);
        }

        public static void StartProcessByCmd(string exeName, string arguments, bool isAdmin = false)
        {
            arguments = string.IsNullOrEmpty(exeName) ? $"/c {arguments}" : $"/c {exeName} {arguments}";

            var processStartInfo = new ProcessStartInfo(CmdPath, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = isAdmin ? "runas" : "open",
            };

            var process = Process.Start(processStartInfo);
            process.WaitForExit();
            process.Close();
        }

        /// <summary>
        /// 一条命令一条输出，每次重新开启cmd
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


        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesireAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetFinalPathNameByHandle(IntPtr hFile, System.Text.StringBuilder lpszFilePath, uint cchFilePath, uint dwFalg);

        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;
        private const uint FILE_SHARE_DELETE = 4;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        public static void FindProcess(string installFolder)
        {
            var files = Directory.GetFiles(installFolder, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var handle = CreateFile(file, 0, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
                if (handle != IntPtr.Zero)
                {
                    try
                    {
                        const int MAX_PATH = 260;
                        System.Text.StringBuilder path = new StringBuilder(MAX_PATH);
                        var result = GetFinalPathNameByHandle(handle, path, (uint)path.Capacity, 0);
                        var executablePath = path.ToString().Remove(0, 4);
                        if (result > 0 && result < MAX_PATH)
                        {
                            using var searcher = new System.Management.ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ExecutablePath = '{executablePath}'");
                            foreach (System.Management.ManagementObject @object in searcher.Get())
                            {
                                var name = @object["Name"];
                                var processId = @object["ProcessId"];
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }
}
