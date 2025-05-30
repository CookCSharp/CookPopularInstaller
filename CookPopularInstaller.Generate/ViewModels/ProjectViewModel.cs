/*
 * Description：ProjectViewModel 
 * Author： Chance.Zheng
 * Create Time: 2023/2/15 17:12:20
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */


using CookPopularUI.WPF.Windows;
using CookPopularInstaller.Generate.Models;
using CookPopularInstaller.Toolkit;
using Ookii.Dialogs.Wpf;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CookPopularInstaller.Generate.ViewModels
{
    public class ProjectViewModel : ViewModelBase<ProjectInfo>
    {
        public DelegateCommand<object> EnterPackageFolderCommand => new Lazy<DelegateCommand<object>>(() => new DelegateCommand<object>(OnEnterPackageFolderAction)).Value;
        public DelegateCommand PackageFolderCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnPackageFolderAction)).Value;
        public DelegateCommand PackageOutputFolderCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnPackageOutputFolderAction)).Value;
        public DelegateCommand AppFileCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnAppFileAction)).Value;
        public DelegateCommand AppLogoCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnAppLogoAction)).Value;


        public ProjectViewModel()
        {
            Model = Build.GetProject(App.PackageJsonFileName);
            GetSubDirectories();
            OnSaveAction();
        }

        private void GetAppInfo()
        {
            if (string.IsNullOrEmpty(Model.AppFileName))
            {
                if (Directory.Exists(Model.PackageFolder))
                {
                    var allAppFiles = Directory.GetFiles(Model.PackageFolder, "*.exe", SearchOption.AllDirectories);
                    var targetAppFileName = allAppFiles.Select(f => Path.GetFileName(f)).FirstOrDefault(f => f.Contains(Model.PackageName));
                    if (targetAppFileName != null)
                    {
                        Model.AppFileName = targetAppFileName;
                        Model.PackageVersion = FileVersionInfo.GetVersionInfo(Path.Combine(Model.PackageFolder, targetAppFileName)).FileVersion;
                    }
                }
            }
            else
            {
                var appFile = Path.Combine(Model.PackageFolder, Model.AppFileName);
                if (File.Exists(appFile))
                    Model.PackageVersion = FileVersionInfo.GetVersionInfo(appFile).FileVersion;
            }
            if (string.IsNullOrEmpty(Model.AppLogo))
            {
                Model.AppLogo = $"{Environment.CurrentDirectory}\\Assets\\Public\\CookPopularInstaller.Generate.ico";
            }
        }

        private bool VerifyPackageFolder(bool isShowWarning = false)
        {
            bool isExist = false;
            string warningMessage = string.Empty;

            if (string.IsNullOrEmpty(Model.PackageFolder))
            {
                isExist = false;
                warningMessage = $"打包目录不能为空!";
            }
            else if (!Directory.Exists(Model.PackageFolder))
            {
                isExist = false;
                warningMessage = $"打包目录不存在!";
            }
            else
            {
                isExist = true;
            }

            if (!isExist)
            {
                Model.SubDirectories?.Clear();
            }

            if (isShowWarning && !string.IsNullOrEmpty(warningMessage))
            {
                MessageDialog.ShowWarning(warningMessage);
            }

            return isExist;
        }

        private void GetSubDirectories(bool isShowWarning = false)
        {
            GetAppInfo();

            if (!VerifyPackageFolder(isShowWarning))
            {
                return;
            }

            var directoryInfo = new DirectoryInfo(Model.PackageFolder);
            var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
            Model.SubDirectories = null;
            Model.SubDirectories = new ObservableCollection<string>(fileInfos.Select(fileInfo => fileInfo.Name));
        }

        private void OnEnterPackageFolderAction(object value)
        {
            GetSubDirectories(bool.Parse(value.ToString()));
        }

        public void OnPackageFolderLostFocusAction()
        {
            GetSubDirectories();
        }

        private void OnPackageFolderAction()
        {
            var folderDialog = new VistaFolderBrowserDialog();
            folderDialog.Description = "选择打包目录";
            folderDialog.ShowNewFolderButton = true;
            folderDialog.UseDescriptionForTitle = true;
            folderDialog.SelectedPath = Model.PackageFolder;
            folderDialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
            if (folderDialog.ShowDialog().Value)
            {
                Model.PackageFolder = folderDialog.SelectedPath;
            }

            //ProcessHelper.CreateFolderRunAsAdmin(PackageFolder);
            //folderDialog.SelectedPath = Directory.GetDirectories(PackageFolder).FirstOrDefault();
        }

        private void OnPackageOutputFolderAction()
        {
            var folderDialog = new VistaFolderBrowserDialog();
            folderDialog.Description = "选择输出目录";
            folderDialog.ShowNewFolderButton = true;
            folderDialog.UseDescriptionForTitle = true;
            folderDialog.SelectedPath = Model.PackageOutputPath;
            folderDialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
            if (folderDialog.ShowDialog().Value)
            {
                Model.PackageOutputPath = folderDialog.SelectedPath;
            }
        }

        private void OnAppFileAction()
        {
            var fileDialog = new VistaOpenFileDialog();
            fileDialog.Filter = "文件(*.exe)|*.exe";
            fileDialog.RestoreDirectory = false;
            fileDialog.Title = "选择启动项";
            fileDialog.InitialDirectory = Model.PackageFolder;
            if (fileDialog.ShowDialog().Value)
            {
                Model.AppFileName = Path.GetFileName(fileDialog.FileName);
            }
        }

        private void OnAppLogoAction()
        {
            var fileDialog = new VistaOpenFileDialog();
            fileDialog.Filter = "图片(*.ico)|*.ico";
            fileDialog.RestoreDirectory = false;
            fileDialog.Title = "选择图标";
            fileDialog.InitialDirectory = Model.PackageFolder;
            //fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (fileDialog.ShowDialog().Value)
            {
                Model.AppLogo = fileDialog.FileName;
            }
        }
    }
}