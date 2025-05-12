using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using CookPopularControl.Controls;
using CookPopularControl.Windows;
using CookPopularCSharpToolkit.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using NCATestInstaller.Toolkit.Helpers;
using Ookii.Dialogs.Wpf;
using Prism.Commands;


namespace NCATestInstaller.CustomUI.ViewModels
{
    public class InstallViewModel : ViewModelBase
    {
        public bool IsAgreeLicense { get; set; }
        public string InstallFolder { get; set; }

        public DelegateCommand QuickInstallCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnQuickInstallAction)).Value;
        public DelegateCommand<string> OpenLicenseCommand => new Lazy<DelegateCommand<string>>(() => new DelegateCommand<string>(OnOpenLicenseAction)).Value;
        public DelegateCommand FolderBrowserCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnFolderBrowserAction)).Value;

        private const string ChinesePattern = "[\u4e00-\u9fbb]";
        private const string InvalidPathPattern = @"([\<\>\:\\\\\/\?\*\|\""]|\.)";
        private const string InvalidFolderPattern = @"([\<\>\:\\\\\/\?\*\|\""])";
        private readonly string DefaultInstallFolder = (Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) + "\\NCATest";

        public InstallViewModel()
        {
            InitInstallFolder();
            EventAggregator.GetEvent<AgreeLicenseEvent>().Subscribe(b => IsAgreeLicense = b);
        }

        private void InitInstallFolder()
        {
#if !ACTUALTEST
            InstallFolder = DefaultInstallFolder;
#else
            var companyName = BootstrapperAgent.GetBurnVariable("CompanyName");
            var productName = BootstrapperAgent.GetBurnVariable("ProductName");
            InstallFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + $"\\{companyName}\\{productName}";
#endif
        }

        private async Task UninstallPreviousProduct()
        {
            await Task.Run(() =>
            {
                var companyName = BootstrapperAgent.GetBurnVariable("CompanyName");
                var productName = BootstrapperAgent.GetBurnVariable("ProductName");
                bool isExistProduct = RegistryHelper.CheckRegistryKeyPath(Microsoft.Win32.RegistryHive.LocalMachine, $"SOFTWARE\\{companyName}\\{productName}");
                if (isExistProduct)
                {
                    string path = RegistryHelper.GetLocalMachineRegistryKeyPathValue($"SOFTWARE\\{companyName}\\{productName}", "Path")?.ToString();
                    ProcessHelper.StartProcess(Path.Combine(path, "Uninst.exe"), "/quiet /uninstall");
                }
            });
        }

        private void OnQuickInstallAction()
        {
#if !ACTUALTEST
            App.SetInstallState(InstallationState.Applying);
#else
            if (CommonTools.IsAnotherInstallerRuning()) return;

            if (!IfCreateFolder()) return;

            BootstrapperAgent.SetBurnVariable("InstallFolder", InstallFolder);
            BootstrapperAgent.PlanAction(LaunchAction.Install);
#endif
        }

        private void OnOpenLicenseAction(string navigatePath)
        {
            if (!string.IsNullOrEmpty(navigatePath))
                RegionManager.RequestNavigate(RegionToken.MainWindowContentRegion, navigatePath);
        }

        private void OnFolderBrowserAction()
        {
            if (!IfCreateFolder(false))
            {
                return;
            }
            var folderDialog = new VistaFolderBrowserDialog();
            folderDialog.Description = "Select Install Path";
            folderDialog.ShowNewFolderButton = true;
            folderDialog.UseDescriptionForTitle = true;
            folderDialog.SelectedPath = InstallFolder;
            folderDialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
            //if (!string.IsNullOrEmpty(InstallFolder))
            //{
            //    folderDialog.SelectedPath = Directory.GetDirectories(InstallFolder).FirstOrDefault();
            //}
            if (folderDialog.ShowDialog().Value)
            {
                InstallFolder = folderDialog.SelectedPath;
            }
        }

        private bool IfCreateFolder(bool isAutoCreate = true)
        {
            bool ret = true;
            string errorMessage = string.Empty;

            if (string.IsNullOrEmpty(InstallFolder))
            {
                errorMessage = $"The install path can not be empty";
                ret = false;
            }

            try
            {
                var folderName = Path.GetFileName(InstallFolder);
                var isValidPath = Path.GetInvalidPathChars().All(c => !InstallFolder.Contains(c));
                isValidPath &= !Regex.IsMatch(folderName, InvalidFolderPattern);
                if (ret && !isValidPath)
                {
                    errorMessage = $"The install path is invalid";
                    ret = false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ret = false;
            }

            string containChinese = ConfigurationManageHelper.ReadItem("ContainChineseOnInstallFolder");
            if (ret && bool.TryParse(containChinese, out bool canContainChinese))
            {
                bool includingChinese = Regex.IsMatch(InstallFolder, ChinesePattern);
                if (!canContainChinese && includingChinese)
                {
                    errorMessage = $"The install path can not contain Chinese";
                    ret = false;
                }
            }

            if (isAutoCreate && ret && !Directory.Exists(InstallFolder))
            {
                try
                {
                    var process = Process.Start(new ProcessStartInfo("cmd", $"/c mkdir \"{InstallFolder}\"")
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Verb = "runas",
                    });
                    process.WaitForExit();
                    process.Close();
                    if (!Directory.Exists(InstallFolder))
                    {
                        errorMessage = $"Could not find a part of the path: {InstallFolder}";
                        ret = false;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    ret = false;
                }
            }

            if (!ret) MessageDialog.ShowError(errorMessage);

            return ret;
        }
    }
}
