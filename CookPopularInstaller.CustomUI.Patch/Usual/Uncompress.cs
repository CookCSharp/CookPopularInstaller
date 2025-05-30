/*
 * Description：Uncompress 
 * Author： Chance.Zheng
 * Create Time: 2024/6/25 9:29:49
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2024 All Rights Reserved.
 */

using Microsoft.Win32;
using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CookPopularInstaller.CustomUI.Patch
{
    public delegate void Message(string msg);
    public delegate void Progress(byte progress);

    public class Uncompress
    {
        public static event Message OnMessage;
        public static event Progress OnProgress;

        public static void UncompressFiles(string sevenZDllPath, string extractPath, Embedded embedded)
        {
            DeleteFiles(extractPath, embedded);
            var diff7zFile = ExtractDiff7zFile(out long diff7ZFilelength);
            if (string.IsNullOrEmpty(diff7zFile)) return;

            if (embedded.DeleteFiles.Count == 0 && diff7ZFilelength == 0)
            {
                App.Over(Level.Info, "This is a unnecessary patch, because it has no different files compare to current product");
                return;
            }

            SevenZipBase.SetLibraryPath(sevenZDllPath);
            using var extractor = new SevenZipExtractor(diff7zFile);
            extractor.FileExtractionStarted += (s, e) =>
            {
                OnMessage.Invoke("Install：" + e.FileInfo.FileName);
            };
            extractor.Extracting += (s, e) =>
            {
                OnProgress.Invoke(e.PercentDone);
            };
            extractor.ExtractionFinished += (s, e) => App.Over(Level.Success, "Success");
            extractor.ExtractArchive(extractPath);
        }

        private static void DeleteFiles(string extractPath, Embedded embedded)
        {
            //var files = reader.ReadToEnd().Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            //                                     .Select(f => Path.Combine(extractPath, f))
            //                                     .ToList();
            var files = embedded.DeleteFiles.Select(f => Path.Combine(extractPath, f)).ToList();
            files.ForEach(f =>
            {
                OnMessage.Invoke("Delete：" + Path.GetFileName(f));
                if (File.Exists(f)) File.Delete(f);
            });
        }

        private static string ExtractDiff7zFile(out long length)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("CookPopularInstaller.CustomUI.Patch.Assets.diff.7z");
            if (stream == null) throw new Exception("Resource Assets\\diff.7z not found");
            length = stream.Length;

            string tempPath = Path.Combine(Path.GetTempPath(), "diff.7z");
            using var fs = new FileStream(tempPath, FileMode.Create);
            stream.CopyTo(fs);

            return tempPath;
        }


        public static void UpdateVersion(string productCode, string upgradeCode, string version)
        {
            //Msi
            if (!string.IsNullOrWhiteSpace(productCode))
            {
                var productCodeKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productCode}";
                var productCodeRegistryKey = GetRegistryKeyPath(RegistryHive.LocalMachine, productCodeKeyPath);
                if (productCodeRegistryKey != null)
                {
                    productCodeRegistryKey.SetValue("DisplayVersion", version);
                    productCodeRegistryKey.Close();
                }
            }

            //Exe
            if (!string.IsNullOrWhiteSpace(upgradeCode))
            {
                var uninstallKeyPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
                var uninstallKey64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(uninstallKeyPath, true);
                var uninstallKey32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(uninstallKeyPath, true);
                var bundleRegistryKey = Get(uninstallKey64);
                bundleRegistryKey ??= Get(uninstallKey32);

                if (bundleRegistryKey != null)
                {
                    bundleRegistryKey.SetValue("DisplayVersion", version);
                    bundleRegistryKey.Close();
                }

                RegistryKey Get(RegistryKey registryKey)
                {
                    foreach (var subKeyName in registryKey.GetSubKeyNames())
                    {
                        var subKey = registryKey.OpenSubKey(subKeyName, true);
                        var value = subKey.GetValue("BundleUpgradeCode") as string[];
                        if (value != null && value.FirstOrDefault()?.ToString() == upgradeCode)
                        {
                            return subKey;
                        }
                    }

                    return default;
                }
            }
        }

        private static RegistryKey GetRegistryKeyPath(RegistryHive registryHive, string keyPath)
        {
            var registryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64).OpenSubKey(keyPath, true);
            registryKey = registryKey ?? RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry32).OpenSubKey(keyPath, true);
            return registryKey;
        }
    }
}
