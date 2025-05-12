/*
 * Description：CommonTools
 * Author： Chance.Zheng
 * Create Time: 2024/5/10 10:02:00
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2024 All Rights Reserved.
 */


using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using NCATestInstaller.Toolkit.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCATestInstaller.CustomUI
{
    public class CommonTools
    {
        public static readonly string MsiPackageId = "NCATestInstaller"; //与Bundle.wxs文件中MsiPackage.Id属性值一致
        public static readonly string ExePackageIdDotnetFramework48 = "Netfx48Full"; //与Bundle.wxs文件中ExePackage.Id属性值一致
        private const string ControlPanelUninstallKeyPath = @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\";
        private static ManualResetEventSlim _eventSlim = new ManualResetEventSlim(false);
        private static Mutex _syncMutex = new Mutex(false, "Global\\NCATestInstallerCustomUI");
        private static ActionState _actionState;
        private static LaunchAction _action;
        private static RelatedOperation _operation;
        private static RelationType _relationType;
        private static string _message;
        private static bool _isUninstalled;


        public static DetectionState DetectState { get; set; }
        public static InstallationState PreInstallState { get; set; }
        public static bool Canceled { get; set; }
        /// <summary>
        /// 安装过程计数
        /// </summary>
        public static int PhaseCount { get; set; }
        public static string InstallDirectory { get; set; }
        public static string LayoutDirectory { get; set; }


        public static void SetActionState(ActionState actionState) => _actionState = actionState;
        public static ActionState GetActionState() => _actionState;

        public static void SetLaunchAction(LaunchAction action) => _action = action;
        public static LaunchAction GetLaunchAction() => _action;

        public static void SetRelatedOperation(RelatedOperation operation) => _operation = operation;
        public static string GetRelatedOperation()
        {
            string msg = string.Empty;
            if (ConfigurationManageHelper.ReadItem("Language") == "Chinese")
            {
                msg = _operation switch
                {
                    RelatedOperation.None => "None",
                    RelatedOperation.Downgrade => "回退",
                    RelatedOperation.MinorUpdate => "更新",
                    RelatedOperation.MajorUpgrade => "升级",
                    RelatedOperation.Remove => "卸载",
                    RelatedOperation.Install => "安装",
                    RelatedOperation.Repair => "修复",
                    _ => "None",
                };

                if (msg == "None")
                {
                    msg = _actionState switch
                    {
                        ActionState.None => "None",
                        ActionState.Uninstall => "卸载",
                        ActionState.Install => "安装",
                        ActionState.AdminInstall => "安装",
                        ActionState.Modify => "更改",
                        ActionState.Repair => "修复",
                        ActionState.MinorUpgrade => "更新",
                        ActionState.MajorUpgrade => "升级",
                        ActionState.Patch => "修补",
                        _ => "None",
                    };
                }
            }
            else if (ConfigurationManageHelper.ReadItem("Language") == "English")
            {
                msg = _operation switch
                {
                    RelatedOperation.None => "None",
                    RelatedOperation.Downgrade => "Rollback",
                    RelatedOperation.MinorUpdate => "Update",
                    RelatedOperation.MajorUpgrade => "Upgrade",
                    RelatedOperation.Remove => "Remove",
                    RelatedOperation.Install => "Install",
                    RelatedOperation.Repair => "Repair",
                    _ => "None",
                };

                if (msg == "None")
                {
                    msg = _actionState switch
                    {
                        ActionState.None => "None",
                        ActionState.Uninstall => "Uninstall",
                        ActionState.Install => "Install",
                        ActionState.AdminInstall => "AdminInstall",
                        ActionState.Modify => "Modify",
                        ActionState.Repair => "Repair",
                        ActionState.MinorUpgrade => "Update",
                        ActionState.MajorUpgrade => "Upgrade",
                        ActionState.Patch => "Patch",
                        _ => "None",
                    };
                }
            }

            return msg;
        }

        public static void SetOperation(RelationType relationType) => _relationType = relationType;
        public static string GetOperation()
        {
            var msg_relation = _relationType switch
            {
                RelationType.None => "None",
                RelationType.Detect => "检测",
                RelationType.Upgrade => "升级",
                RelationType.Addon => "插件",
                RelationType.Patch => "补丁",
                RelationType.Dependent => "依赖",
                RelationType.Update => "更新",
                _ => "None",
            };

            var msg_action = _actionState switch
            {
                ActionState.None => "None",
                ActionState.Uninstall => "卸载",
                ActionState.Install => "安装",
                ActionState.AdminInstall => "安装",
                ActionState.Modify => "更改",
                ActionState.Repair => "修复",
                ActionState.MinorUpgrade => "更新",
                ActionState.MajorUpgrade => "升级",
                ActionState.Patch => "修补",
                _ => "None",
            };

            return _relationType switch
            {
                RelationType.None => msg_action,
                RelationType.Detect => msg_relation,
                RelationType.Upgrade => msg_relation,
                RelationType.Addon => msg_action + msg_relation,
                RelationType.Patch => msg_action + msg_relation,
                RelationType.Dependent => msg_action + msg_relation,
                RelationType.Update => msg_relation,
                _ => msg_action
            };
        }

        public static void SetMessage(string message) => _message = message;
        public static string GetMessage() => _message;

        public static void SetUninstalled() => _isUninstalled = true;
        public static bool GetIfUninstalled() => _isUninstalled;

        public static void Wait() => _syncMutex.WaitOne();
        public static void Release() => _syncMutex.ReleaseMutex();

        public static void Suspend()
        {
            _eventSlim.Wait();
            _eventSlim.Reset();
        }
        public static void Resume() => _eventSlim.Set();

        public static bool IsSucceeded(int status) => status >= 0;
        public static bool IsRelated() => BootstrapperApplicationAgent.Instance.BootstrapperApplication.Command.Relation == RelationType.None;

        public static bool IsAnotherInstallerRuning()
        {
            bool hasAnotherInstaller = false;
            try
            {
                var installerDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "installer");
                var lockFiles = Directory.GetFiles(installerDir, "*.msi")
                                         .Concat(Directory.GetFiles(installerDir, "SourceHash{*}"))
                                         .Concat(Directory.GetFiles(installerDir, "inprogressinstallinfo.ipi"))
                                         .Where(f => new FileInfo(f).LastWriteTime.AddMinutes(30) >= DateTime.Now)
                                         .Where(f => IsFileLocked(f));

                if (lockFiles.Any())
                {
                    hasAnotherInstaller = true;
                    string message = ConfigurationManageHelper.ReadItem("Language") == "Chinese"
                                     ? "另一安装过程正在进行中。您必须先完成那个安装过程，然后才能继续本次安装。" : "Another installation process is under way. You must complete the installation process before you can continue the installation.";
                    System.Windows.MessageBox.Show(message);
                }
            }
            catch (Exception) { }

            return hasAnotherInstaller;


            bool IsFileLocked(string path)
            {
                if (!File.Exists(path)) return false;

                bool isUse = true;
                FileStream fs = null;

                try
                {
                    fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    isUse = false;
                }
                catch (Exception)
                {
                }
                finally
                {
                    fs?.Close();
                }

                return isUse;
            }
        }

        public static string PackageCachePath => Path.Combine(BootstrapperApplicationAgent.Instance.GetBurnVariable("CommonAppDataFolder"), "Package Cache");

        public static string GetBundleCacheFilePath()
        {
            var wixBundleProviderKey = BootstrapperApplicationAgent.Instance.GetBurnVariable("WixBundleProviderKey");
            var wixBundleProviderKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{wixBundleProviderKey}";

            if (!string.IsNullOrWhiteSpace(wixBundleProviderKeyPath))
            {
                var bundleCacheFilePath = RegistryHelper.GetLocalMachineRegistryKeyPathValue(wixBundleProviderKeyPath, "BundleCachePath");
                return bundleCacheFilePath?.ToString();
            }
            else
            {
                var bundleCacheDirectory = Path.Combine(PackageCachePath, wixBundleProviderKey);
                return Directory.GetFiles(bundleCacheDirectory, ".exe").FirstOrDefault();
            }
        }

        public static string GetInstallLocation()
        {
            var wixBundleProviderKey = BootstrapperApplicationAgent.Instance.GetBurnVariable("WixBundleProviderKey");
            var wixBundleProviderKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{wixBundleProviderKey}";

            var bundleCacheFilePath = RegistryHelper.GetLocalMachineRegistryKeyPathValue(wixBundleProviderKeyPath, "InstallLocation");
            return bundleCacheFilePath?.ToString();
        }
    }

    internal class InteropMethod
    {
        private const uint WM_SETTINGCHANGE = 0x001A;
        private const uint SMTO_ABORTIFHUNG = 0x002;
        private const int PRODUCT_GUID_LENGTH = 0x27; //38个字符+1个终止符

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam, uint fuFlage, uint uTimeout, out UIntPtr lpdwResult);

        [DllImport("msi.dll", CharSet = CharSet.Auto)]
        private static extern uint MsiEnumRelatedProducts([MarshalAs(UnmanagedType.LPStr)] string productCode, [MarshalAs(UnmanagedType.LPStr)] StringBuilder relatedProduct, ref uint productBuf);


        public static void RefreshDesktopAndControlPanel()
        {
            SendMessageTimeout(IntPtr.Zero, WM_SETTINGCHANGE, UIntPtr.Zero, IntPtr.Zero, SMTO_ABORTIFHUNG, 2000, out UIntPtr result);
        }

        public static bool CheckRunningInstaller(string productCode)
        {
            bool found = false;
            StringBuilder relatedProductCode = new StringBuilder(PRODUCT_GUID_LENGTH);
            uint size = (uint)relatedProductCode.Capacity;
            uint result = MsiEnumRelatedProducts(productCode, relatedProductCode, ref size);

            if (result == 0) //SUCCESS
            {
                found = true;
                Console.WriteLine($"Related Product Code: {relatedProductCode}");
            }
            else if (result == 234) //ERROR_MORE_DATA  缓冲区大小不足，需要分配更大的缓冲区
            {
                relatedProductCode = new StringBuilder((int)size);
                Console.WriteLine($"Related Product Code: {relatedProductCode}");
                result = MsiEnumRelatedProducts(productCode, relatedProductCode, ref size);

                if (result == 0)
                {
                    found = true;
                    Console.WriteLine($"Related Product Code: {relatedProductCode}");
                }
                else
                {
                    Console.WriteLine($"Error enumerating related products. Error Code: {result}");
                }
            }
            else
            {
                Console.WriteLine($"Error enumerating related products. Error Code: {result}");
            }

            return found;

            //try
            //{
            //    {
            //        string guidSize = new string('\0', PRODUCT_GUID_LENGTH);
            //        uint index = 0;
            //        bool found = false;

            //        while (true)
            //        {
            //            var result = MsiEnumRelatedProducts(null, 0, index++, guidSize);
            //            if (result == 0)  //SUCCESS
            //            {
            //                found = true;
            //                break;
            //            }
            //            else if (result == 259)  //NO_MORE_ITEMS
            //            {
            //                break;
            //            }
            //            else
            //            {
            //                Console.WriteLine("Error enumerating related products.");
            //                return false;
            //            }
            //        }

            //        return found;
            //    }
            //catch (Exception)
            //{
            //    return false;
            //}
        }
    }
}
