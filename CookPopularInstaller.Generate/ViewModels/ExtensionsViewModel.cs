using CookPopularControl.Controls;
using CookPopularControl.Windows;
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
using CookPopularCSharpToolkit.Communal;
using System.IO;
using CookPopularInstaller.Toolkit.Helpers;


/*
 * Description：ExtensionsViewModel 
 * Author： Chance.Zheng
 * Create Time: 2023/3/7 13:37:07
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.ViewModels
{
    public class ExtensionsViewModel : ViewModelBase<ExtensionsModel>
    {
        public DelegateCommand EnvironmentAddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnEnvironmentAddAction)).Value;
        public DelegateCommand RegistryAddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnRegistryAddAction)).Value;
        public DelegateCommand ServiceAddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnServiceAddAction)).Value;
        public DelegateCommand SaveCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnSaveAction)).Value;


        public void OnLoaded()
        {
            Model = Package.Extensions;
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

        private void OnSaveAction()
        {
            Package.Extensions = Model;
            App.SavePackageFile(Package);
        }
    }
}
