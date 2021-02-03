using HtmlEmiters;
using Newtonsoft.Json;

namespace MovHubDb.Model
{
    public class Person
    {
        [HtmlIgnore]
        public int id { get; set; }

        public string Name { get; set; }

        public string Birthday { get; set; }

        public string Deathday { get; set; }

        public string Biography { get; set; }

        public double Popularity { get; set; }

        [JsonProperty("place_of_birth")]
        public string PlaceOfBirth { get; set; }

        
        
        [HtmlAs("<div style=\"position: absolute; top: 0; right: 0;\">" +
            "<img src=\"http://image.tmdb.org/t/p/w185/{value}\" width=\"50%\"></div>")]
        public string profile_path { get; set; }

        [HtmlIgnore]
        public string imdb_id { get; set; }


    }
}
