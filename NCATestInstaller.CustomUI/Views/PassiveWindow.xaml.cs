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
using System.Windows.Shapes;

namespace NCATestInstaller.CustomUI.Views
{
    /// <summary>
    /// PassiveWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PassiveWindow : Window
    {
        public PassiveWindow()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, Executed));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, Executed));
        }

        private void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var win = sender as Window;
            if (e.Command == SystemCommands.MinimizeWindowCommand)
                SystemCommands.MinimizeWindow(win);
            else if (e.Command == SystemCommands.CloseWindowCommand)
                SystemCommands.CloseWindow(win);
        }
    }
}
