#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/UgoiraMetadataResponse.cs
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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Response;

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