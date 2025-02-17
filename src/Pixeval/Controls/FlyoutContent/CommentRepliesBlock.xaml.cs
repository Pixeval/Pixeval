// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using WinUI3Utilities;

namespace Pixeval.Controls.FlyoutContent;

public sealed partial class CommentRepliesBlock
{
    [GeneratedDependencyProperty]
    public partial CommentItemViewModel ViewModel { get; set; }

    public long EntryId { get; set; }

    public SimpleWorkType SimpleWorkType { get; set; }

    public CommentRepliesBlock() => InitializeComponent();

    private void CommentView_OnOpenRepliesButtonClick(CommentItemViewModel viewModel) => ReplyBar.FindDescendant<RichEditBox>()?.Focus(FocusState.Programmatic);

    private void CommentView_OnDeleteButtonClick(CommentItemViewModel viewModel) => ViewModel.DeleteComment(viewModel);

    private SimpleWorkType CommentView_OnRequireEntryType() => SimpleWorkType;

    private async void ReplyBar_OnSendButtonClick(object? sender, SendButtonClickEventArgs e)
    {
        var comment = SimpleWorkType switch
        {
            SimpleWorkType.IllustAndManga => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                EntryId,
                ViewModel.Id,
                e.ReplyContentRichEditBoxStringContent),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                EntryId,
                ViewModel.Id,
                e.ReplyContentRichEditBoxStringContent),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, Comment>(SimpleWorkType)
        };
        ViewModel.AddComment(comment);
    }

    private async void ReplyBar_OnStickerClick(object? sender, StickerClickEventArgs e)
    {
        var comment = SimpleWorkType switch
        {
            SimpleWorkType.IllustAndManga => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                EntryId,
                ViewModel.Id,
                e.StickerViewModel.StickerId),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                EntryId,
                ViewModel.Id,
                e.StickerViewModel.StickerId),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, Comment>(SimpleWorkType)
        };
        ViewModel.AddComment(comment);
    }
}
