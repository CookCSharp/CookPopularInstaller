using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：PackageModel 
 * Author： Chance.Zheng
 * Create Time: 2023/3/24 16:48:27
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.Models
{
    public class PackageModel : BindableBase
    {
        public ProjectModel Project { get; set; } = new ProjectModel();

        public ConfuseModel Confuse { get; set; } = new ConfuseModel();

        public DependsModel Depends { get; set; } = new DependsModel();

        public ExtensionsModel Extensions { get; set; } = new ExtensionsModel();
    }
}
