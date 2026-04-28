// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Mako.Global.Enum;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class CommentItem
{
    [GeneratedDependencyProperty(DefaultValue = null!)]
    public partial CommentItemViewModel ViewModel { get; set; }

    public CommentItem() => InitializeComponent();

    public event Action<CommentItemViewModel>? OpenRepliesButtonClick;

    public event Action<CommentItemViewModel>? DeleteButtonClick;

    public event Func<SimpleWorkType> RequireEntryType = null!;

    async partial void OnViewModelPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
            return;
        _ = viewModel.LoadRepliesAsync(RequireEntryType());
        _ = viewModel.LoadAvatarSource();
        if (viewModel.IsStamp)
            StickerImageContent.Source = await CacheHelper.GetSourceFromCacheAsync(viewModel.StampSource);
        else
            CommentContent.Text = ReplyEmojiHelper.GetContents(viewModel.CommentContent);
    }

    private async void PosterPersonPicture_OnClicked(object sender, RoutedEventArgs e)
    {
        await this.CreateIllustratorPageAsync(ViewModel.PosterId);
    }

    private void OpenRepliesButton_OnClicked(object sender, RoutedEventArgs e) => OpenRepliesButtonClick?.Invoke(ViewModel);

    private async void DeleteReplyButton_OnClicked(object sender, RoutedEventArgs e)
    {
        var result = RequireEntryType() switch
        {
            SimpleWorkType.IllustrationAndManga => await App.AppViewModel.MakoClient.DeleteIllustrationCommentAsync(ViewModel.Id),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.DeleteNovelCommentAsync(ViewModel.Id),
            _ => ThrowHelper.ArgumentOutOfRange<SimpleWorkType, bool>(RequireEntryType())
        };
        if (result)
            DeleteButtonClick?.Invoke(ViewModel);
    }
}
