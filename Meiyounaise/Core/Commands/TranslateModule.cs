using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;

namespace Meiyounaise.Core.Commands
{
    public class TranslateModule : ModuleBase<SocketCommandContext>
    {
        private readonly GoogleCredential _credential = GoogleCredential.FromFile((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\gTranslateKey.json"));

        private string GTranslate(string text, string lang)
        {
            TranslationClient client = TranslationClient.Create(_credential);
            return client.TranslateText(text, lang).TranslatedText;
        }

        //TRANSLATE TO DE
        [Command("de")]
        public async Task Deutsch([Remainder] string text)
        {
            await ReplyAsync(GTranslate(text, LanguageCodes.German));
        }
        //TRANSLATE LAST MESSAGE TO DE
        [Command("de")]
        public async Task Deutsch2()
        {
            var message = await Context.Channel.GetMessagesAsync(2).Flatten();
            await ReplyAsync(GTranslate(message.Last().Content, LanguageCodes.German));
        }
        //TRANSLATE TO EN
        [Command("en")]
        public async Task Englisch([Remainder] string text)
        {
            await ReplyAsync(GTranslate(text, LanguageCodes.English));
        }
        //TRANSLATE LAST MESSAGE TO EN
        [Command("en")]
        public async Task Englisch2()
        {
            var message = await Context.Channel.GetMessagesAsync(2).Flatten();
            await ReplyAsync(GTranslate(message.Last().Content, LanguageCodes.English));
        }
        //TRANSLATE TO ANY LANGUAGE
        [Command("translate")]
        public async Task AnyLanguage(string langcode, [Remainder]string text)
        {
            await ReplyAsync(GTranslate(text, langcode));
        }
        //TRANSLATE TO ANY LANGUAGE
        [Command("translate")]
        public async Task AnyLanguage2(string langcode)
        {
            if (langcode == "codes")
            {
                var embed = new EmbedBuilder();
                var embed2 = new EmbedBuilder();
                var embed3 = new EmbedBuilder();
                var embed4 = new EmbedBuilder();
                var embed5 = new EmbedBuilder();
                TranslationClient client = TranslationClient.Create(_credential);
                int i = 0;
                foreach (var language in client.ListLanguages(LanguageCodes.English))
                {
                    if (i < 25)
                    {
                        embed.AddInlineField(language.Name, language.Code);
                        i++;
                    }
                    else if (i < 50)
                    {
                        embed2.AddInlineField(language.Name, language.Code);
                        i++;
                    }
                    else if (i < 75)
                    {
                        embed3.AddInlineField(language.Name, language.Code);
                        i++;
                    }
                    else if (i < 100)
                    {
                        embed4.AddInlineField(language.Name, language.Code);
                        i++;
                    }
                    else if (i < 125)
                    {
                        embed5.AddInlineField(language.Name, language.Code);
                        i++;
                    }
                }
                try
                {
                    var dm = await Context.User.GetOrCreateDMChannelAsync();
                    await dm.SendMessageAsync("", false, embed.Build());
                    await dm.SendMessageAsync("", false, embed2.Build());
                    await dm.SendMessageAsync("", false, embed3.Build());
                    await dm.SendMessageAsync("", false, embed4.Build());
                    await dm.SendMessageAsync("", false, embed5.Build());
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message);
                }
                return;
            }
            var message = await Context.Channel.GetMessagesAsync(2).Flatten();
            await ReplyAsync(GTranslate(message.Last().Content, langcode));
        }
    }
}