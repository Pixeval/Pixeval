#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/CommentsPage.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
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

    private void CommentList_OnRepliesHyperlinkButtonTapped(CommentBlockViewModel viewModel)
    {
        CommentRepliesBlock.ViewModel = viewModel;
        CommentRepliesTeachingTip.IsOpen = true;
    }

    private async void ReplyBar_OnSendButtonTapped(object? sender, SendButtonTappedEventArgs e)
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

    private async void ReplyBar_OnStickerTapped(object? sender, StickerTappedEventArgs e)
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

    private async void CommentList_OnDeleteHyperlinkButtonTapped(CommentBlockViewModel viewModel)
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
        if (postCommentResponse.IsSuccessStatusCode && await postCommentResponse.Content.ReadFromJsonAsync<PostCommentResponse>() is { Comment: { } comment })
            _viewModel.AddComment(comment);
    }

    private void DeleteComment(HttpResponseMessage postCommentResponse, CommentBlockViewModel viewModel)
    {
        if (postCommentResponse.IsSuccessStatusCode)
            _viewModel.DeleteComment(viewModel);
    }
}
