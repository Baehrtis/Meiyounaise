using System;
using System.Collections.Generic;
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
        [RequireBotPermission(GuildPermission.ManageMessages)]
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

        [Command("say", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task Say([Remainder]string text)
        {
            var guilds = new Dictionary<int, SocketGuild>();
            var i = 1;
            foreach (var guild in Context.Client.Guilds)
            {
                guilds.TryAdd(i, guild);
                i++;
            }
            await ReplyAsync($"Found Guilds\n{string.Join("\n", guilds)}\nChoose one via the number!");
            var gResponse = await NextMessageAsync();
            if (gResponse == null)
            {
                await ReplyAsync("I didn't get your choice!");
                return;
            }
            var target = guilds[Convert.ToInt32(gResponse.Content)];
            var channels = new Dictionary<int, SocketTextChannel>();
            var j = 1;
            foreach (var channel in target.TextChannels)
            {
                channels.TryAdd(j, channel);
                j++;
            }
            await ReplyAsync($"Listing text channels in guild {target.Name}\n{string.Join("\n", channels)}\nChoose one via the number!");
            var cResponse = await NextMessageAsync();
            var targetChannel = channels[Convert.ToInt32(cResponse.Content)];
            await targetChannel.SendMessageAsync(text);
            var messages = await Context.Channel.GetMessagesAsync(4).Flatten();
            try
            {
                await Context.Channel.DeleteMessagesAsync(messages);
            }
            catch (Exception)
            {
                // ignored
            }
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("servers")]
        [RequireOwner]
        public async Task Servers()
        {
            var result = "";
            foreach (var guild in Context.Client.Guilds)
            {
                var invite = "";
                var gResult = guild.Name;
                try
                {
                    var invites = await guild.GetInvitesAsync();
                    if (invites.Count == 0)
                    {
                        foreach (var channel in guild.Channels)
                        {
                            try
                            {
                                var test = await channel.CreateInviteAsync();
                                invite = test.Url + " (just created)";
                                break;
                            }
                            catch (Exception)
                            {
                                //ignore
                            }
                        }
                        if (invite == "")
                        {
                            invite = $"Couldn't create Invite for Server {guild.Name}";
                        }
                    }
                    else
                    {
                        invite = invites.First().Url;
                    }
                }
                catch (Exception)
                {
                    invite = $"Couldn't retrieve Invite for server {guild.Name}";
                }
                result += gResult + "\t" + invite + "\n";
            }

            if (result.Length < 2048)
            {
                var embed = new EmbedBuilder()
                    .WithDescription(result);
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                var rts = result;
                while (rts.Length >= 2000)
                {
                    await ReplyAsync(rts.Substring(0, 2000));
                    rts = rts.Substring(2000);
                }
                await ReplyAsync(rts);
            }
        }
    }
}
