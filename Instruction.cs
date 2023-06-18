using Newtonsoft.Json;

namespace GovGuideBot.Instructions
{
    internal class Instruction
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    internal class DataWrapper
    {
        [JsonProperty("data")]
        public Instruction[] Data { get; set; }
    }
}
