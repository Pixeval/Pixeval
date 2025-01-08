// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Response;
using WinUI3Utilities;

namespace Pixeval.Pages;

public sealed partial class CommentsPage
{
    private CommentsPageViewModel _viewModel = null!;

    public CommentsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        var (type, id) = ((SimpleWorkType, long))e.Parameter;
        var engine = type switch
        {
            SimpleWorkType.IllustAndManga => App.AppViewModel.MakoClient.IllustrationComments(id),
            SimpleWorkType.Novel => App.AppViewModel.MakoClient.NovelComments(id),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, IAsyncEnumerable<Comment?>>(type)
        };
        _viewModel = new CommentsPageViewModel(engine, type, id);
    }

    private void CommentView_OnRepliesHyperlinkButtonClick(CommentItemViewModel viewModel)
    {
        CommentRepliesBlock.ViewModel = viewModel;
        CommentRepliesTeachingTip.IsOpen = true;
    }

    private async void ReplyBar_OnSendButtonClick(object? sender, SendButtonClickEventArgs e)
    {
        using var result = _viewModel.EntryType switch
        {
            SimpleWorkType.IllustAndManga => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                _viewModel.EntryId,
                e.ReplyContentRichEditBoxStringContent),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                _viewModel.EntryId,
                e.ReplyContentRichEditBoxStringContent),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, HttpResponseMessage>(_viewModel.EntryType)
        };

        await AddComment(result);
    }

    private async void ReplyBar_OnStickerClick(object? sender, StickerClickEventArgs e)
    {
        using var result = _viewModel.EntryType switch
        {
            SimpleWorkType.IllustAndManga => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                _viewModel.EntryId,
                e.StickerViewModel.StickerId),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                _viewModel.EntryId,
                e.StickerViewModel.StickerId),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, HttpResponseMessage>(_viewModel.EntryType)
        };

        await AddComment(result);
    }

    private async void CommentView_OnDeleteHyperlinkButtonClick(CommentItemViewModel viewModel)
    {
        using var result = _viewModel.EntryType switch
        {
            SimpleWorkType.IllustAndManga => await App.AppViewModel.MakoClient.DeleteIllustCommentAsync(viewModel.CommentId),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.DeleteNovelCommentAsync(viewModel.CommentId),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, HttpResponseMessage>(_viewModel.EntryType)
        };

        DeleteComment(result, viewModel);
    }

    private async Task AddComment(HttpResponseMessage postCommentResponse)
    {
        if (postCommentResponse.IsSuccessStatusCode && await postCommentResponse.Content.ReadFromJsonAsync(typeof(PostCommentResponse), AppJsonSerializerContext.Default) is PostCommentResponse { Comment: { } comment })
            _viewModel.AddComment(comment);
    }

    private void DeleteComment(HttpResponseMessage postCommentResponse, CommentItemViewModel viewModel)
    {
        if (postCommentResponse.IsSuccessStatusCode)
            _viewModel.DeleteComment(viewModel);
    }
}
