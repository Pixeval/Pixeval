// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response;

[Factory]
public partial record AutoCompletionResponse
{
    [JsonPropertyName("tags")]
    public required Tag[] Tags { get; set; } = [];
}
