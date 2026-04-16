// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views;

public partial class CommentView : UserControl
{
    public CommentView() => InitializeComponent();

    public event Action<CommentItemViewModel>? OpenRepliesButtonClick;

    private void CommentItem_OnOpenRepliesButtonClick(CommentItemViewModel viewModel) => OpenRepliesButtonClick?.Invoke(viewModel);

    private void CommentItem_OnDeleteButtonClick(CommentItemViewModel viewModel) => (DataContext as CommentsViewViewModel)?.DeleteComment(viewModel);

    private void CommentView_OnDataContextChanged(object? sender, EventArgs e) => (DataContext as CommentsViewViewModel)?.RefreshEngine();

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is CommentsViewViewModel vm)
            RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, vm));
    }

    #endregion
}
