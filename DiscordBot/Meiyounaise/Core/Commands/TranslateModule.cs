using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace Meiyounaise.Core.Commands
{
    public class TranslateModule : ModuleBase<SocketCommandContext>
    {
        ///////////////////////////
        // NOTE: Replace this example key with a valid subscription key.

        string key = "";
        string host = "https://api.cognitive.microsofttranslator.com";
        string path = "/translate?api-version=3.0";
        string translated = "";
        void getKey()
        {
            using (var Stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\TranslateKey.txt"), FileMode.Open, FileAccess.Read))
            using (var ReadToken = new StreamReader(Stream))
            {
                this.key = ReadToken.ReadToEnd();
            }
        }

        private string jsontostring(string input, bool de)
        {
            int stelle = input.IndexOf("text\": \"");
            input = input.Substring(stelle + 8);
            if (de)
            {
                input = input.Substring(0, input.IndexOf(",\r\n        \"to\": \"de\"") - 1);
            }
            else
            {
                input = input.Substring(0, input.IndexOf(",\r\n        \"to\": \"en\"") - 1);
            }

            return input.Replace(@"\n", "\n");
        }

        private async Task translate(string lang, [Remainder] string text)
        {
            string uri = host + path + lang;
            System.Object[] body = new System.Object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseBody), Formatting.Indented);
                Console.OutputEncoding = UnicodeEncoding.UTF8;
                translated = result;
            }
        }
        
        //TRANSLATE TO DE
        [Command("de", RunMode = RunMode.Async), Summary("Übersetzt shit zu deutsch")]
        public async Task Deutsch([Remainder] string text)
        {
            getKey();
            await translate("&to=de", text);
            await ReplyAsync(jsontostring(translated, true));
        }

        //TRANSLATE LAST MESSAGE TO DE
        [Command("de", RunMode = RunMode.Async), Summary("Übersetzt shit zu deutsch")]
        public async Task Deutsch2()
        {
            getKey();
            var message = await Context.Channel.GetMessagesAsync(2).Flatten();
            await translate("&to=de", message.Last().Content);
            await ReplyAsync(jsontostring(translated, true));
        }
        //TRANSLATE TO EN
        [Command("en", RunMode = RunMode.Async), Summary("Übersetzt shit zu deutsch")]
        public async Task Englisch([Remainder] string text)
        {
            getKey();
            await translate("&to=en", text);
            await ReplyAsync(jsontostring(translated, false));
        }
        //TRANSLATE LAST MESSAGE TO EN
        [Command("en", RunMode = RunMode.Async), Summary("Übersetzt shit zu deutsch")]
        public async Task Englisch2()
        {
            getKey();
            var message = await Context.Channel.GetMessagesAsync(2).Flatten();
            await translate("&to=de", message.Last().Content);
            await ReplyAsync(jsontostring(translated, false));
        }
    }
}