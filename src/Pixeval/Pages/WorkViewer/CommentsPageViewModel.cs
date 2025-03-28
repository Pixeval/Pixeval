// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Collections;
using Pixeval.Controls;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using CommentViewDataProvider = Pixeval.Controls.SimpleViewDataProvider<
    Mako.Model.Comment,
    Pixeval.Controls.CommentItemViewModel>;

namespace Pixeval.Pages;

public partial class CommentsPageViewModel : ObservableObject, IDisposable
{
    public CommentsPageViewModel(IFetchEngine<Comment> engine, SimpleWorkType type, long entryId)
    {
        EntryId = entryId;
        EntryType = type;
        DataProvider.View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
        DataProvider.ResetEngine(engine);
    }

    /// <summary>
    /// 不用!<see cref="AdvancedObservableCollection{T}.HasMoreItems"/>，此处只是为了表示集合有没有元素
    /// </summary>
    public bool HasNoItem => DataProvider.View.Count is 0;

    public CommentViewDataProvider DataProvider { get; } = new();

    public long EntryId { get; }

    public SimpleWorkType EntryType { get; }

    public async Task AddCommentAsync()
    {
        await DataProvider.View.LoadMoreItemsAsync(20);
    }

    public void DeleteComment(CommentItemViewModel viewModel) => DataProvider.Source.Remove(viewModel);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DataProvider.Dispose();
    }
}
