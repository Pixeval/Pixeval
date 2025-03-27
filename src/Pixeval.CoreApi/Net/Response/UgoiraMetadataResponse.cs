// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Mako.Net.Response;

[Factory]
public partial record UgoiraMetadataResponse
{
    [JsonPropertyName("ugoira_metadata")]
    public required UgoiraMetadata UgoiraMetadataInfo { get; set; }

    public int FrameCount => UgoiraMetadataInfo.Frames.Length;

    public IEnumerable<int> Delays => UgoiraMetadataInfo.Frames.Select(t => (int) t.Delay);

    public string MediumUrl => UgoiraMetadataInfo.ZipUrls.Medium;

    public string LargeUrl => UgoiraMetadataInfo.ZipUrls.Large;

    public string OrignalUrl => UgoiraMetadataInfo.ZipUrls.Large.Replace("1920x1080", "");
}

[Factory]
public partial record UgoiraMetadata
{
    [JsonPropertyName("zip_urls")]
    public required ZipUrls ZipUrls { get; set; }

    [JsonPropertyName("frames")]
    public required Frame[] Frames { get; set; } = [];
}

[Factory]
public partial record Frame
{
    [JsonPropertyName("file")]
    public required string File { get; set; } = "";

    [JsonPropertyName("delay")]
    public required long Delay { get; set; }
}

[Factory]
public partial record ZipUrls
{
    [JsonPropertyName("medium")]
    public required string Medium { get; set; } = DefaultImageUrls.ImageNotAvailable;

    public string Large => Medium.Replace("600x600", "1920x1080");
}
