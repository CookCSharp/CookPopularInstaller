using CookPopularControl.Windows;
using CookPopularCSharpToolkit.Communal;
using CookPopularCSharpToolkit.Windows;
using CookPopularInstaller.Generate.Models;
using CookPopularInstaller.Toolkit.Helpers;
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
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.ViewModels
{
    public class BuildViewModel : ViewModelBase<BuildModel>
    {
        private ProjectModel _project;
        public DelegateCommand BuildCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnBuildAction)).Value;
        public DelegateCommand CancelCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnCancelAction)).Value;


        public BuildViewModel()
        {
            Model.LogDocument = new FlowDocument();
        }

        protected override void OnPubSubMessage()
        {
            EventAggregator.GetEvent<PackageInfoEvent>().Subscribe(project =>
            {
                _project = project;
            });
        }

        private bool VerifyPackageInfo(ProjectModel project)
        {
            bool isRight = false;
            string warningMessage = string.Empty;

            if (project == null)
            {
                project = App.GetProject();
            }
            Package.Project = project;

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

        private void CopyConfusedFilesToPackageFolder()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Obfuscar\\Confused"))
                return;

            var confusedFiles = Microsoft.VisualBasic.FileIO.FileSystem.GetFiles(Environment.CurrentDirectory + "\\Obfuscar\\Confused", Microsoft.VisualBasic.FileIO.SearchOption.SearchAllSubDirectories, "*.dll");
            confusedFiles.ForEach(file =>
            {
                var fileName = Path.GetFileName(file);
                var destination = Path.Combine(Package.Project.PackageFolder, fileName);
                Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(file, destination, true);
            });
        }

        private async void OnBuildAction()
        {
            if (!VerifyPackageInfo(_project))
                return;

            Toolkit.Helpers.ConfigurationManageHelper.ModifyItem(Environment.CurrentDirectory + "\\CookPopularInstaller.CustomUI.exe", "PackageTheme", _project.PackageTheme.ToString(), "appSettings");
            CopyConfusedFilesToPackageFolder();

            Model.LogDocument.Blocks.Clear();
            Model.BuildResultBrush = SystemColors.WindowBrush;

            //1.首先使用heat.exe批量生成要打包的文件
            //2.然后使用candle.exe生成wixproj文件
            //3.再使用light.exe生成安装包

            var heatExePath = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\heat.exe";
            var candleExePath = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\candle.exe";
            var lightExePath = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\light.exe";
            var wixBalExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixBalExtension.dll";
            var wixUIExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixUIExtension.dll";
            var wixUtilExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixUtilExtension.dll";
            var path = EnvironmentHelper.GetEnvironmentVariable("Path").Split(';').Where(p => p.Contains("Python") && File.Exists(p + "Python.exe")).FirstOrDefault();
            var pythonExePath = path == null ? string.Empty : Path.Combine(path, "Python.exe");
            var outputFileName_Msi = Path.Combine(Package.Project.PackageOutputPath, $"{Package.Project.PackageName}.v{Package.Project.PackageVersion}.msi");
            var outputFileName_Exe = Path.Combine(Package.Project.PackageOutputPath, $"{Package.Project.PackageName}.v{Package.Project.PackageVersion}.exe");

            IList<string> arguments = new List<string>();
            GetArguments(Package.Project.PackageType.ToString());

            Model.IsBuilding = true;
            await Model.LogDocument.RunCommands(arguments).ContinueWith(task =>
            {
                var hasError = task.Result;
                Model.IsBuilding = false;
                Model.BuildResultBrush = hasError.BuildResultBrush();
            });

            void GetArguments(string packageType)
            {
                //生成Msi安装包
                var wxiFilePath = Environment.CurrentDirectory + "\\Assets\\Msi\\Directory.wxi";
                var updateWxiScriptPath = Environment.CurrentDirectory + "\\Assets\\Msi\\update_directory_product.py";
                var updateZhcnScriptPath = Environment.CurrentDirectory + @$"\Assets\Public\update_zhcn.py";
                var wxsFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\Product.wxs";
                var wixprojFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\CookPopularInstaller.Msi.wixproj";
                var cultureFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\zh-cn.wxl";

                var csharpCustomActionTargetDir = Environment.CurrentDirectory + @"\\";
                var csharpCustomActionTargetFileName = "CookPopularInstaller.CSharpCustomAction";

                var argument1 = $"\"{heatExePath}\" dir \"{Package.Project.PackageFolder}\" -out \"{wxiFilePath}\" -gg -gl -ke -sreg -scom -srd -platform x64 -template fragment -dr INSTALLFOLDER -cg DependencyLibrariesGroup -var var.DependencyLibrariesDir";
                var argument2 = $"\"{pythonExePath}\" \"{updateWxiScriptPath}\"";
                var argument3 = $"\"{pythonExePath}\" \"{updateZhcnScriptPath}\" \"{Package.Project.PackageName}\" \"{Package.Project.PackageName}\" \"{Package.Project.AppLogo}\" \"{Package.Project.AppFileName}\"";
                var argument4 = $"\"{candleExePath}\" \"{wxsFilePath_Msi}\" -dCookPopularInstaller.CSharpCustomAction.TargetDir=\"{csharpCustomActionTargetDir}\" -dCookPopularInstaller.CSharpCustomAction.TargetName={csharpCustomActionTargetFileName} -out \"{wixprojFilePath_Msi}\"";
                var argument5 = $"\"{lightExePath}\" -ext \"{wixBalExtension}\" -ext \"{wixUIExtension}\" -ext \"{wixUtilExtension}\" -cultures:zh-cn \"{wixprojFilePath_Msi}\" -loc \"{cultureFilePath_Msi}\" -spdb -out \"{outputFileName_Msi}\"";

                arguments.Add(argument1);
                arguments.Add(argument2);
                arguments.Add(argument3);
                arguments.Add(argument4);
                arguments.Add(argument5);

                //生成Exe安装包
                if (packageType.Contains("Exe"))
                {
                    var wxsFilePath_Exe = Environment.CurrentDirectory + @$"\Assets\{packageType}\\Bundle.wxs";
                    var wixprojFilePath_Exe = Environment.CurrentDirectory + @$"\Assets\{packageType}\CookPopularInstaller.{packageType}.wixproj";
                    var cultureFilePath_Exe = Environment.CurrentDirectory + @$"\Assets\{packageType}\zh-cn.wxl";

                    string argument6 = string.Empty;
                    if (packageType == "Exe")
                    {
                        argument6 = $"\"{candleExePath}\" \"{wxsFilePath_Exe}\" -ext \"{wixBalExtension}\" -dCookPopularInstaller.Msi.TargetPath=\"{outputFileName_Msi}\" -out \"{wixprojFilePath_Exe}\"";
                    }
                    else
                    {
                        var customUITargetDir = Environment.CurrentDirectory + @"\\"; //此处结尾需要加双斜杠，否则candle.exe时会报错
                        var customUITargetFileName = "CookPopularInstaller.CustomUI.exe";
                        var customUITargetPath = Environment.CurrentDirectory + @$"\{customUITargetFileName}";
                        argument6 = $"\"{candleExePath}\" \"{wxsFilePath_Exe}\" -ext \"{wixBalExtension}\" -ext \"{wixUtilExtension}\" -dCookPopularInstaller.CustomUI.TargetDir=\"{customUITargetDir}\" -dCookPopularInstaller.CustomUI.TargetFileName=\"{customUITargetFileName}\" -dCookPopularInstaller.CustomUI.TargetPath=\"{customUITargetPath}\" -dCookPopularInstaller.Msi.TargetPath=\"{outputFileName_Msi}\" -out \"{wixprojFilePath_Exe}\"";
                    }
                    var argument7 = $"\"{lightExePath}\" -ext \"{wixBalExtension}\" -ext \"{wixUIExtension}\" -ext \"{wixUtilExtension}\" -cultures:zh-cn \"{wixprojFilePath_Exe}\" -loc \"{cultureFilePath_Exe}\" -spdb -out \"{outputFileName_Exe}\"";

                    arguments.Add(argument6);
                    arguments.Add(argument7);
                }
            }
        }

        private void OnCancelAction()
        {
            ProcessHelper.CurrentProcess?.Kill();
        }
    }
}
