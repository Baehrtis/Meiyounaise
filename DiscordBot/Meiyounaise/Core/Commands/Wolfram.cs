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
            client.APIKey = Utilities.GetKey("wolframkey");
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
                        foreach (var subpod in pod.SubPods)
                        {
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
