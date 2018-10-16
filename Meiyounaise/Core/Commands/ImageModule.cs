using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Meiyounaise.Core.Commands.FaceAppHelper;
using Newtonsoft.Json.Linq;
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Meiyounaise.Core.Commands
{
    [Name("Images")]
    public class ImageModule : ModuleBase<SocketCommandContext>
    {
        //PICTURES
        [Command("angefahren"),Summary("Ich hoffe du wirst ehrenlos angefahren.")]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task Angefahren()
        {
            await Context.Channel.SendFileAsync(Utilities.DataPath + "bastard.jpg");
        }
        [Command("blod"),Summary("Bist du blöd?")]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task Blod()
        {
            await Context.Channel.SendFileAsync(Utilities.DataPath + "blod.png");
        }
        [Command("despacito"),Summary("Despacito like wers kennt.")]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task Despacito()
        {
            await Context.Channel.SendFileAsync(Utilities.DataPath + "despacito.png");
        }
        [Command("fuckingshit"),Summary("Fucking shit")]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task Fuckingshit()
        {
            await Context.Channel.SendFileAsync(Utilities.DataPath + "fuckingshit.jpg");
        }

        //EMOTION
        [Command("emotion")]
        [Summary("Recognize the emotion in the provided picture.")]
        public async Task Emotion(string url = "")
        {
            string key = Utilities.GetKey("emotionkey");
            var lm = await Context.Channel.GetMessagesAsync(2).Flatten();
            var message = lm.Last(); //GET LAST MESSAGE
            url = Utilities.GetImageFromCurrentOrLastMessage(url, message, Context);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            bool cont = true;
            string imageFilePath = Utilities.DataPath + "emotion.png";
            try
            {
                await Utilities.DownloadAsync(new Uri(url), imageFilePath);
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Error downloading picture: `{e.Message}`");
                cont = false;
            }

            if (cont)
            {
                try
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
                    var uri =
                        "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

                    HttpResponseMessage response;
                    string responseContent;

                    byte[] byteData = GetImageAsByteArray(imageFilePath);

                    using (var content = new ByteArrayContent(byteData))
                    {
                        // This example uses content type "application/octet-stream".
                        // The other content types you can use are "application/json" and "multipart/form-data".
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        response = await client.PostAsync(uri, content);
                        responseContent = response.Content.ReadAsStringAsync().Result;
                    }
                    // Processing the JSON into manageable objects.
                    JToken rootToken = JArray.Parse(responseContent).First;
                    // First token is always the faceRectangle identified by the API.
                    JToken faceRectangleToken = rootToken.First;
                    // Second token is all emotion scores.
                    JToken scoresToken = rootToken.Last;
                    // Show all face rectangle dimensions
                    JEnumerable<JToken> unused = faceRectangleToken.First.Children();
                    JEnumerable<JToken> unused2 = scoresToken.First.Children();
                    string result = scoresToken.ToString();
                    result = result.Substring(result.IndexOf("\"emotion\": {") + 12);
                    result.Remove(result.IndexOf("  },"));
                    string fresult = result.Remove(result.IndexOf("},"));
                    fresult = fresult.Replace(" ", "");
                    fresult = fresult.Replace("\"", "");
                    fresult = fresult.Replace(":", ": ");
                    fresult = fresult.Replace(",", "");
                    await Context.Channel.SendMessageAsync(fresult);
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync($"Error: `{e.Message}`");
                    await Context.Channel.SendMessageAsync(
                        "This means that either the API didn't recognize a face, or shit itself in other ways. Try a different picture");
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(Utilities.DataPath + "emotion.png");
        }
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
       
        //SCREENSHOT
        [Command("ss"), Summary("Screenshots a Website for you.")]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task ScreenShot(string url)
        {
            Stopwatch stopwatch = new Stopwatch();
            string utd = $"https://api.thumbnail.ws/api/{Utilities.GetKey("sskey")}/thumbnail/get?url={url}&width=1000";
            var om = await Context.Channel.SendMessageAsync("Ok, this could take a little bit!");
            stopwatch.Start();
            var typing = Context.Channel.EnterTypingState();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, utd))
                    {
                        using (
                            Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                            stream = new FileStream(Utilities.DataPath + "ss.jpg", FileMode.Create, FileAccess.Write,
                                FileShare.None, 4096, true))
                        {
                            await contentStream.CopyToAsync(stream);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
            typing.Dispose();
            await Context.Channel.SendFileAsync(Utilities.DataPath + "ss.jpg");
            stopwatch.Stop();
            await om.ModifyAsync(x => x.Content = $"⏲ Took {stopwatch.Elapsed.Seconds},{stopwatch.Elapsed.Milliseconds} seconds");
            File.Delete(Utilities.DataPath + "ss.jpg");
        }
        
        //FACEAPP
        [Command("faceapp"), Alias("fa")]
        [Summary("List the FaceApp Filters the bot can use.")]
        public async Task FaceAppH()
        {
            string result =
                "**Available Filters:**\nOld, Smile, Smile_2, Young, Hot, Female, Female_2, Pan, Male, Glasses, Hollywood, Goatee, Impression, Heisenberg, Hitman,  Bangs, Wave, Makeup, Mustache, Lion, Hipster";
            result +=
                "\n**Usage:** &[FILTER] (URL)\nIf you provide no attached Image nor a URL, the Bot will check for it in the last message in the channel";
            await ReplyAsync(result);
        }

        [Name("DS")]
        public class Filters : ModuleBase
        {
            private async Task FaceApp(FilterType type, string url = "")
            {
                FaceAppClient faceAppClient = new FaceAppClient(new HttpClient());
                var lm = await Context.Channel.GetMessagesAsync(2).Flatten();
                var message = lm.Last(); //GET LAST MESSAGE
                string durl = Utilities.GetImageFromCurrentOrLastMessage(url,message,Context as SocketCommandContext);
                
                if (Uri.TryCreate(durl, UriKind.Absolute, out Uri uri))
                {
                    try
                    {
                        var code = await faceAppClient.GetCodeAsync(uri);
                        using (var imgStream = await faceAppClient.ApplyFilterAsync(code, type))
                        {
                            var stream = new FileStream(Utilities.DataPath + "fa.png", FileMode.Create, FileAccess.Write,
                                FileShare.None, 4096, true);
                            await imgStream.CopyToAsync(stream);
                            stream.Close();
                            imgStream.Close();
                            await Context.Channel.SendFileAsync(Utilities.DataPath + "fa.png");
                            File.Delete(Utilities.DataPath + "fa.png");
                        }
                    }
                    catch (FaceException e)
                    {
                        await ReplyAsync("❌ " + e.Message);
                        await ReplyAsync(
                            "This either means that the API didn't recognize a face, or that the Bot is being rate limited");
                    }
                }
            }

            [Command("old")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Old(string url = "")
            {
                await FaceApp(FilterType.Old, url);
            }

            [Command("smile")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Smile(string url = "")
            {
                await FaceApp(FilterType.Smile, url);
            }

            [Command("smile_2")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Smile_2(string url = "")
            {
                await FaceApp(FilterType.Smile_2, url);
            }

            [Command("young")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Young(string url = "")
            {
                await FaceApp(FilterType.Young, url);
            }

            [Command("hot")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Hot(string url = "")
            {
                await FaceApp(FilterType.Hot, url);
            }

            [Command("female")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Female(string url = "")
            {
                await FaceApp(FilterType.Female, url);
            }

            [Command("female_2")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Female_2(string url = "")
            {
                await FaceApp(FilterType.Female_2, url);
            }

            [Command("pan")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Pan(string url = "")
            {
                await FaceApp(FilterType.Pan, url);
            }

            [Command("male")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Male(string url = "")
            {
                await FaceApp(FilterType.Male, url);
            }

            [Command("glasses")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Glasses(string url = "")
            {
                await FaceApp(FilterType.Glasses, url);
            }

            [Command("hollywood")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Holly(string url = "")
            {
                await FaceApp(FilterType.Hollywood, url);
            }

            [Command("goatee")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Goatee(string url = "")
            {
                await FaceApp(FilterType.Goatee, url);
            }

            [Command("impression")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Impression(string url = "")
            {
                await FaceApp(FilterType.Impression, url);
            }

            [Command("heisenberg")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Heisen(string url = "")
            {
                await FaceApp(FilterType.Heisenberg, url);
            }

            [Command("hitman")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Hitman(string url = "")
            {
                await FaceApp(FilterType.Hitman, url);
            }

            [Command("bangs")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Bangs(string url = "")
            {
                await FaceApp(FilterType.Bangs, url);
            }

            [Command("wave")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Wave(string url = "")
            {
                await FaceApp(FilterType.Wave, url);
            }

            [Command("makeup")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Makeup(string url = "")
            {
                await FaceApp(FilterType.Makeup, url);
            }

            [Command("mustache")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Mustache(string url = "")
            {
                await FaceApp(FilterType.Mustache, url);
            }

            [Command("lion")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Lion(string url = "")
            {
                await FaceApp(FilterType.Lion, url);
            }

            [Command("hipster")]
            [RequireBotPermission(ChannelPermission.AttachFiles)]
            public async Task Hipster(string url = "")
            {
                await FaceApp(FilterType.Hipster, url);
            }
        }
    }
}
