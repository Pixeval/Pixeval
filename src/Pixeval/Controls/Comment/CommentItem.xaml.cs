// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Util.IO.Caching;
using WinUI3Utilities.Attributes;
using IllustrationCacheTable = Pixeval.Caching.CacheTable<
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheKey,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheHeader,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheProtocol>;

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
            block.StickerImageContent.Source = await App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationCacheTable>().GetSourceFromCacheAsync(viewModel.StampSource);
        }
        else
        {
            block.CommentContent.RawText.Blocks.Clear();
            block.CommentContent.RawText.Blocks.Add(await viewModel.GetReplyContentParagraphAsync());
        }
    }

    private async void PosterPersonPicture_OnClicked(object sender, RoutedEventArgs e)
    {
        await this.CreateIllustratorPageAsync(ViewModel.PosterId);
    }

    private void OpenRepliesHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => RepliesHyperlinkButtonClick?.Invoke(ViewModel);

    private void DeleteReplyHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => DeleteHyperlinkButtonClick?.Invoke(ViewModel);
}
