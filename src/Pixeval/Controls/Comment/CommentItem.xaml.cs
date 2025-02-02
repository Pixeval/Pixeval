// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Util.IO.Caching;
using IllustrationCacheTable = Pixeval.Caching.CacheTable<
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheKey,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheHeader,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheProtocol>;

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
            StickerImageContent.Source = await App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationCacheTable>().GetSourceFromCacheAsync(viewModel.StampSource);
        }
        else
        {
            CommentContent.RawText.Blocks.Clear();
            CommentContent.RawText.Blocks.Add(await viewModel.GetReplyContentParagraphAsync());
        }
    }

    private async void PosterPersonPicture_OnClicked(object sender, RoutedEventArgs e)
    {
        await this.CreateIllustratorPageAsync(ViewModel.PosterId);
    }

    private void OpenRepliesHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => RepliesHyperlinkButtonClick?.Invoke(ViewModel);

    private void DeleteReplyHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => DeleteHyperlinkButtonClick?.Invoke(ViewModel);
}
