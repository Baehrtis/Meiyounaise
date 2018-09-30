using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using IF.Lastfm.Core.Api;
using Meiyounaise.Core.Data;

namespace Meiyounaise.Core.Commands
{
    public class Lastfm : ModuleBase<SocketCommandContext>
    {
        [Command("fm")]
        public async Task FmUser(string cmd, string username = "")
        {
            if (cmd.ToLower() == "set")
            {
                if (username == "") { await ReplyAsync("I need a Name that I can link to your account!"); return; }
                var account = UserAccounts.GetAccount(Context.User);
                account.Last = username;
                UserAccounts.SaveAccounts();
                await Context.Message.AddReactionAsync(new Emoji("✅"));
                return;
            }
            if (cmd.ToLower() == "get")
            {
                if (username == "")
                {
                    var account = UserAccounts.GetAccount(Context.User);
                    var name = account.Last;
                    if (name != null) await ReplyAsync($"I have {account.Last} saved as your Last.fm Account Name!");
                    else await ReplyAsync("I have nothing saved as your Last.fm Account Name!");
                }
                else
                {
                    var account = UserAccounts.GetAccount(Context.Message.MentionedUsers.LastOrDefault());
                    if (account.Last != null)
                        await ReplyAsync(
                            $"I have {account.Last} saved as {Context.Message.MentionedUsers.LastOrDefault()?.Mention} 's Last.fm Account Name!");
                    else
                        await ReplyAsync(
                            $"I have nothing saved as {Context.Message.MentionedUsers.LastOrDefault()?.Mention}' Last.fm Account Name");
                }
            }
        }

        [Command("fm")]
        public async Task Last()
        {
            var name = UserAccounts.GetAccount(Context.User).Last;
            if (name == null)
            {
                await ReplyAsync("I have no Last.fm Username set for you! Set it using `&fm set [Name]`!");
                return;
            }
            var client = new LastfmClient(Utilities.GetKey("lastkey"), Utilities.GetKey("lastsecret"), new HttpClient());
            var response = await client.User.GetRecentScrobbles(name);
            var count = client.User.GetInfoAsync(name).Result.Content.Playcount.ToString();
            string isplaying = "Last Track";
            var isNowPlaying = response.Content.First().IsNowPlaying;
            if (isNowPlaying != null) isplaying = "Now Playing";
            var embed = new EmbedBuilder()
                .WithThumbnailUrl(response.Content.First().Images.Large.AbsoluteUri)
                    .WithAuthor(author =>
                    {
                        author
                            .WithIconUrl("http://icons.iconarchive.com/icons/sicons/basic-round-social/256/last.fm-icon.png")
                            .WithName($"{name} - {isplaying}")
                            .WithUrl($"https://www.last.fm/user/{name}");
                    })
                .WithColor(255, 0, 0)
                .AddField("Artist - Song", response.Content.First().ArtistName + " - " + $"[{response.Content.First().Name}]({response.Content.First().Url})")
                .AddField("Album", response.Content.First().AlbumName)
                .WithFooter(footer => footer.WithText(count + " Total Scrobbles"))
                .WithCurrentTimestamp();
            await ReplyAsync("", false, embed.Build());
        }
    }
}
