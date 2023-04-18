using CookPopularCSharpToolkit.Communal;
using CookPopularCSharpToolkit.Windows;
using CookPopularInstaller.Toolkit.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;


/*
 * Description：DocumentExtensions 
 * Author： Chance.Zheng
 * Create Time: 2023/3/1 11:51:49
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Generate
{
    public static class DocumentExtensions
    {
        private static async Task AddLine(this FlowDocument document, string format, Brush foreground, params object[] args)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var paragraph = new Paragraph() { LineHeight = 1 };
                var level = format.Substring(format.IndexOf('['), format.IndexOf(']') + 1);
                var info = format.Replace(level, "");
                paragraph.Inlines.Add(new Run(level) { Foreground = foreground, FontWeight = FontWeights.DemiBold });
                paragraph.Inlines.Add(new Run(string.Format(info, args)) { Foreground = foreground });
                //paragraph.Inlines.Add(new LineBreak());
                document.Blocks.Add(paragraph);
            });
        }

        private static Task DebugLine(this FlowDocument document, params object[] args)
        {
            return AddLine(document, $"[DEBUG {DateTime.Now.ToLocalTime()}] " + "{0}", Brushes.DarkGray, args);
        }

        private static Task InfoLine(this FlowDocument document, params object[] args)
        {
            return AddLine(document, $"[INFO {DateTime.Now.ToLocalTime()}] " + "{0}", Brushes.Black, args);
        }

        private static Task WarnningLine(this FlowDocument document, params object[] args)
        {
            return AddLine(document, $"[WARN {DateTime.Now.ToLocalTime()}] " + "{0}", Brushes.DarkOrange, args);
        }

        private static Task ErrorLine(this FlowDocument document, params object[] args)
        {
            return AddLine(document, $"[ERRO {DateTime.Now.ToLocalTime()}] " + "{0}", Brushes.Red, args);
        }

        private static Task FatalLine(this FlowDocument document, params object[] args)
        {
            return AddLine(document, $"[FATAL {DateTime.Now.ToLocalTime()}] " + "{0}", Brushes.DarkRed, args);
        }

        public static Task CommandLine(this FlowDocument document, params object[] args)
        {
            return AddLine(document, $"[COMD {DateTime.Now.ToLocalTime()}] " + "{0}", Brushes.DarkSlateGray, args);
        }

        [Pure]
        public static async Task AppendLine(this FlowDocument document, StringBuilder arg)
        {
            var log = arg.ToString();
            if (log.IndexOf("fatal", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                await FatalLine(document, arg);
            }
            else if (log.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                await ErrorLine(document, arg);
            }
            else if (log.IndexOf("warning", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                await WarnningLine(document, arg);
            }
            else if (log.IndexOf("debug", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                await DebugLine(document, arg);
            }
            else if (log.IndexOf("info", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                await InfoLine(document, arg);
            }
            else
            {
                await InfoLine(document, arg);
            }
        }

        public static Task<bool> RunCommands(this FlowDocument document, IList<string> commands, string result = "Build")
        {
            return Task.Run(() =>
            {
                var hasError = false;
                //List<string>.ForEach执行异步Action时不会等待，我们可采用ForEach循环
                commands.ForEach(async command =>
                {
                    _ = document.CommandLine(command);
                    ProcessHelper.StartProcessByCmd(command, out StringBuilder output, out bool isError);
                    hasError |= isError;
                    var outputStr = output.ToString().ToLower();
                    hasError |= (outputStr.Contains("error") || outputStr.Contains("fatal"));
                    await document.AppendLine(output);
                });

                SynchronizationWithAsync.AppInvokeAsync(() =>
                {
                    var run = new Run()
                    {

                        FontWeight = FontWeights.DemiBold,
                    };
                    var paragraph = new Paragraph();
                    if (hasError)
                    {
                        run.Text = $"[INFO {DateTime.Now.ToLocalTime()}] {result} Failed!";
                        run.Foreground = Brushes.Red;
                    }
                    else
                    {
                        run.Text = $"[INFO {DateTime.Now.ToLocalTime()}] {result} Successful!";
                        run.Foreground = Brushes.DarkGreen;
                    }
                    paragraph.Inlines.Add(run);
                    document.Blocks.Add(paragraph);
                });

                return hasError;
            });
        }

        public static bool BuildResult(this FlowDocument document, string value = "error", params string[] args)
        {
            bool isError = false;
            TextRange textRange = new TextRange(document.ContentStart, document.ContentEnd);
            isError = textRange.Text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;

            args.ForEach(arg =>
            {
                isError |= textRange.Text.IndexOf(arg, StringComparison.OrdinalIgnoreCase) >= 0;
            });

            return isError;
        }

        public static bool BuildResult(this FlowDocument document, params string[] args)
        {
            return document.BuildResult("error", args);
        }

        public static Brush BuildResultBrush(this bool hasError)
        {
            return hasError ? Brushes.Red : Brushes.DarkGreen;
        }

        public static Brush BuildResultBrush(this FlowDocument document, params string[] args)
        {
            return document.BuildResult("error", args) ? Brushes.Red : Brushes.DarkGreen;
        }

        public static Brush BuildResultBrush(this FlowDocument document, string value = "error", params string[] args)
        {
            return document.BuildResult(value, args) ? Brushes.Red : Brushes.DarkGreen;
        }

        public static IList<int> FindIndexs(this string str, string value)
        {
            int index = -1;
            IList<int> indexs = new List<int>();
            while ((index = str.IndexOf(value, index + 1)) != -1)
            {
                indexs.Add(index);
            }

            return indexs;
        }
    }
}
