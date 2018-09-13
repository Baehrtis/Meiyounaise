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
                key =  ReadToken.ReadToEnd();
            }
        }
        private Embed buildSteamEmbed(ulong id)
        {
            string curl = sInterface.GetCommunityProfileAsync(id).Result.CustomURL;
            
            if (curl == "")
            {
                curl = "Not set!";
            }
            EmbedBuilder steamEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName(sInterface.GetPlayerSummaryAsync(id).Result.Data.Nickname)
                        //.WithIconUrl(sInterface.GetCommunityProfileAsync(id).Result.AvatarMedium.AbsoluteUri)
                        .WithUrl(sInterface.GetPlayerSummaryAsync(id).Result.Data.ProfileUrl);
                })
                .WithThumbnailUrl(sInterface.GetCommunityProfileAsync(id).Result.AvatarFull.AbsoluteUri)
                .AddField("steamID64", id)
                .AddField("Custom URL", curl);
            return steamEmbed.Build();
        }

        [Command("steam")]
        public async Task steam(string idname)
        {
            getKey();
            sInterface = new SteamUser(key);
            ulong id = sInterface.ResolveVanityUrlAsync(idname).Result.Data;
            try
            {
                await ReplyAsync("", false, buildSteamEmbed(id));
                string lol = "";
                var test = sInterface.GetCommunityProfileAsync(id).Result.MostPlayedGames;//sInterface.GetCommunityProfileAsync(id).Result.MostPlayedGames;
                foreach (var VARIABLE in test)
                {

                    lol+= VARIABLE.Name+ " " + VARIABLE.HoursOnRecord +"\n";
                }

                await ReplyAsync(lol);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
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