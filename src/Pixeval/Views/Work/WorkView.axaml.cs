using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Mako.Engine;
using Mako.Model;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Work;

public partial class WorkView : UserControl, IDisposable
{
    public event EventHandler<WorkView, IWorkViewModel>? RequestAddToBookmark;

    public static readonly DirectProperty<WorkView, double> ItemWidthProperty =
        AvaloniaProperty.RegisterDirect<WorkView, double>(nameof(ItemWidth), t => t.ItemWidth, (t, v) => t.ItemWidth = v);

    public double ItemWidth
    {
        get;
        set => SetAndRaise(ItemWidthProperty, ref field, value);
    }

    public static FuncValueConverter<bool, SelectionMode> SelectionModeConverter { get; } =
        new(b => b ? SelectionMode.Multiple : SelectionMode.Single);

    public ItemsViewLayoutType LayoutType { get; set; }

    public WorkView() => InitializeComponent();

    private async void WorkItem_OnTapped(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (sender is not ListBoxItem { DataContext: IWorkViewModel vm } lbi)
            return;

        if (ListBox.SelectionMode.HasFlag(SelectionMode.Multiple))
        {
            lbi.IsSelected = !lbi.IsSelected;
            return;
        }

        await CreateWorkViewerPage(vm);
    }

    private async void WorkItem_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not ListBoxItem { DataContext: IWorkViewModel vm })
            return;

        if (ListBox.SelectionMode.HasFlag(SelectionMode.Single))
            return;

        await CreateWorkViewerPage(vm);
    }

    private async Task CreateWorkViewerPage(IWorkViewModel vm)
    {
        if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer })
            return;

        switch (vm, DataContext)
        {
            case (NovelItemViewModel { IsBookmarkSupported: false, Entry.Id: var id }, _):
                await viewContainer.CreateNovelPageAsync(id);
                break;
            case (NovelItemViewModel viewModel, NovelViewViewModel viewViewModel):
                viewContainer.CreateNovelPage(viewModel, viewViewModel);
                break;
            case (IllustrationItemViewModel { IsBookmarkSupported: false, Entry: Illustration { Id: var id } }, _):
                var illustration = await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id);
                await viewContainer.CreateIllustrationPageAsync(illustration);
                break;
            case (IllustrationItemViewModel viewModel, IllustrationViewViewModel viewViewModel):
                viewContainer.CreateIllustrationPage(viewModel, viewViewModel);
                break;
        }
    }

    private void WorkView_OnSelectionChanged(object? o, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        if (o is not ListBox listBox || DataContext is not ISortableEntryViewViewModel viewModel)
            return;
        if (listBox.SelectedItems is not { Count: > 0 })
        {
            viewModel.SelectedEntries = viewModel switch
            {
                NovelViewViewModel => (NovelItemViewModel[]) [],
                IllustrationViewViewModel => (IllustrationItemViewModel[]) [],
                _ => viewModel.SelectedEntries
            };
            return;
        }

        viewModel.SelectedEntries = viewModel switch
        {
            NovelViewViewModel => [.. listBox.SelectedItems.Cast<NovelItemViewModel>()],
            IllustrationViewViewModel => [.. listBox.SelectedItems.Cast<IllustrationItemViewModel>()],
            _ => viewModel.SelectedEntries
        };
    }

    /// <summary>
    /// 在调用<see cref="ResetEngine"/>前<see cref="StyledElement.DataContext"/>为<see langword="null"/>
    /// </summary>
    public void ResetEngine(IFetchEngine<IArtworkInfo> newEngine, bool isBookmarkEnabled = true, int itemsPerPage = 20, int itemLimit = -1)
    {
        var isNovelEngine = newEngine is IFetchEngine<Novel>;
        var viewModel = DataContext as ISortableEntryViewViewModel;
        switch (viewModel)
        {
            case NovelViewViewModel when isNovelEngine:
            case IllustrationViewViewModel when !isNovelEngine:
                viewModel.ResetEngine(newEngine, isBookmarkEnabled, itemsPerPage, itemLimit);
                break;
            default:
                ISortableEntryViewViewModel? newViewModel;
                if (isNovelEngine)
                {
                    ItemWidth = 350;
                    newViewModel = new NovelViewViewModel();
                }
                else
                {
                    ItemWidth = LayoutType is ItemsViewLayoutType.Grid ? 240 : double.NaN;
                    newViewModel = new IllustrationViewViewModel();
                }
                newViewModel.ResetEngine(newEngine, isBookmarkEnabled, itemsPerPage, itemLimit);
                DataContext = newViewModel;
                ListBox.ItemsSource = newViewModel.View;
                viewModel?.Dispose();
                break;
        }
    }

    private void WorkItem_OnRequestAddToBookmark(Control sender, IWorkViewModel e) => RequestAddToBookmark?.Invoke(this, e);

    public async void WorkItem_OnRequestOpenUserInfoPage(Control sender, IWorkViewModel e)
    {
        if (e is { IsBookmarkSupported: false, Entry: WorkBase { User.Id: var id } })
        {
            if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
                await viewContainer.CreateUserPageAsync(id);
        }
    }

    private void ListBox_OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is not ListBoxItem lbi)
            return;
        lbi.Tapped += WorkItem_OnTapped;
        lbi.DoubleTapped += WorkItem_OnDoubleTapped;
    }

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, this));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        var d = DataContext;
        DataContext = null!;
        if (d is ISortableEntryViewViewModel viewModel)
            viewModel.Dispose();
    }

    ~WorkView() => Dispatcher.UIThread.InvokeAsync(Dispose);

    #endregion
}
