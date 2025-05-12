using CookPopularCSharpToolkit.Communal;
using CookPopularCSharpToolkit.Windows;
using NCATestInstaller.Generate.Models;
using NCATestInstaller.Toolkit;
using NCATestInstaller.Toolkit.Helpers;
using Ookii.Dialogs.Wpf;
using Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;


/*
 * Description：ConfuseViewModel 
 * Author： Chance.Zheng
 * Create Time: 2023/2/28 10:23:39
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2018-2023 All Rights Reserved.
 */
namespace NCATestInstaller.Generate.ViewModels
{
    public class ConfuseViewModel : ViewModelBase<ConfuseInfo>
    {
        private IList _selectedDlls;

        public Brush ConfuseResultBrush { get; set; } = SystemColors.ControlLightBrush;

        public bool IsConfusing { get; set; }

        public FlowDocument LogDocument { get; set; } = new FlowDocument();

        public DelegateCommand AddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnAddAction)).Value;
        public DelegateCommand<ListBox> SelectedDllsCommand => new Lazy<DelegateCommand<ListBox>>(() => new DelegateCommand<ListBox>(OnSelectionDllsAction)).Value;
        public DelegateCommand DeleteCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnDeleteAction)).Value;
        public DelegateCommand ConfuseCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnConfuseAction)).Value;


        public ConfuseViewModel()
        {
            Model.ConfuseDllNames = Package.Confuse.ConfuseDllNames;
        }

        private void OnAddAction()
        {
            var fileDialog = new VistaOpenFileDialog();
            fileDialog.Filter = "文件(*.dll)|*.dll";
            fileDialog.RestoreDirectory = false;
            fileDialog.Title = "选择混淆文件";
            fileDialog.InitialDirectory = Package.Project.PackageFolder;
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog().Value)
            {
                fileDialog.FileNames.ForEach(fileName =>
                {
                    if (!Model.ConfuseDllNames.Contains(Path.GetFileName(fileName)))
                        Model.ConfuseDllNames.Add(Path.GetFileName(fileName));
                });
            }
        }

        public void OnSelectionDllsAction(ListBox listBox)
        {
            _selectedDlls = listBox.SelectedItems;
        }

        private void OnDeleteAction()
        {
            if (_selectedDlls == null) return;
            for (int i = _selectedDlls.Count - 1; i >= 0; i--)
            {
                if (i < 0) break;
                var dll = _selectedDlls[i].ToString();
                Model.ConfuseDllNames.Remove(dll);
            }
        }

        private async void OnConfuseAction()
        {
            Package.Project = Build.GetProject(App.PackageJsonFileName);
            Package.Confuse = Model;
            App.SavePackageFile(Package);

            LogDocument.Blocks.Clear();
            ConfuseResultBrush = SystemColors.WindowBrush;

            IList<string> commands = Build.GetConfuseArguments(Package.Project.PackageFolder, App.PackageJsonFileName);
            IsConfusing = true;
            await LogDocument.RunCommands(commands, "Confuse").ContinueWith(task =>
            {
                var hasError = task.Result;
                IsConfusing = false;
                ConfuseResultBrush = hasError.BuildResultBrush();
            });
        }
    }
}
