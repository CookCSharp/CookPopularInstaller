using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：ServiceHelper 
 * Author： Chance.Zheng
 * Create Time: 2023/3/16 14:32:50
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Toolkit.Helpers
{
    public class ServiceHelper
    {
        private static readonly string CmdPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\cmd.exe";

        public static void CreateService(string arguments)
        {
            ProcessHelper.StartProcessByCmd(arguments, true);
        }

        public static void DeleteService(string arguments)
        {
            ProcessHelper.StartProcessByCmd(arguments, true);
        }
    }
}
