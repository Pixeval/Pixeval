// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Request;

public record RemoveIllustBookmarkRequest([property: JsonPropertyName("illust_id")] long IllustId);
