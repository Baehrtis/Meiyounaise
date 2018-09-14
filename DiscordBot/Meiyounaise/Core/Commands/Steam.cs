using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SteamWebAPI2.Interfaces;

namespace Meiyounaise.Core.Commands
{
    public class Steam : ModuleBase<SocketCommandContext>
    {
        private SteamUser sInterface;
        private string key = "";

        private void getKey()
        {
            using (var Stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\SteamKey.txt"), FileMode.Open, FileAccess.Read))
            using (var ReadToken = new StreamReader(Stream))
            {
                key = ReadToken.ReadToEnd();
            }
        }

        private Embed buildSteamEmbed(ulong id)
        {
            string curl = sInterface.GetCommunityProfileAsync(id).Result.CustomURL;
            string vac = ":white_check_mark: Clean";
            string tban = ":white_check_mark: Clean";
            if (sInterface.GetCommunityProfileAsync(id).Result.IsVacBanned)
            {
                vac = ":x: Banned";
            }
            if (sInterface.GetCommunityProfileAsync(id).Result.TradeBanState != "None")
            {
                tban = $":x: {sInterface.GetCommunityProfileAsync(id).Result.TradeBanState}";
            }
            if (curl == "")
            {
                curl = "Not set!";
            }
            EmbedBuilder steamEmbed = new EmbedBuilder()
                .WithColor(0, 0, 0)
                .WithThumbnailUrl(sInterface.GetCommunityProfileAsync(id).Result.AvatarFull.AbsoluteUri)
                .WithDescription($"Currently {sInterface.GetPlayerSummaryAsync(id).Result.Data.UserStatus}")
                .WithAuthor(author =>
                {
                    author
                        .WithName(sInterface.GetPlayerSummaryAsync(id).Result.Data.Nickname)
                        .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/Steam_icon_logo.svg/512px-Steam_icon_logo.svg.png")
                        .WithUrl(sInterface.GetPlayerSummaryAsync(id).Result.Data.ProfileUrl);
                })
                .AddInlineField("VAC Status:", vac)
                .AddInlineField("Trade Ban Status:", tban)
                .AddInlineField("Visibility", sInterface.GetPlayerSummaryAsync(id).Result.Data.ProfileVisibility)
                .AddInlineField("Profile created on", sInterface.GetPlayerSummaryAsync(id).Result.Data.AccountCreatedDate.ToString())
                .AddInlineField("steamID64", id)
                .AddInlineField("Custom URL", curl);
            try
            {
                var recentGames = sInterface.GetCommunityProfileAsync(id).Result.MostPlayedGames;
                foreach (var game in recentGames)
                {
                    steamEmbed.AddInlineField(game.Name,
                        $"[{game.HoursPlayed} Hrs played last two weeks]({game.Link})");
                }
            }
            catch (Exception e)
            {
                Context.Channel.SendMessageAsync("🚫 I couldn't access the games on this account due to their Privacy Settings!");
            }
            return steamEmbed.Build();
        }

        [Command("steam")]
        public async Task steam(string idname)
        {
            char[] illegalChars = { '!', '"', '§', '$', '%', '&', '/', '(', ')', '=', '?', '`', '*', '\'', '_', ':', ';', '>', '{', '[', ']', '}', '\\', '`', '+', '#', '-', '.', ',', '<', '|' };
            if (idname.IndexOfAny(illegalChars) != -1)
            {
                await ReplyAsync("This is not a valid Custom URL/SteamID64");
                return;
            }
            getKey();
            sInterface = new SteamUser(key);
            ulong id = ulong.MinValue;
            try
            {
                id = sInterface.ResolveVanityUrlAsync(idname).Result.Data;
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
                return;
            }
            await ReplyAsync("", false, buildSteamEmbed(id));

        }

        [Command("steam")]
        public async Task steam(ulong id)
        {
            getKey();
            sInterface = new SteamUser(key);
            try
            {
                await ReplyAsync("", false, buildSteamEmbed(id));
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }
    }
}