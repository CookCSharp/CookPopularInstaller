﻿using System;
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
    public partial class EnvironmentView : UserControl
    {
        public EnvironmentView()
        {
            InitializeComponent();
        }
    }

    public class EnvironmentViewModel : BindableBase, IDialogResult<EnvironmentVariable>
    {
        public EnvironmentVariable Result { get; set; } = new EnvironmentVariable();
        public Action CloseAction { get; set; }
    }
}
