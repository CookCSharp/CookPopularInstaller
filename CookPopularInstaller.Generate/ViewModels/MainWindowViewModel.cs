/*
 * Description：MainWindowViewModel 
 * Author： Chance.Zheng
 * Create Time: 2023/2/15 17:10:29
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */


using CookPopularToolkit;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using System.Configuration;
using Prism.Ioc;
using System.Windows.Controls;
using Unity;

namespace CookPopularInstaller.Generate.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public int ContentViewSelectedIndex { get; set; } = -1;
        public object ContentView
        {
            get
            {
                if (ContentViewSelectedIndex >= 0)
                {
                    //var type = Type.GetType($"CookPopularInstaller.Generate.Views.{ViewNames.ElementAt(ContentViewSelectedIndex)}View");
                    //return App.Container.Resolve(type);

                    var viewName = $"{ViewNames.ElementAt(ContentViewSelectedIndex)}View";
                    return App.Container.Resolve<UserControl>(viewName);
                }
                else
                {
                    return App.Container.Resolve<Views.MainView>();
                }
            }
        }
        public ObservableCollection<string> ContentViewsList { get; set; }
        public bool IsBeginGenerate { get; set; }


        public DelegateCommand BeginCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnBeginAction)).Value;
        //public DelegateCommand HomeCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(()=> IsBeginGenerate = false)).Value;
        public DelegateCommand ClosingCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnClosingAction)).Value;


        private static readonly IList<string> ViewNames = new List<string>() { "Project", "Confuse", "Depends", "Extensions", "Build" };


        public MainWindowViewModel()
        {
            ContentViewsList = new ObservableCollection<string>();
            ViewNames.ForEach(viewName => ContentViewsList.Add(viewName));
        }

        private void OnBeginAction()
        {
            IsBeginGenerate = true;
            ContentViewSelectedIndex = 0;
        }

        private void OnClosingAction()
        {

        }
    }
}
