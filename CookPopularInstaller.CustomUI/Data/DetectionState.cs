/*
 * Description：DetectState 
 * Author： Chance.Zheng
 * Create Time: 2024/5/15 20:12:03
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookPopularInstaller.CustomUI
{
    /// <summary>
    /// 检测到的安装包状态
    /// </summary>
    public enum DetectionState
    {
        Absent,
        Present,
        /// <summary>
        /// 高版本
        /// </summary>
        Newer,
    }
}
