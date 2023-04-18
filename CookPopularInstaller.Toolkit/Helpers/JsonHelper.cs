using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Description：JsonHelper 
 * Author： Chance.Zheng
 * Create Time: 2023/3/16 14:05:57
 * .Net Version: 6.0
 * CLR Version: 4.0.30319.42000
 * Copyright (c) CookCSharp 2018-2023 All Rights Reserved.
 */
namespace CookPopularInstaller.Toolkit.Helpers
{
    public static class JsonHelper
    {
        private static JsonSerializerSettings _jsonSettings;

        static JsonHelper()
        {
            _jsonSettings = new JsonSerializerSettings();
            _jsonSettings.Formatting = Formatting.Indented;
            _jsonSettings.NullValueHandling = NullValueHandling.Ignore;
        }


        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string JsonSerialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _jsonSettings);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
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
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (FileStream theFileStream = File.Open(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                string theValue = JsonSerialize<T>(value);
                using (StreamWriter theWriter = new StreamWriter(theFileStream))
                {
                    theWriter.Write(theValue);
                    theWriter.Flush();
                }
            }
        }


        public static T JsonDeserializeFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            using (FileStream theFileStream = File.OpenRead(filePath))
            {
                using (StreamReader theReader = new StreamReader(theFileStream))
                {
                    string theValue = theReader.ReadToEnd();
                    if (string.IsNullOrEmpty(theValue))
                        return default(T);

                    return JsonDeserialize<T>(theValue);
                }
            }
        }


        public static string ReadJsonValue(string jsonFilePath, string key)
        {
            using (StreamReader sr = File.OpenText(jsonFilePath))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(sr))
                {
                    try
                    {
                        JObject job = (JObject)JToken.ReadFrom(jsonReader);
                        var value = job[key]?.ToString();

                        return value;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }


        public static string ReadJsonValue(string jsonFilePath, string tokenName, string key)
        {
            using (StreamReader sr = File.OpenText(jsonFilePath))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(sr))
                {
                    try
                    {
                        JObject job = (JObject)JToken.ReadFrom(jsonReader);
                        var serviceToken = job[tokenName];
                        var value = serviceToken[key]?.ToString();

                        return value;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }


        public static void WriteJsonFile<T>(T value, string filePath)
        {
            StringBuilder jsonStr = new StringBuilder(JsonSerialize(value));
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(jsonStr);
                }
            }
        }
    }
}
