using HtmlEmiters;
using HtmlReflect;
using MovHubDb;
using MovHubDb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchMark
{
    class Benchmark
    {

        private static TheMovieDbClient movieDb = new TheMovieDbClient();
        private static HtmlEmit2 htmlEmit = new HtmlEmit2();
        private static HtlmReflect htmlReflect = new HtlmReflect();
        private static MovieSearchItem[] movieSearch;

        static void Main(string[] args)
        {
            CreateMovieSearchArray(1);

            NBench.Bench(ToHtmlReflectTest, "Reflect Test");
            NBench.Bench(ToHtmlEmitTest, "Emit Test");

            CreateMovieSearchArray(2000);

            NBench.Bench(ToHtmlReflectArrayTest, "Reflect Array Test");
            NBench.Bench(ToHtmlEmitArrayTest, "Emit Array Test");
        }

        private static void CreateMovieSearchArray(int numberOfElem) {
            movieSearch = new MovieSearchItem[numberOfElem];
            for (int i = 0; i < numberOfElem; i++)
            {
                MovieSearchItem m = new MovieSearchItem();
                m.Id = 1;
                m.ReleaseDate = "someDate";
                m.Title = "SomeTitle";
                m.VoteAverage = 7.7;
                movieSearch[i] = m;
            }
        }

        public static void ToHtmlReflectTest()
        {
            String thisHtml = htmlReflect.ToHtml(movieSearch);
        }
        public static void ToHtmlEmitTest()
        {
            String thisHtml = htmlEmit.ToHtml(movieSearch);
        }
        public static void ToHtmlReflectArrayTest()
        {
            String thisHtml = htmlReflect.ToHtml(movieSearch);
        }
        public static void ToHtmlEmitArrayTest()
        {
            String thisHtml = htmlEmit.ToHtml(movieSearch);
        }
    
    }
}
