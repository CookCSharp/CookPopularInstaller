/*
 * Description：ConfuseInfo 
 * Author： Chance.Zheng
 * Create Time: 2023/10/18 11:07:02
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
    public class ConfuseInfo : ObservableBase
    {
        /// <summary>
        /// 是否混淆.NET库，默认采用Obfuscar混淆
        /// </summary>
        public bool IsConfuse { get; set; }

        public ObservableCollection<string> ConfuseDllNames { get; set; }
    }
}
