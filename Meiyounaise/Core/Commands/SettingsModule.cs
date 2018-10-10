using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Net.Http;
using Meiyounaise.Core.Data;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable PossibleNullReferenceException

namespace Meiyounaise.Core.Commands
{
    [Name("Bot Settings")]
    public class SettingsModule : ModuleBase<SocketCommandContext>
    {
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
        //PREFIX
        [Command("prefix"),Summary("Changes the Bot's prefix on the current guild")]
        public async Task Prefix([Remainder]string prefix="")
        {
            var guild = Guilds.GetGuild(Context.Guild);
            if (prefix=="")
            {
                await ReplyAsync($"The prefix on this guild is: `{guild.Prefix}`");
                return;
            }
            guild.Prefix = prefix;
            Guilds.SaveGuilds();
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //STATUS
        [Command("status"), Summary("Changes the Bot's \"playing\" status.")]
        public async Task Status([Remainder]string ns)
        {
            await Context.Client.SetGameAsync(ns);
            await Task.CompletedTask;
            await Context.Channel.SendMessageAsync("Status has been changed 👌");
        }
        //NICKNAME
        [Command("nick", RunMode = RunMode.Async), Summary("Changes the Bots Nickname on the current guild.")]
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        public async Task Nick(string n)
        {
            var user = Context.Guild.GetUser(488112585640509442);
            await user.ModifyAsync(x => { x.Nickname = n; });
        }
        //BOT ICON
        [Command("icon", RunMode = RunMode.Async), Summary("Changes the Bots avatar.")]
        public async Task Icon(string url = "")
        {
            var lm = await Context.Channel.GetMessagesAsync(2).Flatten();
            var message = lm.Last(); //GET LAST MESSAGE
            var path = Utilities.DataPath + "icon.png";
            try
            {
                var durl = Utilities.GetImageFromCurrentOrLastMessage(url, message, Context);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await DownloadAsync(new Uri(durl), path);
                var avatar = new FileStream(Utilities.DataPath + "icon.png", FileMode.Open);
                try
                {
                    await (Context.Client.CurrentUser).ModifyAsync(x => x.Avatar = new Image(avatar));
                    var reactTo = lm.FirstOrDefault() as IUserMessage;
                    await reactTo.AddReactionAsync(new Emoji("✅"));
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message);
                    throw;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }
    }
}