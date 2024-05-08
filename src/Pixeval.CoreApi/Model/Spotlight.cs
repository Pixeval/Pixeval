#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/SpotlightArticle.cs
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

using System;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

public record Spotlight : IIdEntry
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("pure_title")]
    public required string PureTitle { get; set; }

    [JsonPropertyName("thumbnail")]
    public required string Thumbnail { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("article_url")]
    public required string ArticleUrl { get; set; }

    [JsonPropertyName("publish_date")]
    public required DateTimeOffset PublishDate { get; set; }

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter<SpotlightCategory>))]
    public required SpotlightCategory Category { get; set; }

    [JsonPropertyName("subcategory_label")]
    public required string SubcategoryLabel { get; set; }
}
