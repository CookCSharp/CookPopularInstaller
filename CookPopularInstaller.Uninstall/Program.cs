using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;

namespace CookPopularInstaller.Uninstall
{
    /// <summary>
    /// 每一个Uninstall.exe需要单独生成后再拷贝至打包目录下
    /// </summary>
    public class Program
    {
        private const string PackageCacheKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";

        static void Main(string[] args)
        {
            GetUpgradeCode(out string productName, out string upgradeCode);
            GetBundleCachePath(upgradeCode, out string wixBundleCachePath);
            if (!string.IsNullOrWhiteSpace(wixBundleCachePath))
            {
                if (args.Length == 0)
                    StartProcess($"\"{wixBundleCachePath}\"", null);
                else
                    StartProcess($"\"{wixBundleCachePath}\"", "/quiet /uninstall");
            }
            else
            {
                GetMsiUninstallCommand(productName, out string uninstallCommand);
                StartProcess("msiexec.exe", uninstallCommand.Remove(0, 11).Trim()); //MsiExec.exe
            }

            //var productKeyPath = GetProductKeyPath();
            //var installFolder = GetRegisterKeyValue(productKeyPath, "Path")?.ToString();
            //var wixBundleProvicerKey = GetRegisterKeyValue(productKeyPath, "WixBundleProviderKey")?.ToString();
            //if (!string.IsNullOrEmpty(wixBundleProvicerKey))
            //{
            //    var wixBundleCachePath = GetRegisterKeyValue(PackageCacheKeyPath + wixBundleProvicerKey, "BundleCachePath");
            //    if (args.Length == 0)
            //        StartProcess($"\"{wixBundleCachePath}\"", null);
            //    else
            //        StartProcess($"\"{wixBundleCachePath}\"", "/quiet /uninstall");
            //}
            //else
            //{
            //    var productCode = GetRegisterKeyValue(productKeyPath, "ProductCode")?.ToString();
            //    var uninstallCommand = GetRegisterKeyValue(PackageCacheKeyPath + productCode, "UninstallString") + " /passive";
            //    StartProcess("msiexec.exe", uninstallCommand.Remove(0, 11).Trim());
            //}
        }

        static void GetUpgradeCode(out string productName, out string upgradeCode)
        {
            using var streamConfig = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program), "App.config");
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;
            XmlReader xmlReader = XmlReader.Create(streamConfig, xmlReaderSettings);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlReader);

            XmlNode configurationXmlNode = xmlDocument.SelectSingleNode("configuration");
            XmlNode appSettingsXmlNode = configurationXmlNode.SelectSingleNode("appSettings");
            XmlElement addXmlElement1 = (XmlElement)appSettingsXmlNode.FirstChild;
            XmlElement addXmlElement2 = (XmlElement)addXmlElement1.NextSibling;
            productName = addXmlElement1.GetAttribute("value");
            upgradeCode = addXmlElement2.GetAttribute("value");

            //using var streamJson = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program), "upgrade_code.json");
            //var buffer = new byte[streamJson.Length];
            //streamJson.Read(buffer, 0, buffer.Length);
            //var jsonString = Encoding.Default.GetString(buffer);
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //dynamic jsonObject = serializer.DeserializeObject(jsonString);
            //var productCode = jsonObject[productName];
        }

        static void GetBundleCachePath(string upgradeCode, out string bundleCachePath)
        {
            using var key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(PackageCacheKeyPath, false);
            var contain64 = Contains(key64, out bundleCachePath);

            if (contain64) return;

            using var key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(PackageCacheKeyPath, false);
            var contain32 = Contains(key32, out bundleCachePath);


            bool Contains(RegistryKey registryKey, out string bundleCachePath)
            {
                foreach (string subkeyName in registryKey.GetSubKeyNames())
                {
                    using (var subkey = registryKey.OpenSubKey(subkeyName))
                    {
                        if (subkey != null)
                        {
                            var value = subkey.GetValue("BundleUpgradeCode", null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                            if (value != null && ((string[])value)[0] == upgradeCode)
                            {
                                bundleCachePath = subkey.GetValue("BundleCachePath", null)?.ToString();

                                return true;
                            }
                        }
                    }
                }

                bundleCachePath = null;
                return false;
            }
        }

        static void GetMsiUninstallCommand(string productName, out string uninstallCommand)
        {
            using var key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(PackageCacheKeyPath, false);
            var contain64 = Contains(key64, out uninstallCommand);

            if (contain64) return;

            using var key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(PackageCacheKeyPath, false);
            var contain32 = Contains(key32, out uninstallCommand);


            bool Contains(RegistryKey registryKey, out string uninstallCommand)
            {
                foreach (string subkeyName in registryKey.GetSubKeyNames())
                {
                    using (var subkey = registryKey.OpenSubKey(subkeyName))
                    {
                        if (subkey != null)
                        {
                            var value = subkey.GetValue("DisplayName", null);
                            if (value != null && value?.ToString() == productName)
                            {
                                uninstallCommand = subkey.GetValue("UninstallString", null)?.ToString();
                                return true;
                            }
                        }
                    }
                }

                uninstallCommand = null;
                return false;
            }
        }


        static string GetProductKeyPath()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program), "App.config");
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;
            XmlReader xmlReader = XmlReader.Create(stream, xmlReaderSettings);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlReader);

            XmlNode configurationXmlNode = xmlDocument.SelectSingleNode("configuration");
            XmlNode appSettingsXmlNode = configurationXmlNode.SelectSingleNode("appSettings");
            XmlElement addXmlElement1 = (XmlElement)appSettingsXmlNode.FirstChild;
            XmlElement addXmlElement2 = (XmlElement)appSettingsXmlNode.LastChild;
            string companyName = addXmlElement1.GetAttribute("value");
            string productName = addXmlElement2.GetAttribute("value");
            string productKeyPath = $"SOFTWARE\\{companyName}\\{productName}";

            return productKeyPath;
        }

        static object GetRegisterKeyValue(string keyPath, string name)
        {
            RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(keyPath, false);
            if (registryKey == null)
            {
                registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(keyPath, false);
                if (registryKey == null)
                    throw new KeyNotFoundException("can not find the key path from registry");
            }

            var value = registryKey.GetValue(name);
            return value;
        }

        static void StartProcess(string fileName, string arguments)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = fileName,
                CreateNoWindow = true,
                UseShellExecute = true,
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Normal,
            });
            process.WaitForExit();
            process.Close();
        }
    }

    internal static class Helper
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

        public static void ShowOrHideConsole(bool isShowConsole = false, string consoleTitle = "")
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