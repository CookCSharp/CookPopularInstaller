using System;
using System.Windows;
using System.Windows.Input;

namespace NCATestInstaller.CustomUI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InputMethod.SetIsInputMethodEnabled(this, false);
            InitCommand();
        }

        private void InitCommand()
        {
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
