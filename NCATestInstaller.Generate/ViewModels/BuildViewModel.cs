using CookPopularControl.Windows;
using CookPopularCSharpToolkit.Communal;
using CookPopularCSharpToolkit.Windows;
using NCATestInstaller.Generate.Models;
using NCATestInstaller.Toolkit;
using NCATestInstaller.Toolkit.Helpers;
using Ookii.Dialogs.Wpf;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;


/*
 * Description：BuildViewModel 
 * Author： Chance.Zheng
 * Create Time: 2023/2/15 17:14:29
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2018-2023 All Rights Reserved.
 */
namespace NCATestInstaller.Generate.ViewModels
{
    public class BuildViewModel : ViewModelBase<BuildModel>
    {
        public DelegateCommand BuildCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnBuildAction)).Value;
        public DelegateCommand CancelCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnCancelAction)).Value;


        public BuildViewModel()
        {
            Model.LogDocument = new FlowDocument();
        }

        private bool VerifyPackageInfo()
        {
            bool isRight = false;
            string warningMessage = string.Empty;

            Package.Project = Build.GetProject(App.PackageJsonFileName);

            if (string.IsNullOrEmpty(Package.Project.PackageFolder) || !Directory.Exists(Package.Project.PackageFolder))
            {
                isRight = false;
                warningMessage = $"检测到打包目录不正确!";
            }
            else if (string.IsNullOrEmpty(Package.Project.PackageOutputPath) || !Directory.Exists(Package.Project.PackageOutputPath))
            {
                try
                {
                    isRight = true;
                    Directory.CreateDirectory(Package.Project.PackageOutputPath);
                }
                catch (Exception ex)
                {
                    isRight = false;
                    warningMessage = "检测到安装包输出目录不正确!" + ex.Message;
                }
            }
            else if (string.IsNullOrEmpty(Package.Project.PackageName))
            {
                isRight = false;
                warningMessage = $"安装包名称不能为空!";
            }
            else if (string.IsNullOrEmpty(Package.Project.PackageVersion))
            {
                isRight = false;
                warningMessage = $"安装包版本不能为空!";
            }
            //else if (string.IsNullOrEmpty(model.AppFileName) || !File.Exists(Path.Combine(model.PackageFolder, model.AppFileName)))
            //{
            //    isRight = false;
            //    warningMessage = $"启动项不存在!";
            //}
            else
                isRight = true;

            if (!isRight)
                MessageDialog.ShowWarning(warningMessage);
            else
                App.SavePackageFile(Package);

            return isRight;
        }

        private async void OnBuildAction()
        {
            if (!VerifyPackageInfo())
                return;

            Build.Configure(App.PackageJsonFileName);
            Build.CopyConfusedFilesToPackageFolder(Package.Project.PackageFolder);

            Model.LogDocument.Blocks.Clear();
            Model.BuildResultBrush = SystemColors.WindowBrush;

            IList<string> arguments = Build.GetBuildArguments(Package, App.PackageJsonFileName);

            Model.IsBuilding = true;
            await Model.LogDocument.RunCommands(arguments).ContinueWith(task =>
            {
                var hasError = task.Result;
                Model.IsBuilding = false;
                Model.BuildResultBrush = hasError.BuildResultBrush();
            });
        }

        private void OnCancelAction()
        {
            ProcessHelper.CurrentProcess?.Kill();
        }
    }
}
