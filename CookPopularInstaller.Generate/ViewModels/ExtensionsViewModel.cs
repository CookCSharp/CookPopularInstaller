/*
 * Description：ExtensionsViewModel 
 * Author： Chance.Zheng
 * Create Time: 2023/3/7 13:37:07
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */


using CookPopularUI.WPF.Controls;
using CookPopularUI.WPF.Windows;
using CookPopularToolkit;
using Microsoft.Win32;
using CookPopularInstaller.Generate.Models;
using CookPopularInstaller.Generate.Views;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using CookPopularInstaller.Toolkit.Helpers;
using CookPopularInstaller.Toolkit;

namespace CookPopularInstaller.Generate.ViewModels
{
    public class ExtensionsViewModel : ViewModelBase<ExtensionInfo>
    {
        public DelegateCommand EnvironmentAddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnEnvironmentAddAction)).Value;
        public DelegateCommand RegistryAddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnRegistryAddAction)).Value;
        public DelegateCommand ServiceAddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnServiceAddAction)).Value;


        public void OnLoaded()
        {
            Model = Package.Extension;
        }

        private async void OnEnvironmentAddAction()
        {
            var environmentVariable = await DialogWindow.Show<EnvironmentView>("环境变量").Initialize<EnvironmentViewModel>(vm => { }).GetResultAsync<EnvironmentVariable>();
            if (!string.IsNullOrEmpty(environmentVariable.Name) && !string.IsNullOrEmpty(environmentVariable.Value))
                Model.EnvironmentVariables.Add(environmentVariable);
        }

        private async void OnRegistryAddAction()
        {
            var registryVariable = await DialogWindow.Show<Views.RegistryView>("注册表").Initialize<RegistryViewModel>(vm => { }).GetResultAsync<RegistryVariable>();
            if (!string.IsNullOrEmpty(registryVariable.Path))
                Model.RegistryVariables.Add(registryVariable);
        }

        private async void OnServiceAddAction()
        {
            var windowsServiceVariable = await DialogWindow.Show<ServiceView>("服务").Initialize<ServiceViewModel>(vm => { }).GetResultAsync<WindowsServiceVariable>();
            if (!string.IsNullOrEmpty(windowsServiceVariable.Name) && !string.IsNullOrEmpty(windowsServiceVariable.Location))
                Model.WindowsServices.Add(windowsServiceVariable);
        }
    }
}
