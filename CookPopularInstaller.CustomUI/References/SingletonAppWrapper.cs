using CookPopularToolkit.Windows;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookPopularInstaller.CustomUI
{
    public class SingletonAppWrapper: WindowsFormsApplicationBase
    {
        private App app;


        public SingletonAppWrapper()
        {
            IsSingleInstance = false;
            ShutdownStyle = ShutdownMode.AfterAllFormsClose;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {          
            app = new App();
            app.InitializeComponent();
            app.Run();

            return false;
        }

        protected override void OnStartupNextInstance(Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs eventArgs)
        {
            app.MainWindow?.SwitchToThisWindow();
        }
    }
}
