using Microsoft.VisualStudio.TestTools.UnitTesting;
using MovHubDb;
using MovHubDb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HtmlEmiters;

namespace ToHtmlEmit2Tests
{
    [TestClass]
    public class AttributesTest
    {
        private TheMovieDbClient movieDb = new TheMovieDbClient();

        /*
         * Se este parametro estiver a NULL é sinal que 
         * [JsonProperty("vote_average")]não está a funcionar 
         */
        [TestMethod]
        public void JsonAttributTest()
        {
            Movie movie = movieDb.MovieDetails(1018);
            Assert.IsNotNull(movie.VoteAverage); // origainalmente é Vote_Average em Json
        }

        [TestMethod]
        public void IgnoreAttributeTest()
        {
            Movie movie = movieDb.MovieDetails(860);
            PropertyInfo idInfo = movie.GetType().GetProperty("Id");
            Assert.IsNotNull(idInfo.GetCustomAttribute(typeof(HtmlEmiters.HtmlIgnoreAttribute)));
        }

        [TestMethod]
        public void HtmlAsAttributeTest()
        {
            Movie movie = movieDb.MovieDetails(860);
            HtmlEmiters.HtmlAsAttribute attribute = (HtmlEmiters.HtmlAsAttribute)movie.GetType().GetProperty("credits").GetCustomAttribute(typeof(HtmlEmiters.HtmlAsAttribute));
            Assert.IsNotNull(attribute);
            Assert.IsTrue(attribute.htmlRef.Contains("<li class='list-group-item'><a href='/movies/{value}/credits'>cast and crew </a></li>"));
        }
    }
}
    