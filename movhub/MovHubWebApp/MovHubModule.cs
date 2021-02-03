using HtmlEmiters;
using HtmlReflect;
using Nancy;
using MovHubDb;
using System;
using MovHubDb.Model;

namespace MovHubWebApp
{
    public class MovHubModule : NancyModule
    {
        TheMovieDbClient movieDb = new TheMovieDbClient();
        //Htmlect html = new Htmlect();
        HtlmReflect html = new HtlmReflect();
        //HtmlEmit2 html = new HtmlEmit2();

        public MovHubModule()
        {
            Get["/movies"] = args =>
            {
                string title = this.Request.Query.title;
                MovieSearchItem[] moviesList = movieDb.Search(title, 1);
                MovHubViewModel model = new MovHubViewModel(
                    "Movies for title: " + title,
                    html.ToHtml(moviesList));
                return View["ViewTable", model];
            };
            Get["/movies/{movieId}"] = args =>
            {
                Movie mov = movieDb.MovieDetails(args.movieId);
                MovHubViewModel model = new MovHubViewModel(
                    "Movie Details:",
                    html.ToHtml(mov));
                return View["ViewDetails", model];
            };

            Get["/movies/{movieId}/credits"] = args =>
            {
                CreditsItem[] credits = movieDb.MovieCredits(args.movieId);
                MovHubViewModel model = new MovHubViewModel(
                    "Cast and Crew:",
                    html.ToHtml(credits));
                return View["ViewTable", model];
            };

            Get["/person/{actorId}/movies"] = args =>
            {
                Person actor = movieDb.PersonDetails(args.actorId);
                MovieSearchItem[] credits = movieDb.PersonMovies(args.actorId);
                MovHubViewModel moviesList = new MovHubViewModel(
                    html.ToHtml(actor),
                    html.ToHtml(credits));
                return View["ViewTable", moviesList];
            };

        }
    }
}
