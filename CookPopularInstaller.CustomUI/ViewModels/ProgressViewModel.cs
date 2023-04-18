using CookPopularCSharpToolkit.Communal;
using CookPopularCSharpToolkit.Windows;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using CookPopularInstaller.Toolkit.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
//using SharpCompress.Archives;
//using SharpCompress.Archives.Rar;
//using SharpCompress.Readers;



/*
 * Description：ProgressViewModel
 * Author：chance
 * Organization: CookCSharp
 * Create Time：2022-05-09 09:37:46
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright (c) 2022 All Rights Reserved.
 */
namespace CookPopularInstaller.CustomUI.ViewModels
{
    /// <summary>
    /// 内置变量类型
    /// </summary>
    public enum VariableType
    {
        InstallFolder,
        AppPath,
        CompanyName,
        ProductName,
        ProductVersion,
    }

    public class ProgressViewModel : ViewModelBase
    {

#if DEBUG
        public bool IsShowNextButton { get; set; } = true;
        public DelegateCommand NextButtonCommand => new DelegateCommand(() =>
        {
            BootstrapperApplicationAgent.Instance.OnStateChanged(InstallState.Applyed);
        });
#else
        public bool IsShowNextButton { get; set; } = false;
#endif

        public string CurrentState
        {
            get
            {
                if (App.GetInstallState() == InstallState.Applying)
                    return "安装中：";
                else if (App.GetInstallState() == InstallState.Cancelled)
                    return "取消中：";
                else
                    return "Applying：";
            }
        }
        public string Message { get; set; }
        public int Progress { get; set; }

        public BitmapSource BackImage
        {
            get
            {
                var stream = System.Windows.Application.GetResourceStream(new Uri("..\\Assets\\Images\\semiconductor.png", UriKind.Relative)).Stream;
                Bitmap bmp = new Bitmap(Image.FromStream(stream));
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                                                                                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                                                                                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
        }


        private static readonly object LockInstanceObject = new object();
        private int _cacheProgress;
        private int _executeProgress;


        public ProgressViewModel()
        {
#if RELEASE
            BootstrapperAgent.BootstrapperApplication.ExecutePackageBegin += BootstrapperApplication_ExecutePackageBegin;
            BootstrapperAgent.BootstrapperApplication.CacheAcquireProgress += BootstrapperApplication_CacheAcquireProgress;
            BootstrapperAgent.BootstrapperApplication.ExecuteProgress += BootstrapperApplication_ExecuteProgress;
            BootstrapperAgent.BootstrapperApplication.ExecuteMsiMessage += BootstrapperApplication_ExecuteMsiMessage;
            BootstrapperAgent.BootstrapperApplication.ExecutePackageComplete += BootstrapperApplication_ExecutePackageComplete;
            BootstrapperAgent.BootstrapperApplication.ApplyBegin += BootstrapperApplication_ApplyBegin;
            BootstrapperAgent.BootstrapperApplication.ApplyComplete += BootstrapperApplication_ApplyComplete;
#endif
        }

        public async void OnLoaded()
        {
#if DEBUG
            await InstallDepends();
            //await InstallExtensions();

            //await UninstallDepends();
            //await UninstallExtensions();
#else
            await Task.Delay(1);
#endif
        }

        private async void BootstrapperApplication_ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
        {
            BootstrapperAgent.LogMessage($"******ExecutePackageBegin---PackageId:{e.PackageId};ShouldExecute:{e.ShouldExecute};Result:{e.Result}*********");

            if (e.PackageId.Equals(App.ExePackageIdDotnetFramework48, StringComparison.Ordinal))
            {
                Message = "Microsoft .NET Framework 4.8 Setup";
            }

            if (App.GetIfUninstalled())
            {
                await UninstallDepends();
                //await UninstallExtensions();
            }

            e.Result = App.GetInstallState() == InstallState.Cancelled ? Result.Cancel : Result.Ok;
        }

        private void BootstrapperApplication_CacheAcquireProgress(object sender, CacheAcquireProgressEventArgs e)
        {
            _cacheProgress = e.OverallPercentage;
            Progress = (_cacheProgress + _executeProgress) / 2;
        }

        private void BootstrapperApplication_ExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            _executeProgress = e.OverallPercentage;
            Progress = (_cacheProgress + _executeProgress) / 2;
        }

