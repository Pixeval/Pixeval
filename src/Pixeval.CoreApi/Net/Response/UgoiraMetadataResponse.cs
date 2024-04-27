#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/UgoiraMetadataResponse.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Response;

[Factory]
public partial record UgoiraMetadataResponse
{
    [JsonPropertyName("ugoira_metadata")]
    public required UgoiraMetadata UgoiraMetadataInfo { get; set; }

    public string LargeUrl => UgoiraMetadataInfo.ZipUrls.Large;
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
