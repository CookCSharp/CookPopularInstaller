/*
 * Description：DependInfo 
 * Author： Chance.Zheng
 * Create Time: 2023/10/18 16:23:39
 * .Net Version: 2.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2023 All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace NCATestInstaller.Toolkit
{
    public class DependInfo : ObservableBase
    {
        public ObservableCollection<DependDialogVariable> DependDialogVariables { get; set; } = new ObservableCollection<DependDialogVariable>();
    }

    public class DependDialogVariable : ObservableBase
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

    public enum CheckType
    {
        Enviorment,
        Registry,
        Folder,
        Other
    }
}
