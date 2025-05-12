/*
 * Description：Sundry 
 * Author： Chance.Zheng
 * Create Time: 2024/5/20 16:38:49
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2024 All Rights Reserved.
 */

using CookPopularCSharpToolkit.Communal;
using CookPopularCSharpToolkit.Windows;
using NCATestInstaller.Toolkit;
using NCATestInstaller.Toolkit.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using SharpCompress.Archives;
//using SharpCompress.Archives.Rar;
//using SharpCompress.Readers;

namespace NCATestInstaller.CustomUI.References
{
    /// <summary>
    /// 杂项类
    /// </summary>
    public class Sundry
    {
        private BootstrapperApplicationAgent BootstrapperAgent => BootstrapperApplicationAgent.Instance;

        private string Message { get; set; }

        private void RemoveCacheFolder()
        {
            try
            {
                //运行时有效
                //var wixBundleExecutePackageCacheFolder = BootstrapperAgent.GetBurnVariable("WixBundleExecutePackageCacheFolder");

                var wixBundleInstalled = BootstrapperAgent.GetBurnVariable("WixBundleInstalled");
                var productState = (ProductState)Enum.Parse(typeof(ProductState), BootstrapperAgent.GetBurnVariable("ProductState"), true);
                if (productState == ProductState.Installed)
                {
                    var commonAppDataFolder = BootstrapperAgent.GetBurnVariable("CommonAppDataFolder");
                    var packageCachePath = Path.Combine(commonAppDataFolder, "Package Cache");
                    var productCodeKey = BootstrapperAgent.GetBurnVariable("PreviousProductCode");
                    var productCodeKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productCodeKey}";
                    var wixBundleProviderKey = BootstrapperAgent.GetBurnVariable("PreviousWixBundleProviderKey");
                    var wixBundleProviderKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{wixBundleProviderKey}";

                    if (!string.IsNullOrWhiteSpace(productCodeKeyPath))
                    {
                        var productVersion = BootstrapperAgent.GetBurnVariable("PreviousProductVersion");
                        var productCachePath = Path.Combine(packageCachePath, $"{productCodeKey}v{productVersion}");
                        if (Directory.Exists(productCachePath))
                            Directory.Delete(productCachePath, true);

                        if (RegistryHelper.CheckRegistryKeyPath(Microsoft.Win32.RegistryHive.LocalMachine, productCodeKeyPath))
                            RegistryHelper.DeleteLocalMachineSubKeyTree(productCodeKeyPath);
                    }

                    if (!string.IsNullOrWhiteSpace(wixBundleProviderKeyPath))
                    {
                        //var bundleCachePath = RegistryHelper.GetLocalMachineRegistryKeyPathValue(wixBundleProviderKeyPath, "BundleCachePath");
                        //if (bundleCachePath != null && !string.IsNullOrWhiteSpace(bundleCachePath.ToString()))
                        //{
                        //    var bundleCacheFolder = Directory.GetParent(bundleCachePath.ToString()).FullName;
                        //    if (Directory.Exists(bundleCacheFolder)) Directory.Delete(bundleCacheFolder, true);
                        //}

                        var bundleCachePath = Path.Combine(packageCachePath, wixBundleProviderKey);
                        if (Directory.Exists(bundleCachePath))
                            Directory.Delete(bundleCachePath, true);

                        if (RegistryHelper.CheckRegistryKeyPath(Microsoft.Win32.RegistryHive.LocalMachine, wixBundleProviderKeyPath))
                            RegistryHelper.DeleteLocalMachineSubKeyTree(wixBundleProviderKeyPath);
                    }

                    //InteropMethod.RefreshDesktopAndControlPanel();
                }
            }
            catch (Exception ex)
            {
                BootstrapperAgent.LogMessage(ex.Message);
            }
        }

