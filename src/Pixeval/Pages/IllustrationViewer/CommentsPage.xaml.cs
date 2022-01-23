#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/CommentsPage.xaml.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Messages;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class CommentsPage
{
    private string? _illustId;

    public CommentsPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        // Dispose current page contents if the parent page (IllustrationViewerPage) is navigating
        WeakReferenceMessenger.Default.TryRegister<CommentsPage, NavigatingFromIllustrationViewerMessage>(this, (recipient, _) =>
        {
            recipient.CommentList.Dispose();
            WeakReferenceMessenger.Default.UnregisterAll(this);
        });

        var (engine, illustId) = ((IAsyncEnumerable<Comment>, string)) e.Parameter;
        _illustId = illustId;
        if (CommentList.ItemsSource is not ICollection<CommentBlockViewModel>)
        {
            CommentList.ItemsSource = new IncrementalLoadingCollection<CommentsIncrementalSource, CommentBlockViewModel>(
                new CommentsIncrementalSource(engine.Select(c => new CommentBlockViewModel(c, illustId))), 30);
        }
    }

    private void CommentList_OnRepliesHyperlinkButtonTapped(object? sender, TappedRoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new CommentRepliesHyperlinkButtonTappedMessage(sender));
    }

    private async void ReplyBar_OnSendButtonTapped(object? sender, SendButtonTappedEventArgs e)
    {
        using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi).PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment,
            ("illust_id", _illustId!),
            ("comment", e.ReplyContentRichEditBoxStringContent));

        await AddComment(result);
    }

    private async void ReplyBar_OnStickerTapped(object? sender, StickerTappedEventArgs e)
    {
        using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi).PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment,
            ("illust_id", _illustId!),
            ("stamp_id", e.StickerViewModel.StickerId.ToString()));

        await AddComment(result);
    }

    private async Task AddComment(HttpResponseMessage postCommentResponse)
    {
        if (CommentList.ItemsSource is IEnumerable<object> enumerable && !enumerable.Any())
        {
            CommentList.ItemsSource = new ObservableCollection<CommentBlockViewModel>();
        }

        if (postCommentResponse.IsSuccessStatusCode)
        {
            var response = await postCommentResponse.Content.ReadFromJsonAsync<PostCommentResponse>();
            (CommentList.ItemsSource as ObservableCollection<CommentBlockViewModel>)?.Insert(0, new CommentBlockViewModel(response?.Comment!, _illustId!));
        }
    }
}