using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Meiyounaise.Core.Commands
{
    public class Purge : ModuleBase<SocketCommandContext>
    {
        [Command("purge", RunMode = RunMode.Async), Alias("prune")]
        public async Task PurgeTask(int amount)
        {
            var trusted = File.ReadAllLines((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\trusted.txt"));
            int pos = Array.IndexOf(trusted, Context.Message.Author.Id.ToString());
            if (pos > -1)
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
                    await ReplyAsync( "❌ I don't have permissions to delete messages on this Server");
                }
            }
            else
            {
                await ReplyAsync("❌ You are not allowed to delete messages!");
            }
        }

        private bool CheckUser(string id)
        {
            var trusted = File.ReadAllLines((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\trusted.txt"));
            var pos = Array.IndexOf(trusted, id);
            return pos > -1;
        }

        [Command("trusted")]
        public async Task Trusted(string aoR, string id = "")
        {
            if (CheckUser(id))//CHECK IF USER IS TRUSTED
            {
                string path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\trusted.txt");
                aoR = aoR.ToLower();
                if (aoR == "add" || aoR == "a")
                {
                    File.AppendAllText(path, "\n" + id);
                    await ReplyAsync($"✅ Added {Context.Guild.GetUser(Convert.ToUInt64(id)).Mention} to trusted Users!");
                }
                else if (aoR == "remove" || aoR == "r")
                {
                    string item = id;
                    var lines = File.ReadAllLines(path).Where(line => line.Trim() != item).ToArray();
                    if (!lines.Contains(id))
                    {
                        await ReplyAsync($"❌ Couldn't find {id} in the trusted list!");
                    }
                    File.WriteAllLines(path, lines);
                    await ReplyAsync($"✅ Removed {Context.Guild.GetUser(Convert.ToUInt64(id)).Mention} from trusted Users!");
                }
            }
            else
            {
                await ReplyAsync("❌ You are not allowed to delete messages!");
            }
        }

        [Command("trusted")]
        public async Task TrustedUserTask(string aoR, SocketUser user)
        {
            var id = Context.Message.MentionedUsers.Last()?.Id.ToString();
            if (CheckUser(id))//CHECK IF USER IS TRUSTED
            {
                await Trusted(aoR, id);
            }
            else
            {
                await ReplyAsync("❌ You are not allowed to delete messages!");
            }
        }
    }
}