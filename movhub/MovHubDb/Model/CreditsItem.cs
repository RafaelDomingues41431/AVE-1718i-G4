using HtmlEmiters;
using Newtonsoft.Json;

namespace MovHubDb.Model
{
    public class CreditsItem
    {
        [JsonProperty("id")]
        [HtmlAs("<td><a href='/person/{value}/movies'> {value} </a></td>")]
        public int Id { get; set; }

        [JsonProperty("character")]
        public string Character { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [HtmlIgnore]
        public string job { get; set; }

        [HtmlIgnore]
        public string department { get; set; }

    }
}