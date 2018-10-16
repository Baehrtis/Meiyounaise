using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HtmlAgilityPack;
using Meiyounaise.Core.Data;

namespace Meiyounaise
{
    class Utilities
    {
        private static readonly Dictionary<string, string> Keys;

        internal static string DataPath =
            (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace($@"bin{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}netcoreapp2.1", $@"Data{Path.DirectorySeparatorChar}");
        
        static Utilities()
        {
            string jsonK = File.ReadAllText(DataPath+"keys.json");
            var dataK = JsonConvert.DeserializeObject<dynamic>(jsonK);
            Keys = dataK.ToObject<Dictionary<string, string>>();
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
        
        public static async Task DownloadAsync(Uri requestUri, string filename)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            HttpClientHandler handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    using (
                        Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
                }
            }
        }

        public static List<string> GetImageUrlsFromWebsite(string url)
        {
            var document = new HtmlWeb().Load(url);
            var urls = document.DocumentNode.Descendants("img")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !String.IsNullOrEmpty(s));
            return urls.ToList();
        }
        
        
    }
}
