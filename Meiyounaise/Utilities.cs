using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Meiyounaise
{
    class Utilities
    {
        private static Dictionary<string, string> keys;
        static Utilities()
        {
            string json = File.ReadAllText((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\keys.json"));
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            keys = data.ToObject<Dictionary<string, string>>();
        }

        public static string GetKey(string name)
        {
            if (keys.ContainsKey(name))
            {
                return keys[name];
            }
            return "";
        }
    }
}
