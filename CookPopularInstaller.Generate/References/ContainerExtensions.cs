﻿using System;
/*
 * Description：ContainerExtensions 
 * Author： Chance.Zheng
 * Create Time: 2023/2/23 11:29:59
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */


using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;

namespace CookPopularInstaller.Generate
{
    internal static class ContainerExtensions
    {
        internal static void Register<T>(this IUnityContainer container, params InjectionMember[] injectionMembers)
        {
            container.RegisterType<T>(typeof(T).Name, injectionMembers);
        }
    }
}
