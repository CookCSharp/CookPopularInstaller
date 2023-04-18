using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;


/*
 * Description：BuildModel 
 * Author： Chance.Zheng
 * Create Time: 2023/2/27 9:59:13
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.Models
{
    public class BuildModel : BindableBase
    {
        public Brush BuildResultBrush { get; set; } = SystemColors.ControlLightBrush;

        public bool IsBuilding { get; set; }

        public FlowDocument LogDocument { get; set; }

        public bool IsShowCommand { get; set; } = true;
    }
}
