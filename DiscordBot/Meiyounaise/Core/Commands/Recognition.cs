using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Discord.Commands;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Meiyounaise.Core.Commands
{


    
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
                        stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await contentStream.CopyToAsync(stream);

                    }
                }
            }
        }


        [Command("emotion")]
        public async Task Emotion(string eurl = "")
        {

            string key = "";
            using (var Stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\EmotionKey.txt"), FileMode.Open, FileAccess.Read))
            using (var ReadToken = new StreamReader(Stream))
            {
                key = ReadToken.ReadToEnd();
            }



            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            bool cont = true;
            //string imageFilePath = @"E:\Programming\DiscordBot\Meiyounaise\Data\emotion.png";
            string imageFilePath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\emotion.png");
            try
            {
                await DownloadAsync(new Uri(eurl), imageFilePath);
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
                    // Request headers - replace this example key with your valid key.
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key); // 
                    Console.WriteLine("test1");
                    // NOTE: You must use the same region in your REST call as you used to obtain your subscription keys.
                    //   For example, if you obtained your subscription keys from westcentralus, replace "westus" in the 
                    //   URI below with "westcentralus".
                    //https://westeurope.api.cognitive.microsoft.com/face/v1.0

                    //detect?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise
                    var uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

                    HttpResponseMessage response;
                    string responseContent;

                    // Request body. Try this sample with a locally stored JPEG image.
                    byte[] byteData = GetImageAsByteArray(imageFilePath);

                    using (var content = new ByteArrayContent(byteData))
                    {
                        // This example uses content type "application/octet-stream".
                        // The other content types you can use are "application/json" and "multipart/form-data".
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        response = await client.PostAsync(uri, content);
                        responseContent = response.Content.ReadAsStringAsync().Result;
                    }
                    Console.WriteLine("test2");
                    // A peek at the raw JSON response.
                    //Console.WriteLine(responseContent);


                    // Processing the JSON into manageable objects.
                    JToken rootToken = JArray.Parse(responseContent).First;

                    // First token is always the faceRectangle identified by the API.
                    JToken faceRectangleToken = rootToken.First;

                    // Second token is all emotion scores.
                    JToken scoresToken = rootToken.Last;

                    // Show all face rectangle dimensions
                    JEnumerable<JToken> faceRectangleSizeList = faceRectangleToken.First.Children();
                    //foreach (var size in faceRectangleSizeList)
                    //{
                    //    Console.WriteLine(size);
                    //}

                    // Show all scores
                    JEnumerable<JToken> scoreList = scoresToken.First.Children();
                    //foreach (var score in scoreList)
                    //{
                    //    Console.WriteLine(score);
                    //    if () { }
                    //    Context.Channel.SendMessageAsync(score.ToString());
                    //}

                    Console.WriteLine("test3");
                    string result = scoresToken.ToString();
                    result = result.Substring(result.IndexOf("\"emotion\": {") + 12);
                    result.Remove(result.IndexOf("  },"));
                    string fresult = result.Remove(result.IndexOf("},"));
                    fresult = fresult.Replace(" ", "");
                    fresult = fresult.Replace("\"", "");
                    fresult = fresult.Replace(":", ": ");
                    fresult = fresult.Replace(",", "");
                    await Context.Channel.SendMessageAsync(fresult);
                    Console.WriteLine("test4");
                }

                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync($"Error: `{e.Message}`");
                    await Context.Channel.SendMessageAsync(
                        "This means that either the API didn't recognize a face, or shit itself in other ways. Try a different picture");

                }
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            //            File.Delete(@"E:\Programming\DiscordBot\Meiyounaise\Data\emotion.png");
           // System.IO.File.Delete(@"E:\Programming\DiscordBot\Meiyounaise\Data\emotion.png");
            System.IO.File.Delete((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1",
                @"Data\emotion.png"));
        }


        [Command("race")]
        public async Task Race(string eurl = "")
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            bool cont = true;
           // string imageFilePath = @"E:\Programming\DiscordBot\Meiyounaise\Data\emotion.png";
            string imageFilePath =
                (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1",
                    @"Data\emotion.png");

            try
            {
                await DownloadAsync(new Uri(eurl), imageFilePath);
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
                    // Request headers - replace this example key with your valid key.
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "d265614f90714ce9ad0e22435328c35a"); // 
                    Console.WriteLine("test1");
                    // NOTE: You must use the same region in your REST call as you used to obtain your subscription keys.
                    //   For example, if you obtained your subscription keys from westcentralus, replace "westus" in the 
                    //   URI below with "westcentralus".
                    //https://westeurope.api.cognitive.microsoft.com/face/v1.0

                    //detect?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise
                    var uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

                    HttpResponseMessage response;
                    string responseContent;

                    // Request body. Try this sample with a locally stored JPEG image.
                    byte[] byteData = GetImageAsByteArray(imageFilePath);

                    using (var content = new ByteArrayContent(byteData))
                    {
                        // This example uses content type "application/octet-stream".
                        // The other content types you can use are "application/json" and "multipart/form-data".
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        response = await client.PostAsync(uri, content);
                        responseContent = response.Content.ReadAsStringAsync().Result;
                    }
                    Console.WriteLine("test2");
                    // A peek at the raw JSON response.
                    //Console.WriteLine(responseContent);


                    // Processing the JSON into manageable objects.
                    JToken rootToken = JArray.Parse(responseContent).First;

                    // First token is always the faceRectangle identified by the API.
                    JToken faceRectangleToken = rootToken.First;

                    // Second token is all emotion scores.
                    JToken scoresToken = rootToken.Last;

                    // Show all face rectangle dimensions
                    JEnumerable<JToken> faceRectangleSizeList = faceRectangleToken.First.Children();
                    //foreach (var size in faceRectangleSizeList)
                    //{
                    //    Console.WriteLine(size);
                    //}

                    // Show all scores
                    JEnumerable<JToken> scoreList = scoresToken.First.Children();
                    //foreach (var score in scoreList)
                    //{
                    //    Console.WriteLine(score);
                    //    if () { }
                    //    Context.Channel.SendMessageAsync(score.ToString());
                    //}

                    Console.WriteLine("test3");
                    string result = scoresToken.ToString();
                    result = result.Substring(result.IndexOf("\"emotion\": {") + 12);
                    result.Remove(result.IndexOf("  },"));
                    string fresult = result.Remove(result.IndexOf("},"));
                    fresult = fresult.Replace(" ", "");
                    fresult = fresult.Replace("\"", "");
                    fresult = fresult.Replace(":", ": ");
                    fresult = fresult.Replace(",", "");
                    await Context.Channel.SendMessageAsync(fresult);
                    Console.WriteLine("test4");
                }

                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync($"Error: `{e.Message}`");
                    await Context.Channel.SendMessageAsync(
                        "This means that either the API didn't recognize a face, or shit itself in other ways. Try a different picture");

                }
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            //            File.Delete(@"E:\Programming\DiscordBot\Meiyounaise\Data\emotion.png");
            System.IO.File.Delete(@"E:\Programming\DiscordBot\Meiyounaise\Data\emotion.png");
        }

    }
}