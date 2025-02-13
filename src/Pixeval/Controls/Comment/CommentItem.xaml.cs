// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;

namespace Pixeval.Controls;

public sealed partial class CommentItem
{
    [GeneratedDependencyProperty(DefaultValue = null!)]
    public partial CommentItemViewModel ViewModel { get; set; }

    public CommentItem() => InitializeComponent();

    public event Action<CommentItemViewModel>? RepliesHyperlinkButtonClick;

    public event Action<CommentItemViewModel>? DeleteHyperlinkButtonClick;

    async partial void OnViewModelPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
            return;
        if (viewModel.HasReplies)
            _ = viewModel.LoadRepliesAsync();
        _ = viewModel.LoadAvatarSource();
        if (viewModel.IsStamp)
        {
            StickerImageContent.Source = await CacheHelper.GetSourceFromCacheAsync(viewModel.StampSource);
        }
        else
        {
            CommentContent.Text = ReplyEmojiHelper.GetContents(viewModel.CommentContent);
        }
    }

    private async void PosterPersonPicture_OnClicked(object sender, RoutedEventArgs e)
    {
        await this.CreateIllustratorPageAsync(ViewModel.PosterId);
    }

    private void OpenRepliesHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => RepliesHyperlinkButtonClick?.Invoke(ViewModel);

    private void DeleteReplyHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => DeleteHyperlinkButtonClick?.Invoke(ViewModel);
}
