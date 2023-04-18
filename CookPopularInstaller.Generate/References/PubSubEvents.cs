using CookPopularInstaller.Generate.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：PubSubEventsBase 
 * Author： Chance.Zheng
 * Create Time: 2023/2/22 16:40:58
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate
{
    public class PackageInfoEvent : PubSubEvent<ProjectModel>
    {

    }
}
