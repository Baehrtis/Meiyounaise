using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using IF.Lastfm.Core.Api;
using Meiyounaise.Core.Data;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api.Enums;
using TMDbLib.Client;
// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable PossibleInvalidOperationException

namespace Meiyounaise.Core.Commands
{
    [Name("Media Lookup")]
    public class MediaModule : ModuleBase<SocketCommandContext>
    {
        //LYRICS
        [Command("lyrics")]
        [Summary("Get the Lyrics for a specified song.")]
        public async Task LyricsTask(string artist, [Remainder]string track)
        {
            string key = Utilities.GetKey("lyricskey");
            string result;
            Uri url = new Uri("https://orion.apiseeds.com/api/music/lyric/" + artist + "/" + track + key);
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        var reponse = await httpClient.SendAsync(request);
                        result = await reponse.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
                return;
            }

            if (result.Contains("Lyric no found, try again later."))
            {
                await ReplyAsync("Lyric not found, try again later.");
                return;
            }
            result = result.Substring(result.IndexOf(",\"text\":\"") + 9);
            result = result.Remove(result.IndexOf("\",\"lang\":{"));
            result = result.Replace(@"\n", "\n");
            if (result.Length > 2000)
            {
                string rts = result;
                while (rts.Length >= 2000)
                {
                    await ReplyAsync(rts.Substring(0, 2000));
                    rts = rts.Substring(2000);
                }
                await ReplyAsync(rts);
                return;
            }
            try
            {
                await ReplyAsync(result);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }
        }

