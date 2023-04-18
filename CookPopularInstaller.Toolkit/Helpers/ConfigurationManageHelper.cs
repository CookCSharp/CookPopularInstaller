using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



/*
 * Description:ConfigurationManageHelper
 * Author: Chance.Zheng
 * Company: CookCSharp
 * CreateTime: 2022/8/25 14:43:28
 * .Net Version: 4.6
 * CLR Version: 4.0.30319.42000
 * Copyright © CookCSharp 2018-2022 All Rights Reserved
 */
namespace CookPopularInstaller.Toolkit.Helpers
{
    public static class ConfigurationManageHelper
    {
        public static void AddItem(string key, string value, string sectionName = "appSettings")
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings.Add(key, value);
            configuration.Save(ConfigurationSaveMode.Modified);
            //保存配置文件后需要刷新配置节点,这样下次查询配置节点数据时将会重新从磁盘中加载配置文件
            ConfigurationManager.RefreshSection(sectionName);
        }

        public static void DeleteItem(string key, string sectionName = "appSettings")
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings.Remove(key);
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(sectionName);
        }

        public static void ModifyItem(string key, string value, string sectionName = "appSettings")
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(sectionName);
        }

        public static void ModifyItem(string exePath, string key, string value, string sectionName = "appSettings")
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(exePath);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(sectionName);
        }

        public static string ReadItem(string key)
        {
            return ConfigurationManager.AppSettings.Get(key);
        }
    }
}
