/*
 * Description：VariableType 
 * Author： Chance.Zheng
 * Create Time: 2023/7/5 15:04:51
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
    /// 安装包内置变量类型
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// 安装目录
        /// </summary>
        InstallFolder,
        /// <summary>
        /// 启动应用完整路径
        /// </summary>
        AppPath,
        /// <summary>
        /// 公司名称
        /// </summary>
        CompanyName,
        /// <summary>
        /// 产品名称
        /// </summary>
        ProductName,
        /// <summary>
        /// 产品版本号
        /// </summary>
        ProductVersion,
    }
}
