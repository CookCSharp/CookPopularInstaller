using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using System.ComponentModel;
using PropertyChanged;
using NCATestInstaller.Generate.Models;
using NCATestInstaller.Toolkit;


/*
 * Description：ViewModelBase 
 * Author： Chance.Zheng
 * Create Time: 2023/2/15 18:12:31
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2018-2023 All Rights Reserved.
 */
namespace NCATestInstaller.Generate.ViewModels
{
    public class ViewModelBase : BindableBase
    {
        [DoNotNotify]
        public IEventAggregator EventAggregator { get; private set; }

        public DelegateCommand ViewLoadedCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnViewLoadedActionAsync)).Value;

        protected virtual void OnViewLoadedActionAsync()
        {

        }

        public ViewModelBase()
        {
            GetEventAggregator();
        }

        ////先触发基类构造函数再触发子类的构造函数，最后执行方法注入
        //[InjectionMethod]
        //public void GetEventAggregator(IEventAggregator eventAggregator)
        //{
        //    EventAggregator = eventAggregator;
        //    OnPubSubMessage();
        //}

        private void GetEventAggregator()
        {
            EventAggregator = App.Container.Resolve<IEventAggregator>();
            OnPubSubMessage();
        }

        /// <summary>
        /// 发布/订阅消息
        /// </summary>
        protected virtual void OnPubSubMessage()
        {

        }
    }

    public class ViewModelBase<TModel> : ViewModelBase where TModel : class, new()
    {
        /// <summary>
        /// ViewModel所使用的Binding数据模型
        /// </summary>
        public TModel Model { get; protected set; } = new TModel();

        public PackageInfo Package { get; set; } = Build.ReadPackageJsonFile(App.PackageJsonFileName);

        public DelegateCommand SaveCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnSaveAction)).Value;

        protected virtual void OnSaveAction()
        {
            switch (typeof(TModel).Name)
            {
                case nameof(ProjectInfo):
                    Package.Project = (dynamic)Model;
                    break;
                case nameof(ConfuseInfo):
                    Package.Confuse = (dynamic)Model;
                    break;
                case nameof(DependInfo):
                    Package.Depend = (dynamic)Model;
                    break;
                case nameof(ExtensionInfo):
                    Package.Extension = (dynamic)Model;
                    break;
                default:
                    break;
            }
            App.SavePackageFile(Package);
        }

        public DelegateCommand ReloadCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnReloadAction)).Value;

        private void OnReloadAction()
        {
            Package = Build.ReadPackageJsonFile(App.PackageJsonFileName);
            switch (typeof(TModel).Name)
            {
                case nameof(ProjectInfo):
                    Model = (dynamic)Package.Project;
                    break;
                case nameof(ConfuseInfo):
                    Model = (dynamic)Package.Confuse;
                    break;
                case nameof(DependInfo):
                    Model = (dynamic)Package.Depend;
                    break;
                case nameof(ExtensionInfo):
                    Model = (dynamic)Package.Extension;
                    break;
                default:
                    break;
            }
        }
    }
}
