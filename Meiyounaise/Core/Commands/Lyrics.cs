using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Meiyounaise.Core.Commands
{
    public class Lyrics : ModuleBase<SocketCommandContext>
    {
        [Command("lyrics")]
        public async Task LyricsTask(string artist, [Remainder]string track)
        {
            string key = Utilities.GetKey("lyricskey");
            string result;
            Uri url = new Uri("https://orion.apiseeds.com/api/music/lyric/" + artist + "/" + track + key);
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        var reponse = await httpClient.SendAsync(request);
                        result = await reponse.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
                return;
            }

            result = result.Substring(result.IndexOf(",\"text\":\"") + 9);
            result = result.Remove(result.IndexOf("\",\"lang\":{"));
            result = result.Replace(@"\n", "\n");
            if (result.Length > 2000)
            {
                string rts = result;
                while (rts.Length >= 2000)
                {
                    await ReplyAsync(rts.Substring(0, 2000));
                    rts = rts.Substring(2000);
                }
                await ReplyAsync(rts);
                return;
            }
            try
            {
                await ReplyAsync(result);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }
    }
}
