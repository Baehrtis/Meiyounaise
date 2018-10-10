using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace Meiyounaise.Core.Data
{
    public class Guilds
    {
        private static List<Guild> guilds;

        private static readonly string GuildFile = Utilities.DataPath + "prefixes.json";
        static Guilds()
        {
            if (DataStorage<Guild>.SaveExists(GuildFile))
            {
                guilds = DataStorage<Guild>.LoadData(GuildFile).ToList();
            }
            else
            {
                guilds = new List<Guild>();
                SaveGuilds();
            }
        }

        public static void SaveGuilds()
        {
            DataStorage<Guild>.SaveData(guilds, GuildFile);
        }

        public static Guild GetGuild(SocketGuild guild)
        {
            return GetOrCreateGuild(guild.Id);
        }

        private static Guild GetOrCreateGuild(ulong id)
        {
            var result = from a in guilds
                where a.Id == id
                select a;
            var guild = result.FirstOrDefault();
            if (guild == null) guild = CreateGuildAccount(id);
            return guild;
        }

        private static Guild CreateGuildAccount(ulong id)
        {
            var newGuild = new Guild()
            {
                Id = id,
                Prefix = "&"
            };
            guilds.Add(newGuild);
            SaveGuilds();
            return newGuild;
        }
    }
}
