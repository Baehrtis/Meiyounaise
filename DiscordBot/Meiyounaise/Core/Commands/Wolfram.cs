using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using WAWrapper;

namespace Meiyounaise.Core.Commands
{
    public class Wolfram : ModuleBase<SocketCommandContext>
    {
        [Command("wa")]
        public async Task WolframTask([Remainder]string input)
        {
            var client = new WAEngine();
            using (var stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\WolframKey.txt"), FileMode.Open, FileAccess.Read))
            using (var readToken = new StreamReader(stream))
            {
                client.APIKey = readToken.ReadToEnd();
            }
            var result = client.RunQuery(input);
            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithIconUrl("https://cdn.freebiesupply.com/logos/large/2x/wolfram-language-1-logo-png-transparent.png")
                        .WithName("Wolfram Result")
                        .WithUrl(result.Host);
                })
                .WithTimestamp(Context.Message.Timestamp)
                .WithColor(255, 0, 0);
            if (result.Success)
            {
                await Context.Message.AddReactionAsync(new Emoji("✅"));
                int i = 0;
                foreach (var pod in result.Pods)
                {
                    if (pod.SubPods != null)
                    {
                        Console.WriteLine(pod.Title);
                        foreach (var subpod in pod.SubPods)
                        {
                            Console.WriteLine("    " + subpod.PlainText);
                            if (subpod.PlainText != ""&&i<25)
                            {
                                embed.AddField(pod.Title, subpod.PlainText);
                                i++;
                            }
                        }
                    }
                }
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                await Context.Message.AddReactionAsync(new Emoji("❌"));
            }
        }
    }
}
