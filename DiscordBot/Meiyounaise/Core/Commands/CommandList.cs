using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class CommandList : ModuleBase<SocketCommandContext>
    {

        //CLAP
        [Command("commands"), Alias("help"), Summary("Insert first word between all others")]
        public async Task commands()
        {
            string botavatar = Context.Guild.GetUser(488112585640509442).GetAvatarUrl();
            EmbedBuilder Embed = new EmbedBuilder()
                .WithColor(255,255,255)
                .WithAuthor(author => {
                    author
                        .WithName("Meiyounaise")
                        .WithIconUrl(botavatar);
                })
                .WithDescription("**Commands for the Bot. [] are necessary, () are optional**")
                .AddField("&avatar [@User]", "Gets the provided users Avatar")
                .AddField("&clap [Word to set] [Sentence]", "Places the first words between all other words in the sentence")
                .AddField("&de (Text to Translate)", "Translates your text to German, if you provide no text, it translates the last message in the channel")
                .AddField("&emotion [Image URL]", "Reads the Emotions of a face")
                .AddField("&en (Text to translate)", "Translates your text to English, if you provide no text, it translates the last message in the channel")
                .AddField("&icon [Attach Image to message]", "Changes the Bots Avatar to whatever you attached to your message")
                .AddField("&nickname [Name]", "Sets the Nickname of the Bot to Name")
                .AddField("&qote [Message ID]", "Quote somebody's message via the message ID, deletes your original message")
                .AddField("&status [Game to play]", "Sets the Bot's Game (Playing X)");
            await Context.Channel.SendMessageAsync("",false,Embed.Build());
        }

    }
}
//Status,Nickname,Icon,Clap,Avatar,Quote,Emotion,Translate
//Avatar,Clap,Emotion,Icon,Nickname,Quote,Status,Translate