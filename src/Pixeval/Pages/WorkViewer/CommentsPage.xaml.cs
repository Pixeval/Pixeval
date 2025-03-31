// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using WinUI3Utilities;

namespace Pixeval.Pages;

public sealed partial class CommentsPage
{
    private CommentsPageViewModel _viewModel = null!;

    public CommentsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        var (entryType, id) = ((SimpleWorkType, long)) e.Parameter;
        var engine = entryType switch
        {
            SimpleWorkType.IllustAndManga => App.AppViewModel.MakoClient.IllustrationComments(id),
            SimpleWorkType.Novel => App.AppViewModel.MakoClient.NovelComments(id),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, IFetchEngine<Comment>>(entryType)
        };
        _viewModel = new CommentsPageViewModel(engine, entryType, id);
    }

    private void CommentView_OnOpenRepliesButtonClick(CommentItemViewModel viewModel)
    {
        CommentRepliesBlock.ViewModel = viewModel;
        CommentRepliesTeachingTip.IsOpen = true;
    }

    private async void ReplyBar_OnSendButtonClick(object? sender, SendButtonClickEventArgs e)
    {
        _ = _viewModel.EntryType switch
        {
            SimpleWorkType.IllustAndManga => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                _viewModel.EntryId,
                e.ReplyContentRichEditBoxStringContent),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                _viewModel.EntryId,
                e.ReplyContentRichEditBoxStringContent),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, Comment>(_viewModel.EntryType)
        };
        await _viewModel.AddCommentAsync();
    }

    private async void ReplyBar_OnStickerClick(object? sender, StickerClickEventArgs e)
    {
        _ = _viewModel.EntryType switch
        {
            SimpleWorkType.IllustAndManga => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                _viewModel.EntryId,
                e.StickerViewModel.StickerId),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                _viewModel.EntryId,
                e.StickerViewModel.StickerId),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, Comment>(_viewModel.EntryType)
        };
        await _viewModel.AddCommentAsync();
    }

    private void CommentView_OnDeleteButtonClick(CommentItemViewModel viewModel) => _viewModel.DeleteComment(viewModel);

    private SimpleWorkType CommentView_OnRequireEntryType() => _viewModel.EntryType;
}
