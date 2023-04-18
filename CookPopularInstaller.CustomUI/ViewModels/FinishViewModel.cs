using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;



/*
 * Description：FinishViewModel
 * Author：chance
 * Organization: CookCSharp
 * Create Time：2022-05-09 11:02:02
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright (c) 2022 All Rights Reserved.
 */
namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class FinishViewModel : ViewModelBase
    {
#if DEBUG
        public bool IsShowNextButton { get; set; } = true;
        public DelegateCommand NextButtonCommand => new DelegateCommand(() =>
        {
            BootstrapperApplicationAgent.Instance.OnStateChanged(InstallState.Present);
        });
#else
        public bool IsShowNextButton { get; set; } = false;
#endif

        public BitmapSource BackImage
        {
            get
            {
                var stream = System.Windows.Application.GetResourceStream(new Uri("..\\Assets\\Images\\finish.png", UriKind.Relative)).Stream;
                Bitmap bmp = new Bitmap(Image.FromStream(stream));
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                                                                                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                                                                                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
        }

        public bool IsRunApp { get; set; }

        public bool IsUninstalled => App.GetIfUninstalled();

        public DelegateCommand FinishInstallCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(FinishInstallAction)).Value;


        public FinishViewModel()
        {
            //var wixBundleProviderKey = BootstrapperAgent.GetBurnVariable("WixBundleProviderKey");
            //var keyPath = $@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{wixBundleProviderKey}";
            //var registryKey = RegisterHelper.GetRegistryKey(keyPath);
            //if(registryKey != null)
            //{
            //    var appKey = RegisterHelper.GetToolsManagementBaseKey();
            //    appKey.SetValue("ProviderKey", wixBundleProviderKey, Microsoft.Win32.RegistryValueKind.String);
            //}
        }

        private void FinishInstallAction()
        {
            if (IsRunApp && !IsUninstalled)
            {
                //RegisterHelper.TryGetToolsManagementPath(out string appPath);
                var installFolder = BootstrapperAgent.GetBurnVariable("InstallFolder");
                var exeName = BootstrapperAgent.GetBurnVariable("ExeName");
                var appPath = Path.Combine(installFolder, exeName);
                Process.Start(appPath);
            }
            StandardBootstrapperApplication.InvokeShutdown();
        }
    }
}
