/*
 * Description：PackageInfo
 * Author： Chance.Zheng
 * Create Time: 2023/10/18 9:50:09
 * .Net Version: 2.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2023 All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace NCATestInstaller.Toolkit
{
    public class PackageInfo : ObservableBase
    {
        public ProjectInfo Project { get; set; }

        public ConfuseInfo Confuse { get; set; }

        public DependInfo Depend { get; set; }

        public ExtensionInfo Extension { get; set; }

        public BeforeInstallInfo BeforeInstall { get; set; }

        public AfterInstallInfo AfterInstall { get; set; }
    }
}
