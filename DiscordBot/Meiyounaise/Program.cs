using System;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Random = System.Random;

namespace Meiyounaise
{
    class Program
    {
        //Private Variables
        private DiscordSocketClient _mClient;
        private CommandService _mCommands;
        // ReSharper disable once NotAccessedField.Local
        private IServiceProvider _mServices;

        //MAIN
        static void Main()
            => new Program().RunAsync().GetAwaiter().GetResult();

        //STARTING UP
        private async Task RunAsync()
        {   //Discord Client
            _mClient = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Info });
            //Command Service to link modules
            _mCommands = new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Debug });
            _mServices = InstallServices();
            string token;
            using (var stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\Token.txt"), FileMode.Open, FileAccess.Read))
            using (var readToken = new StreamReader(stream)) { token = readToken.ReadToEnd(); }
            await _mClient.LoginAsync(TokenType.Bot, token);
            await _mClient.StartAsync();
            await InstallCommands();

            await Task.Delay(-1);
        }
//        private async Task Client_Log(LogMessage Message)
//        {
//            Console.WriteLine($"{DateTime.Now} at {Message.Source}] {Message.Message}");
//        }
        private async Task Ready()
        {
            Random ran = new Random();
            string[] status = { "stndbild😡", "Kack Drecks Wissenschaftliche Arbeit Hurensohn", "Fuck auf die Hater, Hans ist da" };
            await _mClient.SetGameAsync(status[ran.Next(status.Length)], "https://twitch.tv/m3iy0u");
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            var context = new SocketCommandContext(_mClient, message);

            if (context.Message == null || context.Message.Content == "") return;
            if (context.User.IsBot) return;

            int argPos = 0;
            if (!(message.HasStringPrefix("&", ref argPos) || message.HasMentionPrefix(_mClient.CurrentUser, ref argPos))) return;

            var result = await _mCommands.ExecuteAsync(context, argPos);
            if (!result.IsSuccess)
            {
                Console.WriteLine($"{DateTime.Now} at Commands] Something went wrong with executing a command. Text: {context.Message.Content} | Error: {result.ErrorReason}");
                await context.Channel.SendMessageAsync($"Something went wrong. Error: `{result.ErrorReason}`");
            }
        }
        
        private Task Disconnected(Exception arg)
        {
            Console.WriteLine("Disconnected");
            return Task.CompletedTask;
        }

        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            await Task.CompletedTask;
        }

        private IServiceProvider InstallServices()
        {
            ServiceCollection services = new ServiceCollection();
            // Add all additional services here.
           // Return the service provider.
            return services.BuildServiceProvider();
        }

        private async Task InstallCommands()
        {
            // Before we install commands, we should check if everything was set up properly. Check if logged in.
            if (_mClient.LoginState != LoginState.LoggedIn) return;

            // Hook the MessageReceived Event into our Command Handler
            _mClient.MessageReceived += MessageReceived;

            // Add tasks to send Messages
            _mClient.Ready += Ready;
            _mClient.Disconnected += Disconnected;
            _mClient.Log += Log;

            // Discover all of the commands in this assembly and load them.
            await _mCommands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
