using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MovHubDb;
using HtmlEmiters;
using MovHubDb.Model;

namespace ToHtmlEmit2Tests
{
    [TestClass]
    public class ToHtmlEmitTests
    {

        private TheMovieDbClient movieDb = new TheMovieDbClient();
        private HtmlEmit html = new HtmlEmit();

        [TestMethod]
        public void ToHtmlEmitTest()
        {

            Movie movie = movieDb.MovieDetails(860);
            String thisHtml = html.ToHtml(movie);

            Assert.IsTrue(
                thisHtml.Contains("<ul class='list-group'><li class='list-group-item'>" +
                                  "<strong>OriginalTitle</strong>:WarGames</li>"));
        }


        [TestMethod]
        public void EmitArrayBodyTest()
        {

            
            Movie movie = movieDb.MovieDetails(860);
            String thisHtml = HtmlEmit.GetArrayBody(movie);

            Assert.IsTrue(
                thisHtml.Contains("<ul class='list-group'><li class='list-group-item'>" +
                                  "<strong>OriginalTitle</strong>:WarGames</li>"));
        }


        // Utilizou-se para este teste o id 15008 pois pertence a um actriz ja falecido
        // assim sendo o dados de teste são imutaveis
        [TestMethod]
        public void ToHtmlArrayTest()
        {
            MovieSearchItem[] personCredits = movieDb.PersonMovies(15008);
            String thisHtml = html.ToHtml(personCredits);
            Assert.IsTrue(thisHtml.Contains("<td>Mulholland Drive</td><td>2001-05-16</td><td>7.7</td>"));
            //Assert.IsTrue(thisHtml.Contains("<td>Mulholland Drive</td><td>2001-05-16</td><td>7.7</td>"));

        }

    }

}
