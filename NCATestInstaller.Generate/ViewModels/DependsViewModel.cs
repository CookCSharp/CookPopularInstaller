using CookPopularControl.Controls;
using CookPopularControl.Windows;
using NCATestInstaller.Generate.Models;
using NCATestInstaller.Generate.Views;
using NCATestInstaller.Toolkit;
using NCATestInstaller.Toolkit.Helpers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：DependsViewModel 
 * Author： Chance.Zheng
 * Create Time: 2023/3/4 17:48:54
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2018-2023 All Rights Reserved.
 */
namespace NCATestInstaller.Generate.ViewModels
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
