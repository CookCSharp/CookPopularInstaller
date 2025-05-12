using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NCATestInstaller.CustomUI.Views
{
    /// <summary>
    /// DetectingView.xaml 的交互逻辑
    /// </summary>
    public partial class DetectingView : UserControl
    {
        public DetectingView()
        {
            InitializeComponent();

            this.Loaded += async (s, e) =>
            {
#if ACTUALTEST
                await Task.Delay(200);
                //CommonTools.Resume();
#else
                await Task.Delay(1000);
                App.SetInstallState(InstallationState.Absent);
#endif
                (Application.Current.MainWindow.DataContext as ViewModelBase).IsDetecting = false;
            };
        }
    }
}