        private async Task Decompress(string sourceFile, string targetPath)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException(string.Format("未能找到文件'{0}'", sourceFile));
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            await Task.Run(() =>
            {
                //using FileStream fileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
                //using ZipInputStream zipInputStream = new ZipInputStream(fileStream);
                //ZipEntry zipEntry = null;
                //while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                //{
                //    string directoryName = Path.Combine(targetPath, Path.GetDirectoryName(zipEntry.Name));
                //    string fileName = Path.Combine(directoryName, Path.GetFileName(zipEntry.Name));
                //    if (directoryName.Length > 0)
                //    {
                //        Directory.CreateDirectory(directoryName);
                //    }
                //    if (Directory.Exists(fileName))
                //        continue;
                //    if (!string.IsNullOrEmpty(fileName))
                //    {
                //        using FileStream streamWriter = File.Create(fileName);
                //        int size = 4096;
                //        byte[] buffer = new byte[4 * 1024];
                //        while (true)
                //        {
                //            size = zipInputStream.Read(buffer, 0, buffer.Length);
                //            if (size > 0)
                //                streamWriter.Write(buffer, 0, size);
                //            else
                //                break;
                //        }
                //    }
                //}
            });
        }

        private async Task InstallDepends()
        {
#if DEBUG
            var packageFile = Path.Combine(Environment.CurrentDirectory, "package.json");
#else
            var packageFile = Directory.GetFiles(BootstrapperAgent.GetBurnVariable("InstallFolder"), "package*.json").FirstOrDefault();
#endif
            if (!File.Exists(packageFile)) return;

            var package = JsonHelper.JsonDeserializeFile<PackageInfo>(packageFile);
            foreach (var depend in package.Depend.DependDialogVariables)
            {
                if (depend.Value.EndsWith(".zip"))
                    continue;

                bool isExisted = false;
                await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在安装{depend.Name}...");
                switch (depend.CheckType)
                {
                    case CheckType.Enviorment:
                        if (string.Equals(depend.CheckValue, "Path", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var exePath = EnvironmentHelper.GetEnvironmentVariable("Path").Split(';').Where(p => p.Contains(depend.Name) && File.Exists(p + $"{depend.Name}.exe")).FirstOrDefault();
                            if (exePath != null)
                                isExisted = true;
                        }
                        else
                        {
                            isExisted = EnvironmentHelper.CheckEnviorment(depend.CheckValue);
                        }
                        break;
                    case CheckType.Registry:
                        var registryHiveString = depend.CheckValue.Split('\\')[0].Replace("HKEY_", "").Replace('_', '\0');
                        var registryKeyPath = depend.CheckValue.Replace(registryHiveString, "");
                        var isExistedRegistryHive = Enum.TryParse<Microsoft.Win32.RegistryHive>(registryHiveString, out var registryHive);
                        if (isExistedRegistryHive)
                            isExisted = RegistryHelper.CheckRegistryKeyPath(registryHive, registryKeyPath);
                        break;
                    case CheckType.Folder:
                        var dependValueName = Path.GetFileNameWithoutExtension(Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), depend.Value));
                        isExisted = Directory.Exists(Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), dependValueName));
                        break;
                    case CheckType.Other:
                        break;
                    default:
                        break;
                }

                if (!isExisted)
                {
                    await InternalInstall(depend);
                }
                else
                {
                    var dependPath = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), depend.Value);
                    File.Delete(dependPath);
                }
            }

            async Task InternalInstall(DependDialogVariable depend)
            {
#if DEBUG
                var dependPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, depend.Value);
#else
                var dependPath = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), depend.Value);
