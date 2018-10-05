using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Meiyounaise
{
    class Utilities
    {
        private static readonly Dictionary<string, string> Keys;
        
        internal static string DataPath =
            (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace($@"bin{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}netcoreapp2.1", $@"Data{Path.DirectorySeparatorChar}");
        
        static Utilities()
        {
            string json = File.ReadAllText(DataPath+"keys.json");
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            Keys = data.ToObject<Dictionary<string, string>>();
        }

        public static string GetKey(string name)
        {
            if (Keys.ContainsKey(name))
            {
                return Keys[name];
            }
            return "";
        }
    }
}
