using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using CookPopularCSharpToolkit.Windows;


/*
 * Description:BooleanToHorizontalAlignment
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/24 14:29:01
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © CookCSharp 2018-2022 All Rights Reserved
 */
namespace CookPopularInstaller.CustomUI
{
    [MarkupExtensionReturnType(typeof(HorizontalAlignment))]
    [Localizability(LocalizationCategory.NeverLocalize)]
    public class BooleanToHorizontalAlignment : MarkupExtensionBase, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool bl)
            {
                return bl ? HorizontalAlignment.Center : HorizontalAlignment.Right;
            }

            return HorizontalAlignment.Center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
