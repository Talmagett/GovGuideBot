using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovGuideBot
{
    internal class Question
    {        
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("question")]
        public string QuestionTitle { get; set; }
    }
}
