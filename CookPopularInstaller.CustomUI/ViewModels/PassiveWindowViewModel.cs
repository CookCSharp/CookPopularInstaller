/*
 * Description: PassiveWindowViewModel 
 * Author: Chance.Zheng
 * Create Time: 2024/8/6 17:48:09
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */

using CookPopularToolkit.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class PassiveWindowViewModel : BindableBase
    {
        public BootstrapperApplicationAgent BootstrapperAgent => BootstrapperApplicationAgent.Instance;

#if ACTUALTEST
        public string Title => $"{BootstrapperAgent.ProductName}(v{BootstrapperAgent.GetBurnVariable("PackageVersion")})";
#else
        private static readonly string Version = FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\\CookPopularInstaller.CustomUI.exe").FileVersion;
        public string Title => BootstrapperAgent.ProductName + $"(v{Version})";
#endif

        public ImageSource WindowIcon
        {
            get
            {
#if !ACTUALTEST
                var drawingIcon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
                return drawingIcon.ToBitmap().ToImageSource();
#else
                var filePath = BootstrapperApplicationAgent.Instance.GetBurnVariable("WixBundleOriginalSource");
                if (!File.Exists(filePath)) filePath = CommonTools.GetBundleCacheFilePath();
                var drawingIcon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                return drawingIcon.ToBitmap().ToImageSource();
#endif
            }
        }

        public int Progress { get; set; }

        private int _cacheProgress;
        private int _executeProgress;


        public PassiveWindowViewModel()
        {
#if ACTUALTEST
            BootstrapperAgent.BootstrapperApplication.CacheAcquireProgress += BootstrapperApplication_CacheAcquireProgress;
            BootstrapperAgent.BootstrapperApplication.ExecuteProgress += BootstrapperApplication_ExecuteProgress;
#endif
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

        private void BootstrapperApplication_ExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            lock (this)
            {
                _executeProgress = e.OverallPercentage;
                Progress = (_cacheProgress + _executeProgress) / CommonTools.PhaseCount;

                if (BootstrapperAgent.BootstrapperApplication.Command.Display == Display.Embedded)
                {
                    BootstrapperAgent.BootstrapperApplication.Engine.SendEmbeddedProgress(e.ProgressPercentage, Progress);
                }

                e.Result = CommonTools.Canceled ? Result.Cancel : Result.Ok;
            }
        }
    }
}
