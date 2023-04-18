using CookPopularInstaller.Toolkit.Helpers;
using System;
using System.IO;
using System.Text;

namespace CookPopularInstaller.Generate.CommandLine
{
    internal class Program
    {
        private static string jsonFileName = string.Empty;

        static void Usage()
        {
            Console.WriteLine("example: CookPopularInstaller.Generate.CommandLine build package.json");
        }

        static void Main(string[] args)
        {
            //jsonFileName = "package.json";
            //Build();
            if (args.Length == 2)
            {
                if (args[0] == "build" && Path.GetExtension(args[1]) == ".json")
                {
                    jsonFileName = args[1];
                    Build();
                }
                else
                {
                    Usage();
                }
            }
            else
            {
                Usage();
            }
        }

        private static List<string> GetConfuseArguments(Package package)
        {
            List<string> commands = new List<string>();

            var path = EnvironmentHelper.GetEnvironmentVariable("Path").Split(';').Where(p => p.Contains("Python") && File.Exists(p + "Python.exe")).FirstOrDefault();
            var pythonExePath = path == null ? string.Empty : Path.Combine(path, "Python.exe");
            var updateXMLScriptPath = Environment.CurrentDirectory + "\\Obfuscar\\update_obfuscar_xml.py";
            var inPath = package.Project.PackageFolder;

            var command1 = $"\"{pythonExePath}\" \"{updateXMLScriptPath}\" \"{inPath}\" {jsonFileName}";
            var command2 = "Obfuscar\\Obfuscar.Console.exe Obfuscar\\Obfuscar.xml -s";

            commands.Add(command1);
            commands.Add(command2);

            return commands;
        }

        private static List<string> GetBuildArguments(Package package)
        {
            List<string> arguments = new List<string>();

            //1.首先使用heat.exe批量生成要打包的文件
            //2.然后使用candle.exe生成wixproj文件
            //3.再使用light.exe生成安装包
            var heatExePath = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\heat.exe";
            var candleExePath = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\candle.exe";
            var lightExePath = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\light.exe";
            var wixBalExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixBalExtension.dll";
            var wixUIExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixUIExtension.dll";
            var wixUtilExtension = EnvironmentHelper.GetEnvironmentVariable("WIX") + @"bin\WixUtilExtension.dll";
            var path = EnvironmentHelper.GetEnvironmentVariable("Path")?.Split(';').Where(p => p.Contains("Python") && File.Exists(p + "Python.exe")).FirstOrDefault();
            var pythonExePath = path == null ? string.Empty : Path.Combine(path, "Python.exe");
            var outputFileName_Msi = Path.Combine(package.Project.PackageOutputPath, $"{package.Project.PackageName}.v{package.Project.PackageVersion}.msi");
            var outputFileName_Exe = Path.Combine(package.Project.PackageOutputPath, $"{package.Project.PackageName}.v{package.Project.PackageVersion}.exe");

            //生成Msi安装包
            var wxiFilePath = Environment.CurrentDirectory + "\\Assets\\Msi\\Directory.wxi";
            var updateWxiScriptPath = Environment.CurrentDirectory + "\\Assets\\Msi\\update_directory_product.py";
            var updateZhcnScriptPath = Environment.CurrentDirectory + @$"\Assets\Public\update_zhcn.py";
            var wxsFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\Product.wxs";
            var wixprojFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\CookPopularInstaller.Msi.wixproj";
            var cultureFilePath_Msi = Environment.CurrentDirectory + "\\Assets\\Msi\\zh-cn.wxl";

            var csharpCustomActionTargetDir = Environment.CurrentDirectory + @"\\";
            var csharpCustomActionTargetFileName = "CookPopularInstaller.CSharpCustomAction";

            var argument1 = $"\"{heatExePath}\" dir \"{package.Project.PackageFolder}\" -out \"{wxiFilePath}\" -gg -gl -ke -sreg -scom -srd -platform x86 -template fragment -dr INSTALLFOLDER -cg DependencyLibrariesGroup -var var.DependencyLibrariesDir";
            var argument2 = $"\"{pythonExePath}\" \"{updateWxiScriptPath}\" {jsonFileName}";
            var argument3 = $"\"{pythonExePath}\" \"{updateZhcnScriptPath}\" \"{package.Project.PackageName}\" \"{package.Project.PackageName}\" \"{package.Project.AppLogo}\" \"{package.Project.AppFileName}\"";
            var argument4 = $"\"{candleExePath}\" \"{wxsFilePath_Msi}\" -dCookPopularInstaller.CSharpCustomAction.TargetDir=\"{csharpCustomActionTargetDir}\" -dCookPopularInstaller.CSharpCustomAction.TargetName={csharpCustomActionTargetFileName} -out \"{wixprojFilePath_Msi}\"";
            var argument5 = $"\"{lightExePath}\" -ext \"{wixBalExtension}\" -ext \"{wixUIExtension}\" -ext \"{wixUtilExtension}\" -cultures:zh-cn \"{wixprojFilePath_Msi}\" -loc \"{cultureFilePath_Msi}\" -spdb -out \"{outputFileName_Msi}\"";

            arguments.Add(argument1);
            arguments.Add(argument2);
            arguments.Add(argument3);
            arguments.Add(argument4);
            arguments.Add(argument5);

            //生成Exe安装包
            if (package.Project.PackageType.Contains("Exe"))
            {
                var wxsFilePath_Exe = Environment.CurrentDirectory + @$"\Assets\{package.Project.PackageType}\\Bundle.wxs";
                var wixprojFilePath_Exe = Environment.CurrentDirectory + @$"\Assets\{package.Project.PackageType}\CookPopularInstaller.{package.Project.PackageType}.wixproj";
                var cultureFilePath_Exe = Environment.CurrentDirectory + @$"\Assets\{package.Project.PackageType}\zh-cn.wxl";

                string argument6 = string.Empty;
                if (package.Project.PackageType == "Exe")
                {
                    argument6 = $"\"{candleExePath}\" \"{wxsFilePath_Exe}\" -ext \"{wixBalExtension}\" -dCookPopularInstaller.Msi.TargetPath=\"{outputFileName_Msi}\" -out \"{wixprojFilePath_Exe}\"";
                }
                else
                {
                    var customUITargetDir = Environment.CurrentDirectory + @"\\"; //此处结尾需要加双斜杠，否则candle.exe时会报错
                    var customUITargetFileName = "CookPopularInstaller.CustomUI.exe";
                    var customUITargetPath = Environment.CurrentDirectory + @$"\{customUITargetFileName}";
                    argument6 = $"\"{candleExePath}\" \"{wxsFilePath_Exe}\" -ext \"{wixBalExtension}\" -ext \"{wixUtilExtension}\" -dCookPopularInstaller.CustomUI.TargetDir=\"{customUITargetDir}\" -dCookPopularInstaller.CustomUI.TargetFileName=\"{customUITargetFileName}\" -dCookPopularInstaller.CustomUI.TargetPath=\"{customUITargetPath}\" -dCookPopularInstaller.Msi.TargetPath=\"{outputFileName_Msi}\" -out \"{wixprojFilePath_Exe}\"";
                }
                var argument7 = $"\"{lightExePath}\" -ext \"{wixBalExtension}\" -ext \"{wixUIExtension}\" -ext \"{wixUtilExtension}\" -cultures:zh-cn \"{wixprojFilePath_Exe}\" -loc \"{cultureFilePath_Exe}\" -spdb -out \"{outputFileName_Exe}\"";

                arguments.Add(argument6);
                arguments.Add(argument7);
            }

            return arguments;
        }

