using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class Random : ModuleBase<SocketCommandContext>
    {

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
            string url = user.GetAvatarUrl();
            await ReplyAsync($"{user.Username}'s Avatar ist: {url.Replace("size=128", "size=1024")}");
        }
        //QUOTE
        [Command("quote"), Summary("Quote people via a message id")]
        public async Task Quote(ulong id)
        {
            var toQuote = await Context.Channel.GetMessageAsync(id);
            string url = toQuote.Attachments.FirstOrDefault()?.Url;

            EmbedBuilder Embed = new EmbedBuilder()
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
                string type = url.Substring(url.Length - 3);
                if (type == "png" || type == "jpg" || type == "jpeg")
                {
                    Embed.WithImageUrl(toQuote.Attachments.FirstOrDefault().Url);
                }
                else
                {
                    Embed.AddField("Attachment:", toQuote.Attachments.FirstOrDefault().Url);
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
                await Context.Channel.SendMessageAsync("", false, Embed.Build());

            }
        }



    }



}


