// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

using Mako.Model;

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalCommentDto(
    long Id,
    string Content,
    DateTimeOffset Date,
    PixevalUserDto User,
    bool IsMine,
    bool HasReplies,
    PixevalStampDto? Stamp)
{
    public static PixevalCommentDto FromComment(Comment comment, long? currentUserId) =>
        new(
            comment.Id,
            comment.Content,
            comment.Date,
            PixevalUserDto.FromUserInfo(comment.User),
            comment.User.Id == currentUserId,
            comment.HasReplies,
            comment.Stamp is { } stamp ? new(stamp.StampId, stamp.StampUrl) : null);
}