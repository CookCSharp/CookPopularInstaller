using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：Depends 
 * Author： Chance.Zheng
 * Create Time: 2023/3/10 15:46:20
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.CustomUI
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
        public string InstallCommand { get; set; }

        /// <summary>
        /// 依赖项相对路径
        /// </summary>
        public string Value { get; set; }
    }

    public class Depends
    {
        public ObservableCollection<DependDialogVariable> DependDialogVariables { get; set; }
    }


    public class EnvironmentVariable
    {
        public string Name { get; set; }

        [Converter]
        public string Value { get; set; }
    }

    public class RegistryVariable
    {
        public RegistryHive RegistryHive { get; set; }

        public RegistryValueKind RegistryValueKind { get; set; }

        [Converter]
        public string Path { get; set; }

        public string Name { get; set; }

        [Converter]
        public string Value { get; set; }
    }

    public class WindowsServiceVariable
    {
        public string Name { get; set; }

        [Converter]
        public string Location { get; set; }

        public string Description { get; set; }
    }

    public class Extensions
    {
        public ObservableCollection<EnvironmentVariable> EnvironmentVariables { get; set; }
        public ObservableCollection<RegistryVariable> RegistryVariables { get; set; }
        public ObservableCollection<WindowsServiceVariable> WindowsServices { get; set; }
    }

    public class Package
    {
        public Depends Depends { get; set; } = new Depends();

        public Extensions Extensions { get; set; } = new Extensions();
    }
}
