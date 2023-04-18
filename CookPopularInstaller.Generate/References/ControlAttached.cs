using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;



/*
 * Description:ControlAttached
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/25 20:05:50
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © CookCSharp 2018-2022 All Rights Reserved
 */
namespace CookPopularInstaller.Generate
{
    public class ControlAttached
    {
        public static FlowDocument GetRTBDocument(DependencyObject obj)
        {
            return (FlowDocument)obj.GetValue(RTBDocumentProperty);
        }
        public static void SetRTBDocument(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(RTBDocumentProperty, value);
        }
        public static readonly DependencyProperty RTBDocumentProperty =
            DependencyProperty.RegisterAttached("RTBDocument", typeof(FlowDocument), typeof(ControlAttached), new FrameworkPropertyMetadata(null, OnRTBDocumentChanged));

        private static void OnRTBDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is RichTextBox rtb)
            {
                if (args.NewValue != null)
                {
                    rtb.Document = (FlowDocument)args.NewValue;
                    rtb.TextChanged += (s, e) => rtb?.ScrollToEnd();
                }
                else
                    rtb.Document = new FlowDocument();
            }
        }
    }
}