        private void BootstrapperApplication_ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            lock (LockInstanceObject)
            {
                if (e.MessageType == InstallMessage.ActionStart)
                {
                    Message = e.Message.Split('。')[1];
                }

                e.Result = App.GetInstallState() == InstallState.Cancelled ? Result.Cancel : Result.Ok;

                string extendMsg = string.Empty;
                e.Data.ToList().ForEach(s => extendMsg += (s + "&"));
                BootstrapperAgent.LogMessage($"---------------Message:{e.Message},MessageType:{e.MessageType},ExtendMessage:{extendMsg}---------------");
            }
        }

        private void BootstrapperApplication_ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
        {
            BootstrapperAgent.LogMessage($"******ExecutePackageComplete---PackageId:{e.PackageId};Restart:{e.Restart};Status:{e.Status};Result:{e.Result}*********");

            e.Result = App.GetInstallState() == InstallState.Cancelled ? Result.Cancel : Result.Ok;
        }

        private void BootstrapperApplication_ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            BootstrapperAgent.LogMessage($"*************ApplyBegin---Result:{e.Result}****************");
        }

        private async void BootstrapperApplication_ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            BootstrapperAgent.LogMessage($"*************ApplyComplete---Restart:{e.Restart};Status:{e.Status};Result:{e.Result}****************");

            if (!App.GetIfUninstalled())
            {
                await InstallDepends();
                //await InstallExtensions();
            }

            App.SetInstallState(InstallState.Applyed);
            BootstrapperAgent.FinalResult = e.Status;
        }

        private async Task Decompress(string sourceFile, string targetPath)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException(string.Format("未能找到文件'{0}'", sourceFile));
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            await Task.Run(() =>
            {
                using FileStream fileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
                using ZipInputStream zipInputStream = new ZipInputStream(fileStream);
                ZipEntry zipEntry = null;
                while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                {
                    string directoryName = Path.Combine(targetPath, Path.GetDirectoryName(zipEntry.Name));
                    string fileName = Path.Combine(directoryName, Path.GetFileName(zipEntry.Name));
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    if (Directory.Exists(fileName))
                        continue;
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        using FileStream streamWriter = File.Create(fileName);
                        int size = 4096;
                        byte[] buffer = new byte[4 * 1024];
                        while (true)
                        {
                            size = zipInputStream.Read(buffer, 0, buffer.Length);
                            if (size > 0)
                                streamWriter.Write(buffer, 0, size);
                            else
                                break;
                        }
                    }
                }
            });
        }

        private async Task InstallDepends()
        {
#if DEBUG
            var packageFile = Path.Combine(Environment.CurrentDirectory, "package.json");
#else
            var packageFile = Directory.GetFiles(BootstrapperAgent.GetBurnVariable("InstallFolder"), "package*.json").FirstOrDefault();
            //var packageFile = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), "package.json");
#endif
            if (!File.Exists(packageFile)) return;

            var package = JsonHelper.JsonDeserializeFile<Package>(packageFile);
            foreach (var depend in package.Depends.DependDialogVariables)
            {
                if (depend.Value.EndsWith(".zip"))
                    continue;

                await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在安装{depend.Name}...");
                switch (depend.CheckType)
                {
                    case CheckType.Enviorment:
                        bool isExisted = false;
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
                        if (!isExisted)
                        {
                            await InternalInstall(depend);
                        }
                        else
                        {
                            var dependPath = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), depend.Value);
                            File.Delete(dependPath);
                        }
                        break;
                    case CheckType.Registry:
                        break;
                    case CheckType.Folder:
                        var dependValueName = Path.GetFileNameWithoutExtension(Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), depend.Value));
                        if (!Directory.Exists(Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), dependValueName)))
                        {
                            await InternalInstall(depend);
                        }
                        else
                        {
                            var dependPath = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), depend.Value);
                            File.Delete(dependPath);
                        }
                        break;
                    case CheckType.Other:
                        break;
                    default:
                        break;
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
                            //    //        archiveEntry.WriteToDirectory(@"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\bin\Debug\AnyCPU\CookPopularInstaller.CustomUI\net48\Depends\RuntimeChance", new SharpCompress.Common.ExtractionOptions
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
                            //    //        archiveEntry.WriteToDirectory(@"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\bin\Debug\AnyCPU\CookPopularInstaller.CustomUI\net48\Depends\RuntimeChance", new SharpCompress.Common.ExtractionOptions
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

                            //ZipFile.ExtractToDirectory(dependPath, @"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\bin\Debug\AnyCPU\CookPopularInstaller.CustomUI\net48\Depends\RuntimeChance");
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

        /*
         *内置变量
         *InstallFolder
         *AppPath
         *CompanyName
         *ProductName
         *ProductVersion
         */
        /// <summary>
        /// 将内置的变量转为相应的值
        /// </summary>
        /// <param name="extensions"></param>
        private void GetConverterExtensions(Extensions extensions)
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
                        var attr = Attribute.GetCustomAttribute(prop, typeof(ConverterAttribute));
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
            //    content = content.Replace("[AppVersion]", BootstrapperAgent.GetBurnVariable("WixBundleVersion"));
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
            //var packageFile = Path.Combine(BootstrapperAgent.GetBurnVariable("InstallFolder"), "package.json");
