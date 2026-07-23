// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Work;

public sealed partial class WorkView : UserControl, IDisposable
{
    private bool _isDisposed;

    public event EventHandler<Control, IWorkViewModel>? RequestAddToBookmark;

    public ThumbnailLayoutType LayoutType
    {
        get;
        set
        {
            field = value;
            UpdateLayoutPseudoClasses();
        }
    }

    public static FuncValueConverter<bool, SelectionMode> SelectionModeConverter { get; } =
        new(b => b ? SelectionMode.Multiple : SelectionMode.Single);

    private void StyledElement_OnDataContextChanged(object? sender, EventArgs e) => UpdateLayoutPseudoClasses();

    public WorkView() => InitializeComponent();

    private void UpdateLayoutPseudoClasses()
    {
        var actualLayoutType = DataContext is IOperableViewViewModel { RequireAdaptiveGrid: true } ? ThumbnailLayoutType.Grid : LayoutType;
        PseudoClasses.Set(":linedFlow", actualLayoutType is ThumbnailLayoutType.LinedFlow);
        PseudoClasses.Set(":verticalStack", actualLayoutType is ThumbnailLayoutType.VerticalStack);
        PseudoClasses.Set(":grid", actualLayoutType is ThumbnailLayoutType.Grid);
        PseudoClasses.Set(":masonry", actualLayoutType is ThumbnailLayoutType.Masonry);
    }

    private async void WorkItem_OnTapped(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (sender is not ListBoxItem { DataContext: IWorkViewModel vm } lbi)
            return;

        if (WorkListBox.SelectionMode.HasFlag(SelectionMode.Multiple))
        {
            UpdateSelection(lbi, tappedEventArgs);
            return;
        }

        await CreateWorkViewerPage(vm);
    }

    private void UpdateSelection(ListBoxItem item, TappedEventArgs e)
    {
        var index = WorkListBox.IndexFromContainer(item);
        var anchorIndex = WorkListBox.Selection.AnchorIndex;

        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift) || index < 0 || anchorIndex < 0)
        {
            item.IsSelected = !item.IsSelected;
            return;
        }

        using var operation = WorkListBox.Selection.BatchUpdate();
        WorkListBox.Selection.Clear();
        WorkListBox.Selection.SelectRange(anchorIndex, index);
    }

    private async void WorkItem_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not ListBoxItem { DataContext: IWorkViewModel vm })
            return;

        if (WorkListBox.SelectionMode.HasFlag(SelectionMode.Single))
            return;

        await CreateWorkViewerPage(vm);
    }

    private async Task CreateWorkViewerPage(IWorkViewModel vm)
    {
        if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer })
            return;

        switch (vm, DataContext)
        {
            case (NovelItemViewModel viewModel, NovelViewViewModel viewViewModel):
                viewContainer.CreateNovelPage(viewModel, viewViewModel.DataProvider.CloneRef());
                break;
            case (NovelItemViewModel viewModel, SimpleOperableViewViewModel<NovelItemViewModel> viewViewModel):
                viewContainer.CreateNovelPage(viewModel, viewViewModel.SourceView.CloneSourceView(), viewViewModel.NeedRefreshOnOpen);
                break;
            case (IllustrationItemViewModel viewModel, IllustrationViewViewModel viewViewModel):
                viewContainer.CreateIllustrationPage(viewModel, viewViewModel.DataProvider.CloneRef());
                break;
            case (IllustrationItemViewModel viewModel, SimpleOperableViewViewModel<IllustrationItemViewModel> viewViewModel):
                viewContainer.CreateIllustrationPage(viewModel, viewViewModel.SourceView.CloneSourceView(), viewViewModel.NeedRefreshOnOpen);
                break;
            case (NovelItemViewModel { Entry.Id: var id }, _):
                viewContainer.CreateNovelPage(id);
                break;
            case (IllustrationItemViewModel { Entry: Illustration illustration }, _):
                viewContainer.CreateIllustrationPage(illustration);
                break;
        }
    }

    /// <summary>
    /// 在调用<see cref="ResetEngine"/>前<see cref="Control.DataContext"/>为<see langword="null"/>
    /// </summary>
    public void ResetEngine(IAsyncEnumerable<IArtworkInfo> newEngine)
    {
        var isNovelEngine = newEngine is IAsyncEnumerable<Novel>;
        var viewModel = DataContext as IWorkViewViewModel;
        switch (viewModel)
        {
            case NovelViewViewModel when isNovelEngine:
            case IllustrationViewViewModel when !isNovelEngine:
                viewModel.ResetEngine(newEngine);
                break;
            default:
                IWorkViewViewModel newViewModel = isNovelEngine ? new NovelViewViewModel() : new IllustrationViewViewModel();
                newViewModel.ResetEngine(newEngine);
                SetOwnedViewModel(newViewModel);
                break;
        }
    }

    public void SetSource(IReadOnlyCollection<IArtworkInfo> source, SimpleWorkType workType, bool needRefreshOnOpen = false)
    {
        IOperableViewViewModel viewModel = workType is SimpleWorkType.Novel
            ? new SimpleOperableViewViewModel<NovelItemViewModel>(source, needRefreshOnOpen)
            : new SimpleOperableViewViewModel<IllustrationItemViewModel>(source, needRefreshOnOpen);
        SetOwnedViewModel(viewModel);
    }

    /// <summary>
    /// Takes ownership of <paramref name="viewModel"/> and disposes it when it is replaced or this view is disposed.
    /// </summary>
    public void SetViewModel(IWorkViewViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        SetOwnedViewModel(viewModel);
    }

    private void SetOwnedViewModel(ISimpleViewViewModel viewModel)
    {
        var oldViewModel = DataContext as IDisposable;
        DataContext = viewModel;
        oldViewModel?.Dispose();
    }

    private void WorkItem_OnRequestAddToBookmark(Control sender, IWorkViewModel e) => RequestAddToBookmark?.Invoke(sender, e);

    public void WorkItem_OnRequestOpenUserInfoPage(Control sender, IWorkViewModel e)
    {
        if (e is { Entry: WorkBase { User.Id: var id } })
        {
            if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
                viewContainer.CreateUserPage(id);
        }
    }

    private void ListBox_OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is not ListBoxItem lbi)
            return;
        lbi.Tapped += WorkItem_OnTapped;
        lbi.DoubleTapped += WorkItem_OnDoubleTapped;
    }

    private void ListBox_OnContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        if (e.Container is not ListBoxItem lbi)
            return;

        lbi.Tapped -= WorkItem_OnTapped;
        lbi.DoubleTapped -= WorkItem_OnDoubleTapped;
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
        if (_isDisposed)
            return;

        _isDisposed = true;
        WorkListBox.ItemsSource = null;
        RequestAddToBookmark = null;
        var d = DataContext;
        DataContext = null!;
        if (d is IDisposable viewModel)
            viewModel.Dispose();
    }

    #endregion
}
