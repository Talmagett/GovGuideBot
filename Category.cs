using Newtonsoft.Json;

namespace GovGuideBot.Categories
{
    internal class Category
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    internal class DataWrapper
    {
        [JsonProperty("data")]
        public Category[] Data { get; set; }
    }
}
