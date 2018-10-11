using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class RemindModule : ModuleBase<SocketCommandContext>
    {
        private RemindService _remindService;

        public RemindModule(RemindService service)
        {
            _remindService = service;
        }

        [Command("remind")]
        public async Task Remind(double time, [Remainder]string message)
        {
            await _remindService.SetReminder(Context, time, message);
        }
    }
}
