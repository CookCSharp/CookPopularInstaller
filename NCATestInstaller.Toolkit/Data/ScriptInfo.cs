/*
 * Description：ScriptInfo 
 * Author： Chance.Zheng
 * Create Time: 2023/10/18 16:29:30
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
    public class ScriptInfo : ObservableBase
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class BeforeInstallInfo : ObservableBase
    {
        public ObservableCollection<ScriptInfo> Scripts { get; set; } = new ObservableCollection<ScriptInfo>();
    }

    public class AfterInstallInfo : ObservableBase
    {
        public ObservableCollection<ScriptInfo> Scripts { get; set; } = new ObservableCollection<ScriptInfo>();
    }
}
