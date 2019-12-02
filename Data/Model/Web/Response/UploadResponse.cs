using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pzxlane.Data.Model.Web.Response
{
    public class UploadResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("response")]
        public List<Response> ToResponse { get; set; }

        [JsonProperty("pagination")]
        public Pagination UploadPagination { get; set; }

        public class Pagination
        {
            [JsonProperty("previous")]
            public long Previous { get; set; }

            [JsonProperty("next")]
            public long Next { get; set; }

            [JsonProperty("current")]
            public long Current { get; set; }

            [JsonProperty("per_page")]
            public long PerPage { get; set; }

            [JsonProperty("total")]
            public long Total { get; set; }

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