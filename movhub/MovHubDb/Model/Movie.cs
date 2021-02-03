using System;
using HtmlEmiters;
using Newtonsoft.Json;

namespace MovHubDb.Model
{
    public class Movie
    {
        [HtmlIgnore]
        public int Id { get; set; }

        [JsonProperty("original_title")]
        public string OriginalTitle { get; set; }

        public string Tagline {get; set;}

        [HtmlAs("<li class='list-group-item'><a href='/movies/{value}/credits'>cast and crew </a></li>")]
        public string credits { get { return Id.ToString(); } }
    
        public long Budget { get; set; }

        [HtmlIgnore]
        public double popularity { get; set; }

        [JsonProperty("vote_average")]
        public double VoteAverage { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        public string Overview { get; set; }

        [HtmlAs("<div style=\"position: absolute; top: 0; right: 0;\">" +
            "<img src=\"http://image.tmdb.org/t/p/w185/{value}\" width=\"50%\"></div>")]
        public string poster_path { get; set; }



        public override string ToString() {
            return "id=" + Id + "\n" +
                    "OriginalTitle=" + OriginalTitle + "\n" +
                    "credits=" + credits + "\n" +
                    "Budget=" + Budget + "\n" +
                    "popularity=" + popularity + "\n" +
                    "vote_average=" + VoteAverage + "\n" +
                    "release_date=" + ReleaseDate + "\n" +
                    "overview=" + Overview + "\n";

        }
    }
}