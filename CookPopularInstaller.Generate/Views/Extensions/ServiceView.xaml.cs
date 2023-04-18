using CookPopularControl.Controls;
using CookPopularInstaller.Generate.Models;
using Prism.Mvvm;
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

namespace CookPopularInstaller.Generate.Views
{
    /// <summary>
    /// ServiceView.xaml 的交互逻辑
    /// </summary>
    public partial class ServiceView : UserControl
    {
        public ServiceView()
        {
            InitializeComponent();
        }
    }

    public class ServiceViewModel : BindableBase, IDialogResultable<WindowsServiceVariable>
    {
        public WindowsServiceVariable Result { get; set; } = new WindowsServiceVariable();
        public Action CloseAction { get; set; }
    }
}