        //MOVIES
        [Command("movie")]
        [Summary("Search TMDb for a speficied movie.")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task Movie([Remainder] string title)
        {
            TMDbLib.Objects.Movies.Movie movie;
            var client = new TMDbClient(Utilities.GetKey("tmdb"));
            try
            {
                var result = await client.SearchMovieAsync(title);
                movie = await client.GetMovieAsync(result.Results[0].Id);
            }
            catch (Exception)
            {
                await ReplyAsync($"I could not find a movie with the Title {title}");
                return;
            }
            string genres = "";
            foreach (var genre in movie.Genres)
            {
                genres += genre.Name + " | ";
            }
            genres = genres.Remove(genres.Length - 3);
            var embed = new EmbedBuilder()
                .WithDescription(movie.Overview)
                .WithTitle(movie.Title)
                .WithUrl($"https://www.themoviedb.org/movie/{movie.Id}")
                .WithThumbnailUrl($"https://image.tmdb.org/t/p/w500{movie.PosterPath}")
                .WithColor(0, 216, 121)
                .AddField("Runtime", $"~ {movie.Runtime.Value} Minutes")
                .AddField("User Rating", movie.VoteAverage)
                .AddField("Release Date", movie.ReleaseDate.Value.Year)
                .AddField("Genres", genres)
                .AddField("Original Title & Language", $"{movie.OriginalTitle} - {movie.OriginalLanguage}")
                .AddField("Budget | Revenue", $"{movie.Budget} | {movie.Revenue}")
                .WithCurrentTimestamp()
                .WithFooter("Tʜɪs ᴅᴀᴛᴀ ɪs ғʀᴏᴍ ᴛʜᴇᴍᴏᴠɪᴇᴅʙ.ᴏʀɢ ᴀɴᴅ ᴍɪɢʜᴛ ʙᴇ ɪɴᴄᴏᴍᴘʟᴇᴛᴇ");
            await ReplyAsync("", false, embed.Build());
        }

        //MUSIC
        private static readonly LastfmClient Client = new LastfmClient(Utilities.GetKey("lastkey"), Utilities.GetKey("lastsecret"), new HttpClient());
        [Command("fm")]
        [Summary("Get or set your last.fm Username.")]
        public async Task FmUser(string cmd, string username = "")
        {
            if (cmd.ToLower() == "set")
            {
                if (username == "")
                {
                    await ReplyAsync("I need a Name that I can link to your account!");
                    return;
                }
                var account = UserAccounts.GetAccount(Context.User);
                account.Last = username;
                UserAccounts.SaveAccounts();
                await Context.Message.AddReactionAsync(new Emoji("✅"));
                return;
            }
            if (cmd.ToLower() == "get")
            {
                if (username == "")
                {
                    var account = UserAccounts.GetAccount(Context.User);
                    var name = account.Last;
                    if (name != null) await ReplyAsync($"I have {account.Last} saved as your Last.fm Account Name!");
                    else await ReplyAsync("I have nothing saved as your Last.fm Account Name!");
                }
                else
                {
                    var account = UserAccounts.GetAccount(Context.Message.MentionedUsers.LastOrDefault());
                    if (account.Last != null)
                        await ReplyAsync(
                            $"I have {account.Last} saved as {Context.Message.MentionedUsers.LastOrDefault()?.Mention} 's Last.fm Account Name!");
                    else
                        await ReplyAsync(
                            $"I have nothing saved as {Context.Message.MentionedUsers.LastOrDefault()?.Mention}' Last.fm Account Name");
                }
            }
        }

        [Command("fm")]
        [Summary("Get your currently scrobbling or last played Track.")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task Last()
        {
            var name = UserAccounts.GetAccount(Context.User).Last;
            if (name == null)
            {
                await ReplyAsync("I have no Last.fm Username set for you! Set it using `&fm set [Name]`!");
                return;
            }

            var response = await Client.User.GetRecentScrobbles(name);
            var count = Client.User.GetInfoAsync(name).Result.Content.Playcount.ToString();
            string isplaying = "Last Track";
            var isNowPlaying = response.Content.First().IsNowPlaying;
            if (isNowPlaying != null) isplaying = "Now Playing";
            var embed = new EmbedBuilder()
                .WithThumbnailUrl(response.Content.First().Images.Large.AbsoluteUri)
                    .WithAuthor(author =>
                    {
                        author
                            .WithIconUrl("http://icons.iconarchive.com/icons/sicons/basic-round-social/256/last.fm-icon.png")
                            .WithName($"{name} - {isplaying}")
                            .WithUrl($"https://www.last.fm/user/{name}");
                    })
                .WithColor(255, 0, 0)
                .AddField("Artist - Song", response.Content.First().ArtistName + " - " + $"[{response.Content.First().Name}]({response.Content.First().Url})")
                .AddField("Album", response.Content.First().AlbumName)
                .WithFooter(footer => footer.WithText(count + " Total Scrobbles"))
                .WithCurrentTimestamp();
            await ReplyAsync("", false, embed.Build());
        }

        [Command("recommend")]
        [Summary("Recommends 3 Artists similiar to your Top 3 Artists within a given Timespan.")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task Recommend(string timespan = "")
        {
            string tss;
            LastStatsTimeSpan ts;
            switch (timespan.ToLower())
            {
                case "overall":
                    ts = LastStatsTimeSpan.Overall;
                    tss = "Overall";
                    break;
                case "month":
                    ts = LastStatsTimeSpan.Month;
                    tss = "Last Month";
                    break;
                case "quarter":
                    ts = LastStatsTimeSpan.Quarter;
                    tss = "Last Quarter";
                    break;
                case "year":
                    ts = LastStatsTimeSpan.Year;
                    tss = "Last Year";
                    break;
                case "half":
                    ts = LastStatsTimeSpan.Half;
                    tss = "Past Semester";
                    break;
                case "week":
                    ts = LastStatsTimeSpan.Week;
                    tss = "Last week";
                    break;
                default:
                    ts = LastStatsTimeSpan.Overall;
                    tss = "Overall";
                    await Context.Channel.SendMessageAsync("No Timespan provided, using overall");
                    break;
            }

            var name = UserAccounts.GetAccount(Context.User).Last;
            if (name == null)
            {
                await ReplyAsync("I have no Last.fm Username set for you! Set it using `&fm set [Name]`!");
                return;
            }

            var artists = await Client.User.GetTopArtists(name, ts, 1, 3);
            var embed = new EmbedBuilder().WithColor(255, 0, 0).WithDescription($"Timespan: {tss}");
            embed.WithAuthor(author =>
            {
                author
                    .WithIconUrl("http://icons.iconarchive.com/icons/sicons/basic-round-social/256/last.fm-icon.png")
                    .WithName($"Recommendations for {name}")
                    .WithUrl($"https://www.last.fm/user/{name}");
            });
            foreach (var artist in artists)
            {
                var result = new List<string>();
                var similiarArtists = await Client.Artist.GetSimilarAsync(artist.Name, false, 3);
                foreach (var sArtist in similiarArtists)
                {
                    result.Add($"[{sArtist.Name}]({sArtist.Url.AbsoluteUri})");
                }

                embed.AddField($"Based on {artist.Name}", string.Join("\n", result));
            }
            await ReplyAsync("", false, embed.Build());
        }

        [Command("charts")]
        [Summary("Returns the current Global Top 25 Charts.")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task Charts()
        {
            ClientCredentialsAuth auth = new ClientCredentialsAuth()
            {
                ClientId = Utilities.GetKey("spid"),
                ClientSecret = Utilities.GetKey("spsecret"),
                Scope = Scope.UserReadPrivate
            };
            Token token = auth.DoAuth();

            SpotifyWebAPI spotify = new SpotifyWebAPI()
            {
                TokenType = token.TokenType,
                AccessToken = token.AccessToken,
                UseAuth = true
            };

            var playlist = spotify.GetPlaylistTracks("spotifycharts", "37i9dQZEVXbMDoHDwVN2tF", "", 25);
            int i = 1;
            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Global Spotify Charts")
                        .WithUrl("https://open.spotify.com/user/spotifycharts/playlist/37i9dQZEVXbMDoHDwVN2tF?si=ln52hfPKQ-KAKEmXDips2A")
                        .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/1/19/Spotify_logo_without_text.svg/500px-Spotify_logo_without_text.svg.png");
                })
                .WithColor(29, 185, 84);
            foreach (var entry in playlist.Items)
            {
                var artistList = new List<string>();
                foreach (var artist in entry.Track.Artists)
                {
                    artistList.Add(artist.Name);
                }
                var artists = string.Join(", ", artistList);
                embed.AddField($"#{i} {entry.Track.Name}", $"[by {artists}]({entry.Track.ExternUrls.First().Value})");
                i++;
            }
            await ReplyAsync("", false, embed.Build());
        }
    }
}
