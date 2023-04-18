using CookPopularControl.Windows;
using System.Reflection;

namespace CookPopularInstaller.Generate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SideBarWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [Obfuscation]
        public int MyProperty { get; set; }
    }
}
