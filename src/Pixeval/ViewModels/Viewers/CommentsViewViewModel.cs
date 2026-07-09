// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Threading.Tasks;
using Mako.Global.Enum;
using Mako.Model;

namespace Pixeval.ViewModels.Viewers;

public class CommentsViewViewModel(SimpleWorkType parentType, long parentId) : EntryViewViewModel<Comment, CommentItemViewModel>
{
    public long ParentId { get; } = parentId;

    public SimpleWorkType ParentType { get; } = parentType;

    public override SimpleViewDataProvider<Comment, CommentItemViewModel> DataProvider { get; } = new();

    public virtual async Task<Comment> AddCommentAsync(string content)
    {
        return await App.AppViewModel.MakoClient.AddWorkCommentAsync(ParentType, ParentId, content);
    }

    public virtual async Task<Comment> AddStickerAsync(int stampId)
    {
        return await App.AppViewModel.MakoClient.AddWorkCommentAsync(ParentType, ParentId, stampId);
    }

    public virtual void AddComment(Comment comment) => Source.Insert(0, new CommentItemViewModel(comment, ParentType, ParentId, true));

    public void DeleteComment(CommentItemViewModel viewModel) => Source.Remove(viewModel);

    public virtual void RefreshEngine()
    {
        ResetEngine(App.AppViewModel.MakoClient.WorkComments(ParentType, ParentId),
            (comment, _) => new(comment, ParentType, ParentId, true));
    }
}
