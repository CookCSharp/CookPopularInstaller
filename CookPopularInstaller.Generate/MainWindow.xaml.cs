using CookPopularUI.WPF.Windows;
using System.Reflection;

namespace CookPopularInstaller.Generate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NormalWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [Obfuscation]
        public int MyProperty { get; set; }
    }
}
