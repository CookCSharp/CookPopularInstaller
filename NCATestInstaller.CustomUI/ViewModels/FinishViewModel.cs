/*
 * Description：FinishViewModel
 * Author：chance
 * Organization: CookCSharp
 * Create Time：2022-05-09 11:02:02
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright (c) 2022 All Rights Reserved.
 */


using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using NCATestInstaller.Toolkit.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace NCATestInstaller.CustomUI.ViewModels
{
    public class FinishViewModel : ViewModelBase
    {
#if !ACTUALTEST
        public bool IsShowNextButton { get; set; } = true;
        public DelegateCommand NextButtonCommand => new DelegateCommand(() =>
        {
            App.SetInstallState(InstallationState.Present);
        });
#else
        public bool IsShowNextButton { get; set; } = false;
#endif
        public string ResultMessage => string.Format("{0}{1}", CommonTools.GetRelatedOperation(), App.Current.Resources["Finished"]);

        public BitmapSource BackImage
        {
            get
            {
                if (CommonTools.GetLaunchAction() == LaunchAction.Install || CommonTools.GetLaunchAction() == LaunchAction.Modify || CommonTools.GetLaunchAction() == LaunchAction.Repair)
                    return App.GetBitmapSource(new Uri("..\\Assets\\Images\\happy.png", UriKind.Relative));
                else if (CommonTools.GetLaunchAction() == LaunchAction.Uninstall)
                    return App.GetBitmapSource(new Uri("..\\Assets\\Images\\uninst.png", UriKind.Relative));
                else
                    return App.GetBitmapSource(new Uri("..\\Assets\\Images\\unknown.png", UriKind.Relative));
            }
        }

        public bool IsRunApp { get; set; }

        public bool IsUninstalled => CommonTools.GetIfUninstalled();

        public DelegateCommand FinishInstallCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(FinishInstallAction)).Value;

        private void FinishInstallAction()
        {
            if (IsRunApp && !IsUninstalled)
            {
                string appPath = string.Empty;
                if (BootstrapperAgent.ContainsBurnVariable("InstallFolder"))
                    appPath = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), BootstrapperAgent.GetBurnVariable("ExeName"));
                else
                    appPath = Path.Combine(CommonTools.GetInstallLocation(), BootstrapperAgent.GetBurnVariable("ExeName"));

                Process.Start(appPath);
            }
            StandardBootstrapperApplication.InvokeShutdown();
        }
    }
}
