// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Mako.Model;

namespace Mako.Net.Response;

public interface IPixivNextUrlResponse<TEntity> where TEntity : class, IEntry
{
    [JsonPropertyName("next_url")]
    string? NextUrl { get; set; }

    TEntity[] Entities { get; set; }
}
