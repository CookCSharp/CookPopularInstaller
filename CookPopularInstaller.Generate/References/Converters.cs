using CookPopularCSharpToolkit.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;


/*
 * Description：Converters 
 * Author： Chance.Zheng
 * Create Time: 2023/2/21 16:38:06
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate
{
    [MarkupExtensionReturnType(typeof(double))]
    [ValueConversion(typeof(bool), typeof(double))]
    [Localizability(LocalizationCategory.NeverLocalize)]
    public class BooleanToPackageTypeHeaderWidthConverter : MarkupExtensionBase, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool b)
            {
                if (b)
                    return 120;
                else
                    return 170;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
