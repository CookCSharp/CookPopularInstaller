using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：RegistryHelper 
 * Author： Chance.Zheng
 * Create Time: 2023/3/16 14:21:58
 * .Net Version: 6.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Toolkit.Helpers
{
    public static class RegistryHelper
    {
        public static RegistrySecurity GetRegistrySecurity()
        {
            string user = Environment.UserDomainName + "\\" + Environment.UserName;

            // Create a security object that grants no access.
            RegistrySecurity registrySecurity = new RegistrySecurity();

            // Add a rule that grants the current user ReadKey
            // rights. ReadKey is a combination of four other 
            // rights. The rule is inherited by all 
            // contained subkeys.
            RegistryAccessRule rule = new RegistryAccessRule(user,
                                                             RegistryRights.WriteKey | RegistryRights.ReadKey,
                                                             InheritanceFlags.None,
                                                             PropagationFlags.None,
                                                             AccessControlType.Allow);
            registrySecurity.AddAccessRule(rule);

            return registrySecurity;
        }

        private static bool TryGetRegistryKey(RegistryHive registryHive, string keyPath, bool writable, out RegistryKey registryKey)
        {
            registryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry32).OpenSubKey(keyPath, writable);
            if (registryKey == null)
                registryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64).OpenSubKey(keyPath, writable);

            if (registryKey == null)
            {
                return false;
                //throw new KeyNotFoundException("can not find the key path from registry");
            }

            return true;
        }

        public static RegistryKey CreateRegistryKey(RegistryHive registryHive, RegistryValueKind registryValueKind, string keyPath, string name, object value)
        {
            var registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            RegistryKey defaultRegistryKey = RegistryKey.OpenBaseKey(registryHive, registryView);
            RegistryKey registryKey = string.IsNullOrEmpty(keyPath) ? defaultRegistryKey : defaultRegistryKey.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
            if (registryKey == null)
            {
                //HKEY_LOCAL_MACHINE，HKEY_USERS不允许创建子项
                registryKey = defaultRegistryKey.CreateSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
            }

            registryKey.SetValue(name, value ?? string.Empty, registryValueKind);
            return registryKey;
        }

        public static RegistryKey CreateLocalMachineRegistryKey(string keyPath, string name, object value)
        {
            return CreateRegistryKey(RegistryHive.LocalMachine, RegistryValueKind.String, keyPath, name, value);
        }

        public static RegistryKey CreateClassesRootRegistryKey(string keyPath, string name, object value)
        {
            return CreateRegistryKey(RegistryHive.ClassesRoot, RegistryValueKind.String, keyPath, name, value);
        }

        public static bool CheckRegistryKeyPath(RegistryHive registryHive, string keyPath)
        {
            return TryGetRegistryKey(registryHive, keyPath, false, out var registryKey);
        }

        public static bool CheckRegistryKeyPathValue(RegistryHive registryHive, string keyPath, string name)
        {
            if (!TryGetRegistryKey(registryHive, keyPath, false, out var registryKey))
                return false;

            try
            {
                registryKey.GetValue(name);
            }
            catch (Exception)
            {

                return false;
            }

            return true;
        }

        public static object GetRegistryKeyPathValue(RegistryHive registryHive, string keyPath, string name)
        {
            if (TryGetRegistryKey(registryHive, keyPath, false, out var registryKey))
                return registryKey.GetValue(name);
            return null;
        }

        public static RegistryKey GetLocalMachineRegistryKey(string keyPath)
        {
            return GetRegistryKeyPath(RegistryHive.LocalMachine, keyPath);
        }

        public static RegistryKey GetRegistryKeyPath(RegistryHive registryHive, string keyPath)
        {
            var registryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64).OpenSubKey(keyPath, true);
            registryKey = registryKey ?? RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry32).OpenSubKey(keyPath, true);
            return registryKey;
        }

        public static object GetLocalMachineRegistryKeyPathValue(string keyPath, string name)
        {
            return GetRegistryKeyPathValue(RegistryHive.LocalMachine, keyPath, name);
        }

        public static void DeleteSubKeyTree(RegistryHive registryHive, string subkey)
        {
            RegistryKey registryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry32);
            if (registryKey == null)
            {
                registryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64);
                if (registryKey == null)
                    throw new KeyNotFoundException("not find the subkey key from registry");
            }

            registryKey.DeleteSubKeyTree(subkey);
        }

        public static void DeleteLocalMachineSubKeyTree(string subkey)
        {
            DeleteSubKeyTree(RegistryHive.LocalMachine, subkey);
        }

        public static void DeleteRegistryKeyPathValue(RegistryHive registryHive, string keyPath, string name)
        {
            try
            {
                if (TryGetRegistryKey(registryHive, keyPath, true, out var registryKey))
                    registryKey.DeleteValue(name);
            }
            catch (Exception)
            {
            }
        }

        public static void DeleteLocalMachineRegistryKeyPathValue(string keyPath, string name)
        {
            DeleteRegistryKeyPathValue(RegistryHive.LocalMachine, keyPath, name);
        }

        public static bool IsProductInstalled(string upgradeCode)
        {
            var keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using var registryKey = GetLocalMachineRegistryKey(keyPath);
            foreach (string subkeyName in registryKey.GetSubKeyNames())
            {
                using (var subkey = registryKey.OpenSubKey(subkeyName))
                {
                    if (subkey != null)
                    {
                        var value = subkey.GetValue("BundleUpgradeCode", null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                        if (value != null && ((string[])value)[0] == upgradeCode) return true;
                    }
                }
            }

            return false;
        }
    }
}
