/*
 * Description：ProgressViewModel
 * Author：chance
 * Organization: CookCSharp
 * Create Time：2022-05-09 09:37:46
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright (c) 2022 All Rights Reserved.
 */


using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using CookPopularInstaller.Toolkit;
using CookPopularInstaller.Toolkit.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class ProgressViewModel : ViewModelBase
    {
#if !ACTUALTEST
        public bool IsShowNextButton { get; set; } = true;
        public DelegateCommand NextButtonCommand => new DelegateCommand(() =>
        {
            App.SetInstallState(InstallationState.Applyed);
        });
#else
        public bool IsShowNextButton { get; set; } = false;
#endif
        public string CurrentState => string.Format("{0}：", CommonTools.GetRelatedOperation());
        public string Message { get; set; } = App.Current.Resources["Waiting"].ToString();
        public int Progress { get; set; }
        public BitmapSource BackImage => App.GetBitmapSource(new Uri("..\\Assets\\Images\\semiconductor.png", UriKind.Relative));


        private static readonly object LockInstanceObject = new object();
        private System.Threading.Mutex _cancelRequest;

        private int _cacheProgress;
        private int _executeProgress;


        public ProgressViewModel()
        {
#if ACTUALTEST
            BootstrapperAgent.BootstrapperApplication.CacheBegin += BootstrapperApplication_CacheBegin;
            BootstrapperAgent.BootstrapperApplication.CachePackageBegin += BootstrapperApplication_CachePackageBegin;
            BootstrapperAgent.BootstrapperApplication.CacheVerifyBegin += BootstrapperApplication_CacheVerifyBegin;
            BootstrapperAgent.BootstrapperApplication.CacheAcquireProgress += BootstrapperApplication_CacheAcquireProgress;
            BootstrapperAgent.BootstrapperApplication.CacheAcquireComplete += BootstrapperApplication_CacheAcquireComplete;
            BootstrapperAgent.BootstrapperApplication.CacheVerifyComplete += BootstrapperApplication_CacheVerifyComplete;
            BootstrapperAgent.BootstrapperApplication.CachePackageComplete += BootstrapperApplication_CachePackageComplete;

            BootstrapperAgent.BootstrapperApplication.Progress += BootstrapperApplication_Progress;
            BootstrapperAgent.BootstrapperApplication.ExecutePackageBegin += BootstrapperApplication_ExecutePackageBegin;
            BootstrapperAgent.BootstrapperApplication.ExecuteProgress += BootstrapperApplication_ExecuteProgress;
            BootstrapperAgent.BootstrapperApplication.ExecutePackageComplete += BootstrapperApplication_ExecutePackageComplete;
            BootstrapperAgent.BootstrapperApplication.ExecuteMsiMessage += BootstrapperApplication_ExecuteMsiMessage;
#endif
        }

        public void OnLoaded()
        {
            //System.Diagnostics.Debugger.Launch();
        }

        //RegisterBegin之后运行
        private void BootstrapperApplication_CacheBegin(object sender, CacheBeginEventArgs e)
        {
            Cancel(e);
        }

        private void BootstrapperApplication_CachePackageBegin(object sender, CachePackageBeginEventArgs e)
        {
            Cancel(e);
        }

        private void BootstrapperApplication_CacheVerifyBegin(object sender, CacheVerifyBeginEventArgs e)
        {
            Cancel(e);
        }

        private void BootstrapperApplication_CacheAcquireProgress(object sender, CacheAcquireProgressEventArgs e)
        {
            lock (this)
            {
                _cacheProgress = e.OverallPercentage;
                Progress = (_cacheProgress + _executeProgress) / CommonTools.PhaseCount;

                e.Result = CommonTools.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void BootstrapperApplication_CacheAcquireComplete(object sender, CacheAcquireCompleteEventArgs e)
        {
            lock (this)
            {
                _cacheProgress = 100;
                Progress = (_cacheProgress + _executeProgress) / CommonTools.PhaseCount;

                e.Result = CommonTools.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void BootstrapperApplication_CacheVerifyComplete(object sender, CacheVerifyCompleteEventArgs e)
        {
            Cancel(e);
        }

        private void BootstrapperApplication_CachePackageComplete(object sender, CachePackageCompleteEventArgs e)
        {
            Cancel(e);
        }

        private void BootstrapperApplication_Progress(object sender, ProgressEventArgs e)
        {
            Cancel(e);
        }

        private void BootstrapperApplication_ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
        {
            lock (this)
            {
                BootstrapperAgent.LogMessage($"******ExecutePackageBegin---PackageId:{e.PackageId};ShouldExecute:{e.ShouldExecute};Result:{e.Result}*********");

                if (e.PackageId.Equals(CommonTools.ExePackageIdDotnetFramework48, StringComparison.Ordinal))
                {
                    Message = "Microsoft .NET Framework 4.8 Setup";
                }

                e.Result = CommonTools.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void BootstrapperApplication_ExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            lock (this)
            {
                if (App.GetInstallState() == InstallationState.Suspend)
                {
                    var upgradeCode = BootstrapperAgent.GetBurnVariable("UpgradeCode");
                    _cancelRequest = new System.Threading.Mutex(true, "WIXUI_CANCEL_REQUEST" + upgradeCode);
                    CommonTools.Suspend();
                }

                _executeProgress = e.OverallPercentage;
                Progress = (_cacheProgress + _executeProgress) / CommonTools.PhaseCount;

                if (BootstrapperAgent.BootstrapperApplication.Command.Display == Display.Embedded)
                {
                    BootstrapperAgent.BootstrapperApplication.Engine.SendEmbeddedProgress(e.ProgressPercentage, Progress);
                }

                e.Result = CommonTools.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void BootstrapperApplication_ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            lock (this)
            {
                if (e.MessageType == InstallMessage.ActionStart)
                {
                    string languageValue = ConfigurationManageHelper.ReadItem("Language");
                    if (Enum.TryParse(languageValue, out LanguageType languageType))
                    {
                        if (languageType == LanguageType.Chinese)
                            Message = e.Message.Split('。', '，')[1];
                        else if (languageType == LanguageType.English)
                            Message = e.Message.Split('.', ',')[1];
                    }
                }

                string extendMsg = string.Join("&", e.Data);
                BootstrapperAgent.LogMessage($"---------------Message:{e.Message};MessageType:{e.MessageType};ExtendMessage:{extendMsg}---------------");

                e.Result = CommonTools.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void BootstrapperApplication_ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
        {
            lock (this)
            {
                BootstrapperAgent.LogMessage($"******ExecutePackageComplete---PackageId:{e.PackageId};Restart:{e.Restart};Status:{e.Status};Result:{e.Result}*********");

                _cancelRequest?.Close();

                e.Result = CommonTools.Canceled ? Result.Cancel : Result.Ok;
            }
        }

        private void Cancel(ResultEventArgs e)
        {
            lock (this)
            {
                e.Result = CommonTools.Canceled ? Result.Cancel : Result.Ok;
            }
        }
    }
}