#endif
            if (!File.Exists(packageFile)) return;

            await Task.Run(async () =>
            {
                var package = JsonHelper.JsonDeserializeFile<Package>(packageFile);
                GetConverterExtensions(package.Extensions);

                //await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在写环境变量信息...");
                //extensions.EnvironmentVariables.ForEach(variable => EnvironmentHelper.WriteEnviorment(variable.Name, variable.Value));

                //await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在写注册表信息...");
                //extensions.RegistryVariables.ForEach(variable => RegistryHelper.CreateRegistryKey(variable.RegistryHive, variable.RegistryValueKind, variable.Path, variable.Name, variable.Value));

                await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在启动服务...");
                package.Extensions.WindowsServices.ForEach(service =>
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
            //string keyPath = Environment.Is64BitOperatingSystem ? $"Software\\Wow6432Node\\CookCSharp\\ATEStudio" : $"Software\\CookCSharp\\ATEStudio";
            string keyPath = Environment.Is64BitOperatingSystem ? $"Software\\Wow6432Node\\{companyName}\\{productName}" : $"Software\\{companyName}\\{productName}";
            var directory = RegistryHelper.GetLocalMachineRegistryKeyPathValue(keyPath, "Directory").ToString();
            var packageFile = Directory.GetFiles(directory, "package*.json").FirstOrDefault();
            //var packageFile = Path.Combine(directory, "package.json");
#endif
            if (!File.Exists(packageFile)) return;

            await Task.Run(async () =>
            {
                var package = JsonHelper.JsonDeserializeFile<Package>(packageFile);

                //await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在删除环境变量信息...");
                //extensions.EnvironmentVariables.ForEach(variable => EnvironmentHelper.WriteEnviorment(variable.Name, null));

                //await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在删除注册表信息...");
                //extensions.RegistryVariables.ForEach(variable => RegistryHelper.DeleteRegistryKeyPathValue(variable.RegistryHive, variable.Path, variable.Name));

                await SynchronizationWithAsync.AppInvokeAsync(() => Message = $"正在删除服务...");
                package.Extensions.WindowsServices.ForEach(service =>
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