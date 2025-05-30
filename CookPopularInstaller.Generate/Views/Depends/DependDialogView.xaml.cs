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
using PropertyChanged;
using CookPopularUI.WPF.Controls;
using CookPopularInstaller.Generate.Models;
using Prism.Mvvm;
using CookPopularInstaller.Toolkit;

namespace CookPopularInstaller.Generate.Views
{
    /// <summary>
    /// EnvironmentView.xaml 的交互逻辑
    /// </summary>  
    public partial class DependDialogView : UserControl
    {
        public DependDialogView()
        {
            InitializeComponent();
        }
    }

    public class DependDialogViewModel : BindableBase, IDialogResult<DependDialogVariable>
    {
        public DependDialogVariable Result { get; set; } = new DependDialogVariable();
        public Action CloseAction { get; set; }
    }
}
