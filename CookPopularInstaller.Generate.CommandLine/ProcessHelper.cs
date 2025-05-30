using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：ProcessHelper 
 * Author： Chance.Zheng
 * Create Time: 2023/3/3 13:55:14
 * .Net Version: 6.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.CommandLine
{
    public class ProcessHelper
    {
        private static StringBuilder _outputData;
        private static StringBuilder _errorData;
        private static string _command;

        public static string CmdHeader { get; private set; }

        public static void StartCmd(string argument, out StringBuilder cmdOutput, out bool hasError)
        {
            _command = argument;
            _outputData = new StringBuilder();
            _errorData = new StringBuilder();

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            process.BeginOutputReadLine();
            process.OutputDataReceived += Process_OutputDataReceived;
            process.BeginErrorReadLine();
            process.ErrorDataReceived += Process_ErrorDataReceived;

            StreamWriter streamWriter = process.StandardInput;
            streamWriter.WriteLine(argument);

            streamWriter.Close();
            process.WaitForExit();
            process.Close();

            int index = _outputData.ToString().IndexOf(argument) + argument.Length;
            string output = _outputData.ToString().Substring(index, _outputData.ToString().Length - CmdHeader.Length - index - 2).Trim();
            cmdOutput = new StringBuilder(output);

            if (string.IsNullOrEmpty(_errorData.ToString()))
            {
                hasError = false;
            }
            else
            {
                hasError = true;
                cmdOutput.Append(_errorData);
            }
        }

        private static void GetCmdHeader(string data, string argument)
        {
            if (string.IsNullOrEmpty(CmdHeader) && data.Contains(argument))
                CmdHeader = data.Replace(argument, "").Replace("&exit", "");
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                GetCmdHeader(e.Data, _command);
                _outputData.AppendLine(e.Data);
            }
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _errorData.AppendLine(e.Data);
            }
        }
    }
}
