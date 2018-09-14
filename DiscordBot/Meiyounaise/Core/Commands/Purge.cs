using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class Purge : ModuleBase<SocketCommandContext>
    {
        [Command("purge", RunMode = RunMode.Async),Alias("prune")]
        public async Task purge(int amount)
        {
            string id = "137234090309976064";
            if (Context.Message.Author.Id.ToString() == id)
            {
                var messages = await Context.Channel.GetMessagesAsync(amount + 1).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages);
                var m = await ReplyAsync($"Habe {amount} Nachrichten gelöscht 👌");
                await Task.Delay(5000);
                await m.DeleteAsync();
            }
            else
            {
                await ReplyAsync("Denk nicht mal dran");
            }
        }
    }
}