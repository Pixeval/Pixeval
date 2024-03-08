#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/CommentRepliesBlock.xaml.cs
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

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Options;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.FlyoutContent;

[DependencyProperty<CommentBlockViewModel>("ViewModel")]
public sealed partial class CommentRepliesBlock
{
    public CommentRepliesBlock() => InitializeComponent();

    private void CommentList_OnRepliesHyperlinkButtonTapped(CommentBlockViewModel viewModel)
    {
        _ = ReplyBar.FindDescendant<RichEditBox>()?.Focus(FocusState.Programmatic);
    }

    private async void ReplyBar_OnSendButtonTapped(object? sender, SendButtonTappedEventArgs e)
    {
        using var result = ViewModel.EntryType switch
        {
            CommentType.Illustration => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                ViewModel.EntryId,
                ViewModel.CommentId,
                e.ReplyContentRichEditBoxStringContent),
            CommentType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                ViewModel.EntryId,
                ViewModel.CommentId,
                e.ReplyContentRichEditBoxStringContent),
            _ => ThrowHelper.ArgumentOutOfRange<CommentType, HttpResponseMessage>(ViewModel.EntryType)
        };

        await AddComment(result);
    }

    private async void ReplyBar_OnStickerTapped(object? sender, StickerTappedEventArgs e)
    {
        using var result = ViewModel.EntryType switch
        {
            CommentType.Illustration => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                ViewModel.EntryId,
                ViewModel.CommentId,
                e.StickerViewModel.StickerId),
            CommentType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                ViewModel.EntryId,
                ViewModel.CommentId,
                e.StickerViewModel.StickerId),
            _ => ThrowHelper.ArgumentOutOfRange<CommentType, HttpResponseMessage>(ViewModel.EntryType)
        };

        await AddComment(result);
    }

    private async Task AddComment(HttpResponseMessage postCommentResponse)
    {
        if (postCommentResponse.IsSuccessStatusCode && await postCommentResponse.Content.ReadFromJsonAsync<PostCommentResponse>() is { Comment: { } comment })
            ViewModel.AddComment(comment);
    }
}
