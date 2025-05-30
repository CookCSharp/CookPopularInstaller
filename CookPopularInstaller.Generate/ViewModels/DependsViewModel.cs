/*
 * Description：DependsViewModel 
 * Author： Chance.Zheng
 * Create Time: 2023/3/4 17:48:54
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */


using CookPopularUI.WPF.Controls;
using CookPopularUI.WPF.Windows;
using CookPopularInstaller.Generate.Models;
using CookPopularInstaller.Generate.Views;
using CookPopularInstaller.Toolkit;
using CookPopularInstaller.Toolkit.Helpers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookPopularInstaller.Generate.ViewModels
{
    public class DependsViewModel : ViewModelBase<DependInfo>
    {
        public DelegateCommand AddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnAddAction)).Value;


        public void OnLoaded()
        {
            Model = Package.Depend;
        }

        private async void OnAddAction()
        {
            var dependDialogVariable = await DialogWindow.Show<DependDialogView>("依赖").Initialize<DependDialogViewModel>(vm => { }).GetResultAsync<DependDialogVariable>();
            if (!string.IsNullOrEmpty(dependDialogVariable.CheckValue) && !string.IsNullOrEmpty(dependDialogVariable.Name) && !string.IsNullOrEmpty(dependDialogVariable.Value))
                Model.DependDialogVariables.Add(dependDialogVariable);
        }
    }
}
