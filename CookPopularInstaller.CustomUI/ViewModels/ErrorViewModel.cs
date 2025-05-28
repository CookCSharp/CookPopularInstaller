/*
 * Description：ErrorViewModel 
 * Author： Chance.Zheng
 * Create Time: 2024/1/24 10:52:30
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class ErrorViewModel : ViewModelBase
    {
        public BitmapSource BackImage => App.GetBitmapSource(new Uri("..\\Assets\\Images\\cry.png", UriKind.Relative));

        public string ErrorMessage => string.IsNullOrWhiteSpace(CommonTools.GetMessage()) ? string.Format("{0}{1}", CommonTools.GetRelatedOperation(), App.Current.Resources["Failed"]) : CommonTools.GetMessage();
    }
}
