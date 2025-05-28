/*
 * Description：Build 
 * Author： Chance.Zheng
 * Create Time: 2023/10/9 11:31:50
 * .Net Version: 2.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2023 All Rights Reserved.
 */

using CookPopularInstaller.Toolkit.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CookPopularInstaller.Toolkit
{
    public class Build
    {
        public static PackageInfo ReadPackageJsonFile(string packageJsonFileName)
        {
            var packageJsonFile = Directory.GetFiles(Environment.CurrentDirectory, packageJsonFileName).FirstOrDefault();
            if (packageJsonFile == null) return default;

            var package = JsonHelper.JsonDeserializeFile<PackageInfo>(packageJsonFile);
            return package;
        }

        public static ProjectInfo GetProject(string packageJsonFileName)
        {
            var package = ReadPackageJsonFile(packageJsonFileName);
            if (package == null) return default;

            if (string.IsNullOrEmpty(package.Project.PackageOutputPath) | !Directory.Exists(package.Project.PackageOutputPath))
                package.Project.PackageOutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), package.Project.Company);

            if (string.IsNullOrEmpty(package.Project.PackageVersion))
                package.Project.PackageVersion = FileVersionInfo.GetVersionInfo(package.Project.AppFileName).FileVersion;

            return package.Project;
        }

        /// <summary>
        /// 配置CustomUI以及Uninstall
        /// </summary>
        public static void Configure(string packageJsonFileName)
        {
            var project = GetProject(packageJsonFileName);
            ConfigurationManageHelper.ModifyItem(Environment.CurrentDirectory + "\\CookPopularInstaller.CustomUI.exe", "PackageTheme", project.PackageTheme.ToString(), "appSettings");
            ConfigurationManageHelper.ModifyItem(Environment.CurrentDirectory + "\\CookPopularInstaller.CustomUI.exe", "LicenseFileName", project.PackageLicenseName, "appSettings");
            ConfigurationManageHelper.ModifyItem(Environment.CurrentDirectory + "\\CookPopularInstaller.CustomUI.exe", "ContainChineseOnInstallFolder", project.ContainChineseOnInstallFolder, "appSettings");
            ConfigurationManageHelper.ModifyItem(Environment.CurrentDirectory + "\\CookPopularInstaller.CustomUI.exe", "Language", project.Language.ToString(), "appSettings");
            //string uninstallProjectPath = Path.Combine(Environment.CurrentDirectory, "..\\..\\CookPopularInstaller.Uninstall");
            //string updateUninstallAppConfig = $"python {uninstallProjectPath}\\update_appconfig.py {uninstallProjectPath}\\App.config {_project.Company} {_project.PackageName}";
            //ProcessHelper.StartProcessByCmd(updateUninstallAppConfig);
        }

        public static void CopyConfusedFilesToPackageFolder(string packageFolder)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Obfuscar\\Confused"))
                return;

            var confusedFiles = Directory.GetFiles(Environment.CurrentDirectory + "\\Obfuscar\\Confused", "*.dll", SearchOption.AllDirectories);
            Array.ForEach(confusedFiles, file =>
            {
                var fileName = Path.GetFileName(file);
                var destination = Path.Combine(packageFolder, fileName);
                File.Copy(file, destination, true);
            });

            //var confusedFiles = Microsoft.VisualBasic.FileIO.FileSystem.GetFiles(Environment.CurrentDirectory + "\\Obfuscar\\Confused", Microsoft.VisualBasic.FileIO.SearchOption.SearchAllSubDirectories, "*.dll");
            //confusedFiles.ForEach(file =>
            //{
            //    var fileName = Path.GetFileName(file);
            //    var destination = Path.Combine(Package.Project.PackageFolder, fileName);
            //    Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(file, destination, true);
            //});
        }

        public static IList<string> GetConfuseArguments(string packageFolder, string packageJsonFileName)
        {
            IList<string> commands = new List<string>();

            var path = EnvironmentHelper.GetEnvironmentVariable("Path").Split(';').Where(p => p.Contains("Python") && File.Exists(p + "Python.exe")).FirstOrDefault();
            var pythonExePath = path == null ? string.Empty : Path.Combine(path, "Python.exe");
            var updateXMLScriptPath = Environment.CurrentDirectory + "\\Obfuscar\\update_obfuscar_xml.py";

            var command1 = $"\"{pythonExePath}\" \"{updateXMLScriptPath}\" \"{packageFolder}\" {packageJsonFileName}";
            var command2 = "Obfuscar\\Obfuscar.Console.exe Obfuscar\\Obfuscar.xml -s";

            commands.Add(command1);
            commands.Add(command2);

            return commands;
        }

        public static IList<string> GetBuildArguments(PackageInfo package, string packageJsonFileName)
        {
            IList<string> arguments = new List<string>();

            //1.首先使用heat.exe批量生成要打包的文件
            //2.然后使用candle.exe生成wixproj文件
            //3.再使用light.exe生成安装包

            var heatExePath = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\heat.exe";
            var candleExePath = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\candle.exe";
            var lightExePath = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\light.exe";
            var wixBalExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixBalExtension.dll";
            var wixNetFxExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixNetFxExtension.dll";
            var wixUIExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixUIExtension.dll";
            var wixUtilExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixUtilExtension.dll";
            var path = EnvironmentHelper.GetEnvironmentVariable("Path").Split(';').Where(p => p.ToLower().Contains("python") && File.Exists(p + "Python.exe")).FirstOrDefault();
            path ??= EnvironmentHelper.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User).Split(';').Where(p => p.ToLower().Contains("python") && File.Exists(p + "Python.exe")).FirstOrDefault();
            var pythonExePath = path == null ? string.Empty : Path.Combine(path, "Python.exe");
            var outputFileName_Msi = Path.Combine(package.Project.PackageOutputPath, $"{package.Project.PackageName}.v{package.Project.PackageVersion}.msi");
            var outputFileName_Exe = Path.Combine(package.Project.PackageOutputPath, $"{package.Project.PackageName}.v{package.Project.PackageVersion}.exe");

            //生成Msi安装包
            var wxiFilePath = Environment.CurrentDirectory + "\\Assets\\Msi\\Directory.wxi";
            var heatTransformPath = Environment.CurrentDirectory + "\\Assets\\Msi\\HeatTransform.xslt";
            var updateWixScriptPath = Environment.CurrentDirectory + "\\Assets\\Msi\\update_directory_product.py";
            var upgradeJsonFilePath = Environment.CurrentDirectory + "\\Assets\\Public\\upgrade_code.json";
            var updateZhcnScriptPath = Environment.CurrentDirectory + "\\Assets\\Public\\update_zhcn.py";
            var wxsFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\Product.wxs";
            var wixprojFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\CookPopularInstaller.Msi.wixproj";
            string cultureChineseFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\zh-cn.wxl";
            string cultureEnglishFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\en-us.wxl";
            string cultureName = string.Empty;
            if (package.Project.Language == LanguageType.Chinese)
            {
                cultureName = "zh-cn";
            }                
            else if (package.Project.Language == LanguageType.English)
            {
                cultureName = "en-us";
            }

            var csharpCustomActionTargetDir = Environment.CurrentDirectory + @"\\";
            var csharpCustomActionTargetFileName = "CookPopularInstaller.CSharpCustomAction";
            var arch = $"-arch {package.Project.PackagePlatform}";
            var pdbout = package.Project.IsOutputPdb ? $"-pdbout \"{Path.Combine(package.Project.PackageOutputPath, $"{package.Project.PackageName}.wixpdb")}\"" : "-spdb";

            var argument1 = $"\"{heatExePath}\" dir \"{package.Project.PackageFolder}\" -out \"{wxiFilePath}\" -t \"{heatTransformPath}\" -gg -gl -ke -sreg -scom -srd -platform {package.Project.PackagePlatform} -template fragment -dr INSTALLFOLDER -cg DependencyLibrariesGroup -var var.DependencyLibrariesDir";
            var argument2 = $"\"{pythonExePath}\" \"{updateWixScriptPath}\" {packageJsonFileName} \"{upgradeJsonFilePath}\" {package.Project.PackagePlatform}";
            var argument3 = $"\"{pythonExePath}\" \"{updateZhcnScriptPath}\" \"{package.Project.PackageName}\" \"{package.Project.PackageName}\" \"{package.Project.AppLogo}\" \"{package.Project.AppFileName}\"";
            var argument4 = $"\"{candleExePath}\" \"{wxsFilePath_Msi}\" {arch} -dCookPopularInstaller.CSharpCustomAction.TargetDir=\"{csharpCustomActionTargetDir}\" -dCookPopularInstaller.CSharpCustomAction.TargetName={csharpCustomActionTargetFileName} -out \"{wixprojFilePath_Msi}\"";
            var argument5 = $"\"{lightExePath}\" -ext \"{wixBalExtension}\" -ext \"{wixUIExtension}\" -ext \"{wixUtilExtension}\" -cultures:{cultureName} -loc \"{cultureChineseFilePath_Msi}\" -loc \"{cultureEnglishFilePath_Msi}\" \"{wixprojFilePath_Msi}\" {pdbout} -out \"{outputFileName_Msi}\"";

            arguments.Add(argument1);
            arguments.Add(argument2);
            arguments.Add(argument3);
            arguments.Add(argument4);
            arguments.Add(argument5);

            //生成Exe安装包
            if (package.Project.PackageType.ToString().Contains("Exe"))
            {
                var wxsFilePath_Exe = Environment.CurrentDirectory + @$"\Assets\{package.Project.PackageType}\\Bundle.wxs";
                var wixprojFilePath_Exe = Environment.CurrentDirectory + @$"\Assets\{package.Project.PackageType}\CookPopularInstaller.{package.Project.PackageType}.wixproj";
                var cultureFilePath_Exe = Environment.CurrentDirectory + @$"\Assets\{package.Project.PackageType}\zh-cn.wxl";
                var adminFilePath = Environment.CurrentDirectory + @"\Admin\CookPopularInstaller.Admin.exe";

                string argument6 = string.Empty;
                if (package.Project.PackageType == PackageType.Exe)
                {
                    argument6 = $"\"{candleExePath}\" \"{wxsFilePath_Exe}\" {arch} -ext \"{wixBalExtension}\" -dCookPopularInstaller.Msi.TargetPath=\"{outputFileName_Msi}\" -out \"{wixprojFilePath_Exe}\"";
                }
                else
                {
                    var customUITargetDir = Environment.CurrentDirectory + @"\\"; //此处结尾需要加双斜杠，否则candle.exe时会报错
                    var customUITargetFileName = "CookPopularInstaller.CustomUI.exe";
                    var customUITargetPath = Environment.CurrentDirectory + @$"\{customUITargetFileName}";
                    argument6 = $"\"{candleExePath}\" \"{wxsFilePath_Exe}\" {arch} -ext \"{wixBalExtension}\" -ext \"{wixNetFxExtension}\" -ext \"{wixUtilExtension}\" -dCookPopularInstaller.CustomUI.TargetDir=\"{customUITargetDir}\" -dCookPopularInstaller.CustomUI.TargetFileName=\"{customUITargetFileName}\" -dCookPopularInstaller.CustomUI.TargetPath=\"{customUITargetPath}\" -dCookPopularInstaller.Msi.TargetPath=\"{outputFileName_Msi}\" -out \"{wixprojFilePath_Exe}\"";
                }
                var argument7 = $"\"{lightExePath}\" -ext \"{wixBalExtension}\" -ext \"{wixUIExtension}\" -ext \"{wixUtilExtension}\" -loc \"{cultureFilePath_Exe}\" \"{wixprojFilePath_Exe}\" {pdbout} -out \"{outputFileName_Exe}\"";
                var argument8 = $"\"{adminFilePath}\" {outputFileName_Exe}";

                arguments.Add(argument6);
                arguments.Add(argument7);
                arguments.Add(argument8);
            }

            return arguments;
        }
    }
}
