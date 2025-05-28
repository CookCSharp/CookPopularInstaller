/*
 * Description:Events
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/17 15:13:25
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © Chance 2021 All Rights Reserved
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace CookPopularInstaller.CustomUI
{
    public class AgreeLicenseEvent : PubSubEvent<bool>
    {

    }

    public class ProcessMessageEvent : PubSubEvent<string>
    {

    }
}
