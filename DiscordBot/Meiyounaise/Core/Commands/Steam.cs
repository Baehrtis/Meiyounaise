using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SteamWebAPI2.Interfaces;
using SteamStoreQuery;

namespace Meiyounaise.Core.Commands
{
    [SuppressMessage("ReSharper", "SpecifyACultureInStringConversionExplicitly")]
    public class Steam : ModuleBase<SocketCommandContext>
    {
        private SteamUser _sInterface;
        private string _key = "";

        private void GetKey()
        {
            using (var stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\SteamKey.txt"), FileMode.Open, FileAccess.Read))
            using (var readToken = new StreamReader(stream))
            {
                _key = readToken.ReadToEnd();
            }
        }

        private EmbedBuilder BuildBaseEmbed(ulong id)
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

        private Embed BuildPublicEmbed(ulong id)
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

        private Embed BuildPrivateEmbed(ulong id, string curl = "")
        {
            EmbedBuilder privateEmbed = BuildBaseEmbed(id)
                .AddInlineField("Profile created on", "🚫Private Profile")
                .AddInlineField("steamID64", id);
            privateEmbed.AddInlineField("Custom URL", curl == "" ? "🚫Private Profile" : curl);
            privateEmbed.AddInlineField("Games", "🚫 I couldn't access the games on this account due to their Privacy Settings!");
            return privateEmbed.Build();
        }

        [Command("steam")]
        public async Task SteamTask(string idname)
        {
            char[] illegalChars = { '!', '"', '§', '$', '%', '&', '/', '(', ')', '=', '?', '`', '*', '\'', '_', ':', ';', '>', '{', '[', ']', '}', '\\', '`', '+', '#', '-', '.', ',', '<', '|' };
            if (idname.IndexOfAny(illegalChars) != -1)
            {
                await ReplyAsync("This is not a valid Custom URL/SteamID64");
                return;
            }
            GetKey();
            _sInterface = new SteamUser(_key);
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

        [Command("steam")]
        public async Task SteamTask(ulong id)
        {
            GetKey();
            _sInterface = new SteamUser(_key);
            if (_sInterface.GetCommunityProfileAsync(id).Result.VisibilityState.ToString() == "1")//CHECK IF PROFILE IS PRIVATE
            {
                await ReplyAsync("", false, BuildPrivateEmbed(id));
                return;
            }
            await ReplyAsync("", false, BuildPublicEmbed(id));
        }

        [Command("game"),Alias("sg","g")]
        public async Task Game([Remainder]string name)
        {
            GetKey();
            string price = "F2P";
            var result = Query.Search(name);
            if (result.Count<1)
            {
                await ReplyAsync("❌ I found no games using this search query!");
                return;
            }

            if (result[0].PriceUSD!=null)
            {
                price = result[0].PriceUSD.ToString()+"$";
            }

            var appid = result[0].AppId;
            var store = new SteamStore();
            var game = store.GetStoreAppDetailsAsync(Convert.ToUInt32(result[0].AppId)).Result;

            if (game.ReleaseDate.ComingSoon && price=="F2P")
            {
                price = "Not yet available";
            }
            var userStats = new SteamUserStats(_key);
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
            if (game.Metacritic!=null)
            {
                embed.AddInlineField("Metacritic", $"[{game.Metacritic.Score}]({game.Metacritic.Url})");
            }
            else
            {
                if (game.Recommendations!=null)
                {
                    embed.AddInlineField("Recommendations", game.Recommendations.Total);
                }
                else
                {
                    embed.AddInlineField("Recommendations", "🚫 None available");
                }
            }

            embed.AddInlineField("Release Date", game.ReleaseDate.Date);
            if (game.Website!=null)
            {
                embed.AddField("Website", game.Website);
            }
            await ReplyAsync("", false, embed.Build());
        }
    }
}