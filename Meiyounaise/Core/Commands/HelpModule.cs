using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    [Name("Help")]
    public class HelpModule : InteractiveBase
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            var pages = new List<string>();
            var modules = _service.Modules.Where(x => !x.Name.Contains("DS"));
            foreach (var module in modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"&{cmd.Aliases.First()}\n";
                }

                if (!string.IsNullOrWhiteSpace(description) && module.Name != "Help")
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"**Category:** {module.Name}\n");
                    sb.AppendLine(description);
                    pages.Add(sb.ToString());
                }
            }

            var msg = new PaginatedMessage
            {
                Color = Color.Teal,
                Options = new PaginatedAppearanceOptions()
                {
                    JumpDisplayOptions = 0,
                    Timeout = TimeSpan.FromSeconds(60)
                },
                Pages = pages,
                Author = new EmbedAuthorBuilder() { Name = Context.User.Username, IconUrl = Context.User.GetAvatarUrl() },
                Title = $"Commands | Prefix is & or {Context.User.Mention}"

            };
            await PagedReplyAsync(msg);
        }

        [Command("help")]
        [Summary("Search for a specific command.")]
        public async Task HelpAsync(string command)
        {
            var result = _service.Search(Context, command);
            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command called `{command}`.");
                return;
            }
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"Commands matching **{command}**"
            };
            int i = 1;
            string test = "";
            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                var prms = cmd.Parameters.Select(p => p.Name).ToList();
                builder.AddField($"{i}: {string.Join(", ", cmd.Aliases)}",
                    prms.Count == 0
                        ? $"Parameters: None\nSummary: {cmd.Summary}"
                        : $"Parameters: {string.Join(", ", prms)}\nSummary: {cmd.Summary}");
                i++;
            }
            await ReplyAsync(test, false, builder.Build());
        }
    }
}
