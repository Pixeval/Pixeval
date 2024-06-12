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
using Pixeval.CoreApi;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Net.Response;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.FlyoutContent;

[DependencyProperty<CommentItemViewModel>("ViewModel")]
public sealed partial class CommentRepliesBlock
{
    public CommentRepliesBlock() => InitializeComponent();

    private void CommentView_OnRepliesHyperlinkButtonClick(CommentItemViewModel viewModel)
    {
        _ = ReplyBar.FindDescendant<RichEditBox>()?.Focus(FocusState.Programmatic);
    }

    private async void ReplyBar_OnSendButtonClick(object? sender, SendButtonClickEventArgs e)
    {
        using var result = ViewModel.EntryType switch
        {
            SimpleWorkType.IllustAndManga => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                ViewModel.EntryId,
                ViewModel.CommentId,
                e.ReplyContentRichEditBoxStringContent),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                ViewModel.EntryId,
                ViewModel.CommentId,
                e.ReplyContentRichEditBoxStringContent),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, HttpResponseMessage>(ViewModel.EntryType)
        };

        await AddComment(result);
    }

    private async void ReplyBar_OnStickerClick(object? sender, StickerClickEventArgs e)
    {
        using var result = ViewModel.EntryType switch
        {
            SimpleWorkType.IllustAndManga => await App.AppViewModel.MakoClient.AddIllustCommentAsync(
                ViewModel.EntryId,
                ViewModel.CommentId,
                e.StickerViewModel.StickerId),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.AddNovelCommentAsync(
                ViewModel.EntryId,
                ViewModel.CommentId,
                e.StickerViewModel.StickerId),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, HttpResponseMessage>(ViewModel.EntryType)
        };

        await AddComment(result);
    }

    private async Task AddComment(HttpResponseMessage postCommentResponse)
    {
        if (postCommentResponse.IsSuccessStatusCode && await postCommentResponse.Content.ReadFromJsonAsync(typeof(PostCommentResponse), AppJsonSerializerContext.Default) is PostCommentResponse { Comment: { } comment })
            ViewModel.AddComment(comment);
    }
}
