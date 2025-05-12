using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CookPopularControl.Windows;
using CookPopularCSharpToolkit.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using NCATestInstaller.Toolkit.Helpers;
using Ookii.Dialogs.Wpf;



/*
 * Description:StandardBootstrapperApplication
 * Author: Chance.Zheng
 * Company: NCATest
 * CreateTime: 2022/8/17 16:44:38
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © NCATest 2018-2022 All Rights Reserved
 */
namespace NCATestInstaller.CustomUI
{
    public class StandardBootstrapperApplication : BootstrapperApplication
    {
        private Dictionary<string, int> _downloadRetries = new Dictionary<string, int>();
        private static Dispatcher _dispatcher;
        private Mutex _installerMutext;
        private string _arguments;


        public static void InvokeShutdown() => (_dispatcher ?? Dispatcher.CurrentDispatcher).InvokeShutdown();
#if !ACTUALTEST
        public static DispatcherOperation InvokeAsync(Action action) => Application.Current.Dispatcher.InvokeAsync(action, DispatcherPriority.Normal);
#else
        public static DispatcherOperation InvokeAsync(Action action) => _dispatcher.InvokeAsync(action, DispatcherPriority.Normal);
#endif

        private Process GetParent(Process process)
        {
            var processName = Process.GetProcessById(process.Id).ProcessName;
            var processes = Process.GetProcessesByName(processName);
            string processIndexName = null;

            for (int i = 0; i < processes.Length; i++)
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

        //如果不使用管理员方式，使用源安装包与handle.exe检测文件被进程占用情况会很慢
        private void RunAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            //AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            //WindowsPrincipal principal = (WindowsPrincipal)Thread.CurrentPrincipal;
            //bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            //Engine.Log(LogLevel.Standard, $"**************是否以管理员权限运行：{isAdmin}******************");

            if (!isAdmin)
            {
                //测试代码
                //using var sw = new StreamWriter(@"D:\Users\chance.zheng\Desktop\123123.txt", true);
                //Array.ForEach(Environment.GetCommandLineArgs(), arg =>
                //{
                //    sw.WriteLine(arg);
                //});
                //sw.WriteLine(_arguments);
                //sw.Flush();

                _arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(4));
                var process = Process.GetCurrentProcess();
                var fileName = GetParent(process).MainModule.FileName;
                Process.Start(new ProcessStartInfo { FileName = fileName, Verb = "runas", Arguments = _arguments });
                process.Kill();
            }

            Engine.Log(LogLevel.Standard, $"**************是否以管理员权限运行：{isAdmin}******************");
        }

        private bool CheckIsRunning()
        {
            var upgradeCode = Engine.StringVariables["UpgradeCode"];
            _installerMutext = new Mutex(false, $"Global\\CookPopularInstaller_{upgradeCode}", out bool createdNew);
            if (!createdNew)
            {
                string message = ConfigurationManageHelper.ReadItem("Language") == "Chinese"
                    ? "正在运行此实例时，无法启动安装程序的另一实例。" : "You cannot start another instance of the installer, because you are running this instance.";
                MessageBox.Show(message);
            }

            return !createdNew;
        }

        private void LaunchUI()
        {
            //命令行安装
            if (Environment.GetCommandLineArgs().Contains("/quiet", StringComparer.OrdinalIgnoreCase) &&
                Environment.GetCommandLineArgs().Contains("/install", StringComparer.OrdinalIgnoreCase) &&
                Environment.GetCommandLineArgs().Contains("/output", StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var hwndParent = IntPtr.Zero;
                    //只显示安装进度
                    if (Environment.GetCommandLineArgs().Contains("/passive", StringComparer.OrdinalIgnoreCase))
                    {
                        var passiveWin = new Views.PassiveWindow();
                        passiveWin.Show();
                        hwndParent = passiveWin.EnsureHandle();
                    }

                    var index = Array.IndexOf(Environment.GetCommandLineArgs(), "/output", 0);
                    var installFolder = Environment.GetCommandLineArgs()[index + 1];
                    if (!Directory.Exists(installFolder)) Directory.CreateDirectory(installFolder);

                    Engine.StringVariables["InstallFolder"] = installFolder;
                    Engine.Plan(LaunchAction.Install);
                    Engine.Apply(hwndParent);
                    ApplyComplete += (s, e) => InvokeShutdown();
                }
                catch (Exception ex)
                {
                    Engine.Log(LogLevel.Error, ex.Message);
                }
            }
            //命令行卸载，Uninst.exe原理亦如此
            else if (Environment.GetCommandLineArgs().Contains("/quiet", StringComparer.OrdinalIgnoreCase) &&
                     Environment.GetCommandLineArgs().Contains("/uninstall", StringComparer.OrdinalIgnoreCase))
            {
                var hwndParent = IntPtr.Zero;
                //只显示卸载进度
                if (Environment.GetCommandLineArgs().Contains("/passive", StringComparer.OrdinalIgnoreCase))
                {
                    var passiveWin = new Views.PassiveWindow();
                    passiveWin.Show();
                    hwndParent = passiveWin.EnsureHandle();
                }

                Engine.Plan(LaunchAction.Uninstall);
                Engine.Apply(hwndParent);
                ApplyComplete += (s, e) => InvokeShutdown();
            }
            else
            {
                //CommonTools.PreInstallState = InstallationState.Detecting;
                Application.ResourceAssembly = Assembly.GetExecutingAssembly(); //设置WPF Application的资源程序集，避免WPF自己找不到

                SingletonAppWrapper wrapper = new SingletonAppWrapper(); //单例进程
                wrapper.Run(wrapper.CommandLineArgs.ToArray());
            }
        }

