using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FixerSharp;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using SteamStoreQuery;
using SteamWebAPI2.Interfaces;
using WAWrapper;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace Meiyounaise.Core.Commands
{
    [Name("Tools")]
    public class ToolModule : ModuleBase<SocketCommandContext>
    {
        private static readonly GoogleCredential Credential = GoogleCredential.FromFile(Utilities.DataPath + "gTranslateKey.json");
        private static SteamUser _sInterface;
        private static readonly string Key = Utilities.GetKey("steamkey");

        //STEAM
        [Command("steam")]
        [Summary("Returns information about a steam user.")]
        public async Task SteamTask(ulong id)
        {
            _sInterface = new SteamUser(Key);
            if (_sInterface.GetCommunityProfileAsync(id).Result.VisibilityState.ToString() == "1")//CHECK IF PROFILE IS PRIVATE
            {
                await ReplyAsync("", false, BuildPrivateEmbed(id));
                return;
            }
            await ReplyAsync("", false, BuildPublicEmbed(id));
        }

        [Command("game"), Alias("sg", "g")]
        [Summary("Returns information about a game on steam.")]
        public async Task Game([Remainder]string name)
        {
            string price = "F2P";
            var result = Query.Search(name);
            if (result.Count < 1)
            {
                await ReplyAsync("❌ I found no games using this search query!");
                return;
            }

            if (result[0].PriceUSD != null)
            {
                price = result[0].PriceUSD.ToString() + "$";
            }

            var appid = result[0].AppId;
            var store = new SteamStore();
            var game = store.GetStoreAppDetailsAsync(Convert.ToUInt32(result[0].AppId)).Result;

            if (game.ReleaseDate.ComingSoon && price == "F2P")
            {
                price = "Not yet available";
            }
            var userStats = new SteamUserStats(Key);
            var playerCount = userStats.GetNumberOfCurrentPlayersForGameAsync(Convert.ToUInt32(appid));
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(0, 0, 0)
                .WithAuthor(author =>
                {
                    author
                        .WithName(result[0].Name)
                        .WithIconUrl(
                            "https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/Steam_icon_logo.svg/512px-Steam_icon_logo.svg.png")
                        .WithUrl($"https://store.steampowered.com/app/{appid}/");
                })
                .WithThumbnailUrl(result[0].ImageLink);
            embed.AddInlineField("Price", price);
            if (game.ReleaseDate.ComingSoon)
            {
                embed.AddInlineField("Current Players", "🚫 Not available");
            }
            else
            {
                embed.AddInlineField("Current Players", playerCount.Result.Data);
            }
            if (game.Metacritic != null)
            {
                embed.AddInlineField("Metacritic", $"[{game.Metacritic.Score}]({game.Metacritic.Url})");
            }
            else
            {
                if (game.Recommendations != null)
                {
                    embed.AddInlineField("Recommendations", game.Recommendations.Total);
                }
                else
                {
                    embed.AddInlineField("Recommendations", "🚫 None available");
                }
            }

            embed.AddInlineField("Release Date", game.ReleaseDate.Date);
            if (game.Website != null)
            {
                embed.AddField("Website", game.Website);
            }
            await ReplyAsync("", false, embed.Build());
        }
        
        //WOLFRAM ALPHA
        [Command("wa")]
        [Summary("Evaluates your query with Wolfram Alpha and returns the result.")]
        public async Task WolframTask([Remainder]string input)
        {
            var client = new WAEngine {APIKey = Utilities.GetKey("wolframkey")};
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
                            if (subpod.PlainText != "" && i < 25)
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

        //MONEY
        [Command("money")]
        [Summary("Convert money into another currency.")]
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

        //TRANSLATE TO DE
        [Command("de")]
        [Summary("Translates your message to german.")]
        public async Task Deutsch([Remainder] string text)
        {
            await ReplyAsync(GTranslate(text, LanguageCodes.German));
        }
        
        //TRANSLATE TO EN
        [Command("en")]
        [Summary("Translates your message to english.")]
        public async Task Englisch([Remainder] string text)
        {
            await ReplyAsync(GTranslate(text, LanguageCodes.English));
        }
        
        //TRANSLATE TO ANY LANGUAGE
        [Command("translate")]
        [Summary("Translates your text into the desired language. If you enter \"codes\" instead of a code the bot will dm you a list of codes.")]
        public async Task AnyLanguage(string langcode, [Remainder]string text)
        {
            await ReplyAsync(GTranslate(text, langcode));
        }

        //STEAM EMBEDS
        private static EmbedBuilder BuildBaseEmbed(ulong id)
        {
            string vac = ":white_check_mark: Clean";
            string tban = ":white_check_mark: Clean";
            if (_sInterface.GetCommunityProfileAsync(id).Result.IsVacBanned)
            {
                vac = ":x: Banned";
            }
            if (_sInterface.GetCommunityProfileAsync(id).Result.TradeBanState != "None")
            {
                tban = $":x: {_sInterface.GetCommunityProfileAsync(id).Result.TradeBanState}";
            }
            EmbedBuilder baseEmbed = new EmbedBuilder()
                .WithColor(0, 0, 0)
                .WithThumbnailUrl(_sInterface.GetCommunityProfileAsync(id).Result.AvatarFull.AbsoluteUri)
                .WithDescription($"Currently {_sInterface.GetPlayerSummaryAsync(id).Result.Data.UserStatus}")
                .WithAuthor(author =>
                {
                    author
                        .WithName(_sInterface.GetPlayerSummaryAsync(id).Result.Data.Nickname)
                        .WithIconUrl(
                            "https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/Steam_icon_logo.svg/512px-Steam_icon_logo.svg.png")
                        .WithUrl(_sInterface.GetPlayerSummaryAsync(id).Result.Data.ProfileUrl);
                })
                .AddInlineField("VAC Status:", vac)
                .AddInlineField("Trade Ban Status:", tban)
                .AddInlineField("Visibility", _sInterface.GetPlayerSummaryAsync(id).Result.Data.ProfileVisibility);
            return baseEmbed;
        }

        private static Embed BuildPublicEmbed(ulong id)
        {
            EmbedBuilder publicEmbed = BuildBaseEmbed(id);
            string curl = _sInterface.GetCommunityProfileAsync(id).Result.CustomURL;
            if (curl == "")
            {
                curl = "Not set!";
            }
            publicEmbed
            .AddInlineField("Profile created on", _sInterface.GetPlayerSummaryAsync(id).Result.Data.AccountCreatedDate.ToString())
            .AddInlineField("steamID64", id)
            .AddInlineField("Custom URL", curl);
            try
            {
                var recentGames = _sInterface.GetCommunityProfileAsync(id).Result.MostPlayedGames;
                foreach (var game in recentGames)
                {
                    publicEmbed.AddInlineField(game.Name,
                        $"[{game.HoursPlayed} Hrs played last two weeks]({game.Link})");
                }
            }
            catch (Exception)
            {
                publicEmbed.AddInlineField("Games", "🚫 I couldn't access the games on this account due to their Privacy Settings!");
            }
            return publicEmbed.Build();
        }

        private static Embed BuildPrivateEmbed(ulong id, string curl = "")
        {
            EmbedBuilder privateEmbed = BuildBaseEmbed(id)
                .AddInlineField("Profile created on", "🚫Private Profile")
                .AddInlineField("steamID64", id);
            privateEmbed.AddInlineField("Custom URL", curl == "" ? "🚫Private Profile" : curl);
            privateEmbed.AddInlineField("Games", "🚫 I couldn't access the games on this account due to their Privacy Settings!");
            return privateEmbed.Build();
        }

        //TRANSLATING
        private static string GTranslate(string text, string lang)
        {
            var client = TranslationClient.Create(Credential);
            return client.TranslateText(text, lang).TranslatedText;
        }

        [Name("DS")]
        public class BonusCommands : ModuleBase
        {
            //TRANSLATE LAST MESSAGE TO DE
            [Command("de"), Name("DS")]
            [Summary("Translates the last message to german.")]
            public async Task Deutsch2()
            {
                var message = await Context.Channel.GetMessagesAsync(2).Flatten();
                await ReplyAsync(GTranslate(message.Last().Content, LanguageCodes.German));
            }

            //TRANSLATE LAST MESSAGE TO EN
            [Command("en"), Name("DS")]
            [Summary("Translates the last message to english.")]
            public async Task Englisch2()
            {
                var message = await Context.Channel.GetMessagesAsync(2).Flatten();
                await ReplyAsync(GTranslate(message.Last().Content, LanguageCodes.English));
            }

            //TRANSLATE TO ANY LANGUAGE
            [Command("translate"), Name("DS")]
            [Summary("Returns the last message in your provided language.")]
            public async Task AnyLanguage2(string langcode)
            {
                if (langcode == "codes")
                {
                    var embed = new EmbedBuilder();
                    var embed2 = new EmbedBuilder();
                    var embed3 = new EmbedBuilder();
                    var embed4 = new EmbedBuilder();
                    var embed5 = new EmbedBuilder();
                    TranslationClient client = TranslationClient.Create(Credential);
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

            [Command("steam")]
            [Summary("Returns information about a steam user.")]
            public async Task SteamTask(string idname)
            {
                char[] illegalChars = { '!', '"', '§', '$', '%', '&', '/', '(', ')', '=', '?', '`', '*', '\'', '_', ':', ';', '>', '{', '[', ']', '}', '\\', '`', '+', '#', '-', '.', ',', '<', '|' };
                if (idname.IndexOfAny(illegalChars) != -1)
                {
                    await ReplyAsync("This is not a valid Custom URL/SteamID64");
                    return;
                }
                _sInterface = new SteamUser(Key);
                ulong id;
                try
                {
                    id = _sInterface.ResolveVanityUrlAsync(idname).Result.Data;
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message);
                    return;
                }

                if (_sInterface.GetCommunityProfileAsync(id).Result.VisibilityState.ToString() == "1")//CHECK IF PROFILE IS PRIVATE
                {
                    await ReplyAsync("", false, BuildPrivateEmbed(id, idname));
                    return;
                }
                await ReplyAsync("", false, BuildPublicEmbed(id));
            }

        }
    }
}