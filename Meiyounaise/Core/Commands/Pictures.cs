using System.Threading.Tasks;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class Pictures : ModuleBase<SocketCommandContext>
    {
        [Command("angefahren")]
        public async Task Angefahren()
        {
            await Context.Channel.SendFileAsync(Utilities.dataPath + "bastard.jpg");
        }

        [Command("blod")]
        public async Task Blod()
        {
            await Context.Channel.SendFileAsync(Utilities.dataPath + "blod.png");
        }
    }
}
