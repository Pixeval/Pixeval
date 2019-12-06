using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pzxlane.Data.Model.Web.Response
{
    public class GalleryResponse
    {
        [JsonProperty("illusts")]
        public List<Illust> Illusts { get; set; }

        [JsonProperty("next_url")]
        public string NextUrl { get; set; }

        public class Illust
        {
            [JsonProperty("id")]
            public long Id { get; set; }

        }
    }
}