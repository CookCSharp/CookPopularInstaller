using NCATestInstaller.Toolkit.Helpers;
using NCATestInstaller.Toolkit;
using System;
using System.IO;
using System.Text;

namespace NCATestInstaller.Generate.CommandLine
{
    internal class Program
    {
        private static string packageJsonFileName = string.Empty;

        static void Usage()
        {
            Console.WriteLine("example: NCATestInstaller.Generate.CommandLine build package.json");
        }

        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                if (args[0] == "build" && Path.GetExtension(args[1]) == ".json")
                {
                    packageJsonFileName = args[1];
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

        private static bool Start(PackageInfo package, string operation)
        {
            Console.WriteLine($"start {operation.ToLower()} ...");

            int index = 0;
            bool hasError = false;
            IList<string> arguments;
            if (operation == "Confuse")
                arguments = Toolkit.Build.GetConfuseArguments(package.Project.PackageFolder, packageJsonFileName);
            else
                arguments = Toolkit.Build.GetBuildArguments(package, packageJsonFileName);

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

        private static void Build()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory + "..\\";
            Toolkit.Build.Configure(packageJsonFileName);
            var package = Toolkit.Build.ReadPackageJsonFile(packageJsonFileName);

            if (package.Confuse.IsConfuse)
            {
                var confuseResult = Start(package, "Confuse");
                if (!confuseResult)
                {
                    Toolkit.Build.CopyConfusedFilesToPackageFolder(package.Project.PackageFolder);
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