        protected override void Run()
        {
            try
            {
                //RunAsAdmin();
                //Engine.Elevate(IntPtr.Zero);
                //Debugger.Launch();

                if (CheckIsRunning()) return;

                if (Environment.GetCommandLineArgs().Contains("-debug", StringComparer.OrdinalIgnoreCase))
                {
                    Debugger.Launch();
                }

                _dispatcher = Dispatcher.CurrentDispatcher;
                var agent = BootstrapperApplicationAgent.GetBootstrapperApplication(this);

                if ((!Environment.GetCommandLineArgs().Contains("/install", StringComparer.OrdinalIgnoreCase) &&
                     !Environment.GetCommandLineArgs().Contains("/uninstall", StringComparer.OrdinalIgnoreCase)) ||
                      Environment.GetCommandLineArgs().Contains("/passive", StringComparer.OrdinalIgnoreCase))
                {
                    var splashScreen = new SplashScreen(Assembly.GetExecutingAssembly(), "Assets\\Images\\SplashScreen.png");
                    splashScreen.Show(true, true);
                }

                if (CommonTools.IsRelated())
                {
                    Engine.Detect();
                    LaunchUI();

                    //InvokeAsync(LaunchUI);
                    //Engine.Detect();
                    //Dispatcher.Run();
                }
                else
                {
                    Engine.Detect();
                }

                Engine.Log(LogLevel.Standard, $"FinalResult's value is {agent.FinalResult}");
                Engine.Log(LogLevel.Standard, string.Format("The {0} is installed {1}", BootstrapperApplicationAgent.Instance.ProductName, agent.FinalResult == 0 ? "Successfully" : "Failed"));
                Engine.Log(LogLevel.Standard, $"The {BootstrapperApplicationAgent.Instance.ProductName} has exited.");
                Engine.Quit(agent.FinalResult);
            }
            catch (Exception ex)
            {
                Engine.Log(LogLevel.Standard, $"The {BootstrapperApplicationAgent.Instance.ProductName} is failed: {ex}");
            }
            finally
            {
                Engine.Quit(0);

                try
                {
                    //只有在当前线程拥有Mutex的所有权时才能调用ReleaseMutex
                    _installerMutext?.ReleaseMutex();
                }
                catch (ApplicationException) { }
                finally
                {
                    _installerMutext?.Dispose();
                }
            }
        }

        protected override void OnDetectBegin(DetectBeginEventArgs args)
        {
            Engine.Log(LogLevel.Standard, $"**********DetectBegin---Installed:{args.Installed};PackageCount:{args.PackageCount}**********");

            CommonTools.DetectState = args.Installed ? DetectionState.Present : DetectionState.Absent;

            //显示DetectingView界面所用，与141行呼应
            //if (CommonTools.IsRelated()) CommonTools.Suspend();
        }

        protected override void OnDetectRelatedBundle(DetectRelatedBundleEventArgs args)
        {
            Engine.Log(LogLevel.Standard, $"**********DetectRelatedBundle---ProductCode:{args.ProductCode};Version:{args.Version};RelationType:{args.RelationType};RelatedOperation:{args.Operation}**********");

            var productVersion = Engine.StringVariables["WixBundleVersion"];
            if (args.Operation == RelatedOperation.Downgrade || args.Version == new Version(productVersion))
                CommonTools.DetectState = DetectionState.Newer;

            CommonTools.SetRelatedOperation(args.Operation);
            Engine.StringVariables["PreviousWixBundleProviderKey"] = args.ProductCode;
        }

        protected override void OnDetectPackageBegin(DetectPackageBeginEventArgs args)
        {
            Engine.Log(LogLevel.Standard, $"**********DetectPackageBegin---PackageId:{args.PackageId};Status:{args.Result}**********");
        }

