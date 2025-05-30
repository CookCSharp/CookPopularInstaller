using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CookPopularInstaller.Admin
{
    internal class Program
    {
        private const int SizeOfRawDataAddress = 0x000002B8;
        private const int PointerToRawDataAddress = 0x000002BC;
        private const int NumberOfIdEntriesAddress = 0x6D20E; //资源文件数量
        private const string AppMainifest =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n" +
            "<assembly xmlns=\"urn:schemas-microsoft-com:asm.v1\" manifestVersion=\"1.0\">" +
            "<assemblyIdentity name=\"setup.exe\" version=\"1.0.0.0\" processorArchitecture=\"x86\" type=\"win32\"></assemblyIdentity>" +
            "<description>WiX Toolset Bootstrapper</description>" +
            "<dependency><dependentAssembly><assemblyIdentity type=\"win32\" name=\"Microsoft.Windows.Common-Controls\" version=\"6.0.0.0\" processorArchitecture=\"X86\" publicKeyToken=\"6595b64144ccf1df\" language=\"*\"></assemblyIdentity></dependentAssembly></dependency>" +
            "<trustInfo xmlns=\"urn:schemas-microsoft-com:asm.v3\"><security><requestedPrivileges><requestedExecutionLevel level=\"requireAdministrator\" uiAccess=\"false\"></requestedExecutionLevel></requestedPrivileges></security></trustInfo>" +
            "<compatibility xmlns=\"urn:schemas-microsoft-com:compatibility.v1\"><application><supportedOS Id=\"{e2011457-1546-43c5-a5fe-008deee3d3f0}\"></supportedOS><supportedOS Id=\"{35138b9a-5d96-4fbd-8e2d-a2440225f93a}\"></supportedOS><supportedOS Id=\"{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}\"></supportedOS><supportedOS Id=\"{1f676c76-80e1-4239-95bb-83d0f6d0da78}\"></supportedOS><supportedOS Id=\"{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}\"></supportedOS></application></compatibility>" +
            "</assembly>";
        private const string ManifestContent = """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0"><assemblyIdentity name="setup.exe" version="1.0.0.0" processorArchitecture="x86" type="win32"></assemblyIdentity><description>WiX Toolset Bootstrapper</description><dependency><dependentAssembly><assemblyIdentity type="win32" name="Microsoft.Windows.Common-Controls" version="6.0.0.0" processorArchitecture="X86" publicKeyToken="6595b64144ccf1df" language="*"></assemblyIdentity></dependentAssembly></dependency><trustInfo xmlns="urn:schemas-microsoft-com:asm.v3"><security><requestedPrivileges><requestedExecutionLevel level="requireAdministrator" uiAccess="false"></requestedExecutionLevel></requestedPrivileges></security></trustInfo><compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1"><application><supportedOS Id="{e2011457-1546-43c5-a5fe-008deee3d3f0}"></supportedOS><supportedOS Id="{35138b9a-5d96-4fbd-8e2d-a2440225f93a}"></supportedOS><supportedOS Id="{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}"></supportedOS><supportedOS Id="{1f676c76-80e1-4239-95bb-83d0f6d0da78}"></supportedOS><supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}"></supportedOS></application></compatibility></assembly>
            """;

        /// <summary>
        /// 清单文件起始标识
        /// </summary>
        private static readonly byte[] Flag = new byte[]
        {
            0x3C,0x3F,0x78,0x6D,0x6C,0x20,0x76,0x65,0x72,0x73,0x69,0x6F,0x6E,0x3D,0x22,0x31,
            0x2E,0x30,0x22,0x20,0x65,0x6E,0x63,0x6F,0x64,0x69,0x6E,0x67,0x3D,0x22,0x55,0x54,
            0x46,0x2D,0x38,0x22,0x20,0x73,0x74,0x61,0x6E,0x64,0x61,0x6C,0x6F,0x6E,0x65,0x3D,
            0x22,0x79,0x65,0x73,0x22,0x3F,0x3E,0x0D,0x0A,0x3C,0x61,0x73,0x73,0x65,0x6D,0x62,
            0x6C,0x79,0x20,0x78,0x6D,0x6C,0x6E,0x73,0x3D,0x22,0x75,0x72,0x6E,0x3A,0x73,0x63,
            0x68,0x65,0x6D,0x61,0x73,0x2D,0x6D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x2D,
            0x63,0x6F,0x6D,0x3A,0x61,0x73,0x6D,0x2E,0x76,0x31,0x22,0x20,0x6D,0x61,0x6E,0x69,
            0x66,0x65,0x73,0x74,0x56,0x65,0x72,0x73,0x69,0x6F,0x6E,0x3D,0x22,0x31,0x2E,0x30,
            0x22,0x3E
        };

        private static void ElevateAdministratorByModifyPE(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                using var bw = new BinaryWriter(fs, Encoding.UTF8, false);

                var buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);

                //小端模式
                var sizeToRawDataValue = BitConverter.ToInt32(buffer, SizeOfRawDataAddress);
                var pointerToRawDataValue = BitConverter.ToInt32(buffer, PointerToRawDataAddress);
                var index = FindStartIndex(buffer, pointerToRawDataValue, sizeToRawDataValue);

                var adminBuffer = Encoding.UTF8.GetBytes(AppMainifest);
                UpdateBuffer(buffer, adminBuffer, index);
                bw.Seek(0, SeekOrigin.Begin);
                bw.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            long FindStartIndex(byte[] buffer, long pointerToRawDataValue, long sizeToRawDataValue)
            {
                for (long i = 0; i < sizeToRawDataValue; i++)
                {
                    bool found = true;
                    for (int j = 0; j < Flag.Length; j++)
                    {
                        if (buffer[pointerToRawDataValue + i + j] != Flag[j])
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                        return pointerToRawDataValue + i;
                }

                return -1;
            }

            void UpdateBuffer(byte[] buffer, byte[] adminBuffer, long index)
            {
                for (int i = 0; i < adminBuffer.Length; i++)
                {
                    buffer[index + i] = adminBuffer[i];
                }

                //清单文件地址，由有无Bitmap判断该地址
                int AppMainifestSizeAddress = buffer[NumberOfIdEntriesAddress] > 5 ? 0x6D3B4 : 0x6D36C;
                //length(requireAdministrator)-length(asInvoker)=11，故0xD2->0xDD
                buffer[AppMainifestSizeAddress] = 0xDD;
            }
        }

        static void ElevateAdministratorByWrapper()
        {
            var name = "CookPopularInstaller_" + Process.GetCurrentProcess().GetHashCode();
            using var mutex = new Mutex(true, name, out bool isCreated);

            if (!isCreated)
            {
                System.Windows.MessageBox.Show("正在运行此实例时，无法启动安装程序的另一实例。");
                return;
            }

            var packageName = "CookPopularInstaller.exe";
            var path = Extract(packageName);
            var process = Process.Start(path);
            process.WaitForExit();

            string Extract(string name)
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream($"CookPopularInstaller.Admin.{name}");
                string tempPath = Path.Combine(Path.GetTempPath(), name);
                using var fs = new FileStream(tempPath, FileMode.Create);
                stream.CopyTo(fs);

                return tempPath;
            }
        }


        static void Main(string[] args)
        {
            //string filePath = "C:\\Users\\Chance\\Desktop\\CookCSharp\\CookPopularInstaller.Generate.v3.0.0.1.exe";
            //PELocator.ElevateAdministrator(filePath, ManifestContent);

            if (args.Length == 1)
            {
                string filePath = args[0];
                PELocator.ElevateAdministrator(filePath, ManifestContent);
            }
        }
    }
}