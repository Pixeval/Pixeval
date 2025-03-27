// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Mako.Net.Request;

public record DeleteCommentRequest([property: JsonPropertyName("comment_id")] long CommentId);
