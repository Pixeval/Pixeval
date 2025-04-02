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
using Mako.Model;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.Windowing;
using Pixeval.Database.Managers;
using WinUI3Utilities;
using Pixeval.Pages.Capability;

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

    private void AddToBookmark_OnClicked(object sender, RoutedEventArgs e)
    {
        RequestAddToBookmark?.Invoke(this, ViewModel);
    }

    private void OpenUserInfoPage_OnClicked(object sender, RoutedEventArgs e)
    {
        RequestOpenUserInfoPage?.Invoke(this, ViewModel);
    }

    /// <summary>
    /// 用Click会卡死，不知道byd为什么
    /// </summary>
    private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var tag = sender.To<FrameworkElement>().GetTag<Tag>();
        SearchHistoryPersistentManager.AddHistory(tag.Name, tag.TranslatedName);
        this.FindAscendant<TabPage>()?.AddPage(new NavigationViewTag<SearchWorksPage>(tag.Name, (SimpleWorkType.Novel, Name)));
    }
}
