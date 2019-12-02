using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pzxlane.Data.Model.Web.Response
{
    public class UgoiraMetadataResponse
    {
        [JsonProperty("ugoira_metadata")]
        public UgoiraMetadata UgoiraMetadataInfo { get; set; }

        public class UgoiraMetadata
        {
            [JsonProperty("zip_urls")]
            public ZipUrls ZipUrls { get; set; }

            [JsonProperty("frames")]
            public List<Frame> Frames { get; set; }
        }

        public class Frame
        {
            [JsonProperty("file")]
            public string File { get; set; }

            [JsonProperty("delay")]
            public long Delay { get; set; }
        }

        public class ZipUrls
        {
            [JsonProperty("medium")]
            public Uri Medium { get; set; }
        }
    }
}