using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using CookPopularCSharpToolkit.Communal;
using System.IO;
using CookPopularInstaller.Toolkit.Helpers;

namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
#if DEBUG
        private static readonly string AppVersion = FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\\CookPopularInstaller.CustomUI.exe").FileVersion;
        public string Title => BootstrapperAgent.ProductName + $"(v{AppVersion})";
#else
        public string Title => $"{BootstrapperAgent.ProductName}(v{BootstrapperAgent.GetBurnVariable("WixBundleVersion")})";
#endif

        public ImageSource WindowIcon { get; set; }

        public DelegateCommand LoadedCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnLoadedAction)).Value;

        public MainWindowViewModel()
        {
            BootstrapperAgent.SetWindowHandle(Application.Current.MainWindow);
            App.SetInstallState(StandardBootstrapperApplication.InitState);

            SetWindowIcon();
        }

        private void SetWindowIcon()
        {
#if DEBUG
            var drawingIcon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            WindowIcon = drawingIcon.ToBitmap().ToImageSource();
#else
            string filePath = BootstrapperAgent.GetBurnVariable("WixBundleOriginalSource");
            if (!File.Exists(filePath))
            {
                var companyName = BootstrapperAgent.GetBurnVariable("CompanyName");
                var productName = BootstrapperAgent.GetBurnVariable("ProductName");
                //string keyPath = Environment.Is64BitOperatingSystem ? $"Software\\Wow6432Node\\{companyName}\\{productName}" : $"Software\\{companyName}\\{productName}";
                filePath = RegistryHelper.GetLocalMachineRegistryKeyPathValue($"Software\\{companyName}\\{productName}", "AppPath").ToString();
            }
            var drawingIcon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
            WindowIcon = drawingIcon.ToBitmap().ToImageSource();
#endif
        }

        private void OnLoadedAction()
        {

        }
    }
}
