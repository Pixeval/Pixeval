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
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Util.IO;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class CommentsPage
{
    private CommentsPageViewModel _viewModel = null!;

    public CommentsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        var (engine, illustrationId) = ((IAsyncEnumerable<Comment>, long))e.Parameter;
        _viewModel = new CommentsPageViewModel(engine, illustrationId);
    }

    private void CommentList_OnRepliesHyperlinkButtonTapped(CommentBlockViewModel viewModel)
    {
        CommentRepliesBlock.ViewModel = viewModel;
        CommentRepliesTeachingTip.IsOpen = true;
    }

    private async void ReplyBar_OnSendButtonTapped(object? sender, SendButtonTappedEventArgs e)
    {
        using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi)
            .PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment,
                ("illust_id", _viewModel.IllustrationId.ToString()),
                ("comment", e.ReplyContentRichEditBoxStringContent));

        await AddComment(result);
    }

    private async void ReplyBar_OnStickerTapped(object? sender, StickerTappedEventArgs e)
    {
        using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi).PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment,
            ("illust_id", _viewModel.IllustrationId.ToString()),
            ("stamp_id", e.StickerViewModel.StickerId.ToString()));

        await AddComment(result);
    }

    private async Task AddComment(HttpResponseMessage postCommentResponse)
    {
        if (postCommentResponse.IsSuccessStatusCode && await postCommentResponse.Content.ReadFromJsonAsync<PostCommentResponse>() is { Comment: { } comment })
            _viewModel.AddComment(comment);
    }
}
