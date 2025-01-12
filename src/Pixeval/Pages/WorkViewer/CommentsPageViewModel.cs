// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;

namespace Pixeval.Pages;

public partial class CommentsPageViewModel : ObservableObject
{
    public CommentsPageViewModel(IAsyncEnumerable<Comment?> engine, SimpleWorkType type, long entryId)
    {
        EntryId = entryId;
        EntryType = type;
        View = new(new IncrementalLoadingCollection<CommentsIncrementalSource, CommentItemViewModel>(
            new CommentsIncrementalSource(engine.Where(c => c is not null)
                .Select(c => new CommentItemViewModel(c!, type, entryId))), 30));
        View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
    }

    public long EntryId { get; }

    public SimpleWorkType EntryType { get; }

    /// <summary>
    /// 不用!<see cref="AdvancedObservableCollection{T}.HasMoreItems"/>，此处只是为了表示集合有没有元素
    /// </summary>
    public bool HasNoItem => View.Count is 0;

    public AdvancedObservableCollection<CommentItemViewModel> View { get; }

    public void AddComment(Comment comment)
    {
        View.Insert(0, new CommentItemViewModel(comment, EntryType, EntryId));
    }

    public void DeleteComment(CommentItemViewModel viewModel)
    {
        _ = View.Remove(viewModel);
    }
}
