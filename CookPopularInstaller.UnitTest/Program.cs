/*
 * Description：Program 
 * Author： Chance.Zheng
 * Create Time: 2023/7/13 14:30:39
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2023 All Rights Reserved.
 */


//using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using CookPopularInstaller.Toolkit.Helpers;
using SevenZip;
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CookPopularInstaller.UnitTest
{
    public class Program
    {
        private const string InvalidPathPattern = @"([\<\>\:\\\\\/\?\*\|\""])";
        private static readonly string CmdPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\cmd.exe";

        static void Main(string[] args)
        {
            //var dd = FindProcessOccupyFileAsync1(@"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\Package\AFM相关接口.docx").Result;
            //using var sw1 = new StreamWriter(@"D:\Users\chance.zheng\Desktop\123.txt", true);
            //Array.ForEach(dd.ToArray(), arg =>
            //{
            //    sw1.WriteLine(arg.ProcessName);
            //});
            //sw1.Flush();

            var productCode = "{BB4CBD39-0032-4A42-9EBC-7E241243462F}";
            var key64Path = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productCode}";
            var key32Path = $"SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productCode}";
            var registry64Key = RegistryHelper.GetLocalMachineRegistryKey(key64Path);
            var registry32Key = RegistryHelper.GetLocalMachineRegistryKey(key32Path);
            var registryKey = registry64Key ?? registry32Key;
            registryKey.SetValue("InstallLocation", @"C:\Program Files (x86)\NCATest\CookPopularInstaller.Generate\1.0.0.1");
            string s = "";
            //ProcessHelper.StartProcess(@"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\Package\afterinstall.bat", null, true);
            //var ss = IsProductInstalled("{65A1DDBF-92FA-403B-9BC2-2B9869BE49A1}");

            //var runningProcesses = GetAllProcessAsync().Result;
            //Console.WriteLine(args.Length);
            //if (Environment.GetCommandLineArgs().Contains("/q", StringComparer.OrdinalIgnoreCase) &&
            //    Environment.GetCommandLineArgs().Contains("/i", StringComparer.OrdinalIgnoreCase) &&
            //    Environment.GetCommandLineArgs().Contains("/o", StringComparer.OrdinalIgnoreCase))
            //{
            //    foreach (var arg in args)
            //    {
            //        Console.WriteLine(arg);
            //    }
            //}

            //var sss1 = IsFileLocked(@"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\Package\K8860.chm");

            //using var fs = new FileStream(@"D:\Users\chance.zheng\Desktop\123123.txt", FileMode.Create);
            //using var sw = new StreamWriter(@"D:\Users\chance.zheng\Desktop\123123.txt", false);
            //sw.WriteLine("123");

            //CompressWithSevenZip();
            //UncompressWithSevenZip();

            //CompressWithUncompress.UncompressWithSharpZipLib();

            {
                //Thread thread = new Thread(new ThreadStart(() =>
                //{
                //    var processes = GetAllProcesses();
                //    foreach (Process pro in processes)
                //    {
                //        //Process.Start(new ProcessStartInfo(CmdPath, $"/c taskkill /f /pid {pro.Id}")
                //        //{
                //        //    CreateNoWindow = true,
                //        //    WindowStyle = ProcessWindowStyle.Hidden,
                //        //    Verb = "runas",
                //        //});
                //        ProcessHelper.StartProcessByCmd($"taskkill /f /pid {pro.Id}", true);
                //    }
                //}));
                //thread.Start();

                //string key = "{99842e2a-0904-4509-bf84-d087c1072234}";
                //RegistryHelper.CheckRegistryKeyPath(RegistryHive.LocalMachine, $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{key}");
                //RegistryHelper.CheckRegistryKeyPath(RegistryHive.LocalMachine, $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{key}");

                //Console.ReadKey();
            }
        }

        public static void FolderCheck()
        {
            var ss1 = Path.GetInvalidFileNameChars();
            var ss2 = Path.GetInvalidPathChars();


            var pp = "C:\\Program Files(x86)\\NCATest\\CookPopularInstaller.Generate\"";
            var folderName = Path.GetFileName(pp);
            var isValidPath = Path.GetInvalidPathChars().All(c => !pp.Contains(c));
            isValidPath &= !string.IsNullOrEmpty(folderName) && !System.Text.RegularExpressions.Regex.IsMatch(folderName, InvalidPathPattern);
        }

        public static IList<Process> GetAllProcesses()
        {
            var appNames = Directory.GetFiles(@"C:\Program Files (x86)\NCATest\ONES", "*.exe", SearchOption.AllDirectories).Select(f => Path.GetFileNameWithoutExtension(f)); ;
            List<Process> processlist = new List<Process>();
            appNames.ToList().ForEach(name =>
            {
                var processes = Process.GetProcessesByName(name);//.Where(p => p.MainModule.FileName.Contains(installFolder));
                if (processes != null && processes.Count() > 0)
                    processlist.AddRange(processes);
            });

            return processlist;
        }

        public static bool IsFileLocked(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return false;

                using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Close();
            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }

        public static void CheckFilesOccupy()
        {
            var processes = Process.GetProcesses().OrderBy(p => p.ProcessName).ToArray();
            IList<string> files = new List<string>();
            IList<Process> ps = new List<Process>();
            Array.ForEach(processes, p =>
            {
                try
                {
                    var s = p.MainModule.FileName;
                    files.Add(s);
                }
                catch (Exception)
                {
                    ps.Add(p);
                }
            });

            var sss1 = IsFileLocked(@"D:\Users\chance.zheng\Desktop\zhoubao.txt");
            var sss2 = IsFileLocked(@"D:\Users\chance.zheng\Desktop\自动化测试.xlsx");
            var allFilePath = Directory.GetFiles(@"D:\NCATEST\CookPopularInstaller.Generate", "*.*", SearchOption.AllDirectories);
            var count = allFilePath.Where(f => IsFileLocked(f) && Path.GetExtension(f) != ".exe");
            var allFileNames = allFilePath.Where(f => IsFileLocked(f));
            var allFileNames1 = allFilePath.Where(f => IsFileLocked(f)).Select(f => Path.GetFileNameWithoutExtension(f));
        }

        public static void FindProcessWithFile(string filePath)
        {
            Process process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@"D:\Handle\handle.exe", filePath + " /accepteula");
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string pattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
            foreach (Match match in Regex.Matches(output, pattern))
            {
                var p = Process.GetProcessById(int.Parse(match.Value));
            }
        }


        private static IEnumerable<string> GetExeNames()
        {
            var installFolder = @"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\Package";
            var appNames = Directory.GetFiles(installFolder, "*.exe", SearchOption.AllDirectories)
                                    //.Where(f => Path.GetFileName(f) != "Uninst.exe")
                                    .Select(f => Path.GetFileNameWithoutExtension(f));
            return appNames;
        }
        public static async Task<IList<Process>> FindProcessOccupyFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo(Path.Combine(@"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\Package", "handle.exe"), filePath + " /accepteula");
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.Verb = "runas";
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
                IList<Process> processes = new List<Process>();
                string output = process.StandardOutput.ReadToEnd();
                string pattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
                foreach (Match match in Regex.Matches(output, pattern))
                {
                    var p = Process.GetProcessById(int.Parse(match.Value));
                    processes.Add(p);
                }

                return processes;
            });
        }
        public static async Task<IList<Process>> FindAllProcessOccupyFilesExceptExeAsync()
        {
            var installFolder = @"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\Package";
            var allFiles = Directory.GetFiles(installFolder, "*.*", SearchOption.AllDirectories)
                                    .Where(f => !Path.GetFileName(f).Contains(".dll"))
                                    .Where(f => !Path.GetFileName(f).Contains(".exe"));
            var occupiedFiles = allFiles.Where(f => IsFileLocked(f));

            List<Process> processes = new List<Process>();
            if (occupiedFiles.Count() > 0)
            {
                foreach (var file in occupiedFiles)
                {
                    processes.AddRange(await FindProcessOccupyFileAsync(file));
                }
                return processes.Distinct().ToList();
            }

            return processes;
        }
        public static async Task<IList<Process>> GetAllProcessAsync()
        {
            List<Process> allProcess = new List<Process>();

            var exeNames = GetExeNames();
            exeNames.ToList().ForEach(name =>
            {
                var processes = Process.GetProcessesByName(name);//.Where(p => p.MainModule.FileName.Contains(installFolder));
                if (processes != null && processes.Count() > 0)
                    allProcess.AddRange(processes);
            });

            var fileProcess = await FindAllProcessOccupyFilesExceptExeAsync();
            allProcess.AddRange(fileProcess);

            return allProcess;
        }

        public static async Task<IList<Process>> FindProcessOccupyFileAsync1(string filePath)
        {
            return await Task.Run(() =>
            {
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo(Path.Combine(@"D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\Package", "handle.exe"), filePath + " /accepteula");
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.Verb = "runas";
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
                IList<Process> processes = new List<Process>();
                string output = process.StandardOutput.ReadToEnd();
                string pattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
                foreach (Match match in Regex.Matches(output, pattern))
                {
                    var p = Process.GetProcessById(int.Parse(match.Value));
                    processes.Add(p);
                }

                return processes;
            });
        }


        public static void RunAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);


            if (isAdmin)
            {
                MessageBox.Show("Administrator");
            }
            else
            {
                var process = Process.GetCurrentProcess();
                var file = process.MainModule.FileName;
                ProcessHelper.StartProcess(file, null, true);
                MessageBox.Show("No Administrator");
                process.Kill();
            }
        }


        //SevenZipSharp
        private static void CompressWithSevenZip()
        {
            //"lz", "tgz", "rar", "bz2", "gz", "xz" "zip", "7z", "tar",
            string[] suffixes = new string[] { "zip", "7z", "bz2", "lz", "gz", "tgz", "xz", "rar", "tar" };
            //string inputFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime";
            //string outputFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime_Ultra.7z";
            //string outputFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Compress\Level\";

            string inputFilePath = @"D:\Users\chance.zheng\Desktop\csc\cs";
            string outputFilePath = @"D:\Users\chance.zheng\Desktop\csc\123.7z";

            SevenZipBase.SetLibraryPath(@"C:\Users\chance.zheng\.nuget\packages\7z.libs\21.7.0\bin\x64\7z.dll");
            SevenZipCompressor compressor = new SevenZipCompressor
            {
                CompressionLevel = CompressionLevel.Ultra,
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionMode = CompressionMode.Create,
                CompressionMethod = CompressionMethod.Default,
            };

            compressor.FileCompressionStarted += (s, e) =>
            {
                //Console.WriteLine($"{e.PercentDone}%");
            };
            compressor.Compressing += (s, e) =>
            {
                //value += e.PercentDelta;
                //Console.WriteLine($"{value}%");
                Console.WriteLine($"{e.PercentDone}%");
            };
            compressor.CompressDirectory(inputFilePath, outputFilePath);
            //compressor.CompressFiles(inputFilePath, outputFilePath);

            //Array.ForEach(suffixes, suffix =>
            //{
            //    //if (s == "zip" || s == "lz")
            //    //    compressor.ArchiveFormat = OutArchiveFormat.Zip;
            //    //else if (s == "7z")
            //    //    compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
            //    //else if (s == "bz2")
            //    //    compressor.ArchiveFormat = OutArchiveFormat.Zip;
            //    //else if (s == "gz" || s == "tgz")
            //    //    compressor.ArchiveFormat = OutArchiveFormat.GZip;
            //    //else if (s == "xz")
            //    //    compressor.ArchiveFormat = OutArchiveFormat.XZ;
            //    //else if (s == "tar" || s == "rar")
            //    //    compressor.ArchiveFormat = OutArchiveFormat.Tar;


            //    //typeof(CompressionMethod).GetEnumNames().ToList().ForEach(method =>
            //    //{
            //    //    compressor.CompressionMethod = (CompressionMethod)Enum.Parse(typeof(CompressionMethod), method);

            //    //    if (!Directory.Exists(outputFilePath + suffix))
            //    //        Directory.CreateDirectory(outputFilePath + suffix);

            //    //    var outputFilePathBackup = outputFilePath + suffix + $"\\Runtime_{method}.{suffix}";
            //    //    //double value = 0;
            //    //    compressor.Compressing += (s, e) =>
            //    //    {
            //    //        //value += e.PercentDelta;
            //    //        //Console.WriteLine($"{value}%");
            //    //        Console.WriteLine($"{e.PercentDone}%");
            //    //    };
            //    //    compressor.CompressDirectory(inputFilePath, outputFilePathBackup);
            //    //});


            //    typeof(CompressionLevel).GetEnumNames().ToList().ForEach(level =>
            //    {
            //        compressor.CompressionLevel = (CompressionLevel)Enum.Parse(typeof(CompressionLevel), level);

            //        if (!Directory.Exists(outputFilePath))
            //            Directory.CreateDirectory(outputFilePath);

            //        var outputFilePathBackup = outputFilePath+ $"\\Runtime_{level}.{suffix}";
            //        //double value = 0;
            //        compressor.Compressing += (s, e) =>
            //        {
            //            //value += e.PercentDelta;
            //            //Console.WriteLine($"{value}%");
            //            Console.WriteLine($"{e.PercentDone}%");
            //        };
            //        compressor.CompressDirectory(inputFilePath, outputFilePathBackup);
            //    });
            //});
        }


        [DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string fileName);
        public static void SetLibraryPath(string libraryPath)
        {
            var fileP = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, libraryPath);
            var ss12 = File.Exists(fileP);
            var ss = LoadLibrary(fileP);
            var s1 = Path.GetFullPath(@"C:\Users\chance.zheng\.nuget\packages\7z.libs\21.7.0\bin\x64\7z.dll");
            var _libraryFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "7z64.dll" : "7z.dll");
            if (!Path.GetFullPath(libraryPath).Equals(Path.GetFullPath(_libraryFileName), StringComparison.OrdinalIgnoreCase))
            {
                throw new SevenZipLibraryException("can not change the library path while the library \"" + _libraryFileName + "\" is being used.");
            }

            if (!File.Exists(libraryPath))
            {
                throw new SevenZipLibraryException("can not change the library path because the file \"" + libraryPath + "\" does not exist.");
            }
        }

        private static void UncompressWithSevenZip()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string archiveFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime_Ultra.7z";
            string extractPath = @"D:\Users\chance.zheng\Desktop\dcs\Extract_Ultra";
            SevenZipBase.SetLibraryPath(@"C:\Users\chance.zheng\.nuget\packages\7z.libs\21.7.0\bin\x86\7z.dll");
            //if (Environment.Is64BitOperatingSystem)
            //    SevenZipBase.SetLibraryPath(@".\x64\7z.dll");
            //else
            //    SevenZipBase.SetLibraryPath(@".\x64\7z.dll");
            using (var extractor = new SevenZipExtractor(archiveFilePath))
            {
                extractor.Extracting += (s, e) =>
                {
                    Console.WriteLine($"{e.PercentDone}%");
                };
                extractor.ExtractArchive(extractPath);
            }

            stopwatch.Stop();
            Console.WriteLine("总耗时：" + stopwatch.Elapsed.Minutes + "分" + stopwatch.Elapsed.Seconds + "秒");
        }


        void UncompressWith(string archiveFileName, string archiveFilePath, string extractPath)
        {
            try
            {
                var archiveFileExtension = Path.GetExtension(archiveFilePath);
                if (archiveFileExtension == ".zip" || archiveFileExtension == ".7z" ||
                    archiveFileExtension == ".rar" || archiveFileExtension == ".gz" ||
                    archiveFileExtension == ".tgz" || archiveFileExtension == ".xz" ||
                    archiveFileExtension == ".tar" || archiveFileExtension == ".lz" || archiveFileExtension == ".bz2")
                {
                    //if (!Directory.Exists(extractPath))
                    //    Directory.CreateDirectory(extractPath);

                    //ReaderOptions options = new ReaderOptions();
                    //options.ArchiveEncoding.Default = Encoding.GetEncoding("utf-8");
                    //using var archive = ArchiveFactory.Open(archiveFilePath, options);
                    ////long totalUncompressSize = archive.TotalUncompressSize;
                    ////long num = 0L;
                    //HashSet<string> hashSet = new HashSet<string>();
                    //IReader reader = archive.ExtractAllEntries();
                    //while (reader.MoveToNextEntry())
                    //{
                    //    IEntry entry = reader.Entry;
                    //    if (!entry.IsDirectory)
                    //    {
                    //        string text = Path.Combine(extractPath, entry.Key);
                    //        string directoryName = Path.GetDirectoryName(text);
                    //        if (directoryName != null && hashSet.Add(text))
                    //        {
                    //            Directory.CreateDirectory(directoryName);
                    //        }

                    //        using (FileStream writableStream = File.OpenWrite(text))
                    //        {
                    //            reader.WriteEntryTo(writableStream);
                    //            //num += entry.Size;
                    //        }
                    //    }
                    //}

                    //ReaderOptions options = new ReaderOptions();
                    //options.ArchiveEncoding.Default = Encoding.GetEncoding("utf-8");
                    //IArchive archive = ArchiveFactory.Open(archiveFilePath, options);
                    ////MessageBox.Show(archiveFileName + ";" + archiveFilePath + ";" + extractPath);
                    //archive.ExtractToDirectory(extractPath, d =>
                    //{
                    //    //var process = Math.Round(100 * d, 2);
                    //    //session.Log($"{process}%");
                    //});
                    //session.Log($"{archiveFileName}解压完成");
                    //archive.Dispose();

                    //using FileStream fileStream = new FileStream(archiveFilePath, FileMode.Open, FileAccess.Read);
                    //using ZipInputStream zipInputStream = new ZipInputStream(fileStream);
                    //ZipEntry zipEntry = null;
                    //while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                    //{
                    //    string directoryPath = Path.Combine(extractPath, Path.GetDirectoryName(zipEntry.Name));
                    //    string fileName = Path.Combine(directoryPath, Path.GetFileName(zipEntry.Name));
                    //    if (directoryPath.Length > 0)
                    //    {
                    //        ////很慢
                    //        //ProcessHelper.CreateFolderRunAsAdmin(directoryPath);
                    //        Directory.CreateDirectory(directoryPath);
                    //    }

                    //    if (Directory.Exists(fileName))
                    //        continue;
                    //    if (!string.IsNullOrEmpty(fileName))
                    //    {
                    //        using FileStream streamWriter = File.Create(fileName);
                    //        int size = 4096;
                    //        byte[] buffer = new byte[4 * 1024];
                    //        while (true)
                    //        {
                    //            size = zipInputStream.Read(buffer, 0, buffer.Length);
                    //            if (size > 0)
                    //                streamWriter.Write(buffer, 0, size);
                    //            else
                    //                break;
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void UncompressDetail(string archiveFileName, string archiveFilePath, string extractPath)
        {
            try
            {
                if (!Directory.Exists(extractPath))
                    Directory.CreateDirectory(extractPath);

                IArchive archive = null;
                var archiveFileExtension = Path.GetExtension(archiveFilePath);
                switch (archiveFileExtension)
                {
                    case ".zip" or ".bz2" or ".lz":
                        archive = ZipArchive.Open(archiveFilePath);
                        break;
                    case ".7z":
                        MessageBox.Show(archiveFilePath);
                        archive = SevenZipArchive.Open(archiveFilePath);
                        break;
                    case ".rar":
                        archive = RarArchive.Open(archiveFilePath);
                        break;
                    case ".tar":
                        archive = TarArchive.Open(archiveFilePath);
                        break;
                    case ".gz" or ".tgz":
                        archive = GZipArchive.Open(archiveFilePath);
                        break;
                    default:
                        break;
                }
                //if (archive != null)
                //{
                //    archive.ExtractToDirectory(extractPath, d =>
                //    {
                //        var process = Math.Round(100 * d, 2);
                //        session.Log($"{process}%");
                //    });
                //    session.Log($"{archiveFileName}解压完成");
                //    archive.Dispose();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        ////SharpCompress
        //private static void Uncompress()
        //{
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    string seventZipFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime.7z";
        //    string seventZipExtractPath = @"D:\Users\chance.zheng\Desktop\dcs\7Zip\Runtime";

        //    UnCompress(seventZipFilePath, seventZipExtractPath);

        //    //using (SevenZipArchive archive = SevenZipArchive.Open(seventZipFilePath))
        //    //{
        //    //    archive.EntryExtractionBegin += (s, e) =>
        //    //    {

        //    //    };
        //    //    archive.EntryExtractionEnd += (s, e) =>
        //    //    {
        //    //        if (e.Item.IsComplete)
        //    //            archive.Dispose();
        //    //    };
        //    //    archive.ExtractToDirectory(seventZipExtractPath, d =>
        //    //    {
        //    //        var process = Math.Round(100 * d, 2);
        //    //        Console.WriteLine($"{process}%");
        //    //    });
        //    //}



        //    stopwatch.Stop();
        //    Console.WriteLine("总耗时：" + stopwatch.Elapsed.Minutes + "分" + stopwatch.Elapsed.Seconds + "秒");

        //    //string rarFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime.rar";
        //    //string rarExtractPath = @"D:\Users\chance.zheng\Desktop\dcs\Rar";

        //    //using (RarArchive archive = RarArchive.Open(rarFilePath))
        //    //{
        //    //    archive.ExtractToDirectory(rarExtractPath, d =>
        //    //    {
        //    //        var process = Math.Round(100 * d, 2);
        //    //        Console.WriteLine($"{process}%");
        //    //    });
        //    //}

        //    //string zipFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime.zip";
        //    //string zipExtractPath = @"D:\Users\chance.zheng\Desktop\dcs\Zip";
        //    //using (ZipArchive archive = ZipArchive.Open(zipFilePath))
        //    //{
        //    //    archive.ExtractToDirectory(zipExtractPath, d =>
        //    //    {
        //    //        var process = Math.Round(100 * d, 2);
        //    //        Console.WriteLine($"{process}%");
        //    //    });
        //    //}
        //}

        //private static void UnCompress(string archiveFilePath, string extractPath)
        //{
        //    if (!Directory.Exists(extractPath))
        //        Directory.CreateDirectory(extractPath);

        //    ReaderOptions options = new ReaderOptions();
        //    options.ArchiveEncoding.Default = Encoding.GetEncoding("utf-8");
        //    IArchive archive = ArchiveFactory.Open(archiveFilePath, options);

        //    archive.EntryExtractionBegin += (s, e) =>
        //    {

        //    };
        //    archive.EntryExtractionEnd += (s, e) =>
        //    {

        //    };
        //    archive.CompressedBytesRead += (s, e) =>
        //    {

        //    };
        //    archive.FilePartExtractionBegin += (s, e) =>
        //    {

        //    };
        //    archive.ExtractToDirectory(extractPath, d =>
        //    {
        //        var process = Math.Round(100 * d, 2);
        //        Console.WriteLine($"{process}%");
        //    });
        //}

        public static bool IsProductInstalled(string upgradeCode)
        {
            var key32Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using var key32 = RegistryHelper.GetLocalMachineRegistryKey(key32Path);
            foreach (string subkeyName in key32.GetSubKeyNames())
            {
                using (var subkey = key32.OpenSubKey(subkeyName))
                {
                    if (subkey.Name == @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{7f79f8c9-5630-4fd8-a3f3-dc1c4325bfe4}")
                    {

                    }
                    if (subkey != null && subkey.GetValue("BundleUpgradeCode")?.ToString() == upgradeCode)
                    {
                        return true;
                    }
                }
            }

            var key64Path = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            using var key64 = RegistryHelper.GetLocalMachineRegistryKey(key64Path);
            foreach (string subkeyName in key64.GetSubKeyNames())
            {
                using (var subkey = key64.OpenSubKey(subkeyName))
                {
                    if (subkey.Name == @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{7f79f8c9-5630-4fd8-a3f3-dc1c4325bfe4}")
                    {
                        var ss = (string[])subkey.GetValue("BundleUpgradeCode", null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                    }

                    if (subkey != null)
                    {
                        var value = subkey.GetValue("BundleUpgradeCode", null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                        if (value != null && ((string[])value)[0] == upgradeCode)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
