/*
 * Description：ExtensionInfo 
 * Author： Chance.Zheng
 * Create Time: 2023/10/18 16:27:56
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
    public class ExtensionInfo : ObservableBase
    {
        public ObservableCollection<EnvironmentVariable> EnvironmentVariables { get; set; } = new ObservableCollection<EnvironmentVariable>();
        public ObservableCollection<RegistryVariable> RegistryVariables { get; set; } = new ObservableCollection<RegistryVariable>();
        public ObservableCollection<WindowsServiceVariable> WindowsServices { get; set; } = new ObservableCollection<WindowsServiceVariable>();
    }

    public class EnvironmentVariable : ObservableBase
    {
        public string Name { get; set; }

        [CanConverter]
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

    public class RegistryVariable : ObservableBase
    {
        //[TypeConverter(typeof(StringConverter))]
        //public RegistryHive RegistryHive { get; set; }
        //public RegistryValueKind RegistryValueKind { get; set; }
        public RegistryHiveType RegistryHive { get; set; }

        public RegistryValueType RegistryValueKind { get; set; }

        [CanConverter]
        public string Path { get; set; }

        public string Name { get; set; }

        [CanConverter]
        public string Value { get; set; }
    }

    public class WindowsServiceVariable : ObservableBase
    {
        public string Name { get; set; }

        [CanConverter]
        public string Location { get; set; }

        public string Description { get; set; }
    }
}
