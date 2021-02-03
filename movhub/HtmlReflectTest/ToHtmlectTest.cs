using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MovHubDb;
using HtmlReflect;
using MovHubDb.Model;

namespace ToHtmlEmit2Tests
{
    [TestClass]
    public class ToHtmlectTest
    {
        private TheMovieDbClient movieDb = new TheMovieDbClient();
        private Htmlect html = new Htmlect();

        [TestMethod]
        public void ToHtmlTest()
        {
            Movie movie = movieDb.MovieDetails(860);
            String thisHtml = html.ToHtml(movie);

            Assert.IsTrue(
                thisHtml.Contains("<ul class='list-group'><li class='list-group-item'>" +
                                  "<strong>OriginalTitle</strong>:WarGames</li>"));

        }

        [TestMethod]
        public void ToHtmlCacheTest()
        {
            // Without cached values
            long firstTimeCount = Environment.TickCount;
            Movie movie = movieDb.MovieDetails(860);
            String thisHtml = html.ToHtml(movie);
            long secondTimeCount = Environment.TickCount;
            long firstCount = TimeCount(firstTimeCount, secondTimeCount);
            // Whith cached values
            long thirdTimeCount = Environment.TickCount;
            Movie movie2 = movieDb.MovieDetails(860);
            String thisHtml2 = html.ToHtml(movie);
            long forthTimeCount = Environment.TickCount;
            long secondCount = TimeCount(thirdTimeCount, forthTimeCount);
            

            Console.WriteLine("FirstCount:{0}, SecondCount:{1}", firstCount, secondCount);
            Assert.IsTrue(
                thisHtml2.Contains("<ul class='list-group'><li class='list-group-item'>" +
                                  "<strong>OriginalTitle</strong>:WarGames</li>"));
            

        }

        private long TimeCount(long firstTimeCount, long secondTimeCount)
        {
            return secondTimeCount - firstTimeCount;
        }

        // Utilizou-se para este teste o id 15008 pois pertence a um actriz ja falecido
        // assim sendo o dados de teste são imutaveis
        [TestMethod]
        public void ToHtmlArrayTest()
        {
            MovieSearchItem[] personCredits = movieDb.PersonMovies(15008);
            String thisHtml = html.ToHtml(personCredits);
            Assert.IsTrue(thisHtml.Contains("<td>Mulholland Drive</td><td>2001-05-16</td><td>7,7</td>"));
          //Assert.IsTrue(thisHtml.Contains("<td>Mulholland Drive</td><td>2001-05-16</td><td>7.7</td>"));

        }

        [TestMethod]
        public void ToHtmlArrayCacheTest()
        {
            // Without cached values
            long firstTimeCount = Environment.TickCount;
            MovieSearchItem[] personCredits = movieDb.PersonMovies(15008);
            String thisHtml = html.ToHtml(personCredits);
            long secondTimeCount = Environment.TickCount;
            long firstCount = TimeCount(firstTimeCount, secondTimeCount);
            // Whith cached values
            long thirdTimeCount = Environment.TickCount;
            MovieSearchItem[] personCredits2 = movieDb.PersonMovies(15008);
            String thisHtml2 = html.ToHtml(personCredits);
            long forthTimeCount = Environment.TickCount;
            long secondCount = TimeCount(thirdTimeCount, forthTimeCount);

            Console.WriteLine("FirstCount:{0}, SecondCount:{1}", firstCount, secondCount);
            Assert.IsTrue(thisHtml2.Contains("<td>Mulholland Drive</td><td>2001-05-16</td><td>7,7</td>"));

        }

    }
}
