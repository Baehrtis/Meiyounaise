using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;

namespace Meiyounaise.Core.Commands
{
    public class PlaySounds : ModuleBase<SocketCommandContext>
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> _connectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task Join()
        {
            try
            {
                IAudioClient audioClient = await ((IVoiceState)Context.User).VoiceChannel.ConnectAsync();
                if (_connectedChannels.TryAdd(Context.Guild.Id, audioClient))
                {
                }
            }
            catch (Exception)
            {
                await ReplyAsync("You need to be in a Voice Channel");
            }


        }
        public async Task Leave()
        {
            IAudioClient client;
            if (_connectedChannels.TryRemove(Context.Guild.Id, out client))
            {
                await client.StopAsync();
            }
        }

        [Command("play")]
        public async Task PlayTask(string name = "")
        {
            if (name == "")
            {
                await ReplyAsync("Usage: &play [Sound]\n\nAvailable Sounds:\n-`gls`\n-`kolamiteis`");
                return;
            }
            await Join();
            await SendAudioAsync(Context.Guild, Context.Channel, name);
            await Leave();
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\") + path + ".mp3";
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            IAudioClient client;
            if (_connectedChannels.TryGetValue(guild.Id, out client))
            {
                using (var ffmpeg = CreateProcess(path))
                using (var stream = client.CreatePCMStream(AudioApplication.Music))
                {
                    try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                    finally { await stream.FlushAsync(); }
                }
            }
        }

        private Process CreateProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}