using CookPopularUI.WPF.Controls;
using CookPopularUI.WPF.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using CookPopularInstaller.Toolkit.Helpers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class RunningViewModel : ViewModelBase
    {
        public ObservableCollection<string> ProcessNames { get; set; }

        public DelegateCommand LoadCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnLoadedAction)).Value;
        public DelegateCommand KillAllCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnKillAllAction)).Value;


        private LaunchAction _launchAction;
        private IList<Process> _allProcess;


        private void OnLoadedAction()
        {
#if ACTUALTEST
            _launchAction = CommonTools.GetLaunchAction();
            _allProcess = App.RunningProcesses;
            ProcessNames = new ObservableCollection<string>(App.RunningProcesses.Select(p => p.ProcessName));
#else           
            _launchAction = LaunchAction.Unknown;
            _allProcess = App.RunningProcesses;
            ProcessNames = new ObservableCollection<string>() { "Chance1", "Chance2", "Chance3" };
            //ProcessNames = new ObservableCollection<string>(App.RunningProcesses.Select(p => p.ProcessName));
#endif
        }

        private async void OnKillAllAction()
        {
            App.DialogBox = DialogBox.Show<DotCircleLoading>("Detecting");
            await Task.Run(() =>
            {
                foreach (Process pro in _allProcess)
                {
                    while (!pro.HasExited)
                    {
                        try
                        {
                            pro.Kill();
                            pro.WaitForExit();
                            //ProcessHelper.StartProcessByCmd($"taskkill /f /pid {pro.Id}", true);
                            StandardBootstrapperApplication.InvokeAsync(() => ProcessNames.Remove(pro.ProcessName));
                        }
                        catch (Exception ex)
                        {
                            MessageDialog.ShowError(App.Current.Resources["EndProcess"].ToString() + pro.ProcessName + "：" + ex.Message + App.Current.Resources["ManualClose"].ToString());
                        }
                    }
                }
            });

            BootstrapperAgent.PlanAction(_launchAction);
            App.DialogBox?.Close();
        }
    }
}
