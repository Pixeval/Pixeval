#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/CommentRepliesBlock.xaml.cs
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Attributes;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;

using Pixeval.Pages.IllustrationViewer;
using Pixeval.UserControls;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.Popups;

[DependencyProperty("ViewModel", typeof(CommentRepliesBlockViewModel), nameof(OnViewModelChanged))]
public sealed partial class CommentRepliesBlock : IAppPopupContent
{
    private EventHandler<TappedRoutedEventArgs>? _closeButtonTapped;

    public CommentRepliesBlock()
    {
        UniqueId = Guid.NewGuid();
        InitializeComponent();
    }


    public Guid UniqueId { get; }

    public FrameworkElement UIContent => this;

    public event EventHandler<TappedRoutedEventArgs> CloseButtonTapped
    {
        add => _closeButtonTapped += value;
        remove => _closeButtonTapped -= value;
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var block = (CommentRepliesBlock) d;
        var viewModel = (CommentRepliesBlockViewModel) e.NewValue;
        block.RepliesAreEmptyPanel.Visibility = (!viewModel.HasReplies).ToVisibility();
        block.CommentList.Visibility = viewModel.HasReplies.ToVisibility();
        if (viewModel.HasReplies && viewModel.Comment.Replies is { } rs)
        {
            block.CommentList.ItemsSource = rs;
        }
    }

    private void CommentRepliesBlock_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Focus the popup content so that the hot key for closing can work properly
        CloseButton.Focus(FocusState.Programmatic);
    }

    private void CloseButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _closeButtonTapped?.Invoke(sender, e);
    }

    private void CommentList_OnRepliesHyperlinkButtonTapped(object? sender, TappedRoutedEventArgs e)
    {
        ReplyBar.FindDescendant<RichEditBox>()?.Focus(FocusState.Programmatic);
    }

    private async void ReplyBar_OnSendButtonTapped(object? sender, SendButtonTappedEventArgs e)
    {
        using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi).PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment,
            ("illust_id", ViewModel.Comment.IllustrationId),
            ("parent_comment_id", ViewModel.Comment.CommentId),
            ("comment", e.ReplyContentRichEditBoxStringContent));

        await AddComment(result);
    }

    private async void ReplyBar_OnStickerTapped(object? sender, StickerTappedEventArgs e)
    {
        using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi).PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment,
            ("illust_id", ViewModel.Comment.IllustrationId),
            ("parent_comment_id", ViewModel.Comment.CommentId),
            ("stamp_id", e.StickerViewModel.StickerId.ToString()));

        await AddComment(result);
    }

    private async Task AddComment(HttpResponseMessage postCommentResponse)
    {
        RepliesAreEmptyPanel.Visibility = Visibility.Collapsed;
        CommentList.Visibility = Visibility.Visible;

        if (CommentList.ItemsSource is IEnumerable<object> enumerable && !enumerable.Any())
        {
            CommentList.ItemsSource = new ObservableCollection<CommentBlockViewModel>();
        }

        if (postCommentResponse.IsSuccessStatusCode)
        {
            var response = await postCommentResponse.Content.ReadFromJsonAsync<PostCommentResponse>();
            (CommentList.ItemsSource as ObservableCollection<CommentBlockViewModel>)?.Insert(0, new CommentBlockViewModel(response?.Comment!, ViewModel.Comment.IllustrationId));
        }
    }
}