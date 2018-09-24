using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class Purge : ModuleBase<SocketCommandContext>
    {
        [Command("purge", RunMode = RunMode.Async), Alias("prune")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeTask(int amount)
        {
            try
            {
                var messages = await Context.Channel.GetMessagesAsync(amount + 1).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages);
                var m = await ReplyAsync($"Deleted {amount} Messages 👌");
                await Task.Delay(5000);
                await m.DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't delete message on {Context.Guild.Name}, Error: {ex.Message}");
                await ReplyAsync($"❌ Error: {ex.Message}");
            }
        }
    }
}