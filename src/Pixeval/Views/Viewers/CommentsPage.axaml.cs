// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;
using Mako.Global.Enum;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Comment;

namespace Pixeval.Views.Viewers;

public partial class CommentsPage : ContentPage
{
    private CommentsViewViewModel? ViewModel => DataContext as CommentsViewViewModel;

    // Flyout for comment replies
    private Flyout? _repliesFlyout;
    private CommentRepliesBlock? _repliesBlock;

    public CommentsPage() => InitializeComponent();

    private void CommentView_OnOpenRepliesButtonClick(CommentItemViewModel viewModel)
    {
        _repliesBlock ??= new CommentRepliesBlock();
        _repliesBlock.DataContext = viewModel;
        if (ViewModel is not null)
        {
            _repliesBlock.EntryId = ViewModel.EntryId;
            _repliesBlock.SimpleWorkType = ViewModel.EntryType;
        }

        _repliesFlyout ??= new Flyout { Content = _repliesBlock };
        _repliesFlyout.ShowAt(CommentListView);
    }

    private async void ReplyBar_OnSendButtonClick(object? sender, SendButtonClickEventArgs e)
    {
        if (ViewModel is null)
            return;

        _ = ViewModel.EntryType switch
        {
            SimpleWorkType.IllustrationAndManga => await App.AppViewModel.MakoClient.AddIllustrationCommentAsync(
                ViewModel.EntryId, e.ReplyContent),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                ViewModel.EntryId, e.ReplyContent),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void CommentView_OnDeleteButtonClick(CommentItemViewModel viewModel)
    {
        ViewModel?.DeleteComment(viewModel);
    }

    private SimpleWorkType CommentView_OnRequireEntryType() => ViewModel?.EntryType ?? SimpleWorkType.IllustrationAndManga;
}
