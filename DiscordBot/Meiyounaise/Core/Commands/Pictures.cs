using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class Pictures : ModuleBase<SocketCommandContext>
    {
        [Command("bastard")]
        public async Task Bastard()
        {
            await Context.Channel.SendFileAsync((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\bastard.jpg"));
        }

        [Command("blod")]
        public async Task Blod()
        {
            await Context.Channel.SendFileAsync((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\blod.png"));
        }
    }
}