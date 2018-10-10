using System.Collections.Generic;
using System.IO;
using Meiyounaise.Core.Data;
using Newtonsoft.Json;

namespace Meiyounaise.Core
{
    public static class DataStorage<T>
    {
        public static void SaveData(IEnumerable<T> accounts,string filePath)
        {
            string json = JsonConvert.SerializeObject(accounts,Formatting.Indented);
            File.WriteAllText(filePath,json);
        }

        public static IEnumerable<T> LoadData(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public static bool SaveExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
