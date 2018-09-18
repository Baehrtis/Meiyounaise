using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace Meiyounaise.Core.Commands
{
    [SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
    public class TranslateModule : ModuleBase<SocketCommandContext>
    {
        string _key = "";
        string host = "https://api.cognitive.microsofttranslator.com";
        string path = "/translate?api-version=3.0";
        string _translated = "";
        void GetKey()
        {
            using (var stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\TranslateKey.txt"), FileMode.Open, FileAccess.Read))
            using (var readToken = new StreamReader(stream))
            {
                _key = readToken.ReadToEnd();
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

        private async Task Translate(string lang, [Remainder] string text)
        {
            string uri = host + path + lang;
            Object[] body = { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _key);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseBody), Formatting.Indented);
                Console.OutputEncoding = Encoding.UTF8;
                _translated = result;
            }
        }

        //TRANSLATE TO DE
        [Command("de", RunMode = RunMode.Async), Summary("Übersetzt shit zu deutsch")]
        public async Task Deutsch([Remainder] string text)
        {
            GetKey();
            await Translate("&to=de", text);
            await ReplyAsync(jsontostring(_translated, true));
        }

        //TRANSLATE LAST MESSAGE TO DE
        [Command("de", RunMode = RunMode.Async), Summary("Übersetzt shit zu deutsch")]
        public async Task Deutsch2()
        {
            GetKey();
            var message = await Context.Channel.GetMessagesAsync(2).Flatten();
            await Translate("&to=de", message.Last().Content);
            await ReplyAsync(jsontostring(_translated, true));
        }
        //TRANSLATE TO EN
        [Command("en", RunMode = RunMode.Async), Summary("Übersetzt shit zu deutsch")]
        public async Task Englisch([Remainder] string text)
        {
            GetKey();
            await Translate("&to=en", text);
            await ReplyAsync(jsontostring(_translated, false));
        }
        //TRANSLATE LAST MESSAGE TO EN
        [Command("en", RunMode = RunMode.Async), Summary("Übersetzt shit zu deutsch")]
        public async Task Englisch2()
        {
            GetKey();
            var message = await Context.Channel.GetMessagesAsync(2).Flatten();
            await Translate("&to=en", message.Last().Content);
            await ReplyAsync(jsontostring(_translated, false));
        }
    }
}