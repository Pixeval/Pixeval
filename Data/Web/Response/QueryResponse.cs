using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pixeval.Data.Web.Response
{
    public class QueryResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("response")]
        public List<Response> ToResponse { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("pagination")]
        public Pagination QueryPagination { get; set; }

        public class Pagination
        {
            [JsonProperty("pages")]
            public long Pages { get; set; }
        }

        public class Response
        {
            [JsonProperty("id")]
            public long Id { get; set; }
        }
    }
}