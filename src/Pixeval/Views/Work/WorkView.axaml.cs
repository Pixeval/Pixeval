using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Work;

public partial class WorkView : UserControl, IStructuralDisposalCompleter//, IEntryView<ISortableEntryViewViewModel>
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

    public static readonly DirectProperty<WorkView, SimpleWorkType> TypeProperty =
        AvaloniaProperty.RegisterDirect<WorkView, SimpleWorkType>(nameof(Type),
            t => t.Type,
            (t, v) => t.Type = v);

    public SimpleWorkType Type
    {
        get;
        private set => SetAndRaise(TypeProperty, ref field, value);
    }

    private async void WorkItem_OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not { } viewModel
            || sender is not Control { DataContext: IWorkViewModel vm } control
            || sender is not IWorkAnimatable animatable)
            return;
        if (await vm.TryLoadThumbnailAsync(viewModel))
            if (control.IsEffectivelyVisible)
                animatable.StartAnimation();
            else
                control.Opacity = 1;
    }

    private async void WorkItem_OnTapped(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (sender is not ListBoxItem { DataContext: IWorkViewModel vm } lbi)
            return;

        if (ListBox.SelectionMode.HasFlag(SelectionMode.Multiple))
        {
            lbi.IsSelected = !lbi.IsSelected;
            return;
        }

        if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer })
            return;
        switch (vm, DataContext)
        {
            case (NovelItemViewModel { IsBookmarkSupported: false, Entry.Id: var id }, _):
                var novel = await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id);
                // await viewContainer.CreateNovelPageAsync(novel);
                break;
            case (NovelItemViewModel viewModel, NovelViewViewModel viewViewModel):
                // viewContainer.CreateNovelPage(viewModel, viewViewModel);
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

    private void WorkItem_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not ListBoxItem { DataContext: IWorkViewModel vm })
            return;

        if (ListBox.SelectionMode.HasFlag(SelectionMode.Single))
            return;
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
        var type = newEngine.GetType().GetInterfaces()[0].GenericTypeArguments.SingleOrDefault();
        var viewModel = DataContext as ISortableEntryViewViewModel;
        switch (viewModel)
        {
            case NovelViewViewModel when type == typeof(Novel):
            case IllustrationViewViewModel when type != typeof(Novel):
                viewModel.ResetEngine(newEngine, isBookmarkEnabled, itemsPerPage, itemLimit);
                break;
            default:
                if (type == typeof(Novel))
                {
                    Type = SimpleWorkType.Novel;
                    viewModel?.Dispose();
                    ItemWidth = 350;
                    viewModel = new NovelViewViewModel();
                }
                else
                {
                    Type = SimpleWorkType.IllustrationAndManga;
                    viewModel?.Dispose();
                    ItemWidth = LayoutType is ItemsViewLayoutType.Grid ? 240 : double.NaN;
                    viewModel = new IllustrationViewViewModel();
                }

                viewModel.ResetEngine(newEngine, isBookmarkEnabled, itemsPerPage, itemLimit);
                DataContext = viewModel;
                ListBox.ItemsSource = viewModel.View;

                break;
        }
    }

    private void WorkItem_OnRequestAddToBookmark(Control sender, IWorkViewModel e) => RequestAddToBookmark?.Invoke(this, e);

    public async void WorkItem_OnRequestOpenUserInfoPage(Control sender, IWorkViewModel e)
    {
        if (e is { IsBookmarkSupported: false, Entry: WorkBase { User.Id: var id } })
        {
            // await TopLevel.GetTopLevel(this)?.ViewContainer.CreateIllustratorPageAsync(id);
        }
    }

    public void CompleteDisposal()
    {
        var d = DataContext;
        DataContext = null!;
        if (d is not ISortableEntryViewViewModel viewModel)
            return;
        foreach (var vm in viewModel.Source)
            vm.UnloadThumbnail(viewModel);
        viewModel.Dispose();
    }

    public List<Action<IStructuralDisposalCompleter?>> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ((IStructuralDisposalCompleter) this).Hook();
    }

    private void ListBox_OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is not ListBoxItem lbi)
            return;
        lbi.Tapped += WorkItem_OnTapped;
        lbi.DoubleTapped += WorkItem_OnDoubleTapped;
    }
}
