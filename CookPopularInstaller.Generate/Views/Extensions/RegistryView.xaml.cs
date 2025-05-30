using CookPopularUI.WPF.Controls;
using CookPopularInstaller.Generate.Models;
using CookPopularInstaller.Toolkit;
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
    /// RegistryView.xaml 的交互逻辑
    /// </summary>
    public partial class RegistryView : UserControl
    {
        public RegistryView()
        {
            InitializeComponent();
        }
    }

    public class RegistryViewModel : BindableBase, IDialogResult<RegistryVariable>
    {
        public RegistryVariable Result { get; set; } = new RegistryVariable();
        public Action CloseAction { get; set; }
    }
}
