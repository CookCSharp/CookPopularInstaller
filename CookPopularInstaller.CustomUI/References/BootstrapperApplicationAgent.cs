/*
 * Description:BootstrapperApplicationAgent
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/17 16:49:31
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © Chance 2021 All Rights Reserved
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CookPopularToolkit.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Prism.Ioc;
using System.Diagnostics;
using CookPopularUI.WPF.Controls;
using System.IO;
using CookPopularInstaller.Toolkit.Helpers;
using Prism.Navigation;
using Prism.Navigation.Regions;


namespace CookPopularInstaller.CustomUI
{
    public class BootstrapperApplicationAgent
    {
        private IntPtr hwnd = IntPtr.Zero;
#if !ACTUALTEST
        private static BootstrapperApplicationAgent _instance = new BootstrapperApplicationAgent();
#else
        private static BootstrapperApplicationAgent _instance;
#endif      
        public string ProductName { get; private set; }
        public int FinalResult { get; set; }
        public BootstrapperApplication BootstrapperApplication { get; private set; }
        public static BootstrapperApplicationAgent Instance => _instance;


        static BootstrapperApplicationAgent() { }

#if !ACTUALTEST
        private BootstrapperApplicationAgent()
        {
            ProductName = "CookPopularInstaller";
        }
#endif

        private BootstrapperApplicationAgent(BootstrapperApplication bootstrapperApplication)
        {
            BootstrapperApplication = bootstrapperApplication;
            hwnd = IntPtr.Zero;
            ProductName = GetBurnVariable("ProductName");
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
            if (action == LaunchAction.Uninstall)
                CommonTools.SetUninstalled();

            CommonTools.SetLaunchAction(action);
#if ACTUALTEST
            BootstrapperApplication.Engine.Plan(action);
#endif
        }

        public void ApplyAction()
        {
            BootstrapperApplication.Engine.Elevate(hwnd);
            BootstrapperApplication.Engine.Apply(hwnd);
        }

        public bool ContainsBurnVariable(string variableName)
        {
            return BootstrapperApplication.Engine.StringVariables.Contains(variableName);
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

        public void OnStateChanged(InstallationState state)
        {
            switch (state)
            {
                case InstallationState.Initializing:
                    break;
                case InstallationState.Detecting:
                    NavigateToSourceView("DetectingView");
                    break;
                case InstallationState.Absent:
#if ACTUALTEST
                    bool.TryParse(ConfigurationManageHelper.ReadItem("IsAutoOverlay"), out bool isAutoOverlay);
                    if (isAutoOverlay)
                    {
                        if (CommonTools.DetectState == DetectionState.Newer)
                            NavigateToSourceView("DetectInfoView");
                        else
                            NavigateToSourceView("InstallView");
                    }
                    else
                    {
                        NavigateToSourceView("DetectInfoView");
                    }
#else
                    NavigateToSourceView("InstallView");
#endif
                    break;
                case InstallationState.Present:
                    NavigateToSourceView("ChangeRepairUninstallView");
                    break;
                case InstallationState.Running:
                    NavigateToSourceView("RunningView");
                    break;
                case InstallationState.Applying:
                    NavigateToSourceView("ProgressView");
                    break;
                case InstallationState.Suspend:
                    break;
                case InstallationState.Failed:
                    NavigateToSourceView("ErrorView");
                    break;
                case InstallationState.Cancelled:
                    NavigateToSourceView("CancelledView");
                    break;
                case InstallationState.Applyed:
                    NavigateToSourceView("FinishView");
                    break;
                default:
                    break;
            }
        }

        private void NavigateToSourceView(string navigatePath)
        {
            StandardBootstrapperApplication.InvokeAsync(() =>
            {
                if (ContainerLocator.Container != null && !string.IsNullOrEmpty(navigatePath))
                {
                    ContainerLocator.Container.Resolve<IRegionManager>().RequestNavigate(RegionToken.MainWindowContentRegion, navigatePath, NavigateCompleted);
                }
            });
        }

        private void NavigateCompleted(NavigationResult result)
        {
#if ACTUALTEST
            LogMessage(string.Format("****************Navigation to {0} {1} {2}********************* ", result.Context.Uri, result.Success, result.Exception?.Message));
#endif
        }
    }
}
