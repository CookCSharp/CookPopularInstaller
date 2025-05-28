using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using CookPopularInstaller.CustomUI.Views;
using CookPopularInstaller.Toolkit;
using CookPopularInstaller.Toolkit.Helpers;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CookPopularToolkit;
using CookPopularUI.WPF.Controls;
using ConfigurationManageHelper = CookPopularInstaller.Toolkit.Helpers.ConfigurationManageHelper;

namespace CookPopularInstaller.CustomUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private static InstallationState _state;
        internal static DialogBox DialogBox { get; set; }
        internal static IList<Process> RunningProcesses { get; private set; }


        public App()
        {
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Fatal("应用程序域中存在引发但未处理异常:", sender, e.ExceptionObject as Exception);
            //UI线程未捕获异常事件（UI主线程）
            DispatcherUnhandledException += (sender, e) =>
            {
                Fatal("UI主线程存在引发但未处理异常:", sender, e.Exception);
                e.Handled = true;
            };
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += (sender, e) => Fatal("Task线程内存在引发但未处理异常:", sender, e.Exception);
        }

        private void Fatal(string title, object sender, Exception ex)
        {
            DialogBox?.Close();
#if ACTUALTEST
            BootstrapperApplicationAgent.Instance.LogMessage(title + ex.Message, LogLevel.Error);
#endif
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DetectingView>();
            containerRegistry.RegisterForNavigation<InstallView>();
            containerRegistry.RegisterForNavigation<LicenseView>();
            containerRegistry.RegisterForNavigation<ProgressView>();
            containerRegistry.RegisterForNavigation<FinishView>();
            containerRegistry.RegisterForNavigation<ErrorView>();
            containerRegistry.RegisterForNavigation<ChangeRepairUninstallView>();
            containerRegistry.RegisterForNavigation<RunningView>();
            containerRegistry.RegisterForNavigation<DetectInfoView>();
            containerRegistry.RegisterForNavigation<CancelledView>();
        }

        private void SetTheme(PackageThemeType themeType)
        {
            switch (themeType)
            {
                case PackageThemeType.Default:
                    Current.Resources.MergedDictionaries[1] = new ResourceDictionary() { Source = new Uri("pack://application:,,,/CookPopularUI.WPF;component/Themes/DefaultPopularColor.xaml", UriKind.Absolute) };
                    Current.Resources.MergedDictionaries[2] = new ResourceDictionary() { Source = new Uri("/CookPopularInstaller.CustomUI;component/Assets/Themes/Default.xaml", UriKind.Relative) };
                    break;
                case PackageThemeType.Light:
                    Current.Resources.MergedDictionaries[1] = new ResourceDictionary() { Source = new Uri("pack://application:,,,/CookPopularUI.WPF;component/Themes/Colors/LightColor.xaml", UriKind.Absolute) };
                    Current.Resources.MergedDictionaries[2] = new ResourceDictionary() { Source = new Uri("/CookPopularInstaller.CustomUI;component/Assets/Themes/Light.xaml", UriKind.Relative) };
                    break;
                case PackageThemeType.Dark:
                    Current.Resources.MergedDictionaries[1] = new ResourceDictionary() { Source = new Uri("pack://application:,,,/CookPopularUI.WPF;component/Themes/Colors/DarkColor.xaml", UriKind.Absolute) };
                    Current.Resources.MergedDictionaries[2] = new ResourceDictionary() { Source = new Uri("/CookPopularInstaller.CustomUI;component/Assets/Themes/Dark.xaml", UriKind.Relative) };
                    break;
                default:
                    break;
            }
        }

        private void SetLanguage(LanguageType languageType)
        {
            switch (languageType)
            {
                case LanguageType.English:
                    Current.Resources.MergedDictionaries[4] = new ResourceDictionary() { Source = new Uri("pack://application:,,,/CookPopularInstaller.CustomUI;component/Assets/Languages/en-us.xaml", UriKind.Absolute) };
                    break;
                case LanguageType.Chinese:
                    Current.Resources.MergedDictionaries[4] = new ResourceDictionary() { Source = new Uri("pack://application:,,,/CookPopularInstaller.CustomUI;component/Assets/Languages/zh-cn.xaml", UriKind.Absolute) };
                    break;
                default:
                    break;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            string themeValue = ConfigurationManageHelper.ReadItem("PackageTheme");
            if (Enum.TryParse(themeValue, out PackageThemeType themeType))
                SetTheme(themeType);

            string languageValue = ConfigurationManageHelper.ReadItem("Language");
            if (Enum.TryParse(languageValue, out LanguageType languageType))
                SetLanguage(languageType);
        }

        public static BitmapSource GetBitmapSource(Uri uri)
        {
            var stream = GetResourceStream(uri).Stream;
            Bitmap bmp = new Bitmap(Image.FromStream(stream));
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                                                                                IntPtr.Zero, Int32Rect.Empty,
                                                                                BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
        }

        public static void SetInstallState(InstallationState state)
        {
            _state = state;
            BootstrapperApplicationAgent.Instance.OnStateChanged(state);
        }

        public static InstallationState GetInstallState() => _state;

        public static async Task CheckProcessesRunningAsync(LaunchAction action)
        {
            RunningProcesses = await GetAllProcessesAsync();

            if (RunningProcesses.Count > 0)
            {
                CommonTools.SetLaunchAction(action);
                SetInstallState(InstallationState.Running);
            }
            else
            {
#if ACTUALTEST
                BootstrapperApplicationAgent.Instance.PlanAction(action);
#endif
            }
        }

        private static string GetInstallFolder()
        {
            var companyName = BootstrapperApplicationAgent.Instance.GetBurnVariable("CompanyName");
            var productName = BootstrapperApplicationAgent.Instance.GetBurnVariable("ProductName");
            var installFolder = RegistryHelper.GetLocalMachineRegistryKeyPathValue($"Software\\{companyName}\\{productName}", "Directory")?.ToString();

            return installFolder;
        }

        private static IEnumerable<string> GetExeNames()
        {
            var installFolder = GetInstallFolder();
            var appNames = Directory.GetFiles(installFolder, "*.exe", SearchOption.AllDirectories)
                                    .Where(f => Path.GetFileName(f) != "Uninst.exe")
                                    .Select(f => Path.GetFileNameWithoutExtension(f));
            return appNames;
        }

        private static async Task<IList<Process>> GetAllProcessesAsync()
        {
            IList<Process> allProcess = new List<Process>();

            //var exeNames = GetExeNames();
            //exeNames.ForEach(name =>
            //{
            //    var processes = Process.GetProcessesByName(name);//.Where(p => p.MainModule.FileName.Contains(installFolder));
            //    if (processes != null && processes.Count() > 0)
            //        allProcess.AddRange(processes);
            //});

            var fileProcess = await FindAllProcessesOccupyFilesExceptExeAsync();
            allProcess.AddRange(fileProcess);

            return allProcess;
        }

        /// <summary>
        /// 文件是否被某个进程占用
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsFileLocked(string path)
        {
            if (!File.Exists(path))
                return false;

            bool isUse = true;
            FileStream fs = null;

            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                isUse = false;
            }
            catch (Exception)
            {
            }
            finally
            {
                fs?.Close();
            }

            return isUse;
        }

        /// <summary>
        /// 查找某个文件被哪些进程占用
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static async Task<IList<Process>> FindProcessOccupyFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                Process process = new Process();
#if ACTUALTEST
                ProcessStartInfo processStartInfo = new ProcessStartInfo(Path.Combine(GetInstallFolder(), "handle.exe"), $"\"{filePath}\"" + " /accepteula");
