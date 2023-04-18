using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：EnvironmentHelper 
 * Author： Chance.Zheng
 * Create Time: 2023/3/16 14:20:25
 * .Net Version: 6.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Toolkit.Helpers
{
    public static class EnvironmentHelper
    {
        public static void WriteEnviorment(string variable, string value)
        {
            Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.Machine);
        }

        public static bool CheckEnviorment(string variable)
        {
            return Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine) != null;
        }

        public static string GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine) ?? string.Empty;
        }
    }
}
