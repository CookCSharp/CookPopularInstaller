using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;



/*
 * Description:RegisterHelper
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/22 11:38:10
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © CookCSharp 2018-2022 All Rights Reserved
 */
namespace CookPopularInstaller.CustomUI
{
    public class RegisterHelper
    {
        private static readonly string X64 = $@"SOFTWARE\WOW6432Node\{BootstrapperApplicationAgent.Instance.GetBurnVariable("CompanyName")}";
        private static readonly string X86 = $@"SOFTWARE\{BootstrapperApplicationAgent.Instance.GetBurnVariable("CompanyName")}";
        private static readonly string ProductName = BootstrapperApplicationAgent.Instance.GetBurnVariable("ProductName"); //对应注册表中的项


        public static void SetRegistryKey()
        {
            //RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).
        }

        public static RegistryKey GetRegistryKey(string keyPath)
        {
            RegistryKey theRegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey(keyPath, false);
            //if (theRegistryKey == null)
            //{
            //    throw new KeyNotFoundException("Not find the value of key from registry");
            //}
            return theRegistryKey;
        }

        public static RegistryKey GetInstallerBaseKey()
        {
            return GetRegistryKey(@"Software\WOW6432Node\CookCSharp\CookPopularInstaller");
        }

        public static bool TryGetInstallerLocation(out string path)
        {
            path = string.Empty;

            try
            {
                using (RegistryKey theKey = GetInstallerBaseKey())
                {
                    if (theKey == null)
                        return false;

                    object theValue = theKey.GetValue("Path");
                    if (theValue == null)
                        return false;

                    path = theValue.ToString();

                    return !string.IsNullOrEmpty(path);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static RegistryKey GetToolsManagementBaseKey()
        {
            return GetRegistryKey(@"Software\WOW6432Node\CookCSharp\ToolsManagement");
        }

        public static bool TryGetToolsManagementPath(out string path)
        {
            path = string.Empty;

            try
            {
                using (RegistryKey theKey = GetToolsManagementBaseKey())
                {
                    if (theKey == null)
                        return false;

                    object theValue = theKey.GetValue("Path");
                    if (theValue == null)
                        return false;

                    path = theValue.ToString();

                    return !string.IsNullOrEmpty(path);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 查找注册表看是否已经安装本产品的其他版本
        /// </summary>
        /// <returns></returns>
        public static bool ChekExistFromRegistry()
        {
            try
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    //64位
                    using (RegistryKey pathKey = Registry.LocalMachine.OpenSubKey(X64))
                    {
                        var strs = pathKey.GetSubKeyNames();
                        foreach (string str in strs)
                        {
                            if (str.Equals(ProductName))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    //32位
                    using (RegistryKey pathKey = Registry.LocalMachine.OpenSubKey(X86))
                    {
                        var strs = pathKey.GetSubKeyNames();
                        foreach (string str in strs)
                        {
                            if (str.Equals(ProductName))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return false;
        }
    }
}
