using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FixerSharp;

namespace Meiyounaise.Core.Commands
{
    public class Currency : ModuleBase<SocketCommandContext>
    {
        [Command("money")]
        public async Task FixerTask(double input, string from, string to = Symbols.EUR)
        {
            Fixer.SetApiKey(Utilities.GetKey("fixerkey"));
            try
            {
                double result = Fixer.Convert(from, to, input);
                ExchangeRate er = await Fixer.RateAsync(from, to);
                var embed = new EmbedBuilder()
                    .WithColor(50, 255, 50)
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("Money Converter")
                            .WithIconUrl("https://cdn2.iconfinder.com/data/icons/business-seo-vol-5/100/1-15-512.png");
                    })
                    .AddInlineField(from.ToUpper(), input)
                    .AddInlineField(to.ToUpper(), result)
                    .AddField("Exchange Rate", er.Rate);
                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception e)
            {
                await ReplyAsync($"Error: `{e.Message}`\n**Usage:** &money [100] [from] (to)\nPossible Currency Codes: https://fixer.io/symbols");
            }
        }
    }
}