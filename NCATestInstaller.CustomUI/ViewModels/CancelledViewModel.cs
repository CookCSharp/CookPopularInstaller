using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Prism.Commands;
using Prism.Regions;



/*
 * Description:DetectInfoViewModel
 * Author: Chance.Zheng
 * Company: NCATest
 * CreateTime: 2022/8/24 15:06:07
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © NCATest 2018-2022 All Rights Reserved
 */
namespace NCATestInstaller.CustomUI.ViewModels
{
    public class CancelledViewModel : ViewModelBase
    {
        public string CancelledMessage => string.Format("{0}{1}", CommonTools.GetRelatedOperation(), App.Current.Resources["Cancelled"]);
    }
}
