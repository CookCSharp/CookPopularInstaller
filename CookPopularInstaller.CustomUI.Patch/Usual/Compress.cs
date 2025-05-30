/*
 * Description：Compress 
 * Author： Chance.Zheng
 * Create Time: 2024/6/24 14:34:39
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2024 All Rights Reserved.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CookPopularInstaller.CustomUI.Patch
{
    public class Compress
    {
        private static Embedded _embeddedContent;

        public static void CompressFiles(string name, string version, string oldDir, string newDir, string targetDir, string sevenZDllPath)
        {
            _embeddedContent = new Embedded();

            var files = CollectDiffFiles(name, version, oldDir, newDir);
            var targetFiles = files.Select(f => f.Replace(newDir, targetDir)).ToList();
            for (int i = 0; i < files.Count; i++)
            {
                var directory = Path.GetDirectoryName(targetFiles[i]);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.Copy(files[i], targetFiles[i], true);
            }

            SevenZipBase.SetLibraryPath(sevenZDllPath);
            SevenZipCompressor compressor = new SevenZipCompressor
            {
                CompressionLevel = CompressionLevel.Ultra,
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionMode = CompressionMode.Create,
                CompressionMethod = CompressionMethod.Default,
            };
            compressor.CompressDirectory(targetDir, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "diff.7z"));
        }

        private static IList<string> CollectDiffFiles(string name, string version, string oldDir, string newDir)
        {
            //python调用时会增加EnvironmentVariableTarget.Process的获取，导致获取diff.exe路径错误
            var gitPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine).Split(';').Where(p => Directory.Exists(p) && p.ToLower().Contains("\\git\\")).FirstOrDefault();
            var diffExePath = Path.Combine(Path.GetDirectoryName(gitPath), "usr\\bin\\diff.exe");

            var process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd", $"/c \"{diffExePath}\" -rq {oldDir} {newDir}");
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo = processStartInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.StandardOutput.Close();
            process.WaitForExit();
            process.Close();

            var files = new List<string>();
            var deleteFiles = new List<string>();
            string patternOnly = @"Only in (?<directory>.+): (?<filename>.+)";
            foreach (Match match in Regex.Matches(output, patternOnly))
            {
                string directory = match.Groups["directory"].Value;
                string filename = match.Groups["filename"].Value;
                if (directory.Contains(newDir))
                {
                    files.Add(Path.Combine(directory, filename));
                }
                else
                {
                    deleteFiles.Add(string.Concat(directory.Remove(0, oldDir.Length), "/", filename).Remove(0, 1));
                }
            }
            string patternFile = @"Files (?<filename1>.+) and (?<filename2>.+) differ";
            foreach (Match match in Regex.Matches(output, patternFile))
            {
                string filename = match.Groups["filename2"].Value;
                files.Add(filename);
            }

            var jsonStr = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\upgrade_code.json"));
            var data = (JObject)JsonConvert.DeserializeObject(jsonStr);
            var upgradeCode = data[name].ToString();

            _embeddedContent.DeleteFiles = deleteFiles;
            _embeddedContent.UpgradeCode = upgradeCode;
            _embeddedContent.PatchVersion = version;
            JsonTool.JsonSerializeFile(_embeddedContent, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "content.json"));

            return files;
        }
    }
}
