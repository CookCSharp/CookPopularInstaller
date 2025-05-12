/*
 * Description：BooleanToVisibilityReConverter 
 * Author： Chance.Zheng
 * Create Time: 2024/6/24 15:53:12
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2024 All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NCATestInstaller.CustomUI.Patch
{
    public class StateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is State && parameter is State)
            {
                return ((State)value) == ((State)parameter) ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
