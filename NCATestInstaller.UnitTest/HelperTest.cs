using Microsoft.Win32;
using NCATestInstaller.Toolkit.Helpers;
using System.IO;

namespace NCATestInstaller.UnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void JsonHelperTest()
        {
            var str = Path.GetDirectoryName(@"D:\Users\chance.zheng\Desktop\Company\NCATestInstaller\Output\bin\Release\x86\NCATestInstaller.CustomUI.Exe\NCATestInstaller.exe");
            var str1 = Directory.GetParent(@"D:\Users\chance.zheng\Desktop\Company\NCATestInstaller\Output\bin\Release\x86\NCATestInstaller.CustomUI.Exe\NCATestInstaller.exe").Name;
            //JsonHelper.ReadJsonValue("", "BeforeInstall", "Scripts");
            //Assert.Pass();
        }

        [Test]
        public void ProcessHelperTest()
        {
            //ProcessHelper.StartProcess(@"C:\ProgramData\Package Cache\{8d1104d3-a15f-4308-b0e6-708fb3764faf}\NCATestInstaller.exe", "/quiet /uninstall");

            ProcessHelper.StartProcess(Path.Combine(@"C:\Program Files (x86)\NCATest\NCATestInstaller.Generate", "Uninst.exe"), null);


            //RegistryHelper.CreateLocalMachineRegistryKey(@"SOFTWARE\NCATest\NCATestInstaller.Generate", "Test", "Chance");
            var ss1 = RegistryHelper.CheckRegistryKeyPath(RegistryHive.LocalMachine, @"SOFTWARE\NCATest\NCATestInstaller.Generate");
            var ss2 = RegistryHelper.CheckRegistryKeyPathValue(RegistryHive.LocalMachine, @"SOFTWARE\NCATest\NCATestInstaller.Generate", "Path");
            var ss3 = RegistryHelper.GetLocalMachineRegistryKeyPathValue(@"SOFTWARE\NCATest\NCATestInstaller.Generate", "Path");
            RegistryHelper.DeleteLocalMachineRegistryKeyPathValue(@"SOFTWARE\NCATest\NCATestInstaller.Generate", "Test");
        }

        [Test]
        public void RegistryHelperTest()
        {
            var ss = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey($"SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + "\"{fdb4f67e-0d8d-4855-96e8-77fa3a5d63e4}\"");
            //RegistryHelper.DeleteLocalMachineSubKeyTree("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{31D35E4B-43ED-47FE-99AF-D1940CE89F6D}");
        }
    }
}