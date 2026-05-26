// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Pixeval.ViewModels.Viewers;

public sealed class CommentItemViewModel(Comment comment, SimpleWorkType parentType, long parentId, bool isTopComment)
    : CommentsViewViewModel(parentType, parentId)
{
    /// <summary>
    /// 若为<see langword="true"/>，表示这个评论是作品下面的评论；
    /// 否则表示这个评论是另一个评论下面的回复。
    /// Pixiv目前只支持两层评论
    /// </summary>
    public bool IsTopComment { get; } = isTopComment;

    public Comment Comment { get; } = comment;

    public bool HasReplies => Comment.HasReplies;

    [MemberNotNullWhen(true, nameof(StampUrl))]
    public bool IsStamp => Comment.Stamp is not null;

    public string? StampUrl => Comment.Stamp?.StampUrl;

    public DateTimeOffset Date => Comment.Date;

    public string User => Comment.User.Name;

    public long UserId => Comment.User.Id;

    public string CommentContent => CommentImageHelper.GetContents(Comment.Content);

    public bool IsMe => UserId == App.AppViewModel.PixivUid;

    public long Id => Comment.Id;

    public override async Task<Comment> AddCommentAsync(string content)
    {
        if (!IsTopComment)
            return Comment.CreateDefault();
        return await (ParentType is SimpleWorkType.Novel
            ? App.AppViewModel.MakoClient.AddNovelCommentAsync(ParentId, Id, content)
            : App.AppViewModel.MakoClient.AddIllustrationCommentAsync(ParentId, Id, content));
    }

    public override async Task<Comment> AddStickerAsync(int stampId)
    {
        if (!IsTopComment)
            return Comment.CreateDefault();
        return await (ParentType is SimpleWorkType.Novel
            ? App.AppViewModel.MakoClient.AddNovelCommentAsync(ParentId, Id, stampId)
            : App.AppViewModel.MakoClient.AddIllustrationCommentAsync(ParentId, Id, stampId));
    }

    public Task<bool> DeleteAsync()
    {
        if (!IsMe)
            return Task.FromResult(false);
        return ParentType is SimpleWorkType.Novel
            ? App.AppViewModel.MakoClient.DeleteNovelCommentAsync(Id)
            : App.AppViewModel.MakoClient.DeleteIllustrationCommentAsync(Id);
    }

    public override void AddComment(Comment comment) => Source.Insert(0, new CommentItemViewModel(comment, ParentType, ParentId, false));

    public override void RefreshEngine()
    {
        ResetEngine(ParentType is SimpleWorkType.Novel
            ? App.AppViewModel.MakoClient.NovelCommentReplies(Id)
            : App.AppViewModel.MakoClient.IllustrationCommentReplies(Id),
            (comment, _) => new(comment, ParentType, ParentId, false));
    }
}
