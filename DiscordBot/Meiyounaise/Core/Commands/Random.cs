using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class Random : ModuleBase<SocketCommandContext>
    {
        //DMAU
        [Command("unnerum"), Summary("ok")]
        public async Task AberUnneRumTask([Remainder]string input = "")
        {
            string input2 = input;
            if (input == "")
            {
                var message = await Context.Channel.GetMessagesAsync(2).Flatten();
                input2 = message.Last().Content;
            }
            string result = "***";
            for (int i = 1; i <= input2.Length; i += 2)
            {
                input2 = input2.Insert(i, " ");
            }
            input2 = input2.ToUpper();
            result += "D E I N E  M U T T E R  " + input2 + ",  A B E R   U N N E R U M" + "***";
            await ReplyAsync(result);
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
        public async Task Clap(string toIns, [Remainder]string text)
        {
            string result = text.Replace(" ", $" {toIns} ");
            await ReplyAsync(result);
        }
        //AVATAR
        [Command("avatar"), Summary("mine now")]
        public async Task Avatar(string name)
        {
            var user = Context.Message.MentionedUsers.FirstOrDefault();
            string url = user?.GetAvatarUrl();
            await ReplyAsync($"{user?.Username}'s Avatar ist: {url?.Replace("size=128", "size=1024")}");
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
    }
}