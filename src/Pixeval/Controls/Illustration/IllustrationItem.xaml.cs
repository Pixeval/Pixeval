// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Options;
using Windows.Foundation;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class IllustrationItem
{
    [GeneratedDependencyProperty]
    public partial IllustrationItemViewModel ViewModel { get; set; }

    public event TypedEventHandler<IllustrationItem, IllustrationItemViewModel>? ViewModelChanged;

    public event TypedEventHandler<IllustrationItem, IllustrationItemViewModel>? RequestOpenUserInfoPage;

    public event TypedEventHandler<IllustrationItem, IllustrationItemViewModel>? RequestAddToBookmark;

    public event Func<(ThumbnailDirection ThumbnailDirection, double DesiredHeight)> RequiredParam = null!;

    public event Func<TeachingTip> RequestTeachingTip = null!;

    partial void OnViewModelPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        ViewModelChanged?.Invoke(this, ViewModel);
    }

    public IllustrationItem() => InitializeComponent();

    private double DesiredHeight => RequiredParam().DesiredHeight;

    private TeachingTip QrCodeTeachingTip => RequestTeachingTip();

    /// <summary>
    /// 这些方法本来用属性就可以实现，但在ViewModel更新的时候更新，使用了{x:Bind GetXXX(ViewModel)}的写法。
    /// 这样可以不需要写OnPropertyChanged就实现更新
    /// </summary>
    private double GetDesiredWidth(IllustrationItemViewModel viewModel)
    {
        var illustration = viewModel.Entry;
        var thumbnailDirection = RequiredParam().ThumbnailDirection;
        return thumbnailDirection switch
        {
            ThumbnailDirection.Landscape => WorkView.LandscapeHeight * illustration.Width / illustration.Height,
            ThumbnailDirection.Portrait => WorkView.PortraitHeight * illustration.Width / illustration.Height,
            _ => ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(thumbnailDirection)
        };
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
