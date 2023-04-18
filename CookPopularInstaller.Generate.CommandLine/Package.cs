using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：Package 
 * Author： Chance.Zheng
 * Create Time: 2023/3/3 10:49:11
 * .Net Version: 6.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.CommandLine
{
    public class Project
    {
        /// <summary>
        /// 打包文件目录
        /// </summary>
        public string PackageFolder { get; set; }
        /// <summary>
        /// 安装包输出目录
        /// </summary>
        public string PackageOutputPath { get; set; }
        /// <summary>
        /// 安装包名称
        /// </summary>
        public string PackageName { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// 安装包版本
        /// </summary>
        public string PackageVersion { get; set; }
        /// <summary>
        /// 安装包格式
        /// </summary>
        public string PackageFileExtension { get; set; } = ".exe";
        /// <summary>
        /// 安装包类型
        /// </summary>
        public string PackageType { get; set; } = "CustomUIExe";
        /// <summary>
        /// 安装包主题
        /// </summary>
        public string PackageTheme { get; set; } = "Default";
        /// <summary>
        /// App名称
        /// </summary>
        public string AppFileName { get; set; }
        /// <summary>
        /// AppLogo
        /// </summary>
        public string AppLogo { get; set; }
    }

    public class Confuse
    {
        /// <summary>
        /// 是否混淆.Net库
        /// </summary>
        public bool IsConfuse { get; set; }

        public List<string> ConfuseDllNames { get; set; }
    }

    public class Package
    {
        public Project Project { get; set; }

        public Confuse Confuse { get; set; }
    }
}
