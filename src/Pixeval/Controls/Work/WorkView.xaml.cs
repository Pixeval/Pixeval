using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Pages.NovelViewer;
using Pixeval.Pages.IllustrationViewer;
using WinUI3Utilities;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Engine;
using Pixeval.Options;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<ThumbnailDirection>("ThumbnailDirection", DependencyPropertyDefaultValue.Default)]
[ObservableObject]
public sealed partial class WorkView : IEntryView<ISortableEntryViewViewModel>
{
    public const double LandscapeHeight = 180;
    public const double PortraitHeight = 250;

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

    public WorkView() => InitializeComponent();

    /// <summary>
    /// 在调用<see cref="ResetEngine"/>前为<see langword="null"/>
    /// </summary>
    public ISortableEntryViewViewModel ViewModel { get; private set; } = null!;

    public AdvancedItemsView AdvancedItemsView => ItemsView;

    public ScrollView ScrollView => ItemsView.ScrollView;

    private async void WorkItem_OnViewModelChanged(FrameworkElement sender, IWorkViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(ViewModel);
        _ = await viewModel.TryLoadThumbnailAsync(ViewModel);
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

    [MemberNotNull(nameof(ViewModel))]
    public void ResetEngine(IFetchEngine<IWorkEntry> newEngine, int itemLimit = -1)
    {
        var type = newEngine.GetType().GetInterfaces()[0].GenericTypeArguments.FirstOrDefault();
        switch (ViewModel)
        {
            case NovelViewViewModel when type == typeof(Novel):
                ViewModel.ResetEngine(newEngine, itemLimit);
                break;
            case IllustrationViewViewModel when type == typeof(Illustration):
                ViewModel.ResetEngine(newEngine, itemLimit);
                break;
            default:
                if (type == typeof(Illustration))
                {
                    ViewModel?.Dispose();
                    ViewModel = null!;
                    ItemsView.MinItemWidth = DesiredWidth;
                    ItemsView.MinItemHeight = DesiredHeight;
                    ItemsView.LayoutType = LayoutType;
                    ItemsView.ItemTemplate = this.GetResource<DataTemplate>("IllustrationItemDataTemplate");
                    ViewModel = new IllustrationViewViewModel();
                    OnPropertyChanged(nameof(ViewModel));
                    ViewModel.ResetEngine(newEngine, itemLimit);
                    ItemsView.ItemsSource = ViewModel.View;
                }
                else if (type == typeof(Novel))
                {
                    ViewModel?.Dispose();
                    ViewModel = null!;
                    ItemsView.MinItemWidth = 200;
                    ItemsView.MinItemHeight = 0;
                    ItemsView.LayoutType = ItemsViewLayoutType.Staggered;
                    ItemsView.ItemTemplate = this.GetResource<DataTemplate>("NovelItemDataTemplate");
                    ViewModel = new NovelViewViewModel();
                    OnPropertyChanged(nameof(ViewModel));
                    ViewModel.ResetEngine(newEngine, itemLimit);
                    ItemsView.ItemsSource = ViewModel.View;
                }
                else
                    ThrowHelper.ArgumentOutOfRange(ViewModel);
                break;
        }
    }

    private TeachingTip WorkItem_OnRequestTeachingTip() => EntryView.QrCodeTeachingTip;

    private (ThumbnailDirection ThumbnailDirection, double DesiredHeight) IllustrationItem_OnRequiredParam() => (ThumbnailDirection, DesiredHeight);
}