        protected override void OnDetectRelatedMsiPackage(DetectRelatedMsiPackageEventArgs args)
        {
            Engine.Log(LogLevel.Standard, $"**********DetectRelatedMsiPackage---PackageId:{args.PackageId};ProductCode:{args.ProductCode};Version:{args.Version};RelatedOperation:{args.Operation}**********");

            Engine.StringVariables["PreviousProductCode"] = args.ProductCode;
            Engine.StringVariables["PreviousProductVersion"] = args.Version.ToString();
        }

        protected override void OnDetectPackageComplete(DetectPackageCompleteEventArgs args)
        {
            Engine.Log(LogLevel.Standard, $"**********DetectPackageComplete---PackageId:{args.PackageId};State:{args.State};Status:{args.Status}**********");

            //if (args.PackageId.Equals(CommonTools.MsiPackageId, StringComparison.Ordinal))
            //{
            //    var installState = args.State == PackageState.Present ? InstallationState.Present : InstallationState.Absent;
            //    App.SetInstallState(installState);
            //}
        }

        protected override void OnDetectComplete(DetectCompleteEventArgs args)
        {
            Engine.Log(LogLevel.Standard, $"**********DetectComplete---Status:{args.Status}**********");

            ParseCommandLine();

            if (CommonTools.IsRelated())
            {
                switch (CommonTools.DetectState)
                {
                    case DetectionState.Absent:
                        App.SetInstallState(InstallationState.Absent);
                        break;
                    case DetectionState.Present:
                        App.SetInstallState(InstallationState.Present);
                        break;
                    case DetectionState.Newer:
                        App.SetInstallState(InstallationState.Absent);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (Command.Action == LaunchAction.Uninstall)
                {
                    Engine.Plan(Command.Action);
                }
                else if (CommonTools.IsSucceeded(args.Status))
                {
                    if (Command.Action == LaunchAction.Layout) PlanLayout();
                    else if (Command.Display != Display.Full) Engine.Plan(Command.Action);
                }
                else
                {
                    App.SetInstallState(InstallationState.Failed);
                }
            }
        }

        protected override void OnPlanBegin(PlanBeginEventArgs args)
        {

        }

        protected override void OnPlanRelatedBundle(PlanRelatedBundleEventArgs args)
        {

        }

        protected override void OnPlanPackageBegin(PlanPackageBeginEventArgs args)
        {
            //if (Engine.StringVariables.Contains("MbaNetfxPackageId") && args.PackageId.Equals(Engine.StringVariables["MbaNetfxPackageId"], StringComparison.Ordinal))
            //{
            //    args.State = RequestState.None;
            //}
        }

        protected override void OnPlanPackageComplete(PlanPackageCompleteEventArgs args)
        {
            Engine.Log(LogLevel.Standard, $"**********PlanPackageComplete---State:{args.State};Requested:{args.Requested};Execute:{args.Execute};Rollback:{args.Rollback}**********");

            var action = Enum.Parse(typeof(LaunchAction), Engine.StringVariables["WixBundleAction"], true);
            CommonTools.SetActionState(args.Execute);
        }

        /// <summary>
        /// Action开始时调用的方法为BootstrapperAgent.PlanAction(LaunchAction),
        /// 该方法执行完成之后触发本事件，事件中调用ApplyAction()方法开始执行安装进程
        /// </summary>
        protected override void OnPlanComplete(PlanCompleteEventArgs args)
        {
            if (CommonTools.IsSucceeded(args.Status))
            {
                CommonTools.PreInstallState = App.GetInstallState();
                BootstrapperApplicationAgent.Instance.ApplyAction();
            }
            else
            {
                App.SetInstallState(InstallationState.Failed);
            }
        }

        protected override void OnApplyPhaseCount(ApplyPhaseCountArgs args)
        {
            CommonTools.PhaseCount = args.PhaseCount;
        }

        /// <summary>
        /// 当Action进程开始时触发事件，将当前状态更改为Applying
        /// </summary>
        /// <param name="args"></param>
        protected override void OnApplyBegin(ApplyBeginEventArgs args)
        {
            _downloadRetries.Clear();
            App.SetInstallState(InstallationState.Applying);
        }

        //耗时
        protected override void OnElevate(ElevateEventArgs args)
        {

        }

        protected override void OnRegisterBegin(RegisterBeginEventArgs args)
        {

        }

        protected override void OnRegisterComplete(RegisterCompleteEventArgs args)
        {

        }

        protected override void OnUnregisterBegin(UnregisterBeginEventArgs args)
        {

        }

        protected override void OnUnregisterComplete(UnregisterCompleteEventArgs args)
        {

        }

        protected override void OnApplyComplete(ApplyCompleteEventArgs args)
        {
            BootstrapperApplicationAgent.Instance.FinalResult = args.Status;

            args.Result = CommonTools.Canceled ? Result.Cancel : Result.Ok;
            if (args.Result == Result.Cancel)
            {
                App.SetInstallState(InstallationState.Cancelled);
            }
            else if (CommonTools.IsSucceeded(args.Status))
            {
                if (CommonTools.IsRelated())
                {
                    if (App.GetInstallState() != CommonTools.PreInstallState)
                    {
                        App.SetInstallState(InstallationState.Applyed);
                    }
                }
                else
                {
                    if (Command.Display != Display.Full)
                    {
                        if (Command.Display == Display.Passive)
                        {
                            //InvokeShutdown();
                        }
                        else
                        {
                            //InvokeShutdown();
                        }
                    }
                    else if (CommonTools.IsSucceeded(args.Status) && CommonTools.GetLaunchAction() == LaunchAction.UpdateReplace)
                    {
                        //InvokeShutdown();
                    }
                }
            }
            else
            {
                App.SetInstallState(InstallationState.Failed);
            }

            base.OnApplyComplete(args);
        }

        protected override void OnError(Microsoft.Tools.WindowsInstallerXml.Bootstrapper.ErrorEventArgs args)
        {
            lock (this)
            {
                Engine.Log(LogLevel.Error, $"*************BootstrapperApplicationError---ErrorCode:{args.ErrorType};ErrorCode:{args.ErrorCode};ErrorMessage:{args.ErrorMessage}****************");

                if (!CommonTools.Canceled)
                {
                    if (App.GetInstallState() == InstallationState.Applying && args.ErrorCode == (int)ErrorCode.UserCancelled)
                    {
                        App.SetInstallState(InstallationState.Cancelled);
                    }
                    else
                    {
                        if (Command.Display == Display.Full)
                        {
                            if (ErrorType.HttpServerAuthentication == args.ErrorType || ErrorType.HttpProxyAuthentication == args.ErrorType)
                            {
                                args.Result = Result.TryAgain;
                            }
                            else
                            {
                                MessageBoxButton msgbox = MessageBoxButton.OK;
                                switch (args.UIHint & 0xF)
                                {
                                    case 0:
                                        msgbox = MessageBoxButton.OK;
                                        break;
                                    case 1:
                                        msgbox = MessageBoxButton.OKCancel;
                                        break;
                                    // There is no 2! That would have been MB_ABORTRETRYIGNORE.
                                    case 3:
                                        msgbox = MessageBoxButton.YesNoCancel;
                                        break;
                                    case 4:
                                        msgbox = MessageBoxButton.YesNo;
                                        break;
                                    // default: stay with MBOK since an exact match is not available.
                                    default:
                                        break;
                                }

                                MessageBoxResult result = MessageBoxResult.None;
                                InvokeAsync(() => result = MessageDialog.Show(args.ErrorMessage, "WiX Toolset", msgbox, MessageBoxImage.Error));
                                if ((args.UIHint & 0xF) == (int)msgbox) args.Result = (Result)result;
                            }
                        }
                    }
                }
                else
                {
                    args.Result = Result.Cancel;
                }

                CommonTools.SetMessage(args.ErrorMessage);
                App.SetInstallState(InstallationState.Failed);
            }
        }

        protected override void OnResolveSource(ResolveSourceEventArgs args)
        {
            _downloadRetries.TryGetValue(args.PackageOrContainerId, out int retries);
            _downloadRetries[args.PackageOrContainerId] = retries + 1;

            args.Result = retries < 3 && !string.IsNullOrEmpty(args.DownloadSource) ? Result.Download : Result.Ok;
        }

        private void ParseCommandLine()
        {
            string[] args = Command.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith("InstallFolder=", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] param = args[i].Split(new char[] { '=' }, 2);
                    CommonTools.InstallDirectory = Path.Combine(Environment.CurrentDirectory, param[1]);
                }
            }
        }

        private void PlanLayout()
        {
            if (string.IsNullOrEmpty(CommonTools.LayoutDirectory))
            {
                CommonTools.LayoutDirectory = Directory.GetCurrentDirectory();

                if (Command.Display == Display.Full)
                {
                    InvokeAsync(delegate
                    {
                        var folderDialog = new VistaFolderBrowserDialog();
                        folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                        folderDialog.SelectedPath = CommonTools.LayoutDirectory;
                        if (folderDialog.ShowDialog().Value)
                        {
                            CommonTools.LayoutDirectory = folderDialog.SelectedPath;
                            Engine.Plan(Command.Action);
                        }
                        else
                        {
                            InvokeShutdown();
                        }
                    });
                }
            }
            else
            {
                CommonTools.LayoutDirectory = Command.LayoutDirectory;
                Engine.Plan(Command.Action);
            }
        }
    }
}
