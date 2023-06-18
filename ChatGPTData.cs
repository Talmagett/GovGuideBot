using GovGuideBot.Instructions;
using Newtonsoft.Json;

namespace GovGuideBot.ChatGPT
{
    internal class ChatGPTData
    {
        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("instructions")]
        public Instruction[] Instructions { get; set; }

        [JsonProperty("popular_questions")]
        public Question[] PopularQuestions { get; set; }
    }
    internal class DataWrapper
    {
        [JsonProperty("data")]
        public ChatGPTData Data { get; set; }
    }
}
