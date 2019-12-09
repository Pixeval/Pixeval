using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pixeval.Data.Model.Web.Response
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