using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：ConverterAttribute 
 * Author： Chance.Zheng
 * Create Time: 2023/3/17 9:39:00
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.CustomUI
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class ConverterAttribute : Attribute
    {
        public ConverterAttribute()
        {
            
        }
    }
}
