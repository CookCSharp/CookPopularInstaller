using Microsoft.Win32;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace NCATestInstaller.CustomUI.Patch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class MainWindow : Window
    {
        private const string UpgradeCodeRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UpgradeCodes\{0}";
        private const string ProductCodeRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\{0}\InstallProperties";
        private const string InstallLocationRegistryKeyPath = @"SOFTWARE\{0}\{1}";
        private Embedded _embeddedContent;
        private string _productCode;
        private string _installLocation;


        public State State { get; set; }
        public string Explain { get; set; }
        public string ProductName { get; set; } = "Unknown";
        public string TargetDirectory { get; set; }
        public string Message { get; set; }
        public byte Progress { get; set; }
        public string Result { get; set; }
        public Geometry Geometry { get; set; }
        public Brush Brush { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            Uncompress.OnMessage += msg => Message = msg;
            Uncompress.OnProgress += p => Progress = p;
            App.OnResult += (s, r, g, b) =>
            {
                State = s;
                Result = r;
                Geometry = g;
                Brush = b;
            };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GetProductInfoByUpgradeCode();
            Title = string.Format("Install Patch For {0}", ProductName);
            Explain = string.Format("This is the patch package for {0}，please click install button to start", ProductName);
        }

        private void GetProductInfoByUpgradeCode()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("NCATestInstaller.CustomUI.Patch.Assets.content.json");
            if (stream == null)
            {
                App.Over(Level.Info, "Can not find product for this patch");
                return;
            }
            using var reader = new StreamReader(stream);
            _embeddedContent = JsonTool.JsonDeserialize<Embedded>(reader.ReadToEnd().Trim());

            var upgradeCode = _embeddedContent.UpgradeCode.Replace("{", "").Replace("}", "");
            var upgradeCodeArray = upgradeCode.Split('-');
            var first = string.Join("", upgradeCodeArray[0].Reverse());
            var second = string.Join("", upgradeCodeArray[1].Reverse());
            var third = string.Join("", upgradeCodeArray[2].Reverse());
            var fourth = Get(upgradeCodeArray[3]);
            var fifth = Get(upgradeCodeArray[4]);
            var registryUpgradeCode = string.Concat(first, second, third, fourth, fifth);

            _productCode = GetLocalMachineRegistryKeyPathValue(string.Format(UpgradeCodeRegistryKeyPath, registryUpgradeCode), "")?.ToString();
            if (string.IsNullOrWhiteSpace(_productCode))
            {
                App.Over(Level.Info, "Can not find product for this patch");
                return;
            }

            var productName = GetLocalMachineRegistryKeyPathValue(string.Format(ProductCodeRegistryKeyPath, _productCode), "DisplayName")?.ToString();
            if (string.IsNullOrWhiteSpace(productName))
            {
                App.Over(Level.Info, "Can not find product for this patch");
                return;
            }
            ProductName = productName;

            var productManufacturer = GetLocalMachineRegistryKeyPathValue(string.Format(ProductCodeRegistryKeyPath, _productCode), "Publisher");
            _installLocation = GetLocalMachineRegistryKeyPathValue(string.Format(InstallLocationRegistryKeyPath, productManufacturer, ProductName), "Path").ToString();

            string Get(string str)
            {
                var arr = str.ToArray();
                var tar = new char[arr.Length];
                for (int i = 0; i < arr.Length; i += 2)
                {
                    var temp = arr[i];
                    tar[i] = arr[i + 1];
                    tar[i + 1] = temp;
                }

                return string.Join("", tar);
            }
        }

        private object GetLocalMachineRegistryKeyPathValue(string keyPath, string name = null)
        {
            var registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(keyPath, false);
            registryKey ??= RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(keyPath, false);

            object value = default;

            if (registryKey != null)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    value = registryKey.GetValueNames().FirstOrDefault();
                }
                else
                {
                    value = registryKey.GetValue(name);
                }

                registryKey.Close();
            }

            return value;
        }

        private async void Install_Click(object sender, RoutedEventArgs e)
        {
            State = State.Installing;

            Uncompress.UncompressFiles(App.SevenZDllPath, _installLocation, _embeddedContent);
            await Task.Delay(500);

            Uncompress.UpdateVersion(_productCode, _embeddedContent.UpgradeCode, _embeddedContent.PatchVersion);
        }

        /// <summary>
        /// 查找所有进程(包含文件被占用的进程和运行的进程)
        /// </summary>
        /// <returns></returns>
        private async Task<IList<Process>> FindAllProcessesOccupyFilesExceptExeAsync()
        {
            return await Task.Run(async () =>
            {
                List<Process> processes = new List<Process>();
                var occupiedFiles = Directory.GetFiles(_installLocation, "*", SearchOption.AllDirectories).Where(f => !Path.GetFileName(f).Contains(".dll"))
                                                                                                          .Where(f => !Path.GetFileName(f).Contains(".exe"))
                                                                                                          .Where(f => IsFileLocked(f));
                var exeFiles = Directory.GetFiles(_installLocation, "*", SearchOption.AllDirectories).Where(f => Path.GetFileName(f).Contains(".exe"));
                if (occupiedFiles.Count() > 0)
                {
                    foreach (var file in occupiedFiles)
                    {
                        processes.AddRange(await FindProcessOccupyFileAsync(file));
                    }
                }

                if (exeFiles.Count() > 0)
                {
                    processes.AddRange(await FindProcessByRunningExAsync(exeFiles));
                }

                return processes.Distinct().ToList();
            });

            bool IsFileLocked(string path)
            {
                if (!File.Exists(path))
                    return false;

                bool isUse = true;
                FileStream fs = null;

                try
                {
                    fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    isUse = false;
                }
                catch (Exception)
                {
                }
                finally
                {
                    fs?.Close();
                }

                return isUse;
            }
        }

        private async Task<IList<Process>> FindProcessOccupyFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo(Path.Combine(_installLocation, "handle.exe"), $"\"{filePath}\"" + " /accepteula");
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

        private async Task<IList<Process>> FindProcessByRunningExAsync(IEnumerable<string> exeFiles)
        {
            return await Task.Run(() =>
            {
                IList<Process> processes = new List<Process>();
                foreach (var file in exeFiles)
                {
                    var processName = Path.GetFileName(file);
                    string query = $"SELECT * FROM Win32_Process WHERE Name = '{processName}'";
                    using var searcher = new System.Management.ManagementObjectSearcher(query);
                    using var collection = searcher.Get();
                    foreach (System.Management.ManagementObject @object in collection)
                    {
                        if (string.Equals(@object["Name"].ToString(), processName, StringComparison.OrdinalIgnoreCase))
                        {
                            var handle = @object["Handle"];
                            var processId = int.Parse(@object["processId"].ToString());
                            processes.Add(Process.GetProcessById(processId));
                        }
                    }
                }

                return processes;
            });
        }
    }
}
