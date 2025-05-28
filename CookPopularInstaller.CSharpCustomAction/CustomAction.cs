/*
 * Description：CustomAction 
 * Author： Chance.Zheng
 * Create Time: 2023/3/28 20:19:16
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */


using Microsoft.Deployment.WindowsInstaller;
using CookPopularInstaller.Toolkit.Helpers;
using Newtonsoft.Json.Linq;
using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CookPopularInstaller.CSharpCustomAction
{
    public class CustomAction
    {
        [CustomAction]
        public static ActionResult SetInstallMessageCustomAction(Session session)
        {
            session["INSTALLDEPENDSMESSAGE"] = "Initializing";

            return ActionResult.Success;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="session"></param>
        /// <remarks>安装之前触发</remarks>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult InitCustomAction(Session session)
        {
            session.Log("Begin init");

            try
            {
                var installFolder = session["INSTALLFOLDER"];
                var packageFiles = Directory.GetFiles(installFolder, "package*.json");
                string packageFile = packageFiles.Length > 1 ? packageFiles.Where(f => Path.GetFileName(f) != "package.json").FirstOrDefault() : packageFiles.FirstOrDefault();
                if (packageFile == null) return ActionResult.NotExecuted;

                ExecuteScriptsWithInstallFinish(session, installFolder, packageFile, "BeforeInstall");
            }
            catch (Exception ex)
            {
                session.Log("InitCustomAction:" + ex.Message);
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// 安装依赖
        /// </summary>
        /// <param name="session"></param>
        /// <remarks>安装完成之后触发</remarks>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult InstallDependsCustomAction(Session session)
        {
            session.Log("Begin install depends");

            try
            {
                InitRegistryKeyPathValue(session);

                string installFolder = session["INSTALLFOLDER"];
                var packageFiles = Directory.GetFiles(installFolder, "package*.json");
                string packageFile = packageFiles.Length > 1 ? packageFiles.Where(f => Path.GetFileName(f) != "package.json").FirstOrDefault() : packageFiles.FirstOrDefault();
                if (File.Exists(packageFile))
                {
                    UncompressArchiveFile(installFolder, packageFile, session);

                    ExecuteScriptsWithInstallFinish(session, installFolder, packageFile, "AfterInstall");
                }
            }
            catch (Exception ex)
            {
                session.Log("InstallDependsCustomAction:" + ex.Message);
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// 修改注册表值
        /// </summary>
        /// <param name="session"></param>
        private static void InitRegistryKeyPathValue(Session session)
        {
            //System.Diagnostics.Debugger.Launch();

            var productCode = session["ProductCode"];
            var wixBundleProviderKey = session["WIXBUNDLEPROVIDERKEY"];
            var installFolder = session["INSTALLFOLDER"];
            var version = session["PACKAGEVERSION"];

            //Msi
            if (!string.IsNullOrWhiteSpace(productCode))
            {
                var productCodeKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productCode}";
                var productCodeRegistryKey = RegistryHelper.GetLocalMachineRegistryKey(productCodeKeyPath);
                if (productCodeRegistryKey != null)
                {
                    productCodeRegistryKey.SetValue("InstallLocation", installFolder);
                    //productCodeRegistryKey.SetValue("DisplayVersion", version);
                }
            }

            //Exe
            if (!string.IsNullOrWhiteSpace(wixBundleProviderKey))
            {
                var bundleCodeKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{wixBundleProviderKey}";
                var bundleRegistryKey = RegistryHelper.GetLocalMachineRegistryKey(bundleCodeKeyPath);
                if (bundleRegistryKey != null)
                {
                    bundleRegistryKey.SetValue("InstallLocation", installFolder);
                    //bundleRegistryKey.SetValue("DisplayVersion", version);
                }
            }
        }

        /// <summary>
        /// 使用SevenZipSharp解压文件
        /// </summary>
        /// <param name="installFolder"></param>
        /// <param name="packageFile"></param>
        /// <param name="session"></param>
        private static void UncompressArchiveFile(string installFolder, string packageFile, Session session)
        {
            try
            {
                var depends = JsonHelper.ReadJsonValue(packageFile, "Depend", "DependDialogVariables");
                var variables = JToken.Parse(depends).ToArray();

                foreach (var variable in variables)
                {
                    string archiveFileName = variable["Value"].ToString();
                    string extractPath = variable["CheckValue"].ToString().Replace("[InstallFolder]", installFolder);
                    string archiveFilePath = Path.Combine(installFolder, archiveFileName);

                    if (!File.Exists(archiveFilePath))
                    {
                        session.Log(string.Format("未能找到文件'{0}'", archiveFilePath));
                        continue;
                    }

                    Uncompress(archiveFileName, archiveFilePath, extractPath);
                }
            }
            catch (Exception ex)
            {
                session.Log("UncompressArchiveFile:" + ex.Message);
            }

            void Uncompress(string archiveFileName, string archiveFilePath, string extractPath)
            {
                try
                {
                    var archiveFileExtension = Path.GetExtension(archiveFilePath);
                    if (archiveFileExtension == ".zip" || archiveFileExtension == ".7z" ||
                        archiveFileExtension == ".rar" || archiveFileExtension == ".gz" ||
                        archiveFileExtension == ".tgz" || archiveFileExtension == ".xz" ||
                        archiveFileExtension == ".tar" || archiveFileExtension == ".lz" || archiveFileExtension == ".bz2")
                    {
                        if (Environment.Is64BitProcess)
                            SevenZipBase.SetLibraryPath(Path.Combine(installFolder, "7z64.dll"));
                        else
                            SevenZipBase.SetLibraryPath(Path.Combine(installFolder, "7z.dll"));

                        using (var extractor = new SevenZipExtractor(archiveFilePath))
                        {
                            extractor.Extracting += (s, e) =>
                            {
                                Console.WriteLine($"{e.PercentDone}%");
                            };
                            extractor.ExtractArchive(extractPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private static void ExecuteScriptsWithInstallFinish(Session session, string installFolder, string packageFile, string tokenName)
        {
            session.Log("Begin execute scripts");

            var scriptsJson = JsonHelper.ReadJsonValue(packageFile, tokenName, "Scripts");
            var scripts = JToken.Parse(scriptsJson).ToList();
            scripts.ForEach(s => ProcessHelper.StartProcess(s["Path"].ToString().Replace("[InstallFolder]", installFolder), null, true));

            //List<string>.ForEach执行异步Action时不会等待，我们可采用foreach循环，故不可用以下写法
            //scripts.ForEach(s => File.Delete(s["Path"].ToString().Replace("[InstallFolder]", installFolder)));
        }

        [CustomAction]
        public static ActionResult CancelRequestHandler(Session session)
        {
            //System.Diagnostics.Debugger.Launch();

            return IsCancelRequestedFromUI(session) ? ActionResult.UserExit : ActionResult.Success;

            bool IsCancelRequestedFromUI(Session session)
            {
                string upgradeCode = session["UpgradeCode"];
                using (var m = new System.Threading.Mutex(true, "WIXUI_CANCEL_REQUEST" + upgradeCode, out bool createdNew))
                {
                    return (!createdNew);
                }
            }
        }

        [CustomAction]
        public static ActionResult UninstallCustomAction(Session session)
        {
            try
            {
                //也可以在Component下面增加RemoveFolder和RemoveFiles标签
                var installFolder = session["INSTALLFOLDER"];
                //if (Directory.Exists(installFolder))
                //    Directory.Delete(installFolder, true);
            }
            catch (Exception ex)
            {
                session.Log("UninstallCustomAction:" + ex.Message);
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult CheckPreviousVersionCustomAction(Session session)
        {
            var previousVersion = session["PREVIOUSPRODUCT"];
            session["PREVIOUSPRODUCT_PROPERTY"] = previousVersion;

            session.Log("旧产品版本：" + previousVersion);

            using (Database db = session.Database)
            {
                var view = db.OpenView("SELECT UpgradeCode FROM Upgrade");
                view.Execute();

                while (true)
                {
                    var record = view.Fetch();
                    if (record == null) break;

                    var upgradeCode = record.GetString(1);
                    session.Log("升级Code：" + upgradeCode);
                }

                view.Close();
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult RemoveCacheFolderCustomAction(Session session)
        {
            try
            {
                //System.Diagnostics.Debugger.Launch();

                var productCodeKey = session.CustomActionData["PreviousProductCode"];
                var productCodeKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productCodeKey}";
                var wixBundleProviderKey = session.CustomActionData["PreviousWixBundleProviderKey"];
                var wixBundleProviderKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{wixBundleProviderKey}";

                session.Log("productCodeKey:" + productCodeKey);
                session.Log("wixBundleProviderKey:" + wixBundleProviderKey);

                //if (!string.IsNullOrWhiteSpace(productCodeKeyPath))
                //{
                //    var installSource = RegistryHelper.GetLocalMachineRegistryKeyPathValue(productCodeKeyPath, "InstallSource");
                //    if (installSource != null && !string.IsNullOrWhiteSpace(installSource.ToString()) && Directory.Exists(installSource.ToString()))
                //        Directory.Delete(installSource.ToString(), true);

                //    if (RegistryHelper.CheckRegistryKeyPath(Microsoft.Win32.RegistryHive.LocalMachine, productCodeKeyPath))
                //        RegistryHelper.DeleteLocalMachineSubKeyTree(productCodeKeyPath);
                //}

                //if (!string.IsNullOrWhiteSpace(wixBundleProviderKeyPath))
                //{
                //    var bundleCachePath = RegistryHelper.GetLocalMachineRegistryKeyPathValue(wixBundleProviderKeyPath, "BundleCachePath");
                //    if (bundleCachePath != null && !string.IsNullOrWhiteSpace(bundleCachePath.ToString()))
                //    {
                //        var bundleCacheFolder = Directory.GetParent(bundleCachePath.ToString()).FullName;
                //        if (Directory.Exists(bundleCacheFolder)) Directory.Delete(bundleCacheFolder, true);
                //    }

                //    if (RegistryHelper.CheckRegistryKeyPath(Microsoft.Win32.RegistryHive.LocalMachine, wixBundleProviderKeyPath))
                //        RegistryHelper.DeleteLocalMachineSubKeyTree(wixBundleProviderKeyPath);
                //}
            }
            catch (Exception ex)
            {
                session.Log("RemoveCacheFolderCustomAction:" + ex.Message);
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult DeferredCustomAction(Session session)
        {
            try
            {
                ProductInstallation installation = new ProductInstallation(session["ProductCode"]);
                var productCode = installation.ProductCode;
                var wixBundleProviderKey = session["WIXBUNDLEPROVIDERKEY"];

                CustomActionData customActionData = session.CustomActionData;
                var productState = customActionData["PRODUCTSTATE"];
                var previousWixBundleProviderKey = customActionData["PREVIOUSWIXBUNDLEPROVIDERKEY"];
                var packageCachePath = customActionData["PACKAGECACHEPATH"];
                var installFolder = customActionData["INSTALLFOLDER"];

                //MessageBox.Show("productState:" + productState + "previousWixBundleProviderKey:" + previousWixBundleProviderKey + "packageCachePath:" + packageCachePath + "installFolder:" + installFolder);
                //if (productState == "5")
                //{
                //    //MessageBox.Show(Directory.GetParent(packageCachePath).Name);
                //    //MessageBox.Show(packageCachePath);
                //    RegistryHelper.DeleteLocalMachineSubKeyTree($"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{previousWixBundleProviderKey}");
                //    //C:\ProgramData\Package Cache  Exe安装包
                //    //C:\Windows\Installer  Msi安装包
                //    Directory.Delete(Path.GetDirectoryName(packageCachePath), true);
                //}
            }
            catch (Exception ex)
            {
                session.Log("DeferredCustomAction:" + ex.Message);
            }

            return ActionResult.Success;
        }
    }
}