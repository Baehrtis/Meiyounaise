using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.Commands;

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

        public static string GetImageFromCurrentOrLastMessage(string url, IMessage message, SocketCommandContext context)
        {
            string durl;
            if (context.Message.Attachments.Count != 0)//CURRENT MESSAGE HAS ATTACHMENT
            {
                durl = context.Message.Attachments.FirstOrDefault()?.Url;
            }
            else//CURRENT MESSAGE DOES NOT HAVE ATTACHMENT
            {
                if (url != "")//IMAGE URL PROVIDED
                {
                    durl = url;
                }
                else//NO IMAGE URL PROVIDED
                {
                    if (message.Attachments.Count != 0)
                    {
                        durl = message.Attachments.FirstOrDefault()?.Url;
                    }
                    else
                    {
                        durl = message.Content;
                    }
                }
            }
            return durl;
        }
    }
}
