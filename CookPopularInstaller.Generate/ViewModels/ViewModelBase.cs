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
using CookPopularInstaller.Generate.Models;


/*
 * Description：ViewModelBase 
 * Author： Chance.Zheng
 * Create Time: 2023/2/15 18:12:31
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.ViewModels
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

        public PackageModel Package { get; set; } = App.ReadPackageFile();
    }
}
