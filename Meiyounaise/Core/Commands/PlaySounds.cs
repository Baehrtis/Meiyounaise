using System;
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
        [Command("play")]
        public async Task PlayTask(string name = "")
        {
            if (name == "")
            {
                await ReplyAsync("Usage: &play [Sound]\n\nAvailable Sounds:\n-`gls`\n-`kolamiteis`");
                return;
            }
            IAudioClient audioClient = null;
            try
            {
                audioClient = await ((IVoiceState)Context.User).VoiceChannel.ConnectAsync();
            }
            catch (Exception)
            {
                await ReplyAsync("Bot couldn't connect, make sure you're in a voice channel!");
            }
            if (audioClient != null) await SendAudioAsync(audioClient, Context.Guild, Context.Channel, name);
            if (audioClient != null) await audioClient.StopAsync();
        }

        public async Task SendAudioAsync(IAudioClient client, IGuild guild, IMessageChannel channel, string path)
        {
            path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\") + path + ".mp3";
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync("No Sound with that name!");
                return;
            }

            using (var ffmpeg = CreateProcess(path))
            using (var stream = client.CreatePCMStream(AudioApplication.Music))
            {
                try
                {
                    await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
                }
                finally
                {
                    await stream.FlushAsync();
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