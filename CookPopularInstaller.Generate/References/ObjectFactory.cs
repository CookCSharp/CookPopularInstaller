﻿/*
 * Description：ObjectFactory 
 * Author： Chance.Zheng
 * Create Time: 2023/2/21 16:09:07
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) Chance 2021 All Rights Reserved.
 */


using CookPopularToolkit;
using Prism.Ioc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CookPopularInstaller.Generate
{
    public class ObjectFactory
    {
        private static readonly ConcurrentDictionary<string, object> ViewDicts = new ConcurrentDictionary<string, object>();

        public static void Register<TType>(object instance) => Register(typeof(TType).Name, instance);

        public static void Register(string name, object instance) => ViewDicts.TryAdd(name, instance);

        public static void ResolveInstance<TType>() => ResolveInstance(typeof(TType).Name);

        public static object ResolveInstance(string key)
        {
            if (ViewDicts.TryGetValue(key, out object instance))
            {
                return instance;
            }

            return null;
        }

        public static object CreateInstanceInActivator(string className)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var namespaceStr = assembly.GetName().Name;
            var typeName = $"{namespaceStr}.Views.{className}";
            var instance = ObjectBuilder.CreateInstanceInActivator(typeName);

            return instance;
        }
    }
}
