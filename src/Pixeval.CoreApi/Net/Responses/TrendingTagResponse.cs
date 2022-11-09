﻿#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/TrendingTagResponse.cs
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
using Pixeval.CoreApi.Models;

namespace Pixeval.CoreApi.Net.Responses;

// ReSharper disable UnusedAutoPropertyAccessor.Global
public class TrendingTagResponse
{
    [JsonPropertyName("trend_tags")]
    public IEnumerable<TrendTag>? TrendTags { get; set; }

    public class TrendTag
    {
        [JsonPropertyName("tag")]
        public string? TagStr { get; set; }

        [JsonPropertyName("translated_name")]
        public string? TranslatedName { get; set; }

        [JsonPropertyName("illust")]
        public Illustration? Illust { get; set; }
    }
}