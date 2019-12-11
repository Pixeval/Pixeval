using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pixeval.Data.Web.Response
{
    public class DnsResolveResponse
    {
        [JsonProperty("Status")]
        public long Status { get; set; }

        [JsonProperty("TC")]
        public bool Tc { get; set; }

        [JsonProperty("RD")]
        public bool Rd { get; set; }

        [JsonProperty("RA")]
        public bool Ra { get; set; }

        [JsonProperty("AD")]
        public bool Ad { get; set; }

        [JsonProperty("CD")]
        public bool Cd { get; set; }

        [JsonProperty("Question")]
        public List<Question> Questions { get; set; }

        [JsonProperty("Answer")]
        public List<Answer> Answers { get; set; }

        public class Answer
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public long Type { get; set; }

            [JsonProperty("TTL")]
            public long Ttl { get; set; }

            [JsonProperty("data")]
            public string Data { get; set; }
        }

        public class Question
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public long Type { get; set; }
        }
    }
}