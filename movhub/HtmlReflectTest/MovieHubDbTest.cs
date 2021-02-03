using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MovHubDb;
using MovHubDb.Model;

namespace ToHtmlEmit2Tests
{
    [TestClass]
    public class MovieHubDbTest
    {
        private TheMovieDbClient movieDb = new TheMovieDbClient();

        [TestMethod]
        public void MovieSearchTest()
        {
            MovieSearchItem[] movieSearch = movieDb.Search("war games", 1);
            Assert.AreEqual(movieSearch.Length, 6);
            Assert.AreEqual(movieSearch[0].Id, 14154);  
        }

        [TestMethod]
        public void MovieDetailsTest()
        {
            Movie movie = movieDb.MovieDetails(860);
            Assert.AreEqual(movie.OriginalTitle, "WarGames");
        }

        [TestMethod]
        public void MovieCredits()
        {
            CreditsItem[] credits = movieDb.MovieCredits(860);
            Assert.AreEqual(credits[0].Id, 4756);
            Assert.AreEqual(credits[0].Name, "Matthew Broderick");
        }

        [TestMethod]
        public void PersonDetailsTest()
        {
            Person p = movieDb.PersonDetails(15008);
            Assert.AreEqual(p.Name, "Ann Miller");
            
        }

        [TestMethod]
        public void PersonMoviesTest()
        {
            MovieSearchItem[] personMovies = movieDb.PersonMovies(15008);
            Assert.AreEqual(personMovies[0].Title, "Mulholland Drive");

        }


    }
}
