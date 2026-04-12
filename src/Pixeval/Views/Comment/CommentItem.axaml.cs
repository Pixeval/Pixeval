// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.Views.Comment;

public partial class CommentItem : UserControl
{
    public CommentItem() => InitializeComponent();

    private ViewModels.Viewers.CommentItemViewModel ViewModel => (ViewModels.Viewers.CommentItemViewModel) DataContext!;

    public event Action<ViewModels.Viewers.CommentItemViewModel>? OpenRepliesButtonClick;

    public event Action<ViewModels.Viewers.CommentItemViewModel>? DeleteButtonClick;

    public event Func<SimpleWorkType>? RequireEntryType;

    protected override async void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not ViewModels.Viewers.CommentItemViewModel viewModel)
            return;
        if (RequireEntryType is { } requireEntryType)
            _ = viewModel.LoadRepliesAsync(requireEntryType());
        _ = viewModel.LoadAvatarSource();
        if (viewModel.IsStamp)
            StickerImage.Source = await CacheHelper.GetBitmapFromCacheAsync(viewModel.StampSource);
    }

    private void PosterButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        // TODO: Navigate to illustrator page
    }

    private void OpenRepliesButton_OnClicked(object? sender, RoutedEventArgs e) => OpenRepliesButtonClick?.Invoke(ViewModel);

    private async void DeleteReplyButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (RequireEntryType?.Invoke() is not { } entryType)
            return;

        using var result = entryType switch
        {
            SimpleWorkType.IllustrationAndManga => await App.AppViewModel.MakoClient.DeleteIllustrationCommentAsync(ViewModel.Id),
            SimpleWorkType.Novel => await App.AppViewModel.MakoClient.DeleteNovelCommentAsync(ViewModel.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(entryType))
        };
        if (result.IsSuccessStatusCode)
            DeleteButtonClick?.Invoke(ViewModel);
    }
}
