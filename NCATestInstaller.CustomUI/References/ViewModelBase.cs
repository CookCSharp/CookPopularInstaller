/*
 * Description:ViewModelBase
 * Author: Chance.Zheng
 * Company: NCATest
 * CreateTime: 2022/8/17 10:23:27
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © NCATest 2018-2022 All Rights Reserved
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Ioc;
using System.Windows;
using Prism.Commands;
using CookPopularControl.Windows;

namespace NCATestInstaller.CustomUI
{
    public class ViewModelBase : BindableBase
    {
        public BootstrapperApplicationAgent BootstrapperAgent => BootstrapperApplicationAgent.Instance;

        public string ProductName => BootstrapperAgent.ProductName;

        public IRegionManager RegionManager { get; private set; }

        public IEventAggregator EventAggregator { get; private set; }

        public bool IsDetecting { get; set; }

        public DelegateCommand CloseWindowCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnCancelAction)).Value;

        public DelegateCommand CancelCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnCancelAction)).Value;

        private void OnCancelAction()
        {
#if ACTUALTEST
            if (App.GetInstallState() == InstallationState.Applying)
            {
                App.SetInstallState(InstallationState.Suspend);
                if (MessageDialog.ShowQuestion(string.Format("是否取消{0}", CommonTools.GetRelatedOperation())) == MessageBoxResult.OK)
                {
                    CommonTools.Canceled = true;
                    BootstrapperAgent.SetBurnVariable("CancelRequest", "yes");
                }
                else
                {
                    CommonTools.Canceled = false;
                }
                App.SetInstallState(InstallationState.Applying);
                CommonTools.Resume();
            }
            else
            {
                StandardBootstrapperApplication.InvokeShutdown();
            }
#else
            StandardBootstrapperApplication.InvokeShutdown();
#endif
        }

        public ViewModelBase()
        {
            RegionManager = ContainerLocator.Container.Resolve<IRegionManager>();
            EventAggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
        }
    }
}
