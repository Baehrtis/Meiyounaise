using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Urban.NET;

namespace Meiyounaise.Core.Commands
{
    public class Random : ModuleBase<SocketCommandContext>
    {
        //DMAU
        [Command("unnerum"), Summary("ok")]
        public async Task AberUnneRumTask([Remainder] string input = "")
        {
            string input2 = input;
            if (input == "")
            {
                var message = await Context.Channel.GetMessagesAsync(2).Flatten();
                input2 = message.Last().Content;
            }

            string result = "**";
            for (int i = 1; i <= input2.Length; i += 2)
            {
                input2 = input2.Insert(i, " ");
            }

            input2 = input2.ToUpper();
            result += "D E I N E  M U T T E R  " + input2 + ",  A B E R   U N N E R U M" + "**";
            await ReplyAsync(result);
        }

        //EMOTE
        [Command("e")]
        public async Task Emote(string input)
        {
            var emote = Discord.Emote.Parse(input);
            await ReplyAsync(emote.Url);
        }

        //PING
        [Command("ping"), Summary("Returns Latency")]
        public async Task Ping()
        {
            var temp = await Context.Channel.SendMessageAsync("Ping...");
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(0, 0, 0)
                .WithDescription("Pong!")
                .AddInlineField($"Latency", $"{(temp.CreatedAt - Context.Message.CreatedAt).Milliseconds}ms")
                .AddInlineField($"API-Latency", $"{Context.Client.Latency}ms");
            await temp.ModifyAsync(msg => msg.Embed = embed.Build());
        }

        //CLAP
        [Command("clap"), Alias("klatsch"), Summary("Insert first word between all others")]
        public async Task Clap(string toIns, [Remainder] string text)
        {
            string result = text.Replace(" ", $" {toIns} ");
            await ReplyAsync(result);
        }

        //AVATAR
        [Command("avatar"), Summary("Show a users avatar")]
        public async Task Avatar(string name)
        {
            var user = Context.Message.MentionedUsers.FirstOrDefault();
            string url = user?.GetAvatarUrl();
            await ReplyAsync($"{user?.Username}'s Avatar is: {url?.Replace("size=128", "size=1024")}");
        }

        //QUOTE
        [Command("quote"), Summary("Quote people via a message id")]
        public async Task Quote(ulong id)
        {
            var toQuote = await Context.Channel.GetMessageAsync(id);
            string url = toQuote.Attachments.FirstOrDefault()?.Url;
            EmbedBuilder embed = new EmbedBuilder()
                .WithDescription(toQuote.Content)
                .WithColor(new Color(0x3C3175))
                .WithTimestamp(toQuote.Timestamp)
                .WithThumbnailUrl(toQuote.Author.GetAvatarUrl())
                .AddField("Link:",
                    "https://discordapp.com/channels/" + Context.Guild.Id.ToString() + "/" + toQuote.Channel.Id +
                    "/" +
                    toQuote.Id)
                .WithFooter(footer =>
                {
                    footer
                        .WithText($"in #{toQuote.Channel.Name}");
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName(toQuote.Author.Username)
                        .WithUrl("https://nesewebel.de/")
                        .WithIconUrl(Context.Guild.IconUrl);
                });

            if (toQuote.Attachments.Count == 1)
            {
                string type = url?.Substring(url.Length - 3);
                if (type == "png" || type == "jpg" || type == "jpeg")
                {
                    embed.WithImageUrl(toQuote.Attachments.FirstOrDefault()?.Url);
                }
                else
                {
                    embed.AddField("Attachment:", toQuote.Attachments.FirstOrDefault()?.Url);
                }
            }

            try
            {
                await Context.Message.DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't delete message on {Context.Guild.Name}, Error: {ex.Message}");
            }
            finally
            {
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        //REGIONAL INDICATOR
        [Command("ri")]
        public async Task RegionalIndicator([Remainder] string input)
        {
            string result = input.ToLower();
            result = result.Replace("a", "🇦 ").Replace("b", "🅱 ").Replace("c", "🇨 ").Replace("d", "🇩 ")
                .Replace("e", "🇪 ").Replace("f", "🇫 ").Replace("g", "🇬 ").Replace("h", "🇭 ").Replace("i", "🇮 ")
                .Replace("j", "🇯 ").Replace("k", "🇰 ").Replace("l", "🇱 ").Replace("m", "🇲 ").Replace("n", "🇳 ")
                .Replace("o", "🇴 ").Replace("p", "🇵 ").Replace("q", "🇶 ").Replace("r", "🇷 ").Replace("s", "🇸 ")
                .Replace("t", "🇹 ").Replace("u", "🇺 ").Replace("v", "🇻 ").Replace("w", "🇼 ").Replace("x", "🇽 ")
                .Replace("y", "🇾 ").Replace("z", "🇿 ");
            try
            {
                await ReplyAsync(result);
            }
            catch (Exception e)
            {
                await ReplyAsync($"Error: `{e.Message}`");
            }
        }

        //UD
        [Command("ud")]
        public async Task UrbanDictionary([Remainder] string word)
        {
            UrbanService client = new UrbanService();
            var data = await client.Data(word);
            string def = data.List.First().Definition.Replace("[", "");
            def = def.Replace("]", "");
            var embed = new EmbedBuilder()
                .WithColor(200, 200, 0)
                .WithTitle(data.List.First().Word)
                .WithUrl(data.List.First().Permalink)
                .WithDescription(def)
                .AddInlineField("Rating", $"👍{data.List.First().ThumbsUp}\t👎{data.List.First().ThumbsDown}");

            await ReplyAsync($"Top Definition for {data.List.First().Word}:", false, embed.Build());
        }

        //SCREENSHOT
        [Command("ss")]
        public async Task ScreenShot(string url)
        {
            string utd = $"https://api.thumbnail.ws/api/{Utilities.GetKey("sskey")}/thumbnail/get?url={url}&width=1000";
            var om = await Context.Channel.SendMessageAsync("Ok, this could take a little bit!");
            var typing = Context.Channel.EnterTypingState();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, utd))
                    {
                        using (
                            Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                            stream = new FileStream(Utilities.dataPath + "ss.jpg", FileMode.Create, FileAccess.Write,
                                FileShare.None, 4096, true))
                        {
                            await contentStream.CopyToAsync(stream);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
            typing.Dispose();
            var reply = await Context.Channel.SendFileAsync(Utilities.dataPath + "ss.jpg");
            await om.ModifyAsync(x => x.Content = $"⏲ Took {(reply.CreatedAt.Millisecond - om.CreatedAt.Millisecond)} milliseconds!");
            File.Delete(Utilities.dataPath + "ss.jpg");
        }
    }
}