using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Ookii.Dialogs.Wpf;
using Prism.Commands;


namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class InstallViewModel : ViewModelBase
    {
        public bool IsAgreeLicense { get; set; }
        public string InstallFolder { get; set; }

        public DelegateCommand QuickInstallCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnQuickInstallAction)).Value;
        public DelegateCommand<string> OpenLicenseCommand => new Lazy<DelegateCommand<string>>(() => new DelegateCommand<string>(OnOpenLicenseAction)).Value;
        public DelegateCommand FolderBrowserCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnFolderBrowserAction)).Value;


        private readonly string DefaultInstallFolder = (Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) + "\\CookCSharp";

        public InstallViewModel()
        {
            InitInstallFolder();
            EventAggregator.GetEvent<AgreeLicenseEvent>().Subscribe(b => IsAgreeLicense = b);

#if RELEASE
            BootstrapperAgent.BootstrapperApplication.PlanComplete += BootstrapperApplication_PlanComplete;
            BootstrapperAgent.BootstrapperApplication.ApplyBegin += BootstrapperApplication_ApplyBegin;
#endif
        }

        private void InitInstallFolder()
        {
#if DEBUG
            InstallFolder = DefaultInstallFolder;
#else
            var companyName = BootstrapperAgent.GetBurnVariable("CompanyName");
            var productName = BootstrapperAgent.GetBurnVariable("ProductName");
            InstallFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + $"\\{companyName}\\{productName}";
#endif

            // 从Bundle.wxs中获取变量"InstallFolder"得值进行赋值
            //InstallFolder = BootstrapperAgent.GetBurnVariable("InstallFolder");

            //if (RegisterHelper.TryGetInstallerLocation(out string installerPath))
            //{
            //    InstallFolder = installerPath + "ToolsManagement";
            //}
            //else
            //{
            //    InstallFolder = DefaultInstallFolder;
            //}
        }

        private void OnQuickInstallAction()
        {
#if DEBUG
            BootstrapperApplicationAgent.Instance.OnStateChanged(InstallState.Applying);
#else
            CreateFolder();
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
            CreateFolder();

            var folderDialog = new VistaFolderBrowserDialog();
            folderDialog.Description = "选择安装路径";
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

        private void CreateFolder()
        {
            if (!string.IsNullOrEmpty(InstallFolder) && !Directory.Exists(InstallFolder))
            {
                var process= Process.Start(new ProcessStartInfo("cmd", $"/c mkdir \"{InstallFolder}\"")
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas",
                });

                process.WaitForExit();
                process.Close();
            }
        }

        /// <summary>
        /// 开始安装时调用的方法为BootstrapperAgent.PlanAction(LaunchAction.Install),
        /// 该方法执行完成之后触发本事件，事件中调用ApplyAction()方法开始执行安装进程
        /// </summary>
        private void BootstrapperApplication_PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            BootstrapperAgent.ApplyAction();
        }

        /// <summary>
        /// 当安装进程开始时触发事件,
        /// 将当前状态更改为Applying
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BootstrapperApplication_ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            App.SetInstallState(InstallState.Applying);
        }
    }
}
