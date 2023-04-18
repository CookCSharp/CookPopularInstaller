using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;



/*
 * Description:DetectInfoViewModel
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/24 15:06:07
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © CookCSharp 2018-2022 All Rights Reserved
 */
namespace CookPopularInstaller.CustomUI.ViewModels
{
    public class DetectInfoViewModel : ViewModelBase
    {
        public string DetectMessage => "您的计算机已经安装了该产品的其它版本，请先前往控制面板卸载该产品现有版本";
        public DelegateCommand SureCommand => new Lazy<DelegateCommand>(() => new DelegateCommand(OnSureAction)).Value;

        private void OnSureAction()
        {
            StandardBootstrapperApplication.InvokeShutdown();
        }
    }
}
