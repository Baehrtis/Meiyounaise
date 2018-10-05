using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace Meiyounaise.Core.Commands
{
    [Name("Admin")]
    public class AdminModule : InteractiveBase
    {
        [Command("purge", RunMode = RunMode.Async), Alias("prune")]
        [Summary("Bulk delete messages.")]
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

        [Command("timeout", RunMode = RunMode.Async)]
        [Summary("Moves the specified user into the AFK Channel and then moves them back.")]
        [RequireBotPermission(GuildPermission.MoveMembers)]
        [RequireUserPermission(GuildPermission.MoveMembers)]
        public async Task Timeout(string user)
        {
            SocketVoiceChannel channelToMoveTo;
            if (Context.Guild.AFKChannel == null)
            {
                await ReplyAsync("No AFK Channel found! Give me a channel ID within the next 10 secs to continue!");
                var response = await NextMessageAsync() as SocketUserMessage;
                try
                {
                    channelToMoveTo = Context.Guild.GetVoiceChannel(Convert.ToUInt64(response?.Content));
                }
                catch (Exception)
                {
                    if (response != null) await response.AddReactionAsync(new Emoji("❌"));
                    return;
                }
            }
            else
            {
                channelToMoveTo = Context.Guild.AFKChannel;
            }

            var toMove = Context.Message.MentionedUsers.Last() as IGuildUser;
            if (toMove?.VoiceChannel == null)
            {
                await Context.Message.AddReactionAsync(new Emoji("❌"));
                return;
            } 

            var originalChannel = toMove.VoiceChannel as SocketVoiceChannel;
            await toMove.ModifyAsync(x => x.Channel = channelToMoveTo);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
            await Task.Delay(5000);
            await toMove.ModifyAsync(x => x.Channel = originalChannel);
        }
    }
}