/*
 * Description：CompressWithUnCompress 
 * Author： Chance.Zheng
 * Create Time: 2023/11/24 15:32:38
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2023 All Rights Reserved.
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCATestInstaller.UnitTest
{
    public class CompressWithUncompress
    {
        //[CustomAction]
        //public static ActionResult InstallDependsCustomAction(Session session)
        //{
        //    session.Log("Begin Install Depends");

        //    try
        //    {
        //        string installFolder = session["INSTALLFOLDER"];
        //        string packageFile = Directory.GetFiles(installFolder, "package*.json").FirstOrDefault();
        //        if (packageFile == null) return ActionResult.NotExecuted;

        //        ExecuteScriptsBeforeInstallFinish(installFolder, packageFile);

        //        var depends = JsonHelper.ReadJsonValue(Path.Combine(installFolder, packageFile), "Depend", "DependDialogVariables");
        //        var variables = JToken.Parse(depends).ToArray();
        //        foreach (var variable in variables)
        //        {
        //            //session["INSTALLDEPENDSMESSAGE"] = $"正在安装{variable["Name"]}";
        //            string dependFileName = variable["Value"].ToString();
        //            string sourceFile = Path.Combine(installFolder, dependFileName);
        //            string targetPath = variable["CheckValue"].ToString().Replace("[InstallFolder]", installFolder);

        //            if (!File.Exists(sourceFile))
        //            {
        //                session.Log(string.Format("未能找到文件'{0}'", sourceFile));
        //                continue;
        //            }

        //            //暂时只支持Zip的压缩包
        //            if (dependFileName.EndsWith(".zip"))
        //            {
        //                using FileStream fileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
        //                using ZipInputStream zipInputStream = new ZipInputStream(fileStream);
        //                ZipEntry zipEntry = null;
        //                while ((zipEntry = zipInputStream.GetNextEntry()) != null)
        //                {
        //                    string directoryPath = Path.Combine(targetPath, Path.GetDirectoryName(zipEntry.Name));
        //                    string fileName = Path.Combine(directoryPath, Path.GetFileName(zipEntry.Name));
        //                    if (directoryPath.Length > 0)
        //                    {
        //                        ////很慢
        //                        //ProcessHelper.CreateFolderRunAsAdmin(directoryPath);
        //                        Directory.CreateDirectory(directoryPath);
        //                    }

        //                    if (Directory.Exists(fileName))
        //                        continue;
        //                    if (!string.IsNullOrEmpty(fileName))
        //                    {
        //                        using FileStream streamWriter = File.Create(fileName);
        //                        int size = 4096;
        //                        byte[] buffer = new byte[4 * 1024];
        //                        while (true)
        //                        {
        //                            size = zipInputStream.Read(buffer, 0, buffer.Length);
        //                            if (size > 0)
        //                                streamWriter.Write(buffer, 0, size);
        //                            else
        //                                break;
        //                        }
        //                    }
        //                }
        //            }
        //            //else if (dependFileName.EndsWith(".exe") || dependFileName.EndsWith(".msi"))
        //            //{
        //            //    ProcessHelper.StartProcess(sourceFile, variable["InstallCommand"].ToString(), true);
        //            //}

        //            //File.Delete(sourceFile);
        //        }

        //        ExecuteScriptsAfterInstallFinish(installFolder, packageFile);
        //    }
        //    catch (Exception)
        //    {

        //    }

        //    return ActionResult.Success;
        //}

        public static void CompressWithSharpZipLib()
        {

        }

        public static void UncompressWithSharpZipLib()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string archiveFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime_Ultra.zip";
            string extractPath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime_Ultra";

            using FileStream fileStreamIn = new FileStream(archiveFilePath, FileMode.Open, FileAccess.Read);
            using var zipInputStream = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(fileStreamIn);
            ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry = null;
            while ((zipEntry = zipInputStream.GetNextEntry()) != null)
            {
                string fileName = Path.Combine(extractPath, zipEntry.Name);
                string directoryPath = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                if (Directory.Exists(fileName))
                    continue;

                FileStream fileStreamOut = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                byte[] buffer = new byte[4096];
                int length;
                while (true)
                {
                    length = zipInputStream.Read(buffer, 0, buffer.Length);
                    if (length > 0)
                        fileStreamOut.Write(buffer, 0, length);
                    else
                        break;
                }
                fileStreamOut.Close();
                zipInputStream.CloseEntry();
            }
            zipInputStream.Close();

            stopwatch.Stop();
            Console.WriteLine("总耗时：" + stopwatch.Elapsed.Minutes + "分" + stopwatch.Elapsed.Seconds + "秒");
        }

        public static void UnCompressWithSharpCompress()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string archiveFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime_Ultra.7z";
            string extractPath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime_Ultra";

            SharpCompress.Readers.ReaderOptions options = new SharpCompress.Readers.ReaderOptions();
            options.ArchiveEncoding.Default = Encoding.GetEncoding("utf-8");
            var archive = SharpCompress.Archives.ArchiveFactory.Open(archiveFilePath, options);

            archive.EntryExtractionBegin += (s, e) =>
            {

            };
            archive.EntryExtractionEnd += (s, e) =>
            {

            };
            archive.CompressedBytesRead += (s, e) =>
            {

            };
            archive.FilePartExtractionBegin += (s, e) =>
            {

            };
            SharpCompress.Archives.IArchiveExtensions.ExtractToDirectory(archive, extractPath, d =>
            {
                 var process = Math.Round(100 * d, 2);
                 Console.WriteLine($"{process}%");
            });

            //2m16s
            stopwatch.Stop();
            Console.WriteLine("总耗时：" + stopwatch.Elapsed.Minutes + "分" + stopwatch.Elapsed.Seconds + "秒");
        }

        public static void UncompressWithSevenZip()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string archiveFilePath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime_Ultra.7z";
            string extractPath = @"D:\Users\chance.zheng\Desktop\dcs\Runtime_Ultra";

            if (Environment.Is64BitOperatingSystem)
                SevenZip.SevenZipBase.SetLibraryPath(@"x64\7z.dll");
            else
                SevenZip.SevenZipBase.SetLibraryPath(@"x86\7z.dll");

            using (var extractor = new SevenZip.SevenZipExtractor(archiveFilePath))
            {
                extractor.Extracting += (s, e) =>
                {
                    Console.WriteLine($"{e.PercentDone}%");
                };
                extractor.ExtractArchive(extractPath);
            }

            //40s
            stopwatch.Stop();
            Console.WriteLine("总耗时：" + stopwatch.Elapsed.Minutes + "分" + stopwatch.Elapsed.Seconds + "秒");
        }
    }
}
