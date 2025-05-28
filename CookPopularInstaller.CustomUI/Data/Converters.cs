using System;
using System.Collections.Generic;
using System.Globalization;
/*
 * Description:Converters
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/24 14:29:01
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © Chance 2021 All Rights Reserved
 */


using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using CookPopularToolkit.Windows;

namespace CookPopularInstaller.CustomUI
{
    [MarkupExtensionReturnType(typeof(HorizontalAlignment))]
    [Localizability(LocalizationCategory.NeverLocalize)]
    public class BooleanToHorizontalAlignmentConverter : MarkupExtensionBase, IValueConverter
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

    [MarkupExtensionReturnType(typeof(WindowState))]
    [Localizability(LocalizationCategory.NeverLocalize)]
    public class BooleanToWindowStateConverter : MarkupExtensionBase, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bl)
            {
                return bl ? WindowState.Minimized : WindowState.Normal;
            }

            return WindowState.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
