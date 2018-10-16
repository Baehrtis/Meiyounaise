    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Urban.NET;
    using Discord.Addons.Interactive;
    
    namespace Meiyounaise.Core.Commands
    {
        [Name("Random Stuff")]
        public class RandomStuffModule : InteractiveBase
        {
            //EMOTE
            [Command("e"), Summary("Enlarges the provided Emote.")]
            public async Task Emote(string input)
            {
                var emote = Discord.Emote.Parse(input);
                await ReplyAsync(emote.Url);
            }
    
            //PING
            [Command("ping"), Summary("Returns the Bot's Latencies.")]
            [RequireBotPermission(ChannelPermission.EmbedLinks)]
            public async Task Ping()
            {
                Stopwatch stopwatch = new Stopwatch();
                var temp = await Context.Channel.SendMessageAsync("Ping...");
                stopwatch.Start();
                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(0, 0, 0)
                    .AddInlineField($"API-Latency", $"{Context.Client.Latency}ms");
                await temp.ModifyAsync(msg => msg.Content = "Pong!");
                stopwatch.Stop();
                embed.AddInlineField($"Latency", $"{stopwatch.Elapsed.Milliseconds}ms");
                await temp.ModifyAsync(msg => msg.Embed = embed.Build());
            }
    
            //CLAP
            [Command("clap"), Alias("klatsch"), Summary("Insert first word between all others.")]
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
                var url = user?.GetAvatarUrl();
                await ReplyAsync($"{user?.Username}'s Avatar is: {url?.Replace("size=128", "size=1024")}");
            }
    
            //QUOTE
            [Command("quote"), Summary("Quote people via a message id.")]
            [RequireBotPermission(ChannelPermission.EmbedLinks)]
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
            [Command("ri"), Summary("Returns the text you provided in regional indicators.")]
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
    
            //URBAN DICTIONARY
            [Command("ud"), Summary("Gives you the definition of your word on Urban Dictionary.")]
            [RequireBotPermission(ChannelPermission.EmbedLinks)]
            public async Task UrbanDictionary([Remainder] string word)
            {
                UrbanService client = new UrbanService();
                var data = await client.Data(word);
                var pages = new List<string>();
                var pm = new PaginatedMessage();
                pm.Color = new Color(200, 200, 0);
                foreach (var entry in data.List)
                {
                    string result = $"[{entry.Word}]({entry.Permalink})\n\n";
                    string def = entry.Definition.Replace("[", "");
                    result += def.Replace("]", "");
                    result += $"\n\n👍{entry.ThumbsUp}\t👎{entry.ThumbsDown}";
                    pages.Add(result);
                }
    
                pm.Pages = pages;
                await PagedReplyAsync(pm);
                await Context.Message.DeleteAsync();
            }
    
            //TWITCH FOLLOWING
            [Command("followage"), Alias("following"),
             Summary("Returns how long you've been following a specified streamer.")]
            public async Task Following(string user, string channel)
            {
                var url = $"https://2g.be/twitch/following.php?user={user}&channel={channel}";
                string result;
                var handler = new HttpClientHandler();
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        var reponse = await httpClient.SendAsync(request);
                        result = await reponse.Content.ReadAsStringAsync();
                    }
                }
    
                await ReplyAsync(result);
            }
    
            [Command("ascii"),Summary("Returns your Text as ASCII Art.")]
            public async Task Ascii([Remainder] string text)
            {
                string result;
                var handler = new HttpClientHandler();
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get,
                        "http://artii.herokuapp.com/make?text=" + text.Replace(" ", "+")))
                    {
                        var reponse = await httpClient.SendAsync(request);
                        result = await reponse.Content.ReadAsStringAsync();
                    }
                }
    
                try
                {
                    await ReplyAsync("```" + result + "```");
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message);
                }
            }
    
            //DILBERT
            [Command("dilbert"),Summary("Returns a random or specified Dilbert Strip. Format: dilbert (year) (month) (day)")]
            public async Task Dilbert(string year = "", string month = "", string day = "")
            {
                var path = Utilities.DataPath + "dilbert.gif";
                var rand = new Random();
                var randomDate = new DateTime(1995, 1, 1);
                var range = (DateTime.Today - randomDate).Days;
                randomDate = randomDate.AddDays(rand.Next(range));
                if (year != "")
                {
                    randomDate = new DateTime(Convert.ToInt32(year), randomDate.Month, randomDate.Day);
                    if (month != "")
                    {
                        randomDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), randomDate.Day);
                        if (day != "")
                        {
                            randomDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
                        }
                    }
                }
                var list = Utilities.GetImageUrlsFromWebsite($"http://dilbert.com/strip/{randomDate:yyyy-M-d}");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Utilities.DownloadAsync(new Uri(list.Find(x => x.Contains("amuniversal"))), path);
                await Context.Channel.SendFileAsync(path, $"From {randomDate.ToLongDateString()}");
                File.Delete(Utilities.DataPath + "dilbert.gif");
            }

            //GARFIELD
            [Command("garfield"),Summary("Returns a random or specified Garfield Strip. Format: garfield (year) (month) (day)")]
            public async Task Garfield(string year = "", string month = "", string day = "")
            {
                var path = Utilities.DataPath + "garfield.gif";
                var rand = new Random();
                var randomDate = new DateTime(1978, 6, 19);
                var range = (DateTime.Today - randomDate).Days;
                randomDate = randomDate.AddDays(rand.Next(range));
                if (year != "")
                {
                    randomDate = new DateTime(Convert.ToInt32(year), randomDate.Month, randomDate.Day);
                    if (month != "")
                    {
                        randomDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), randomDate.Day);
                        if (day != "")
                        {
                            randomDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
                        }
                    }
                }
                //var list = Utilities.GetImageUrlsFromWebsite($"https://garfield.com/comic/{randomDate:yyyy'/'m'/'d}");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Utilities.DownloadAsync(new Uri($"https://d1ejxu6vysztl5.cloudfront.net/comics/garfield/{randomDate.Year}/{randomDate:yyyy-MM-dd}.gif"), path);
                await Context.Channel.SendFileAsync(path, $"From {randomDate.ToLongDateString()}");
                File.Delete(Utilities.DataPath + "garfield.gif");
            }
        }
    }