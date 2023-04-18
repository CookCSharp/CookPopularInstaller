using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：DependsModel 
 * Author： Chance.Zheng
 * Create Time: 2023/3/9 18:53:47
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.Models
{
    public enum CheckType
    {
        Enviorment,
        Registry,
        Folder,
        Other
    }

    public class DependDialogVariable
    {
        /// <summary>
        /// 校验类型
        /// </summary>
        public CheckType CheckType { get; set; }

        /// <summary>
        /// 校验值
        /// </summary>
        public string CheckValue { get; set; }

        /// <summary>
        /// 依赖项名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 依赖项安装命令
        /// </summary>
        public string InstallCommand { get; set; } = "/quiet /norestart";

        /// <summary>
        /// 依赖项相对路径
        /// </summary>
        public string Value { get; set; }
    }

    public class DependsModel : BindableBase
    {
        public ObservableCollection<DependDialogVariable> DependDialogVariables { get; set; } = new ObservableCollection<DependDialogVariable>();
    }
}
