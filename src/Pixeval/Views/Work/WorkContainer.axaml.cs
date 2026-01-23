// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
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
                    ExtraCommandsBar.Items.Insert(0, argsNewItem);
            else
                throw new ArgumentException("This collection does not support operations except the Add");
        };
    }

    public SimpleWorkType Type => WorkView.Type;

    private void SelectAllToggleButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        WorkView.ListBox.SelectAll();
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
         if (WorkView.ListBox.Scroll is { } scrollView)
             scrollView.Offset = new(0, 0);
    }

    private IWorkViewModel? _currentBookmarkItem;

    private void AddAllToBookmarkButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        _currentBookmarkItem = null;
        OpenBookmarkTagSelector();
    }

    private void WorkView_OnRequestAddToBookmark(WorkView sender, IWorkViewModel e)
    {
        _currentBookmarkItem = e;
        OpenBookmarkTagSelector();
    }

    private async void OpenBookmarkTagSelector()
    {
        TagSelector.IsVisible = false;
        await TagSelector.ResetSourceAsync();
        TagSelector.IsVisible = true;
    }

    private async void SaveAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ISortableEntryViewViewModel viewModel)
            return;

        // TODO: CreateOkCancelAsync dialog equivalent in Avalonia
        // if (viewModel.SelectedEntries.Count >= 20 && await this.CreateOkCancelAsync(...) is not ...)
        //     return;

        foreach (var i in viewModel.SelectedEntries)
            i.SaveCommand.Execute(null);

        TopLevel.GetTopLevel(this)?.ViewContainer?.ShowInformation(
            I18NManager.GetResource(WorkContainerResources.DownloadItemsQueuedFormatted, viewModel.SelectedEntries.Count));
    }

    private async void OpenAllInBrowserButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ISortableEntryViewViewModel viewModel)
            return;

        // TODO: CreateOkCancelAsync dialog equivalent in Avalonia
        // if (viewModel.SelectedEntries.Count > 15 && await this.CreateOkCancelAsync(...) is not ...)
        //     return;

        foreach (var selectedEntry in viewModel.SelectedEntries)
        {
            _ = await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(selectedEntry.Entry.WebsiteUri);
        }
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e) => RefreshRequested?.Invoke(sender, e);

    private void TagSelector_OnTagsSelected(TagSelector sender, (bool IsPrivate, IReadOnlyList<string> Tags) e)
    {
        if (_currentBookmarkItem is { } current)
        {
            current.AddToBookmarkCommand.Execute((e.Tags, e.IsPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(I18NManager.GetResource(EntryViewResources.AddedToBookmark));
            return;
        }

        if (DataContext is not ISortableEntryViewViewModel viewModel)
            return;

        foreach (var i in viewModel.SelectedEntries)
            i.AddToBookmarkCommand.Execute((e.Tags, e.IsPrivate, this));
        if (viewModel.SelectedEntries.Count is var c and > 0)
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(I18NManager.GetResource(WorkContainerResources.AddedAllToBookmarkContentFormatted, c));
    }

    private void CancelSelectionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        WorkView.ListBox.UnselectAll();
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

    public void ResetEngine(IFetchEngine<IArtworkInfo> newEngine, int itemsPerPage = 20, int itemLimit = -1)
    {
        TagSelector.IsVisible = false;
        WorkView.ResetEngine(newEngine, itemsPerPage, itemLimit);
    }

    public static readonly FuncValueConverter<int, string> CancelSelectionButtonConverter = new(i => i > 0
        ? I18NManager.GetResource(WorkContainerResources.CancelSelectionButtonFormatted, i)
        : I18NManager.GetResource(WorkContainerResources.CancelSelectionButtonDefaultLabel));
}
