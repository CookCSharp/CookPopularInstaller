/*
 * Description: JsonTool 
 * Author: Chance.Zheng
 * Create Time: 2024/7/17 13:51:12
 * .Net Version: 4.8
 * CLR Version: 4.0.30319.42000
 * Copyright (c) NCATest 2020-2024 All Rights Reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookPopularInstaller.CustomUI.Patch
{
    public class JsonTool
    {
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static T JsonDeserialize<T>(string value)
        {
            T local = default(T);
            if (string.IsNullOrEmpty(value))
            {
                return local;
            }
            return JsonConvert.DeserializeObject<T>(value, _jsonSettings);
        }

        public static void JsonSerializeFile<T>(T value, string filePath)
        {
            using (FileStream theFileStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                string theValue = JsonConvert.SerializeObject(value, _jsonSettings);
                using (StreamWriter theWriter = new StreamWriter(theFileStream))
                {
                    theWriter.Write(theValue);
                    theWriter.Flush();
                }
            }
        }
    }
}
