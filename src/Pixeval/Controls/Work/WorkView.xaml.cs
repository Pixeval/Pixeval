// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Pages.NovelViewer;
using Pixeval.Util.UI;
using Windows.Foundation;
using Misaki;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class WorkView : IEntryView<ISortableEntryViewViewModel>, IStructuralDisposalCompleter
{
    public const double LandscapeHeight = 180;
    public const double PortraitHeight = 240;

    public double DesiredHeight => ThumbnailDirection switch
    {
        ThumbnailDirection.Landscape => LandscapeHeight,
        ThumbnailDirection.Portrait => PortraitHeight,
        _ => ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(ThumbnailDirection)
    };

    public double DesiredWidth => ThumbnailDirection switch
    {
        ThumbnailDirection.Landscape => PortraitHeight,
        ThumbnailDirection.Portrait => LandscapeHeight,
        _ => ThrowHelper.ArgumentOutOfRange<ThumbnailDirection, double>(ThumbnailDirection)
    };

    public ItemsViewLayoutType LayoutType { get; set; }

    public ThumbnailDirection ThumbnailDirection { get; set; }

    public WorkView() => InitializeComponent();

    public event TypedEventHandler<WorkView, ISortableEntryViewViewModel>? ViewModelChanged;

    public AdvancedItemsView AdvancedItemsView => ItemsView;

    public ScrollView ScrollView => AdvancedItemsView.ScrollView;

    /// <summary>
    /// 在调用<see cref="ResetEngine"/>前为<see langword="null"/>
    /// </summary>
    [GeneratedDependencyProperty]
    public partial ISortableEntryViewViewModel ViewModel { get; set; }

    [GeneratedDependencyProperty]
    public partial SimpleWorkType Type { get; set; }

    private async void WorkItem_OnViewModelChanged(FrameworkElement sender, IWorkViewModel viewModel)
    {
        if (ViewModel == null!)
            return;
        if (await viewModel.TryLoadThumbnailAsync(ViewModel))
            if (sender.IsFullyOrPartiallyVisible(this))
                sender.GetResource<Storyboard>("ThumbnailStoryboard").Begin();
            else
                sender.Opacity = 1;
    }

    private void ItemsView_OnItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs e)
    {
        switch (e.InvokedItem, ViewModel)
        {
            case (NovelItemViewModel viewModel, NovelViewViewModel viewViewModel):
                this.CreateNovelPage(viewModel, viewViewModel);
                break;
            case (IllustrationItemViewModel viewModel, IllustrationViewViewModel viewViewModel):
                this.CreateIllustrationPage(viewModel, viewViewModel);
                break;
        }
    }

    private void WorkView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        if (ViewModel == null!)
            return;
        if (sender.SelectedItems is not { Count: > 0 })
        {
            ViewModel.SelectedEntries = ViewModel switch
            {
                NovelViewViewModel => (NovelItemViewModel[]) [],
                IllustrationViewViewModel => (IllustrationItemViewModel[]) [],
                _ => ViewModel.SelectedEntries
            };
            return;
        }

        ViewModel.SelectedEntries = ViewModel switch
        {
            NovelViewViewModel => [.. sender.SelectedItems.Cast<NovelItemViewModel>()],
            IllustrationViewViewModel => [.. sender.SelectedItems.Cast<IllustrationItemViewModel>()],
            _ => ViewModel.SelectedEntries
        };
    }

    [MemberNotNull(nameof(ViewModel))]
    public void ResetEngine(IFetchEngine<IArtworkInfo> newEngine, int itemsPerPage = 20, int itemLimit = -1)
    {
        var type = newEngine.GetType().GetInterfaces()[0].GenericTypeArguments.SingleOrDefault();
        switch (ViewModel)
        {
            case NovelViewViewModel when type == typeof(Novel):
            case IllustrationViewViewModel:
                ViewModel.ResetEngine(newEngine, itemsPerPage, itemLimit);
                break;
            default:
                if (type == typeof(Novel))
                {
                    Type = SimpleWorkType.Novel;
                    ViewModel?.Dispose();
                    ViewModel = null!;
                    AdvancedItemsView.MinItemWidth = 350;
                    AdvancedItemsView.MinItemHeight = 200;
                    AdvancedItemsView.LayoutType = ItemsViewLayoutType.Grid;
                    AdvancedItemsView.ItemTemplate = this.GetResource<DataTemplate>("NovelItemDataTemplate");
                    ViewModel = new NovelViewViewModel();
                }
                else
                {
                    Type = SimpleWorkType.IllustAndManga;
                    ViewModel?.Dispose();
                    ViewModel = null!;
                    AdvancedItemsView.MinItemWidth = DesiredWidth;
                    AdvancedItemsView.MinItemHeight = DesiredHeight;
                    AdvancedItemsView.LayoutType = LayoutType;
                    AdvancedItemsView.ItemTemplate = this.GetResource<DataTemplate>("IllustrationItemDataTemplate");
                    ViewModel = new IllustrationViewViewModel();
                }

                ViewModel.ResetEngine(newEngine, itemsPerPage, itemLimit);
                ViewModelChanged?.Invoke(this, ViewModel);
                AdvancedItemsView.ItemsSource = ViewModel.View;

                break;
        }
    }

    private TeachingTip WorkItem_OnRequestTeachingTip() => EntryView.QrCodeTeachingTip;

    private (ThumbnailDirection ThumbnailDirection, double DesiredHeight) IllustrationItem_OnRequiredParam() => (ThumbnailDirection, DesiredHeight);

    private void AddToBookmarkTeachingTip_OnCloseButtonClick(TeachingTip sender, object e)
    {
        if (sender.GetTag<IWorkViewModel>().AddToBookmarkCommand is not { } command)
            return;

        command.Execute((BookmarkTagSelector.SelectedTags, BookmarkTagSelector.IsPrivate, null as object));

        this.SuccessGrowl(EntryViewResources.AddedToBookmark);
    }

    private void WorkItem_OnRequestAddToBookmark(FrameworkElement sender, IWorkViewModel e)
    {
        AddToBookmarkTeachingTip.Tag = e;
        AddToBookmarkTeachingTip.IsOpen = true;
    }

    public async void WorkItem_OnRequestOpenUserInfoPage(FrameworkElement sender, IWorkViewModel e)
    {
        if (e.Entry.Platform is IPlatformInfo.Pixiv)
            await this.CreateIllustratorPageAsync(long.Parse(e.Entry.Authors[0].Id));
    }

    public void CompleteDisposal()
    {
        if (ViewModel == null!)
            return;
        var viewModel = ViewModel;
        ViewModel = null!;
        foreach (var vm in viewModel.Source)
            vm.UnloadThumbnail(viewModel);
        viewModel.Dispose();
    }

    public List<Action<IStructuralDisposalCompleter?>> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    private void WorkView_OnLoaded(object sender, RoutedEventArgs e)
    {
        ((IStructuralDisposalCompleter) this).Hook();
    }
}
