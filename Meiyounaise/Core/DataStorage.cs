using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Meiyounaise.Core.Data;
using Newtonsoft.Json;

namespace Meiyounaise.Core
{
    public static class DataStorage
    {
        public static void SaveUsers(IEnumerable<UserAccount> accounts,string filePath)
        {
            string json = JsonConvert.SerializeObject(accounts,Formatting.Indented);
            File.WriteAllText(filePath,json);
        }

        public static IEnumerable<UserAccount> LoadUsers(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<UserAccount>>(json);
        }

        public static bool SaveExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
