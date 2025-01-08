// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response;

public interface IPixivNextUrlResponse<TEntity> where TEntity : class, IEntry
{
    [JsonPropertyName("next_url")]
    string? NextUrl { get; set; }

    TEntity[] Entities { get; set; }
}
