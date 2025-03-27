// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Mako.Net.Request;

public record AddStampIllustCommentRequest(
    [property: JsonPropertyName("illust_id")] long Id,
    [property: JsonPropertyName("parent_comment_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? ParentCommentId,
    [property: JsonPropertyName("stamp_id")] int StampId
);

public record AddStampNovelCommentRequest(
    [property: JsonPropertyName("novel_id")] long Id,
    [property: JsonPropertyName("parent_comment_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? ParentCommentId,
    [property: JsonPropertyName("stamp_id")] int StampId
);
