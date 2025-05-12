/*
 * Description：ChangeRepairUninstallViewModel
 * Author：chance
 * Organization: CookCSharp
 * Create Time：2022-05-08 03:44:34
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright (c) 2022 All Rights Reserved.
 */


using CookPopularControl.Controls;
using CookPopularControl.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;


namespace NCATestInstaller.CustomUI.ViewModels
{
    public class ChangeRepairUninstallViewModel : ViewModelBase
    {

#if !ACTUALTEST
        public bool IsShowNextButton { get; set; } = true;
        public DelegateCommand NextButtonCommand => new DelegateCommand(() =>
        {
            App.SetInstallState(InstallationState.Running);
        });
#else
        public bool IsShowNextButton { get; set; } = false;
#endif

        public DelegateCommand<LaunchAction?> LaunchActionCommand => new Lazy<DelegateCommand<LaunchAction?>>(() => new DelegateCommand<LaunchAction?>(OnLaunchAction)).Value;

        private async void OnLaunchAction(LaunchAction? launchAction)
        {
            if (launchAction == LaunchAction.Uninstall)
            {
                if (MessageDialog.ShowQuestion(string.Concat(App.Current.Resources["Ask"].ToString(), ProductName, "?"), App.Current.Resources["Remove"].ToString()) == MessageBoxResult.OK)
                {
                    App.DialogBox = DialogBox.Show<DotCircleLoading>("Detecting");
#if ACTUALTEST
                    await App.CheckProcessesRunningAsync(launchAction.Value);
#else
                    await System.Threading.Tasks.Task.Delay(2000);
                    App.SetInstallState(InstallationState.Applying);
#endif
                }
            }
            else
            {
                App.DialogBox = DialogBox.Show<DotCircleLoading>("Detecting");
#if ACTUALTEST
                await App.CheckProcessesRunningAsync(launchAction.Value);
#else
                await System.Threading.Tasks.Task.Delay(2000);
                App.SetInstallState(InstallationState.Applying);
#endif               
            }

            App.DialogBox?.Close();
        }
    }
}
