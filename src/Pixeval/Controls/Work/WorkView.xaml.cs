using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Windows.Foundation;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Pages.NovelViewer;
using Pixeval.Pages.IllustrationViewer;
using WinUI3Utilities;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Engine;
using Pixeval.Options;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Util.UI;
using Pixeval.Controls.Windowing;
using Pixeval.Pages.IllustratorViewer;

namespace Pixeval.Controls;

[ObservableObject]
public sealed partial class WorkView : IEntryView<ISortableEntryViewViewModel>
{
    public const double LandscapeHeight = 180;
    public const double PortraitHeight = 250;

    public ulong HWnd => WindowFactory.GetWindowForElement(this).HWnd;

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

    public ScrollView ScrollView => ItemsView.ScrollView;

    /// <summary>
    /// 在调用<see cref="ResetEngine"/>前为<see langword="null"/>
    /// </summary>
    [ObservableProperty] private ISortableEntryViewViewModel _viewModel = null!;

    [ObservableProperty] private SimpleWorkType _type;

    private async void WorkItem_OnViewModelChanged(FrameworkElement sender, IWorkViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(ViewModel);
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
                viewModel.CreateWindowWithPage(viewViewModel);
                break;
            case (IllustrationItemViewModel viewModel, IllustrationViewViewModel viewViewModel):
                viewModel.CreateWindowWithPage(viewViewModel);
                break;
        }
    }

    private void NovelItem_OnOpenNovelRequested(NovelItem sender, NovelItemViewModel viewModel)
    {
        if (ViewModel is NovelViewViewModel viewViewModel)
            viewModel.CreateWindowWithPage(viewViewModel);
    }

    private void WorkView_OnSelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        if (sender.SelectedItems is not { Count: > 0 })
        {
            ViewModel.SelectedEntries = ViewModel switch
            {
                NovelViewViewModel => (NovelItemViewModel[])[],
                IllustrationViewViewModel => (IllustrationItemViewModel[])[],
                _ => ViewModel.SelectedEntries
            };
            return;
        }

        ViewModel.SelectedEntries = ViewModel switch
        {
            NovelViewViewModel => sender.SelectedItems.Cast<NovelItemViewModel>().ToArray(),
            IllustrationViewViewModel => sender.SelectedItems.Cast<IllustrationItemViewModel>().ToArray(),
            _ => ViewModel.SelectedEntries
        };
    }

    [MemberNotNull(nameof(ViewModel))]
    public void ResetEngine(IFetchEngine<IWorkEntry> newEngine, int itemsPerPage = 20, int itemLimit = -1)
    {
        var type = newEngine.GetType().GetInterfaces()[0].GenericTypeArguments.FirstOrDefault();
        switch (ViewModel)
        {
            case NovelViewViewModel when type == typeof(Novel):
                ViewModel.ResetEngine(newEngine, itemsPerPage, itemLimit);
                break;
            case IllustrationViewViewModel when type == typeof(Illustration):
                ViewModel.ResetEngine(newEngine, itemsPerPage, itemLimit);
                break;
            default:
                if (type == typeof(Illustration))
                {
                    Type = SimpleWorkType.IllustAndManga;
                    ViewModel?.Dispose();
                    ViewModel = null!;
                    ItemsView.MinItemWidth = DesiredWidth;
                    ItemsView.MinItemHeight = DesiredHeight;
                    ItemsView.LayoutType = LayoutType;
                    ItemsView.ItemTemplate = this.GetResource<DataTemplate>("IllustrationItemDataTemplate");
                    ViewModel = new IllustrationViewViewModel();
                    OnPropertyChanged(nameof(ViewModel));
                    ViewModel.ResetEngine(newEngine, itemsPerPage, itemLimit);
                    ViewModelChanged?.Invoke(this, ViewModel);
                    ItemsView.ItemsSource = ViewModel.View;
                }
                else if (type == typeof(Novel))
                {
                    Type = SimpleWorkType.Novel;
                    ViewModel?.Dispose();
                    ViewModel = null!;
                    ItemsView.MinItemWidth = 350;
                    ItemsView.MinItemHeight = 200;
                    ItemsView.LayoutType = ItemsViewLayoutType.Grid;
                    ItemsView.ItemTemplate = this.GetResource<DataTemplate>("NovelItemDataTemplate");
                    ViewModel = new NovelViewViewModel();
                    OnPropertyChanged(nameof(ViewModel));
                    ViewModel.ResetEngine(newEngine, itemsPerPage, itemLimit);
                    ViewModelChanged?.Invoke(this, ViewModel);
                    ItemsView.ItemsSource = ViewModel.View;
                }
                else
                    ThrowHelper.ArgumentOutOfRange(ViewModel);
                break;
        }
    }

    private TeachingTip WorkItem_OnRequestTeachingTip() => EntryView.QrCodeTeachingTip;

    private (ThumbnailDirection ThumbnailDirection, double DesiredHeight) IllustrationItem_OnRequiredParam() => (ThumbnailDirection, DesiredHeight);

    private void WorkView_OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null!)
            return;
        var viewModel = ViewModel;
        ViewModel = null!;
        foreach (var vm in viewModel.Source)
            vm.UnloadThumbnail(viewModel);
        viewModel.Dispose();
    }

    private void AddToBookmarkTeachingTip_OnCloseButtonClick(TeachingTip sender, object e)
    {
        sender.GetTag<IWorkViewModel>().AddToBookmarkCommand.Execute((BookmarkTagSelector.SelectedTags, BookmarkTagSelector.IsPrivate, null as object));

        HWnd.SuccessGrowl(EntryViewResources.AddedToBookmark);
    }

    private void WorkItem_OnRequestAddToBookmark(FrameworkElement sender, IWorkViewModel e)
    {
        AddToBookmarkTeachingTip.Tag = e;
        AddToBookmarkTeachingTip.IsOpen = true;
    }

    public async void WorkItem_OnRequestOpenUserInfoPage(FrameworkElement sender, IWorkViewModel e)
    {
        await IllustratorViewerHelper.CreateWindowWithPageAsync(e.User.Id);
    }
}
