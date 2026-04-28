// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Engine;
using Mako.Global.Enum;
using Misaki;
using Pixeval.Controls;
using Pixeval.Filters;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Work;

/// <summary>
/// 所有插画集合通用的容器
/// </summary>
public partial class WorkContainer : UserControl
{
    public event EventHandler<RoutedEventArgs>? RefreshRequested;

    /// <summary>
    /// The command elements that will appear at the left of the TopCommandBar
    /// </summary>
    public ObservableCollection<Control> CommandBarElements { get; } = [];

    public WorkContainer()
    {
        InitializeComponent();

        CommandBarElements.CollectionChanged += (_, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: { } newItems })
                foreach (Control argsNewItem in newItems)
                    ExtraCommandsBar.Children.Insert(0, argsNewItem);
            else
                throw new ArgumentException("This collection does not support operations except the Add");
        };
    }

    private void SelectAllToggleButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        WorkView.WorkListBox.SelectAll();
    }

    private void SortOptionComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e) => SetSortOption();

    private void WorkView_OnDataContextChanged(object? sender, EventArgs args)
    {
        DataContext = (sender as Control)?.DataContext;
        SetSortOption();
    }

    public void SetSortOption()
    {
        if (DataContext is ISortableEntryViewViewModel vm && SortOptionComboBox.GetSelectedValue<WorkSortOption>() is var sortOption)
        {
            switch (MakoHelper.GetSortDescription(sortOption))
            {
                case { } desc:
                    vm.SetSortDescription(desc);
                    break;
                default:
                    // reset the view so that it can resort its item to the initial order
                    vm.ClearSortDescription();
                    break;
            }
            ScrollToTop();
        }
    }

    private void ScrollToTop()
    {
         if (WorkView.WorkListBox.Scroll is { } scrollView)
             scrollView.Offset = new(0, 0);
    }

    private async void AddAllToBookmarkButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        await ShowBookmarkTagSelectorAsync(AddAllToBookmarkButton, null).ConfigureAwait(false);
    }

    private async void WorkView_OnRequestAddToBookmark(Control sender, IWorkViewModel e)
    {
        await ShowBookmarkTagSelectorAsync(sender, e).ConfigureAwait(false);
    }

    private async Task ShowBookmarkTagSelectorAsync(Control placementTarget, IWorkViewModel? target)
    {
        if (target is null && DataContext is not ISortableEntryViewViewModel { SelectedEntries.Count: > 0 })
            return;

        await BookmarkTagSelectorFlyoutHelper.ShowAsync(
            placementTarget,
            GetTagSelectorWorkType(target),
            async e =>
            {
                await AddToBookmarkAsync(target, e);
            },
            PlacementMode.Bottom);
    }

    private async Task AddToBookmarkAsync(IWorkViewModel? target, (bool isPrivate, IReadOnlyList<string> tags) e)
    {
        if (target is { } current)
        {
            await current.AddToBookmarkCommand.ExecuteAsync((e.tags, e.isPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(I18NManager.GetResource(EntryViewResources.AddedToBookmark));
            return;
        }

        if (DataContext is not ISortableEntryViewViewModel viewModel)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer
            && viewModel.SelectedEntries.Count >= 20
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForBookmarkTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForBookmarkContent)) is not ContentDialogResult.Primary)
            return;

        foreach (var i in viewModel.SelectedEntries)
            await i.AddToBookmarkCommand.ExecuteAsync((e.tags, e.isPrivate, this));
        if (viewModel.SelectedEntries.Count is var c and > 0)
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(I18NManager.GetResource(WorkContainerResources.AddedAllToBookmarkContentFormatted, c));
    }

    private SimpleWorkType GetTagSelectorWorkType(IWorkViewModel? target)
        => target is NovelItemViewModel || DataContext is NovelViewViewModel
            ? SimpleWorkType.Novel
            : SimpleWorkType.IllustrationAndManga;

    private async void SaveAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ISortableEntryViewViewModel viewModel)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer
            && viewModel.SelectedEntries.Count >= 20
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForSaveContent)) is not ContentDialogResult.Primary)
            return;

        foreach (var i in viewModel.SelectedEntries)
            i.SaveCommand.Execute(null);

        TopLevel.GetTopLevel(this)?.ViewContainer?.ShowInformation(
            I18NManager.GetResource(WorkContainerResources.DownloadItemsQueuedFormatted, viewModel.SelectedEntries.Count));
    }

    private async void OpenAllInBrowserButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ISortableEntryViewViewModel viewModel)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer
            && viewModel.SelectedEntries.Count > 15
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItemsForOpenInBrowserContent)) is not ContentDialogResult.Primary)
            return;

        foreach (var selectedEntry in viewModel.SelectedEntries)
        {
            _ = await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(selectedEntry.Entry.WebsiteUri);
        }
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e) => RefreshRequested?.Invoke(sender, e);

    private void CancelSelectionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        WorkView.WorkListBox.UnselectAll();
    }

    private void FilterAutoSuggestBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter || sender is not TextBox textBox)
            return;

        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
            if (DataContext is ISortableEntryViewViewModel vm)
            {
                vm.Filter = null;
                vm.ViewRange = Range.All;
            }
        }
        else
            PerformSearch(textBox.Text);
    }

    public void PerformSearch(string text)
    {
        if (DataContext is not ISortableEntryViewViewModel viewModel)
            return;

        try
        {
            var sequence = Parser.Parse(text, out var index);

            viewModel.Filter = o => o.Filter(sequence);
            viewModel.ViewRange = index?.NarrowRange ?? Range.All;
        }
        catch (Exception e)
        {
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowError(
                I18NManager.GetResource(MacroParserResources.FilterQueryError), e.Message);
        }
    }

    public void ResetEngine(IFetchEngine<IArtworkInfo> newEngine, bool isBookmarkEnabled = true, int itemsPerPage = 20, int itemLimit = -1)
    {
        WorkView.ResetEngine(newEngine, isBookmarkEnabled, itemsPerPage, itemLimit);
    }

    public static readonly FuncValueConverter<int, string> CancelSelectionButtonConverter = new(i => i > 0
        ? I18NManager.GetResource(WorkContainerResources.CancelSelectionButtonFormatted, i)
        : I18NManager.GetResource(WorkContainerResources.CancelSelectionButtonDefaultLabel));
}
