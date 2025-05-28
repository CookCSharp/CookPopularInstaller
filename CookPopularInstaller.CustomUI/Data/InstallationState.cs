/*
 * Description:InstallState
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/17 10:48:43
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © Chance 2021 All Rights Reserved
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookPopularInstaller.CustomUI
{
    /// <summary>
    /// 安装包状态
    /// </summary>
    public enum InstallationState
    {
        /// <summary>
        /// 初始化
        /// </summary>
        Initializing,
        /// <summary>
        /// Burn检测中
        /// </summary>
        Detecting,
        /// <summary>
        /// 未安装
        /// </summary>
        Absent,
        ///<summary>
        /// 已安装
        /// </summary>
        Present,
        /// <summary>
        /// 安装目录下正在运行的应用
        /// </summary>
        Running,
        /// <summary>
        /// 进行中
        /// </summary>
        Applying,
        /// <summary>
        /// 挂起
        /// </summary>
        Suspend,
        /// <summary>
        /// 取消
        /// </summary>
        Cancelled,
        /// <summary>
        /// 完成
        /// </summary>
        Applyed,
        /// <summary>
        /// 失败
        /// </summary>
        Failed,
    }
}
