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



/*
 * Description:ViewModelBase
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/17 10:23:27
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © CookCSharp 2018-2022 All Rights Reserved
 */
namespace CookPopularInstaller.CustomUI
{
    public class ViewModelBase : BindableBase
    {
        public BootstrapperApplicationAgent BootstrapperAgent => BootstrapperApplicationAgent.Instance;

        public string ProductName => BootstrapperAgent.ProductName;

        public IRegionManager RegionManager { get; private set; }

        public IEventAggregator EventAggregator { get; private set; }

        public DelegateCommand CancelCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnCancelAction)).Value;

        private void OnCancelAction()
        {
            App.SetInstallState(InstallState.Cancelled);
        }

        public ViewModelBase()
        {
            RegionManager = ContainerLocator.Container.Resolve<IRegionManager>();
            EventAggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
        }
    }
}