#endif
                //考虑等待目标项目的所有依赖安装完成再安装目标项目
                if (File.Exists(dependPath))
                {
                    if (dependPath.EndsWith(".exe") || dependPath.EndsWith(".msi"))
                    {
                        ProcessHelper.StartProcessByCmd(dependPath, depend.InstallCommand);
                    }
                    else if (dependPath.EndsWith(".rar") || dependPath.EndsWith(".zip"))
                    {
                        {
                            //try
                            //{
                            //    //using var stream = File.OpenRead(dependPath);
                            //    //using var archive = RarArchive.Open(stream);
                            //    //foreach (var archiveEntry in archive.Entries)
                            //    //{
                            //    //    if (!archiveEntry.IsDirectory)
                            //    //    {
                            //    //        archiveEntry.WriteToDirectory(@"D:\Users\chance.zheng\Desktop\Company\NCATestInstaller\Output\bin\Debug\AnyCPU\NCATestInstaller.CustomUI\net48\Depends\RuntimeChance", new SharpCompress.Common.ExtractionOptions
                            //    //        {
                            //    //            ExtractFullPath = true,
                            //    //            Overwrite = true,
                            //    //        });
                            //    //    }
                            //    //}

                            //    //ReaderOptions options = new ReaderOptions();
                            //    //options.ArchiveEncoding.Default = Encoding.UTF8;
                            //    //IArchive archive = ArchiveFactory.Open(dependPath, options);
                            //    //foreach (var archiveEntry in archive.Entries)
                            //    //{
                            //    //    if (!archiveEntry.IsDirectory)
                            //    //    {
                            //    //        archiveEntry.WriteToDirectory(@"D:\Users\chance.zheng\Desktop\Company\NCATestInstaller\Output\bin\Debug\AnyCPU\NCATestInstaller.CustomUI\net48\Depends\RuntimeChance", new SharpCompress.Common.ExtractionOptions
                            //    //        {
                            //    //            ExtractFullPath = true,
                            //    //            Overwrite = true,
                            //    //        });
                            //    //    }
                            //    //}
                            //}
                            //catch (Exception ex)
                            //{
                            //    throw ex;
                            //}

                            //ZipFile.ExtractToDirectory(dependPath, @"D:\Users\chance.zheng\Desktop\Company\NCATestInstaller\Output\bin\Debug\AnyCPU\NCATestInstaller.CustomUI\net48\Depends\RuntimeChance");
                        }
#if DEBUG
                        string targetPath = Environment.CurrentDirectory;
#else
                        string targetPath = BootstrapperAgent.GetBurnVariable("InstallFolder");
#endif
                        await Decompress(dependPath, targetPath);
                        File.Delete(dependPath);
                    }
                }
            }
        }

        /// <summary>
        /// 将内置的变量转为相应的值
        /// </summary>
        /// <param name="extensions"></param>
        /// <remarks>
        /// 内置变量:
        /// InstallFolder、AppPath、CompanyName、ProductName、ProductVersion
        /// </remarks>
        private void GetCanConverterExtensions(ExtensionInfo extensions)
        {
            SetValue(extensions.EnvironmentVariables);
            SetValue(extensions.RegistryVariables);
            SetValue(extensions.WindowsServices);

            void SetValue(IEnumerable<object> variables)
            {
                foreach (var variable in variables)
                {
                    Type type = variable.GetType();
                    foreach (var prop in type.GetProperties())
                    {
                        var attr = Attribute.GetCustomAttribute(prop, typeof(CanConverterAttribute));
                        if (attr != null)
                        {
                            var value = prop.GetValue(variable) ?? string.Empty;
                            var propValue = GetValue(value.ToString());
                            prop.SetValue(variable, propValue);
                        }
                    }
                }
            }

            object GetValue(string value)
            {
                var start = value.IndexOf('[');
                var end = value.IndexOf(']');
                bool hasExisted = false;
                if (start == -1 || end == -1)
                {
                    return value;
                }
                else
                {
                    hasExisted = Enum.TryParse(value.Substring(start + 1, end - start - 1), true, out VariableType variableType);
                    string replaceStr = string.Empty;
#if RELEASE
                    switch (variableType)
                    {
                        case VariableType.InstallFolder:
                            replaceStr = BootstrapperAgent.GetBurnVariable("InstallFolder");
                            break;
                        case VariableType.AppPath:
                            replaceStr = BootstrapperAgent.GetBurnVariable("LaunchTarget");
                            break;
                        case VariableType.CompanyName:
                            replaceStr = BootstrapperAgent.GetBurnVariable("CompanyName");
                            break;
                        case VariableType.ProductName:
                            replaceStr = BootstrapperAgent.GetBurnVariable("ProductName");
                            break;
                        case VariableType.ProductVersion:
                            replaceStr = BootstrapperAgent.GetBurnVariable("WixBundleVersion");
                            break;
                        default:
                            break;
                    }
#endif
                    if (hasExisted)
                        return value.Replace($"[{variableType}]", replaceStr);
                    else
                        return value;
                }
            }

            //var registryFile = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), "Registry-ATEStudio.reg");
            //using (var fs = new FileStream(registryFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            //{
            //    using var sr = new StreamReader(fs);
            //    var content = sr.ReadToEnd();
            //    content = content.Replace("[InstallFolder]", BootstrapperAgent.GetBurnVariable("InstallFolder"));
            //    content = content.Replace("[AppPath]", BootstrapperAgent.GetBurnVariable("LaunchTarget"));
            //    content = content.Replace("[AppName]", BootstrapperAgent.GetBurnVariable("ProductName"));
            //    content = content.Replace("[Version]", BootstrapperAgent.GetBurnVariable("WixBundleVersion"));
            //    var buffer = Encoding.Default.GetBytes(content);
            //    fs.Seek(0, SeekOrigin.Begin);
            //    fs.Write(buffer, 0, buffer.Length);
            //}
            //System.Diagnostics.Process.Start("regedit", $"/s {registryFile}");
        }

        private async Task InstallExtensions()
        {
#if DEBUG
            var packageFile = Path.Combine(Environment.CurrentDirectory, "package.json");
#else
            var packageFile = Directory.GetFiles(BootstrapperAgent.GetBurnVariable("InstallFolder"), "package*.json").FirstOrDefault();
#endif
            if (!File.Exists(packageFile)) return;

            await Task.Run(async () =>
            {
                var package = JsonHelper.JsonDeserializeFile<PackageInfo>(packageFile);
                GetCanConverterExtensions(package.Extension);

                //await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在写环境变量信息...");
                //extensions.EnvironmentVariables.ForEach(variable => EnvironmentHelper.WriteEnviorment(variable.Name, variable.Value));

                //await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在写注册表信息...");
                //extensions.RegistryVariables.ForEach(variable => RegistryHelper.CreateRegistryKey(variable.RegistryHive, variable.RegistryValueKind, variable.Path, variable.Name, variable.Value));

                await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在启动服务...");
                package.Extension.WindowsServices.ForEach(service =>
                {
#if DEBUG
                    var pythonScriptFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions\\create_windows_service.py");
#else
                    var pythonScriptFilePath = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), "Extensions\\create_windows_service.py");
#endif
                    //var arguments = $"python \"{pythonScriptFilePath}\" {service.Name} \"{service.Location}\" \"{service.Description}\"";
                    //ProcessHelper.StartProcessByCmd(arguments, true);

                    var create_service_cmd = $"create {service.Name} binPath=\"{service.Location}\" start=auto DisplayName={service.Name}";
                    var set_description_cmd = $"description {service.Name} {service.Description}";
                    var start_service_cmd = $"start {service.Name}";
                    IList<string> commands = new List<string>();
                    commands.Add(create_service_cmd);
                    commands.Add(set_description_cmd);
                    commands.Add(start_service_cmd);
                    commands.ForEach(command => ProcessHelper.StartProcess("sc.exe", command, true));
                });
            });
        }

        private async Task UninstallDepends()
        {
            await Task.Run(() =>
            {

            });
        }

        private async Task UninstallExtensions()
        {
#if DEBUG
            var packageFile = Path.Combine(Environment.CurrentDirectory, "package.json");
#else
            var companyName = BootstrapperAgent.GetBurnVariable("CompanyName");
            var productName = BootstrapperAgent.GetBurnVariable("ProductName");
            //string keyPath = Environment.Is64BitOperatingSystem ? $"Software\\Wow6432Node\\NCATest\\ATEStudio" : $"Software\\NCATest\\ATEStudio";
            string keyPath = Environment.Is64BitOperatingSystem ? $"Software\\Wow6432Node\\{companyName}\\{productName}" : $"Software\\{companyName}\\{productName}";
            var directory = RegistryHelper.GetLocalMachineRegistryKeyPathValue(keyPath, "Directory").ToString();
            var packageFile = Directory.GetFiles(directory, "package*.json").FirstOrDefault();
#endif
            if (!File.Exists(packageFile)) return;

            await Task.Run(async () =>
            {
                var package = JsonHelper.JsonDeserializeFile<PackageInfo>(packageFile);

                //await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在删除环境变量信息...");
                //extensions.EnvironmentVariables.ForEach(variable => EnvironmentHelper.WriteEnviorment(variable.Name, null));

                //await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在删除注册表信息...");
                //extensions.RegistryVariables.ForEach(variable => RegistryHelper.DeleteRegistryKeyPathValue(variable.RegistryHive, variable.Path, variable.Name));

                await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在删除服务...");
                package.Extension.WindowsServices.ForEach(service =>
                {
                    //var pythonScriptFilePath = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), "Extensions\\delete_windows_service.py");
                    //var arguments = $"python \"{pythonScriptFilePath}\" {service.Name}";
                    //ProcessHelper.StartProcessByCmd(arguments, true);

                    var stop_service_cmd = $"stop {service.Name}";
                    var delete_service_cmd = $"delete {service.Name}";
                    IList<string> commands = new List<string>();
                    commands.Add(stop_service_cmd);
                    commands.Add(delete_service_cmd);
                    commands.ForEach(command => ProcessHelper.StartProcess("sc.exe", command, true));
                });
            });
        }
    }
}
