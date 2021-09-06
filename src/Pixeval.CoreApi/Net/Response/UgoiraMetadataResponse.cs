using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Response
{
    public class UgoiraMetadataResponse
    {
        [JsonPropertyName("ugoira_metadata")]
        public UgoiraMetadata? UgoiraMetadataInfo { get; set; }
    }

    public class UgoiraMetadata
    {
        [JsonPropertyName("zip_urls")]
        public ZipUrls? ZipUrls { get; set; }

        [JsonPropertyName("frames")]
        public IEnumerable<Frame>? Frames { get; set; }
    }

    public class Frame
    {
        [JsonPropertyName("file")]
        public string? File { get; set; }

        [JsonPropertyName("delay")]
        public long Delay { get; set; }
    }

    public class ZipUrls
    {
        [JsonPropertyName("medium")]
        public string? Medium { get; set; }
    }
}