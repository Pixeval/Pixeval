// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.CoreApi.Net.Request;

public record AddIllustBookmarkRequest(
    [property: JsonPropertyName("restrict")] PrivacyPolicy Restrict,
    [property: JsonPropertyName("illust_id")] long Id,
    [property: JsonPropertyName("tags[]")] string? Tags);
