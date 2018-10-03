using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TMDbLib.Client;
using TMDbLib.Objects.General;
// ReSharper disable PossibleInvalidOperationException

namespace Meiyounaise.Core.Commands
{
    public class Movies: ModuleBase<SocketCommandContext>
    {
        [Command("movie")]
        public async Task Movie([Remainder] string title)
        {
            SearchContainer<TMDbLib.Objects.Search.SearchMovie> result;
            TMDbLib.Objects.Movies.Movie movie;
            var client = new TMDbClient(Utilities.GetKey("tmdb"));
            try
            {
                result = await client.SearchMovieAsync(title);
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
            await ReplyAsync("",false,embed.Build());
        }
    }
}
