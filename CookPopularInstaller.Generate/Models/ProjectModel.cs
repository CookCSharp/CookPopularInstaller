using CookPopularInstaller.Generate.ViewModels;
using Newtonsoft.Json;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;


/*
 * Description：PackageInfo 
 * Author： Chance.Zheng
 * Create Time: 2023/2/22 16:43:05
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.Models
{
    public class ProjectModel : BindableBase
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
        /// 公司名称
        /// </summary>
        public string Company { get; set; }
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
        public string PackageFileExtension { get; set; } 
        /// <summary>
        /// 安装包类型
        /// </summary>
        public PackageType PackageType { get; set; }
        /// <summary>
        /// 安装包主题
        /// </summary>
        public PackageThemeType PackageTheme { get; set; }
        /// <summary>
        /// App名称
        /// </summary>
        public string AppFileName { get; set; }
        /// <summary>
        /// AppLogo
        /// </summary>
        public string AppLogo { get; set; }

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

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            App.Container.Resolve<IEventAggregator>().GetEvent<PackageInfoEvent>().Publish(this);
        }
    }
}
