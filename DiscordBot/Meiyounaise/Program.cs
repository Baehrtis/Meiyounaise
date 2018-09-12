using System;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Meiyounaise.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Random = System.Random;

namespace Meiyounaise
{
    class Program
    {
        //Private Variables
        private DiscordSocketClient m_Client;
        private CommandService m_Commands;
        private IServiceProvider m_Services;

        //MAIN
        static void Main(string[] args)
            => new Program().RunAsync().GetAwaiter().GetResult();

        //STARTING UP
        private async Task RunAsync()
        {   //Discord Client
            m_Client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Info });
            //Command Service to link modules
            m_Commands = new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Debug });
            m_Services = InstallServices();
            string Token = "";
            using (var Stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\Token.txt"), FileMode.Open, FileAccess.Read))
            using (var ReadToken = new StreamReader(Stream)) { Token = ReadToken.ReadToEnd(); }
            await m_Client.LoginAsync(TokenType.Bot, Token);
            await m_Client.StartAsync();
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
            await m_Client.SetGameAsync(status[ran.Next(status.Length)], "https://twitch.tv/m3iy0u", StreamType.NotStreaming);
        }

        private async Task MessageReceived(SocketMessage MessageParam)
        {
            var Message = MessageParam as SocketUserMessage;
            var Context = new SocketCommandContext(m_Client, Message);

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.IsBot) return;

            int ArgPos = 0;
            if (!(Message.HasStringPrefix("&", ref ArgPos) || Message.HasMentionPrefix(m_Client.CurrentUser, ref ArgPos))) return;

            var Result = await m_Commands.ExecuteAsync(Context, ArgPos);
            if (!Result.IsSuccess)
            {
                Console.WriteLine($"{DateTime.Now} at Commands] Something went wrong with executing a command. Text: {Context.Message.Content} | Error: {Result.ErrorReason}");
                await Context.Channel.SendMessageAsync($"Something went wrong. Error: `{Result.ErrorReason}`");
            }
        }
        
        private Task Disconnected(Exception arg)
        {
            Console.WriteLine("Disconnected", arg);
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
            if (m_Client.LoginState != LoginState.LoggedIn) return;

            // Hook the MessageReceived Event into our Command Handler
            m_Client.MessageReceived += MessageReceived;

            // Add tasks to send Messages
            m_Client.Ready += Ready;
            m_Client.Disconnected += Disconnected;
            m_Client.Log += Log;

            // Discover all of the commands in this assembly and load them.
            await m_Commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
