using CookPopularInstaller.CustomUI.Views;
using CookPopularInstaller.Toolkit.Helpers;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.IO;
using System.Linq;
using System.Windows;


namespace CookPopularInstaller.CustomUI
{
    enum ThemeType
    {
        Default,
        Light,
        Dark,
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private static InstallState _state;
        private static bool _isUninstalled;
        public static readonly string MsiPackageId = "CookPopularInstaller"; //与Bundle.wxs文件中MsiPackage.Id属性值一致
        public static readonly string ExePackageIdDotnetFramework48 = "Netfx48Full"; //与Bundle.wxs文件中ExePackage.Id属性值一致


        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<InstallView>();
            containerRegistry.RegisterForNavigation<LicenseView>();
            containerRegistry.RegisterForNavigation<ProgressView>();
            containerRegistry.RegisterForNavigation<FinishView>();
            containerRegistry.RegisterForNavigation<ChangeRepairUninstallView>();
            containerRegistry.RegisterForNavigation<DetectInfoView>();
        }

        private void SetTheme(ThemeType themeType)
        {
            switch (themeType)
            {
                case ThemeType.Default:
                    App.Current.Resources.MergedDictionaries[1] = new ResourceDictionary() { Source = new Uri("pack://application:,,,/CookPopularControl;component/Themes/CookColors/OrangeColor.xaml", System.UriKind.Absolute) };
                    App.Current.Resources.MergedDictionaries[2] = new ResourceDictionary() { Source = new Uri("/CookPopularInstaller.CustomUI;component/Assets/Themes/Default.xaml", System.UriKind.Relative) };
                    break;
                case ThemeType.Light:
                    App.Current.Resources.MergedDictionaries[1] = new ResourceDictionary() { Source = new Uri("pack://application:,,,/CookPopularControl;component/Themes/CookColors/LightColor.xaml", System.UriKind.Absolute) };
                    App.Current.Resources.MergedDictionaries[2] = new ResourceDictionary() { Source = new Uri("/CookPopularInstaller.CustomUI;component/Assets/Themes/Light.xaml", System.UriKind.Relative) };
                    break;
                case ThemeType.Dark:
                    App.Current.Resources.MergedDictionaries[1] = new ResourceDictionary() { Source = new Uri("pack://application:,,,/CookPopularControl;component/Themes/CookColors/DarkColor.xaml", System.UriKind.Absolute) };
                    App.Current.Resources.MergedDictionaries[2] = new ResourceDictionary() { Source = new Uri("/CookPopularInstaller.CustomUI;component/Assets/Themes/Dark.xaml", System.UriKind.Relative) };
                    break;
                default:
                    break;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            string themeValue = ConfigurationManageHelper.ReadItem("PackageTheme");
            if (Enum.TryParse(themeValue, out ThemeType themeType))
                SetTheme(themeType);
        }

        public static void SetInstallState(InstallState state)
        {
            _state = state;
            BootstrapperApplicationAgent.Instance.OnStateChanged(state);
        }

        public static InstallState GetInstallState() => _state;

        public static void SetUninstalled() => _isUninstalled = true;
        public static bool GetIfUninstalled() => _isUninstalled;
    }
}
