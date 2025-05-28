/*
 * Description：ProjectInfo
 * Author： Chance.Zheng
 * Create Time: 2023/10/18 10:20:56
 * .Net Version: 2.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2023 All Rights Reserved.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace CookPopularInstaller.Toolkit
{
    public class ProjectInfo : ObservableBase
    {
        /// <summary>
        /// App名称
        /// </summary>
        public string AppFileName { get; set; }
        /// <summary>
        /// AppLogo
        /// </summary>
        public string AppLogo { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// 安装路径是否可以包含中文
        /// </summary>
        public string ContainChineseOnInstallFolder { get; set; }
        /// <summary>
        /// 是否生成PDB文件
        /// </summary>
        public bool IsOutputPdb { get; set; }
        /// <summary>
        /// 语言
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LanguageType Language { get; set; }
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
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType PackageType { get; set; } = PackageType.CustomUIExe;
        /// <summary>
        /// 安装包主题
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageThemeType PackageTheme { get; set; }
        /// <summary>
        /// 安装包用户协议名称
        /// </summary>
        public string PackageLicenseName { get; set; }
        /// <summary>
        /// 安装包架构(x64,x86)
        /// </summary>
        public string PackagePlatform { get; set; }

        [JsonIgnore]
        public ObservableCollection<string> SubDirectories { get; set; }

        private void OnPackageTypeChanged()
        {
            switch (PackageType)
            {
                case PackageType.Msi:
                    PackageFileExtension = ".msi";
                    break;
                case PackageType.CustomUIMsi:
                    PackageFileExtension = ".msi";
                    break;
                case PackageType.Exe:
                    PackageFileExtension = ".exe";
                    break;
                case PackageType.CustomUIExe:
                    PackageFileExtension = ".exe";
                    break;
                default:
                    break;
            }
        }
    }



    public enum PackageType
    {
        Msi,
        CustomUIMsi,
        Exe,
        CustomUIExe
    }

    public enum PackageThemeType
    {
        Default,
        Light,
        Dark,
    }

    public enum LanguageType
    {
        English,
        Chinese
    }
}
