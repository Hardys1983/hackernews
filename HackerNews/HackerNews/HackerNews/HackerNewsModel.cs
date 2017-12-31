using Newtonsoft.Json;

namespace HackerNews.HackerNews
{
    public class HackerNewsModel
    {
        public static HackerNewsModel FromJson(string json) => 
            JsonConvert.DeserializeObject<HackerNewsModel>(json, new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
            });

        [JsonProperty("by")]
        public string By { get; set; }

        [JsonProperty("descendants")]
        public long Descendants { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("kids")]
        public long[] Kids { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}