// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response;

public class PostCommentResponse
{
    [JsonPropertyName("comment")]
    public required Comment Comment { get; set; }
}
