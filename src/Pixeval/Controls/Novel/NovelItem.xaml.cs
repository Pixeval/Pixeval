// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mako.Global.Enum;
using Pixeval.Messages;
using Windows.Foundation;

namespace Pixeval.Controls;

public sealed partial class NovelItem
{
    [GeneratedDependencyProperty]
    public partial NovelItemViewModel ViewModel { get; set; }

    public event TypedEventHandler<NovelItem, NovelItemViewModel>? ViewModelChanged;

    public event TypedEventHandler<NovelItem, NovelItemViewModel>? RequestOpenUserInfoPage;

    public event TypedEventHandler<NovelItem, NovelItemViewModel>? RequestAddToBookmark;

    public event Func<TeachingTip> RequestTeachingTip = null!;

    private TeachingTip QrCodeTeachingTip => RequestTeachingTip();

    public NovelItem() => InitializeComponent();

    partial void OnViewModelPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        ViewModelChanged?.Invoke(this, ViewModel);
    }

    private void TagButton_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.Send(new WorkTagClickedMessage(SimpleWorkType.Novel, ((TextBlock) ((Button) sender).Content).Text));
    }

    private void AddToBookmark_OnClicked(object sender, RoutedEventArgs e)
    {
        RequestAddToBookmark?.Invoke(this, ViewModel);
    }

    private void OpenUserInfoPage_OnClicked(object sender, RoutedEventArgs e)
    {
        RequestOpenUserInfoPage?.Invoke(this, ViewModel);
    }
}