#else
                ProcessStartInfo processStartInfo = new ProcessStartInfo(Path.Combine(@"C:\Program Files (x86)\CookCSharp\CookPopularInstaller", "handle.exe"), $"\"{filePath}\"" + " /accepteula");
#endif
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.Verb = "runas";
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
                IList<Process> processes = new List<Process>();
                string output = process.StandardOutput.ReadToEnd();
                string pattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
                foreach (Match match in Regex.Matches(output, pattern))
                {
                    var p = Process.GetProcessById(int.Parse(match.Value));
                    processes.Add(p);
                }

                return processes;
            });
        }

        /// <summary>
        /// 查找运行的进程
        /// </summary>
        /// <remarks>使用Windows API中的性能计数器(Performance Counter)</remarks>
        /// <param name="installFolder"></param>
        /// <returns></returns>
        private static async Task<IList<Process>> FindProcessByRunningExAsync(IEnumerable<string> exeFiles)
        {
            return await Task.Run(() =>
            {
                IList<Process> processes = new List<Process>();
                foreach (var file in exeFiles)
                {
                    var processName = Path.GetFileName(file);
                    string query = $"SELECT * FROM Win32_Process WHERE Name = '{processName}'";
                    using var searcher = new System.Management.ManagementObjectSearcher(query);
                    using var collection = searcher.Get();
                    foreach (System.Management.ManagementObject @object in collection)
                    {
                        if (string.Equals(@object["Name"].ToString(), processName, StringComparison.OrdinalIgnoreCase))
                        {
                            var handle = @object["Handle"];
                            var processId = int.Parse(@object["processId"].ToString());
                            processes.Add(Process.GetProcessById(processId));
                        }
                    }
                }

                //var category = new PerformanceCounterCategory("Process");
                //var instanceNames = category.GetInstanceNames();
                //foreach (var instanceName in instanceNames)
                //{
                //    var counter = new PerformanceCounter("Process", "Handle Count", instanceName);
                //    if (counter.NextValue() > 0)
                //    {
                //        var processCounter = new PerformanceCounter("Process", "ID Process", instanceName);
                //        var processId = (int)processCounter.RawValue;
                //        var process = Process.GetProcessById(processId);
                //        {
                //            try
                //            {
                //                if (!string.IsNullOrWhiteSpace(process.MainModule.FileName) && exeFiles.Contains(process.MainModule.FileName))
                //                {
                //                    processes.Add(process);
                //                }
                //                //foreach (ProcessModule module in process.Modules)
                //                //{
                //                //    if (!string.IsNullOrWhiteSpace(module.FileName) && exeFiles.Contains(process.MainModule.FileName))
                //                //    {
                //                //        processes.Add(process);
                //                //    }
                //                //}
                //            }
                //            catch (Exception) { }
                //        }
                //    }
                //}

                return processes;
            });
        }

        /// <summary>
        /// 查找所有进程(包含文件被占用的进程和运行的进程)
        /// </summary>
        /// <returns></returns>
        private static async Task<IList<Process>> FindAllProcessesOccupyFilesExceptExeAsync()
        {
            return await Task.Run(async () =>
            {
#if ACTUALTEST
                var installFolder = GetInstallFolder();
#else
                var installFolder = @"C:\Program Files (x86)\CookCSharp\CookPopularInstaller";
#endif
                IList<Process> processes = new List<Process>();
                var occupiedFiles = Directory.GetFiles(installFolder, "*", SearchOption.AllDirectories).Where(f => !Path.GetFileName(f).Contains(".dll"))
                                                                                                       .Where(f => !Path.GetFileName(f).Contains(".exe"))
                                                                                                       .Where(f => IsFileLocked(f));
                var exeFiles = Directory.GetFiles(installFolder, "*", SearchOption.AllDirectories).Where(f => Path.GetFileName(f).Contains(".exe"));
                if (occupiedFiles.Any())
                {
                    foreach (var file in occupiedFiles)
                    {
                        processes.AddRange(await FindProcessOccupyFileAsync(file));
                    }
                }

                if (exeFiles.Any())
                {
                    processes.AddRange(await FindProcessByRunningExAsync(exeFiles));
                }

                return processes.Distinct(new ProcessComparer()).ToList();
            });
        }
    }
}
