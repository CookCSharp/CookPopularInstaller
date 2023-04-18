using CookPopularCSharpToolkit.Communal;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;


/*
 * Description：ConfuseModel 
 * Author： Chance.Zheng
 * Create Time: 2023/2/28 10:31:08
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate.Models
{
    public class ConfuseModel : BindableBase
    {
        public ObservableCollectionPlus<string> ConfuseDllNames { get; set; } = new ObservableCollectionPlus<string>();

        [JsonIgnore]
        public Brush ConfuseResultBrush { get; set; } = SystemColors.ControlLightBrush;

        [JsonIgnore]
        public bool IsConfusing { get; set; }

        [JsonIgnore]
        public FlowDocument LogDocument { get; set; } = new FlowDocument();
    }
}
