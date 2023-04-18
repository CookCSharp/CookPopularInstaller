using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;



/*
 * Description:StandardBootstrapperApplication
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/17 16:44:38
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © CookCSharp 2018-2022 All Rights Reserved
 */
namespace CookPopularInstaller.CustomUI
{
    public class StandardBootstrapperApplication : BootstrapperApplication
    {
        private static Dispatcher _dispatcher;
        public static InstallState InitState { get; private set; }
        public static void InvokeShutdown() => _dispatcher?.InvokeShutdown();
#if DEBUG
        public static void InvokeAsync(Action action) => Application.Current.Dispatcher.InvokeAsync(action, DispatcherPriority.Normal);
#else
        public static void InvokeAsync(Action action) => _dispatcher.InvokeAsync(action, DispatcherPriority.Normal);
#endif

        private bool CheckIfAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            string msg = isAdmin ? "管理员" : "非管理员";
            Engine.Log(LogLevel.Standard, $"当前用户{principal.Identity.Name}正以{msg}权限运行");
            MessageBox.Show(msg);

            AppDomain domain = Thread.GetDomain();
            domain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal windowsPrincipal = (WindowsPrincipal)Thread.CurrentPrincipal;
            windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);

            return isAdmin;
        }

        private Process FindParentProcess(Process process)
        {
            var processName = Process.GetProcessById(process.Id).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexName = null;

            for (int i = 0; i < processesByName.Length; i++)
            {
                processIndexName = i == 0 ? processName : processName + "#" + i;
                var processId = new PerformanceCounter("Process", "Creating Process ID", processIndexName);
                if ((int)processId.NextValue() == process.Id)
                {
                    break;
                }
            }

            var parentId = new PerformanceCounter("Process", "Creating Process ID", processIndexName);
            var parentProcess = Process.GetProcessById((int)parentId.NextValue());

            return parentProcess;
        }

        private int LaunchUI()
        {
            // 设置WPF Application 的资源程序集，避免 WPF 自己找不到。
            Application.ResourceAssembly = Assembly.GetExecutingAssembly();

            // 正常启动 WPF Application。
            var app = new App();
            app.InitializeComponent();
            return app.Run();
        }

        protected override void Run()
        {
            try
            {
                if (Environment.GetCommandLineArgs().Contains("-debug", StringComparer.OrdinalIgnoreCase))
                {
                    Debugger.Launch();
                }

                _dispatcher = Dispatcher.CurrentDispatcher;
                var agent = BootstrapperApplicationAgent.GetBootstrapperApplication(this);

                Engine.Detect();
                LaunchUI();

                Engine.Log(LogLevel.Standard, $"The {BootstrapperApplicationAgent.Instance.ProductName} is Installed Successfully");
                Engine.Log(LogLevel.Standard, $"FinalResult's value is {agent.FinalResult}");
                Engine.Quit(agent.FinalResult);
            }
            catch (Exception ex)
            {
                Engine.Log(LogLevel.Standard, $"The {BootstrapperApplicationAgent.Instance.ProductName} is failed: {ex}");
                Engine.Quit(0);
            }
            finally
            {
                Engine.Log(LogLevel.Standard, $"The {BootstrapperApplicationAgent.Instance.ProductName} has exited.");
            }
        }

        protected override void OnDetectBegin(DetectBeginEventArgs args)
        {
            base.OnDetectBegin(args);

            var wixBundleProviderKey = Engine.StringVariables["WixBundleProviderKey"];
            var keyPath = $@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{wixBundleProviderKey}";
            var registryKey = RegisterHelper.GetRegistryKey(keyPath);
            if (registryKey != null)
            {
                //args.Result = Result.
                //var appKey = RegisterHelper.GetToolsManagementBaseKey();
                //appKey.SetValue("ProviderKey", wixBundleProviderKey, Microsoft.Win32.RegistryValueKind.String);
            }
        }

        protected override void OnDetectPackageComplete(DetectPackageCompleteEventArgs args)
        {
            base.OnDetectPackageComplete(args);

            Engine.Log(LogLevel.Standard, $"**********DetectPackageComplete---PackageId:{args.PackageId};State:{args.State};Status:{args.Status}**********");

            if (args.PackageId.Equals(App.MsiPackageId, StringComparison.Ordinal))
            {
                InitState = args.State == PackageState.Present ? InstallState.Present : InstallState.NotPresent;
            }
        }

        protected override void OnDetectComplete(DetectCompleteEventArgs args)
        {
            base.OnDetectComplete(args);

            Engine.Log(LogLevel.Standard, $"**********DetectComplete---Status:{args.Status}**********");
        }
    }
}
