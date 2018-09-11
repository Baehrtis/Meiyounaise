using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class Settings : ModuleBase<SocketCommandContext>
    {
        //STATUS
        [Command("status"), Summary("Changes the bots Game")]
        public async Task Status([Remainder]string ns)
        {
            await Context.Client.SetGameAsync(ns);
            await Task.CompletedTask;
            await Context.Channel.SendMessageAsync("Status wurde geändert 👌");
        }
        //NICKNAME
        [Command("nick", RunMode = RunMode.Async), Summary("Changes the Bots Nickname on the current guild")]
        public async Task Nick(string n)
        {
            var user = Context.Guild.GetUser(488112585640509442);
            await user.ModifyAsync(x => { x.Nickname = n; });
        }
        //BOT ICON
        [Command("icon", RunMode = RunMode.Async), Summary("Changes the Bots avatar. Bot Owner only")]
        public async Task Icon()
        {
            if (Context.Message.Author.Id == 137234090309976064)
            {
                if (Context.Message.Attachments.Count == 0)
                {
                    await ReplyAsync("Keine Bild angehängt!");
                    return;
                }
                string localFilename = @"E:\Programming\DiscordBot\Meiyounaise\Data\icon.png";
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(Context.Message.Attachments.FirstOrDefault().Url, localFilename);
                }
               
                var fileStream = new FileStream(@"E:\Programming\DiscordBot\Meiyounaise\Data\icon.png",
                    FileMode.Open);
                var image = new Image(fileStream);
                fileStream.Position = 0;

                await (Context.Client.CurrentUser).ModifyAsync(x => x.Avatar = image);

            }
            else
            {
                await ReplyAsync("Only for the Bot Owner, sorry!");
            }
        }

    }
}

