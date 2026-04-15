// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Global.Enum;
using Mako.Model;

namespace Pixeval.ViewModels.Viewers;

public class CommentsViewViewModel(SimpleWorkType parentType, long parentId) : ObservableObject, IDisposable
{
    public long ParentId { get; } = parentId;

    public SimpleWorkType ParentType { get; } = parentType;

    public SimpleViewDataProvider<Comment, CommentItemViewModel> DataProvider { get; } = new();

    public virtual async Task<Comment> AddCommentAsync(string content)
    {
        return await (ParentType is SimpleWorkType.Novel
            ? App.AppViewModel.MakoClient.AddNovelCommentAsync(ParentId, content)
            : App.AppViewModel.MakoClient.AddIllustrationCommentAsync(ParentId, content));
    }

    public virtual async Task<Comment> AddStickerAsync(int stampId)
    {
        return await (ParentType is SimpleWorkType.Novel
            ? App.AppViewModel.MakoClient.AddNovelCommentAsync(ParentId, stampId)
            : App.AppViewModel.MakoClient.AddIllustrationCommentAsync(ParentId, stampId));
    }

    public virtual void AddComment(Comment comment) => DataProvider.Source.Insert(0, new CommentItemViewModel(comment, ParentType, ParentId, true));

    public void DeleteComment(CommentItemViewModel viewModel) => DataProvider.Source.Remove(viewModel);

    public virtual void RefreshEngine()
    {
        DataProvider.ResetEngine(ParentType is SimpleWorkType.Novel
                ? App.AppViewModel.MakoClient.NovelComments(ParentId)
                : App.AppViewModel.MakoClient.IllustrationComments(ParentId),
            (comment, _) => new(comment, ParentType, ParentId, true));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DataProvider.Dispose();
    }
}
