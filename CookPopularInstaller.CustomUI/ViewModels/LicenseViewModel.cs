/*
 * Description：LicenseViewModel
 * Author：chance
 * Organization: CookCSharp
 * Create Time：2022-05-06 11:06:32
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright (c) 2022 All Rights Reserved.
 */


using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using CookPopularInstaller.Toolkit.Helpers;
using Prism.Navigation.Regions;

namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class LicenseViewModel : ViewModelBase
    {
        public bool IsUnloaded { get; set; }

        public DelegateCommand<RichTextBox> LicenseLoadedCommand => new Lazy<DelegateCommand<RichTextBox>>(() => new DelegateCommand<RichTextBox>(LicenseLoadedAction)).Value;
        public DelegateCommand<string> AgreeLicenseCommand => new Lazy<DelegateCommand<string>>(() => new DelegateCommand<string>(AgreeLicenseAction)).Value;
        public DelegateCommand<string> DisagreeLicenseCommand => new Lazy<DelegateCommand<string>>(() => new DelegateCommand<string>(DisagreeLicenseAction)).Value;


        public LicenseViewModel()
        {
            IsUnloaded = false;
        }

        public void LicenseLoadedAction(RichTextBox rtb)
        {
            var licenseFileName = ConfigurationManageHelper.ReadItem("LicenseFileName");
            var streamResourceInfo = Application.GetResourceStream(new Uri($"..\\Assets\\Licenses\\{licenseFileName}", UriKind.Relative));
            rtb.Document.IsOptimalParagraphEnabled = true;
            (rtb.Document.Blocks.ElementAt(0) as Paragraph).LineHeight = 2;
            rtb.Selection.Load(streamResourceInfo.Stream, DataFormats.Text);
        }

        private void AgreeLicenseAction(string navigatePath)
        {
            NavigateToSourcePath(navigatePath, true);
        }

        private void DisagreeLicenseAction(string navigatePath)
        {
            NavigateToSourcePath(navigatePath, false);
        }

        private void NavigateToSourcePath(string navigatePath, bool isAgreeLicense)
        {
            IsUnloaded = true;

            if (!string.IsNullOrEmpty(navigatePath))
                RegionManager.RequestNavigate(RegionToken.MainWindowContentRegion, navigatePath);

            EventAggregator.GetEvent<AgreeLicenseEvent>().Publish(isAgreeLicense);
        }
    }
}
