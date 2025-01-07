// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<CommentItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class CommentItem
{
    public CommentItem() => InitializeComponent();

    public event Action<CommentItemViewModel>? RepliesHyperlinkButtonClick;

    public event Action<CommentItemViewModel>? DeleteHyperlinkButtonClick;

    private static async void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CommentItem { ViewModel: { } viewModel } block)
            return;
        if (viewModel.HasReplies)
            _ = viewModel.LoadRepliesAsync();
        _ = viewModel.LoadAvatarSource();
        if (viewModel.IsStamp)
        {
            block.StickerImageContent.Source = await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>().GetSourceFromMemoryCacheAsync(viewModel.StampSource);
        }
        else
        {
            block.CommentContent.Blocks.Clear();
            block.CommentContent.Blocks.Add(await viewModel.GetReplyContentParagraphAsync());
        }
    }

    private async void PosterPersonPicture_OnClicked(object sender, RoutedEventArgs e)
    {
        await IllustratorViewerHelper.CreateWindowWithPageAsync(ViewModel.PosterId);
    }

    private void OpenRepliesHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => RepliesHyperlinkButtonClick?.Invoke(ViewModel);

    private void DeleteReplyHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => DeleteHyperlinkButtonClick?.Invoke(ViewModel);
}
