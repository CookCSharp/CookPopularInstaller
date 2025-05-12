using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NCATestInstaller.Generate.Models;
//using Microsoft.VisualBasic.ApplicationServices;
using NCATestInstaller.Generate.Views;
using NCATestInstaller.Toolkit;
using NCATestInstaller.Toolkit.Helpers;
using Prism.Events;
using Prism.Ioc;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Resolution;


namespace NCATestInstaller.Generate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string PackageJsonFileName = "package.json";
        private static readonly Lazy<IUnityContainer> _container = new Lazy<IUnityContainer>(() => new UnityContainer());
        public static IUnityContainer Container => _container.Value;


        public App()
        {
            this.InitializeComponent();
            this.Startup += (s, e) => RegisterTypes();
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        private void RegisterTypes()
        {
            //new InjectionMember[] { new InjectionConstructor() }
            //Container.RegisterSingleton<ViewModels.ViewModelBase>(new InjectionMember[] { new InjectionMethod("GetEventAggregator") });
            Container.RegisterSingleton<IEventAggregator, EventAggregator>();
            Container.Register<MainView>();
            Container.RegisterInstance<UserControl>(typeof(ProjectView).Name, Container.Resolve<ProjectView>());
            Container.RegisterInstance<UserControl>(typeof(ConfuseView).Name, Container.Resolve<ConfuseView>());
            Container.RegisterInstance<UserControl>(typeof(DependsView).Name, Container.Resolve<DependsView>());
            Container.RegisterInstance<UserControl>(typeof(ExtensionsView).Name, Container.Resolve<ExtensionsView>());
            Container.RegisterInstance<UserControl>(typeof(BuildView).Name, Container.Resolve<BuildView>());
            //Container.RegisterSingleton<UserControl, ProjectView>(typeof(ProjectView).Name);
            //Container.RegisterType<UserControl, BuildView>(typeof(BuildView).Name, new ContainerControlledLifetimeManager());
        }

        public static void SavePackageFile(PackageInfo package)
        {
            string suffixName = string.Empty;
            if (string.IsNullOrEmpty(package.Project.PackageName))
                suffixName = "default";
            else
                suffixName = package.Project.PackageName.Split('.', '-')[0];
            JsonHelper.WriteJsonFile(package, Path.Combine(Environment.CurrentDirectory, PackageJsonFileName));
        }
    }
}
