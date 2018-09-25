using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class Pictures : ModuleBase<SocketCommandContext>
    {
        [Command("angefahren")]
        public async Task Angefahren()
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
