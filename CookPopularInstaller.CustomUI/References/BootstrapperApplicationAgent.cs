using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CookPopularCSharpToolkit.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Prism.Regions;
using Prism.Ioc;


/*
 * Description:BootstrapperApplicationAgent
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/17 16:49:31
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © CookCSharp 2018-2022 All Rights Reserved
 */
namespace CookPopularInstaller.CustomUI
{
    public class BootstrapperApplicationAgent
    {
        private IntPtr hwnd = IntPtr.Zero;
#if DEBUG
        private static BootstrapperApplicationAgent _instance = new BootstrapperApplicationAgent();
#else
        private static BootstrapperApplicationAgent _instance;
#endif

        public string ProductName { get; private set; }
        public int FinalResult { get; set; }
        public BootstrapperApplication BootstrapperApplication { get; private set; }
        public static BootstrapperApplicationAgent Instance => _instance;


        static BootstrapperApplicationAgent() { }

#if DEBUG
        private BootstrapperApplicationAgent()
        {
            ProductName = "CookPopularInstaller";
        }
#endif

        private BootstrapperApplicationAgent(BootstrapperApplication bootstrapperApplication)
        {
            BootstrapperApplication = bootstrapperApplication;
            hwnd = IntPtr.Zero;
            string[] strs = GetCommandLine();
            ProductName = BootstrapperApplication.Engine.StringVariables["ProductName"];
        }

        public static BootstrapperApplicationAgent GetBootstrapperApplication(BootstrapperApplication bootstrapperApplication)
        {
            if (_instance == null)
            {
                _instance = new BootstrapperApplicationAgent(bootstrapperApplication);
            }

            return _instance;
        }

        public void SetWindowHandle(Window win)
        {
            //var win = WindowExtension.GetActiveWindow();
            hwnd = win.EnsureHandle();
        }

        public void PlanAction(LaunchAction action)
        {
            BootstrapperApplication.Engine.Plan(action);
        }

        public void ApplyAction()
        {
            BootstrapperApplication.Engine.Apply(hwnd);
        }

        public void SetBurnVariable(string variableName, string value)
        {
            BootstrapperApplication.Engine.StringVariables[variableName] = value;
        }

        public string GetBurnVariable(string variableName)
        {
            return BootstrapperApplication.Engine.StringVariables[variableName];
        }

        public string[] GetCommandLine()
        {
            return BootstrapperApplication.Command.GetCommandLineArgs();
        }

        public bool HelpRequested()
        {
            return BootstrapperApplication.Command.Action == LaunchAction.Help;
        }

        public void LogMessage(string message, LogLevel level = LogLevel.Standard)
        {
            BootstrapperApplication.Engine.Log(level, message);
        }


        public void OnStateChanged(InstallState state)
        {
            switch (state)
            {
                case InstallState.Initializing:
                    NavigateToSourceView("InstallView");
                    break;
                case InstallState.Present:
                    NavigateToSourceView("ChangeRepairUninstallView");
                    break;
                case InstallState.NotPresent:
                    if (RegisterHelper.ChekExistFromRegistry())
                    {
                        NavigateToSourceView("DetectInfoView");
                        //PlanAction(LaunchAction.UpdateReplace);
                    }
                    else
                        NavigateToSourceView("InstallView");
                    break;
                case InstallState.Applying:
                    NavigateToSourceView("ProgressView");
                    break;
                case InstallState.Cancelled:
                    StandardBootstrapperApplication.InvokeShutdown();
                    PlanAction(LaunchAction.Uninstall);
                    break;
                case InstallState.Applyed:
                    NavigateToSourceView("FinishView");
                    break;
                case InstallState.Failed:
                    break;
                default:
                    break;
            }
        }
        private void NavigateToSourceView(string navigatePath)
        {
            StandardBootstrapperApplication.InvokeAsync(() =>
            {
                if (!string.IsNullOrEmpty(navigatePath))
                    ContainerLocator.Container.Resolve<IRegionManager>().RequestNavigate(RegionToken.MainWindowContentRegion, navigatePath, NavigateCompleted);
            });
        }

        private void NavigateCompleted(NavigationResult result)
        {
#if RELEASE
            LogMessage(string.Format("****************Navigation to {0} {1} {2}********************* ", result.Context.Uri, result.Result.Value, result.Error?.Message));
#endif
        }
    }
}
