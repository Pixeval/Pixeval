// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Mako.Model;

namespace Mako.Net.Response;

public class PostCommentResponse
{
    [JsonPropertyName("comment")]
    public required Comment Comment { get; set; }
}