        private static bool Start(Package package, string operation)
        {
            Console.WriteLine($"start {operation.ToLower()} ...");

            int index = 0;
            bool hasError = false;
            IList<string> arguments;
            if (operation == "Confuse")
                arguments = GetConfuseArguments(package);
            else
                arguments = GetBuildArguments(package);

            do
            {
                ProcessHelper.StartCmd(arguments.ElementAt(index), out StringBuilder output, out bool isError);
                hasError |= isError;
                var outputStr = output.ToString().ToLower();
                hasError |= (outputStr.Contains("error") || outputStr.Contains("fatal"));
                Console.WriteLine(output.ToString());
                index++;
            } while (index < arguments.Count && !hasError);

            if (hasError)
                Console.WriteLine($"{operation} Failed!");
            else
                Console.WriteLine($"{operation} Successfully!");

            return hasError;
        }

        private static void CopyConfusedFilesToPackageFolder(Package package)
        {
            var confusedFiles = Microsoft.VisualBasic.FileIO.FileSystem.GetFiles(Environment.CurrentDirectory + "\\Obfuscar\\Confused", Microsoft.VisualBasic.FileIO.SearchOption.SearchAllSubDirectories, "*.dll");
            confusedFiles.ToList().ForEach(file =>
            {
                var fileName = Path.GetFileName(file);
                var destination = Path.Combine(package.Project.PackageFolder, fileName);
                Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(file, destination, true);
            });
        }

        private static void Build()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory + "..\\";
            var package = JsonHelper.JsonDeserializeFile<Package>(Environment.CurrentDirectory + $"\\{jsonFileName}");
            ConfigurationManageHelper.ModifyItem(Environment.CurrentDirectory + "\\CookPopularInstaller.CustomUI.exe", "PackageTheme", package.Project.PackageTheme.ToString(), "appSettings");
            if (package.Confuse.IsConfuse)
            {
                var confuseResult = Start(package, "Confuse");
                if (!confuseResult)
                {
                    CopyConfusedFilesToPackageFolder(package);
                    Start(package, "Build");
                }
            }
            else
            {
                Start(package, "Build");
            }
        }
    }
}