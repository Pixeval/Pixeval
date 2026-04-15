// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Viewers;

namespace Pixeval.Views;

public partial class CommentItem : UserControl
{
    public CommentItem() => InitializeComponent();

    private CommentItemViewModel ViewModel => (CommentItemViewModel) DataContext!;

    public event Action<CommentItemViewModel>? OpenRepliesButtonClick;

    public event Action<CommentItemViewModel>? DeleteButtonClick;

    private async void PosterButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
            await viewContainer.CreateUserPageAsync(ViewModel.UserId);
    }

    private void OpenRepliesButton_OnClicked(object? sender, RoutedEventArgs e) => OpenRepliesButtonClick?.Invoke(ViewModel);

    private async void DeleteReplyButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (await ViewModel.DeleteAsync())
            DeleteButtonClick?.Invoke(ViewModel);
    }
}
