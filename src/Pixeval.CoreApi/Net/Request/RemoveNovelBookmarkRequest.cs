// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Request;

public record RemoveNovelBookmarkRequest([property: JsonPropertyName("novel_id")] long NovelId);
