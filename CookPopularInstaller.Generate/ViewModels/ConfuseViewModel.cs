using CookPopularCSharpToolkit.Communal;
using CookPopularCSharpToolkit.Windows;
using CookPopularInstaller.Generate.Models;
using CookPopularInstaller.Toolkit.Helpers;
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
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.ViewModels
{
    public class ConfuseViewModel : ViewModelBase<ConfuseModel>
    {
        private ProjectModel _project;
        private IList _selectedDlls;

        public DelegateCommand AddCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnAddAction)).Value;
        public DelegateCommand<ListBox> SelectedDllsCommand => new Lazy<DelegateCommand<ListBox>>(() => new DelegateCommand<ListBox>(OnSelectionDllsAction)).Value;
        public DelegateCommand DeleteCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnDeleteAction)).Value;
        public DelegateCommand ConfuseCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnConfuseAction)).Value;


        public ConfuseViewModel()
        {
            Model.ConfuseDllNames = Package.Confuse.ConfuseDllNames;
        }

        protected override void OnPubSubMessage()
        {
            EventAggregator.GetEvent<PackageInfoEvent>().Subscribe(project =>
            {
                _project = project;
            });
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
            if (_project == null)
            {
                _project = App.GetProject();
            }

            Package.Project = _project;
            Package.Confuse = Model;
            App.SavePackageFile(Package);

            Model.LogDocument.Blocks.Clear();
            Model.ConfuseResultBrush = SystemColors.WindowBrush;

            var path = EnvironmentHelper.GetEnvironmentVariable("Path").Split(';').Where(p => p.Contains("Python") && File.Exists(p + "Python.exe")).FirstOrDefault();
            var pythonExePath = path == null ? string.Empty : Path.Combine(path, "Python.exe");
            var updateXMLScriptPath = Environment.CurrentDirectory + "\\Obfuscar\\update_obfuscar_xml.py";
            var inPath = Package.Project.PackageFolder;

            var command1 = $"\"{pythonExePath}\" \"{updateXMLScriptPath}\" \"{inPath}\" {App.PackageFileName}";
            var command2 = "Obfuscar\\Obfuscar.Console.exe Obfuscar\\Obfuscar.xml -s";

            IList<string> commands = new List<string>();
            commands.Add(command1);
            commands.Add(command2);

            Model.IsConfusing = true;
            await Model.LogDocument.RunCommands(commands, "Confuse").ContinueWith(task =>
            {
                var hasError = task.Result;
                Model.IsConfusing = false;
                Model.ConfuseResultBrush = hasError.BuildResultBrush();
            });
        }
    }
}
