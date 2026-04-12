// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;

namespace Pixeval.ViewModels.Viewers;

public class CommentsViewViewModel : ObservableObject, IDisposable
{
    public CommentsViewViewModel(IFetchEngine<Comment> engine, SimpleWorkType type, long entryId)
    {
        EntryId = entryId;
        EntryType = type;
        DataProvider.ResetEngine(engine, (comment, _) => CommentItemViewModel.CreateInstance(comment));
    }

    public SimpleViewDataProvider<Comment, CommentItemViewModel> DataProvider { get; } = new();

    public long EntryId { get; }

    public SimpleWorkType EntryType { get; }

    public void DeleteComment(CommentItemViewModel viewModel) => DataProvider.Source.Remove(viewModel);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DataProvider.Dispose();
    }
}
