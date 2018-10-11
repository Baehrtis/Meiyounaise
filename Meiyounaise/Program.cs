using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Addons.Interactive;
using System.Reflection;
using Meiyounaise.Core;
using Meiyounaise.Core.Data;

namespace Meiyounaise
{
    class Program
    {
        static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task MainAsync()
        {
            var token = Utilities.GetKey("token");

            _client = new DiscordSocketClient();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<InteractiveService>()
                .AddSingleton<AntiSpamService>()
                .BuildServiceProvider();

            _commands = new CommandService();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            _client.MessageReceived += HandleCommandAsync;
            _client.Ready += Ready;
            _client.Log += Log;

            await Task.Delay(-1);
        }

        public async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg)) return;
            if (msg.Author.IsBot) return;
            
            var message = (SocketUserMessage)m;
            int argPos = 0;

            var context = new SocketCommandContext(_client, msg);
            if (!(msg.HasStringPrefix((Guilds.GetGuild(context.Guild).Prefix), ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            if (AntiSpamService.ContainsUser(m.Author.Id))
            {
                await context.Channel.SendMessageAsync(
                    $"**{context.Message.Author.Username + "#" + context.Message.Author.Discriminator}:** No Spammerino in the Chatterino\n(Please wait another {AntiSpamService.GetTimeForUser(m.Author.Id).Seconds}.{AntiSpamService.GetTimeForUser(m.Author.Id).Milliseconds}s)");
                return;
            }

            AntiSpamService.RateLimitUser(m);
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync($"Something went wrong. Error: `{result.ErrorReason}`");
        }

        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            await Task.CompletedTask;
        }

        private async Task Ready()
        {
            Random ran = new Random();
            string[] status = { "stndbild😡", "Kack Drecks Wissenschaftliche Arbeit Hurensohn", "Fuck auf die Hater, Hans ist da", "thema meiyou naise" };
            await _client.SetGameAsync(status[ran.Next(status.Length)]);
        }
    }
}
