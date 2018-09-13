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

        private Embed buildSteamEmbed(ulong id)
        {
            EmbedBuilder steamEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName(sInterface.GetPlayerSummaryAsync(id).Result.Data.Nickname)
                        //.WithIconUrl(sInterface.GetCommunityProfileAsync(id).Result.AvatarMedium.AbsoluteUri)
                        .WithUrl($"https://steamcommunity.com/profiles/{id}/");
                })
                .WithThumbnailUrl(sInterface.GetCommunityProfileAsync(id).Result.AvatarFull.AbsoluteUri)
                .AddField("steamID64", id)
                .AddField("Custom URL", sInterface.GetPlayerSummaryAsync(id).Result.Data.Nickname);
            return steamEmbed.Build();
        }

        [Command("steam")]
        public async Task steam(string idname)
        {
            using (var Stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\SteamKey.txt"), FileMode.Open, FileAccess.Read))
            using (var ReadToken = new StreamReader(Stream))
            {
                key = ReadToken.ReadToEnd();
            }
            sInterface = new SteamUser(key);
            ulong id = sInterface.ResolveVanityUrlAsync(idname).Result.Data;
            await ReplyAsync("", false, buildSteamEmbed(id));
        }

        [Command("steam")]
        public async Task steam(ulong id)
        {
            using (var Stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\SteamKey.txt"), FileMode.Open, FileAccess.Read))
            using (var ReadToken = new StreamReader(Stream))
            {
                key = ReadToken.ReadToEnd();
            }
            sInterface = new SteamUser(key);
            await ReplyAsync("", false, buildSteamEmbed(id));
        }
    }
}