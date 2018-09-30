using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace Meiyounaise.Core.Commands
{
    [SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
    public class Recognition : ModuleBase<SocketCommandContext>
    {
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        public static async Task DownloadAsync(Uri requestUri, string filename)
        {
            HttpClientHandler handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    using (
                        Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096,
                            true))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
                }
            }
        }

        [Command("emotion")]
        public async Task Emotion(string url = "")
        {
            string key = Utilities.GetKey("emotionkey");
            var lm = await Context.Channel.GetMessagesAsync(2).Flatten();
            var message = lm.Last(); //GET LAST MESSAGE
            if (Context.Message.Attachments.Count != 0)//CURRENT MESSAGE HAS ATTACHMENT
            {
                url = Context.Message.Attachments.FirstOrDefault()?.Url;
            }
            else//CURRENT MESSAGE DOES NOT HAVE ATTACHMENT
            {
                if (url != "")//IMAGE URL PROVIDED
                {
                    //do nothing
                }
                else//NO IMAGE URL PROVIDED
                {
                    if (message.Attachments.Count != 0)
                    {
                        url = message.Attachments.FirstOrDefault()?.Url;
                    }
                    else
                    {
                        url = message.Content;
                    }
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            bool cont = true;
            string imageFilePath = Utilities.dataPath + "emotion.png";
            try
            {
                await DownloadAsync(new Uri(url), imageFilePath);
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
            File.Delete(Utilities.dataPath + "emotion.png");
        }
    }
}