using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class CommandList : ModuleBase<SocketCommandContext>
    {
        [Command("commands"), Alias("help"), Summary("Insert first word between all others")]
        public async Task Commands()
        {
            string botavatar = Context.Guild.GetUser(488112585640509442).GetAvatarUrl();
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(255,255,255)
                .WithAuthor(author => {
                    author
                        .WithName("Meiyounaise")
                        .WithIconUrl(botavatar);
                }) //Avatar,Bilder,Clap,Emotion,Icon,Nickname,Ping,Quote,Status,Steam,Translate,Unnerum
                .WithDescription("**Commands for the Bot. [] are necessary, () are optional**")
                .AddField("&avatar [@User]", "Gets the provided users Avatar")
                .AddField("Pictures","&bastard, &blod")
                .AddField("&clap [Word to set] [Sentence]", "Places the first words between all other words in the sentence")
                .AddField("&de (Text to Translate)", "Translates your text to German, if you provide no text, it translates the last message in the channel")
                .AddField("&emotion [Image URL]", "Reads the Emotions of a face")
                .AddField("&en (Text to translate)", "Translates your text to English, if you provide no text, it translates the last message in the channel")
                .AddField("&icon [Attach Image to message]", "Changes the Bots Avatar to whatever you attached to your message")
                .AddField("&nickname [Name]", "Sets the Nickname of the Bot to Name")
                .AddField("&ping", "Returns the Latencies of the Bot")
                .AddField("&quote [Message ID]", "Quote somebody's message via the message ID, deletes your original message")
                .AddField("&status [Game to play]", "Sets the Bot's Game (Playing X)")
                .AddField("&steam [steamID64/Custom URL]", "Get Information about the provided Steam account")
                .AddField("&unnerum (Sentence)","Yeah idek");
                
            await Context.Channel.SendMessageAsync("",false,embed.Build());
        }
    }
}