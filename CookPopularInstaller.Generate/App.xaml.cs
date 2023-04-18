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
using CookPopularInstaller.Generate.Models;
//using Microsoft.VisualBasic.ApplicationServices;
using CookPopularInstaller.Generate.Views;
using CookPopularInstaller.Toolkit.Helpers;
using Prism.Events;
using Prism.Ioc;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Resolution;


namespace CookPopularInstaller.Generate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string PackageFileName = "package.json";
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

            //var s1 = App.Container.Registrations;
            //var s2 = App.Container.Parent;
            //var EventAggregator1 = Container.Resolve<IEventAggregator>();
            //var EventAggregator2 = Container.Resolve<IEventAggregator>();
            //var b1 = EventAggregator1.Equals(EventAggregator2);

            //var ProjectView1 = Container.Resolve<UserControl>(typeof(ProjectView).Name);
            //var ProjectView2 = Container.Resolve<UserControl>(typeof(ProjectView).Name);
            //var b2 = ProjectView1.Equals(ProjectView2);

            //var BuildView1 = Container.Resolve<BuildView>(typeof(BuildView).Name);
            //var BuildView2 = Container.Resolve<BuildView>(typeof(BuildView).Name);
            //var b3 = BuildView1.Equals(BuildView2);

            //var ViewModelBase = Container.Resolve<ViewModels.ViewModelBase>();
            //System.ComponentModel.EditorBrowsableState.
            //LocalizationCategory.
        }

        public static ProjectModel GetProject()
        {
            var package = ReadPackageFile();
            if (package == null) return default;

            if (string.IsNullOrEmpty(package.Project.PackageOutputPath) | !Directory.Exists(package.Project.PackageOutputPath))
                package.Project.PackageOutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), package.Project.Company);

            if (string.IsNullOrEmpty(package.Project.PackageVersion))
                package.Project.PackageVersion = FileVersionInfo.GetVersionInfo(package.Project.AppFileName).FileVersion;

            return package.Project;
        }

        public static PackageModel ReadPackageFile()
        {
            var packageFile = Directory.GetFiles(Environment.CurrentDirectory, PackageFileName).FirstOrDefault();
            if (packageFile == null) return default;

            var package = JsonHelper.JsonDeserializeFile<PackageModel>(packageFile);
            return package;
        }

        public static void SavePackageFile(PackageModel package)
        {
            string suffixName = string.Empty;
            if (string.IsNullOrEmpty(package.Project.PackageName))
                suffixName = "default";
            else
                suffixName = package.Project.PackageName.Split('.', '-')[0];
            JsonHelper.WriteJsonFile(package, Path.Combine(Environment.CurrentDirectory, PackageFileName));
        }
    }
}
