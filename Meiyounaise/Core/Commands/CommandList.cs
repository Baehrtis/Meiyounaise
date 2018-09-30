﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class CommandList : ModuleBase<SocketCommandContext>
    {
        [Command("commands"), Alias("help")]
        public async Task Commands()
        {
            string botavatar = Context.Guild.GetUser(488112585640509442).GetAvatarUrl();
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(255, 255, 255)
                .WithAuthor(author =>
                {
                    author
                        .WithName("Meiyounaise")
                        .WithIconUrl(botavatar);
                }) //Avatar,Bilder,Clap,Emotion,Icon,Nickname,Ping,Quote,Status,Steam,Translate,Unnerum
                .WithDescription("**Commands for the Bot. [] are necessary, () are optional**")
                .AddField("&avatar [@User]", "Gets the provided users Avatar")
                .AddField("Pictures", "&angefahren, &blod")
                .AddField("&clap [Word to set] [Sentence]", "Places the first words between all other words in the sentence")
                .AddField("&de (Text to Translate)", "Translates your text to German, if you provide no text, it translates the last message in the channel")
                .AddField("&e [Emote]", "Enlarges the emote you provided")
                .AddField("&emotion [Image URL]", "Reads the Emotions of a face")
                .AddField("&en (Text to translate)", "Translates your text to English, if you provide no text, it translates the last message in the channel")
                .AddField("&faceapp | &fa", "Shows the available FaceApp Filters and how to use them")
                .AddField("&game [Game]| &sg | &g", "Returns information about the Game on Steam")
                .AddField("&icon (Image URL/Attached Image)", "Changes the Bots Avatar to the provided URL/Image. If you provide none, it looks at the last message in the channel")
                .AddField("&lyrics [\"Artist\"] [Song name]","Gets the Lyrics to the provided song. If the Artist's name is more than one word, use \"\" around it. Yes, you need both Artist AND Songname")
                .AddField("&money [Amount] [From] (To)","Converts Money to another currency, default \"To\" is EUR")
                .AddField("&nickname [Name]", "Sets the Nickname of the Bot to Name")
                .AddField("&ping", "Returns the Latencies of the Bot")
                .AddField("&quote [Message ID]", "Quote somebody's message via the message ID, deletes your original message")
                .AddField("ri [Text]", "Returns your Input as Regional Indicators")
                .AddField("&status [Game to play]", "Sets the Bot's Game (Playing X)")
                .AddField("&steam [steamID64/Custom URL]", "Get Information about the provided Steam account")
                .AddField("&translate [languagecode] (Text)","Translates text to the language you provided. You can get the bot to send you the language codes by typing &translate codes")
                .AddField("&unnerum (Sentence)", "Yeah idek")
                .AddField("&wa [Input]", "Uses Wolfram Alpha for evaluation and returns the result");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}