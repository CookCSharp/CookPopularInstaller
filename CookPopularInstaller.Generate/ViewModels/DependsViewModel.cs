using CookPopularControl.Controls;
using CookPopularControl.Windows;
using CookPopularInstaller.Generate.Models;
using CookPopularInstaller.Generate.Views;
using CookPopularInstaller.Toolkit.Helpers;
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
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.ViewModels
{
    public class DependsViewModel : ViewModelBase<DependsModel>
    {
        public DelegateCommand AddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnAddAction)).Value;
        public DelegateCommand SaveCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnSaveAction)).Value;


        public void OnLoaded()
        {
            Model = Package.Depends;
        }

        private async void OnAddAction()
        {
            var dependDialogVariable = await DialogWindow.Show<DependDialogView>("依赖").Initialize<DependDialogViewModel>(vm => { }).GetResultAsync<DependDialogVariable>();
            if (!string.IsNullOrEmpty(dependDialogVariable.CheckValue) && !string.IsNullOrEmpty(dependDialogVariable.Name) && !string.IsNullOrEmpty(dependDialogVariable.Value))
                Model.DependDialogVariables.Add(dependDialogVariable);
        }

        private void OnSaveAction()
        {
            Package.Depends = Model;
            App.SavePackageFile(Package);
        }
    }
}
