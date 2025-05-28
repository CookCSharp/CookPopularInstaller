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
using System.IO;
using CookPopularInstaller.Toolkit.Helpers;
using System.Linq;
using CookPopularUI.WPF.Windows;
using CookPopularToolkit.Windows;

namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
#if !ACTUALTEST
        private static readonly string Version = FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\\CookPopularInstaller.CustomUI.exe").FileVersion;
        public string Title => BootstrapperAgent.ProductName + $"(v{Version})";
#else
        //public string Title => $"{BootstrapperAgent.ProductName}(v{BootstrapperAgent.GetBurnVariable("WixBundleVersion")})";
        public string Title => $"{BootstrapperAgent.ProductName}(v{BootstrapperAgent.GetBurnVariable("PackageVersion")})";
#endif        
        public ImageSource WindowIcon { get; set; }

        public bool IsQuiet { get; set; }

        public DelegateCommand<Window> LoadedCommand => new Lazy<DelegateCommand<Window>>(() => new DelegateCommand<Window>(OnLoadedAction)).Value;


        private void OnLoadedAction(Window win)
        {
#if !ACTUALTEST
            CommonTools.PreInstallState = InstallationState.Detecting;
#endif
            SetWindowIcon();
            BootstrapperAgent.SetWindowHandle(Application.Current.MainWindow);
            App.SetInstallState(CommonTools.PreInstallState);
            IsDetecting = App.GetInstallState() == InstallationState.Detecting;
#if ACTUALTEST
            BootstrapperAgent.BootstrapperApplication.Engine.CloseSplashScreen();
#endif
        }

        private void SetWindowIcon()
        {
#if !ACTUALTEST
            var drawingIcon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            WindowIcon = drawingIcon.ToBitmap().ToImageSource();
#else
            string filePath = BootstrapperAgent.GetBurnVariable("WixBundleOriginalSource");
            if (!File.Exists(filePath)) filePath = CommonTools.GetBundleCacheFilePath();
            var drawingIcon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
            WindowIcon = drawingIcon.ToBitmap().ToImageSource();
#endif
        }
    }
}
