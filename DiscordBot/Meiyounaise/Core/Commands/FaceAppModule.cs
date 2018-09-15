using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FaceApp;

namespace Meiyounaise.Core.Commands
{
    public class FaceAppModule : ModuleBase<SocketCommandContext>
    {
        private async Task FaceApp(FilterType type, string url = "")
        {
            FaceAppClient faceAppClient = new FaceAppClient(new HttpClient());
            var lm = await Context.Channel.GetMessagesAsync(2).Flatten();
            var message = lm.Last();//GET LAST MESSAGE
            string durl;
            if (Context.Message.Attachments.Count != 0)//CURRENT MESSAGE HAS ATTACHMENT
            {
                durl = Context.Message.Attachments.FirstOrDefault()?.Url;
            }
            else//CURRENT MESSAGE DOES NOT HAVE ATTACHMENT
            {
                if (url != "")//IMAGE URL PROVIDED
                {
                    durl = url;
                }
                else//NO IMAGE URL PROVIDED
                {
                    if (message.Attachments.Count != 0)
                    {
                        durl = message.Attachments.FirstOrDefault()?.Url;
                    }
                    else
                    {
                        durl = message.Content;
                    }
                }
            }
            if (Uri.TryCreate(durl, UriKind.Absolute, out Uri uri))
            {
                try
                {
                    var code = await faceAppClient.GetCodeAsync(uri);
                    using (var imgStream = await faceAppClient.ApplyFilterAsync(code, type))
                    {
                        var stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\fa.png"), FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                        await imgStream.CopyToAsync(stream);
                        stream.Close();
                        imgStream.Close();
                        await Context.Channel.SendFileAsync(
                            (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(
                                @"bin\Debug\netcoreapp2.1", @"Data\fa.png"));
                        File.Delete((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(
                            @"bin\Debug\netcoreapp2.1", @"Data\fa.png"));
                    }
                }
                catch (FaceException e)
                {
                    await ReplyAsync("❌ " + e.Message);
                    await ReplyAsync( "This either means that the API didn't recognize a face, or that the Bot is being rate limited");
                }
            }
        }

        [Command("faceapp"),Alias("fa")]
        public async Task FaceAppH()
        {
            string result = "**Available Filters:**\nOld, Smile, Smile_2, Young, Hot, Female, Female_2, Pan, Male, Glasses, Hollywood, Goatee, Impression, Heisenberg, Hitman,  Bangs, Wave, Makeup, Mustache, Lion, Hipster";
            result += "\n**Usage:** &[FILTER] (URL)\nIf you provide no attached Image nor a URL, the Bot will check for it in the last message in the channel";
            await ReplyAsync(result);
        }

        [Command("old")]
        public async Task Old(string url = "")
        {
            await FaceApp(FilterType.Old, url);
        }

        [Command("smile")]
        public async Task Smile(string url = "")
        {
            await FaceApp(FilterType.Smile, url);
        }

        [Command("smile_2")]
        public async Task Smile_2(string url = "")
        {
            await FaceApp(FilterType.Smile_2, url);
        }

        [Command("young")]
        public async Task Young(string url = "")
        {
            await FaceApp(FilterType.Young, url);
        }

        [Command("hot")]
        public async Task Hot(string url = "")
        {
            await FaceApp(FilterType.Hot, url);
        }

        [Command("female")]
        public async Task Female(string url = "")
        {
            await FaceApp(FilterType.Female, url);
        }

        [Command("female_2")]
        public async Task Female_2(string url = "")
        {
            await FaceApp(FilterType.Female_2, url);
        }

        [Command("pan")]
        public async Task Pan(string url = "")
        {
            await FaceApp(FilterType.Pan, url);
        }

        [Command("male")]
        public async Task Male(string url = "")
        {
            await FaceApp(FilterType.Male, url);
        }

        [Command("glasses")]
        public async Task Glasses(string url = "")
        {
            await FaceApp(FilterType.Glasses, url);
        }

        [Command("hollywood")]
        public async Task Holly(string url = "")
        {
            await FaceApp(FilterType.Hollywood, url);
        }

        [Command("goatee")]
        public async Task Goatee(string url = "")
        {
            await FaceApp(FilterType.Goatee, url);
        }

        [Command("impression")]
        public async Task Impression(string url = "")
        {
            await FaceApp(FilterType.Impression, url);
        }

        [Command("heisenberg")]
        public async Task Heisen(string url = "")
        {
            await FaceApp(FilterType.Heisenberg, url);
        }

        [Command("hitman")]
        public async Task Hitman(string url = "")
        {
            await FaceApp(FilterType.Hitman, url);
        }

        [Command("bangs")]
        public async Task Bangs(string url = "")
        {
            await FaceApp(FilterType.Bangs, url);
        }

        [Command("wave")]
        public async Task Wave(string url = "")
        {
            await FaceApp(FilterType.Wave, url);
        }

        [Command("makeup")]
        public async Task Makeup(string url = "")
        {
            await FaceApp(FilterType.Makeup, url);
        }

        [Command("mustache")]
        public async Task Mustache(string url = "")
        {
            await FaceApp(FilterType.Mustache, url);
        }

        [Command("lion")]
        public async Task Lion(string url = "")
        {
            await FaceApp(FilterType.Lion, url);
        }

        [Command("hipster")]
        public async Task Hipster(string url = "")
        {
            await FaceApp(FilterType.Hipster, url);
        }
    }
}
