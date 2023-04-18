using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Deployment.WindowsInstaller;
using CookPopularInstaller.Toolkit.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


/*
 * Description：CustomAction 
 * Author： Chance.Zheng
 * Create Time: 2023/3/28 20:19:16
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.CSharpCustomAction
{
    public class CustomAction
    {
        [CustomAction]
        public static ActionResult SetInstallMessage(Session session)
        {
            session["INSTALLDEPENDSMESSAGE"] = $"正在安装依赖环境";

            return ActionResult.Success;
        }


        [CustomAction]
        public static ActionResult DecompressCustomAction(Session session)
        {
            session.Log("Begin Decompress");
            //MessageBox.Show(session["INSTALLDEPENDSMESSAGE"]);

            string installFolder = session["INSTALLFOLDER"];
            string packageFile = Directory.GetFiles(installFolder, "package*.json").FirstOrDefault();
            if (packageFile == null) return ActionResult.NotExecuted;
            var depends = JsonHelper.ReadJsonValue(Path.Combine(installFolder, packageFile), "Depends", "DependDialogVariables");
            var variables = JToken.Parse(depends).ToArray();
            foreach (var variable in variables)
            {
                string zipFileName = variable["Value"].ToString();
                if (!zipFileName.EndsWith(".zip"))
                    continue;

                //session["INSTALLDEPENDSMESSAGE"] = $"正在安装{variable["Name"]}";
                string sourceFile = Path.Combine(installFolder, zipFileName);
                string targetPath = variable["CheckValue"].ToString().Replace("[InstallFolder]", installFolder);

                if (!File.Exists(sourceFile))
                {
                    session.Log(string.Format("未能找到文件'{0}'", sourceFile));
                    continue;
                }

                using FileStream fileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
                using ZipInputStream zipInputStream = new ZipInputStream(fileStream);
                ZipEntry zipEntry = null;
                while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                {
                    string directoryName = Path.Combine(targetPath, Path.GetDirectoryName(zipEntry.Name));
                    string fileName = Path.Combine(directoryName, Path.GetFileName(zipEntry.Name));
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);
                    if (Directory.Exists(fileName))
                        continue;
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        using FileStream streamWriter = File.Create(fileName);
                        int size = 4096;
                        byte[] buffer = new byte[4 * 1024];
                        while (true)
                        {
                            size = zipInputStream.Read(buffer, 0, buffer.Length);
                            if (size > 0)
                                streamWriter.Write(buffer, 0, size);
                            else
                                break;
                        }
                    }
                }

                File.Delete(sourceFile);
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult DeferredCustomAction(Session session)
        {
            CustomActionData customActionData = session.CustomActionData;
            string installFolder = customActionData["INSTALLFOLDER"];

            return ActionResult.Success;
        }
    }
}
