﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Meiyounaise.Core.Commands
{
    [Name("PlaySounds")]
    public class PlaySoundModule : ModuleBase<SocketCommandContext>
    {
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Play a sound.")]
        [RequireBotPermission(GuildPermission.Speak)]
        public async Task PlayTask(string name = "")
        {
            if (name == "")
            {
                string[] files = Directory.GetFiles(Utilities.DataPath, "*.mp3");
                string result = "";
                foreach (var file in files)
                {
                    string file2 = file.Substring(file.IndexOf("Data") + 5);
                    file2 = file2.Substring(0, file2.IndexOf(".mp3"));
                    result += $"-`{file2}`\n";
                }
                await ReplyAsync($"Usage: &play [Sound]\n\nAvailable Sounds:\n{result}");
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

        public async Task SendAudioAsync(IAudioClient client, IGuild guild, IMessageChannel channel, string fn)
        {
            string path = Utilities.DataPath + fn + ".mp3";
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

        private static Process CreateProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        [Name("DS")]
        public class BonusCommands : ModuleBase
        {
            //DOWNLOAD AUDIO
            [Command("dla")]
            [RequireOwner]
            public async Task DownloadAudio(string name, string url)
            {
                try
                {
                    var handler = new HttpClientHandler();
                    using (var httpClient = new HttpClient(handler, false))
                    {
                        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            using (
                                Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                                stream = new FileStream(Utilities.DataPath + name + ".mp3", FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                            {
                                await contentStream.CopyToAsync(stream);
                            }
                        }
                    }
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message);
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            //REMOVE AUDIO
            [Command("rma")]
            [RequireOwner]
            public async Task Delete(string name)
            {
                try
                {
                    File.Delete(Utilities.DataPath + name + ".mp3");
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message);
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}