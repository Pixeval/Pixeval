#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/ReverseSearchResponse.cs
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

public class ReverseSearchResponse
{
    [JsonPropertyName("header")]
    public ReverseSearchResponseHeader? Header { get; set; }

    [JsonPropertyName("results")]
    public Result[]? Results { get; set; }

    public class ReverseSearchResponseHeader
    {
        [JsonPropertyName("status")]
        public long Status { get; set; }
    }

    public class Result
    {
        [JsonPropertyName("header")]
        public ResultHeader? Header { get; set; }

        [JsonPropertyName("data")]
        public Data? Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("pixiv_id")]
        public long PixivId { get; set; }

        [JsonPropertyName("member_name")]
        public string? MemberName { get; set; }

        [JsonPropertyName("member_id")]
        public long MemberId { get; set; }
    }

    public class ResultHeader
    {
        [JsonPropertyName("similarity")]
        public string? Similarity { get; set; }

        [JsonPropertyName("index_id")]
        public long IndexId { get; set; }
    }
}