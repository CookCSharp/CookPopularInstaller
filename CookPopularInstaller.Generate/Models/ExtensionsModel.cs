using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：ExtensionsModel 
 * Author： Chance.Zheng
 * Create Time: 2023/3/7 13:44:16
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.Models
{
    public class EnvironmentVariable
    {
        public string Name { get; set; }
        public string Value { get; set; } = "[InstallFolder]\\";
    }

    public enum RegistryHiveType
    {
        HKCR,
        HKCU,
        HKLM,
        HKU,
        HKMU
    }

    public enum RegistryValueType
    {
        String,
        Integer,
        Binary,
        Expandable,
        MultiString
    }

    public class RegistryVariable
    {
        //[TypeConverter(typeof(StringConverter))]
        //public RegistryHive RegistryHive { get; set; }
        //public RegistryValueKind RegistryValueKind { get; set; }
        public RegistryHiveType RegistryHive { get; set; }
        public RegistryValueType RegistryValueKind { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class WindowsServiceVariable
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }

    public class ExtensionsModel : BindableBase
    {
        public ObservableCollection<EnvironmentVariable> EnvironmentVariables { get; set; } = new ObservableCollection<EnvironmentVariable>();
        public ObservableCollection<RegistryVariable> RegistryVariables { get; set; } = new ObservableCollection<RegistryVariable>();
        public ObservableCollection<WindowsServiceVariable> WindowsServices { get; set; } = new ObservableCollection<WindowsServiceVariable>();
    }
}
