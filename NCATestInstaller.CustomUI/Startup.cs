using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCATestInstaller.CustomUI
{
    public class Startup
    {
        [STAThread]
        public static void Main(string[] arg)
        {
            SingletonAppWrapper wrapper = new SingletonAppWrapper();
            wrapper.Run(arg);

            //Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out bool isCreated);
            //if (!isCreated)
            //{
            //    System.Windows.MessageBox.Show("Is Running");
            //}
            //else
            //{
            //    var app = new App();
            //    app.InitializeComponent();
            //    app.Run();
            //}
        }
    }
}
