using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



/*
 * Description：ChangeRepairUninstallViewModel
 * Author：chance
 * Organization: CookCSharp
 * Create Time：2022-05-08 03:44:34
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright (c) 2022 All Rights Reserved.
 */
namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class ChangeRepairUninstallViewModel: ViewModelBase
    {      
        public DelegateCommand ModifyCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnModifyAction)).Value;
        public DelegateCommand RepairCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnRepairAction)).Value;
        public DelegateCommand UninstallCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnUninstallAction)).Value;


        public ChangeRepairUninstallViewModel()
        {
#if RELEASE
            BootstrapperAgent.BootstrapperApplication.PlanComplete += BootstrapperApplication_PlanComplete;
            BootstrapperAgent.BootstrapperApplication.ApplyBegin += BootstrapperApplication_ApplyBegin;
#endif
        }

        private void BootstrapperApplication_PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            BootstrapperAgent.ApplyAction();
        }

        private void BootstrapperApplication_ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            App.SetInstallState(InstallState.Applying);
        }

        private void OnModifyAction()
        {
#if RELEASE
            BootstrapperAgent.PlanAction(LaunchAction.Modify);
#endif
        }

        private void OnRepairAction()
        {
#if RELEASE
            BootstrapperAgent.PlanAction(LaunchAction.Repair);
#endif
        }

        private void OnUninstallAction()
        {
#if RELEASE
            App.SetUninstalled();
            BootstrapperAgent.PlanAction(LaunchAction.Uninstall);
#endif
        }
    }
}
