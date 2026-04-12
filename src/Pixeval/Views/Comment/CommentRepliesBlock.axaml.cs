// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Mako.Global.Enum;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Comment;

public partial class CommentRepliesBlock : UserControl
{
    public CommentRepliesBlock() => InitializeComponent();

    private CommentItemViewModel? ViewModel => DataContext as CommentItemViewModel;

    public long EntryId { get; set; }

    public SimpleWorkType SimpleWorkType { get; set; }

    private void CommentView_OnOpenRepliesButtonClick(CommentItemViewModel viewModel)
    {
        ReplyBar.FindControl<TextBox>("ReplyContentTextBox")?.Focus();
    }

    private void CommentView_OnDeleteButtonClick(CommentItemViewModel viewModel) => ViewModel?.DeleteComment(viewModel);

    private SimpleWorkType CommentView_OnRequireEntryType() => SimpleWorkType;

    private async void ReplyBar_OnSendButtonClick(object? sender, SendButtonClickEventArgs e)
    {
        if (ViewModel is null)
            return;

        var comment = SimpleWorkType switch
        {
            SimpleWorkType.IllustrationAndManga => await App.AppViewModel.MakoClient.AddIllustrationCommentAsync(
                EntryId, ViewModel.Id, e.ReplyContent),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                EntryId, ViewModel.Id, e.ReplyContent),
            _ => throw new ArgumentOutOfRangeException()
        };
        ViewModel.AddComment(comment);
    }
}
