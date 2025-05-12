/*
 * Description：ProductState 
 * Author： Chance.Zheng
 * Create Time: 2023/7/11 15:33:06
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2023 All Rights Reserved.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCATestInstaller.CustomUI
{
    /// <summary>
    /// 产品安装状态
    /// </summary>
    public enum ProductState
    {
        Advertised = 1,
        Absent = 2,
        Installed = 5,
    }
}
