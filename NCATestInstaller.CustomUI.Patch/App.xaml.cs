using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace NCATestInstaller.CustomUI.Patch
{
    public delegate void Result(State state, string res, System.Windows.Media.Geometry geometry, System.Windows.Media.Brush brush);

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static event Result OnResult;
        public static string SevenZDllPath { get; private set; }
        public static Level Level { get; set; }

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

        private void Fatal(string title, object sender, Exception ex) => Over(Level.Error, ex.Message);

        public static void Over(Level level, string msg)
        {
            var state = State.Finish;
            var result = string.Format("{0}: {1}", level.ToString(), msg);
            var geometry = App.Current.Resources[$"{level}Geometry"] as System.Windows.Media.Geometry;
            var brush = level switch
            {
                Level.Info => System.Windows.Media.Brushes.DodgerBlue,
                Level.Success => System.Windows.Media.Brushes.ForestGreen,
                Level.Error => System.Windows.Media.Brushes.Red,
                _ => throw new NotImplementedException(),
            };
            OnResult(state, result, geometry, brush);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SevenZDllPath = Extract7zDll();

            ////调试
            //string name = "NCATestInstaller.Generate";
            //string version = "1.0.0.1P01";
            //string oldDir = @"D:\Users\chance.zheng\Desktop\Company\NCATestInstaller\Output\Package";
            //string newDir = @"D:\Users\chance.zheng\Desktop\Company\NCATestInstaller\Output\Publish";
            //string targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Diff");
            //if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
            //Compress.CompressFiles(name, version, oldDir, newDir, targetDir, SevenZDllPath);
            //Environment.Exit(0);

            if (e.Args.Length == 4)
            {
                string name = e.Args[0];
                string version = e.Args[1];
                string oldDir = e.Args[2];
                string newDir = e.Args[3];
                string targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Diff");
                if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
                Compress.CompressFiles(name, version, oldDir, newDir, targetDir, SevenZDllPath);

                Environment.Exit(0);
            }
        }

        private string Extract7zDll()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var rs = assembly.GetManifestResourceStream("NCATestInstaller.CustomUI.Patch.Assets.7z.dll");
            if (rs == null) throw new Exception("Resource 7z.dll not found");

            string tempPath = Path.Combine(Path.GetTempPath(), "7z64.dll");
            using var fs = new FileStream(tempPath, FileMode.Create);
            rs.CopyTo(fs);

            return tempPath;
        }
    }
}
