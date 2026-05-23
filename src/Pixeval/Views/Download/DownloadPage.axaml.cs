// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Download;

public partial class DownloadPage : ContentPage
{
    private DownloadViewViewModel? ViewModel => DataContext as DownloadViewViewModel;

    public static FuncValueConverter<int, string> SelectionCountLabelConverter { get; } = new(count =>
        count > 0
            ? I18NManager.GetResource(DownloadPageResources.CancelSelectionButtonFormatted, count)
            : I18NManager.GetResource(DownloadPageResources.CancelSelectionButtonDefaultLabel));

    public static IReadOnlyList<DownloadOptionItem> Options { get; } =
    [
        new(DownloadListOption.AllQueued, I18NManager.GetResource(DownloadPageResources.DownloadListOptionAllQueued)),
        new(DownloadListOption.Running, I18NManager.GetResource(DownloadPageResources.DownloadListOptionRunning)),
        new(DownloadListOption.Completed, I18NManager.GetResource(DownloadPageResources.DownloadListOptionCompleted)),
        new(DownloadListOption.Cancelled, I18NManager.GetResource(DownloadPageResources.DownloadListOptionCancelled)),
        new(DownloadListOption.Error, I18NManager.GetResource(DownloadPageResources.DownloadListOptionError)),
        new(DownloadListOption.CustomSearch, I18NManager.GetResource(DownloadPageResources.DownloadListOptionCustomSearch))
    ];

    public DownloadPage()
    {
        DataContext = new DownloadViewViewModel(App.AppViewModel.HistoryPersistHelper.DownloadManager.QueuedTasks);
        InitializeComponent();
        ViewModel?.ResetFilter();
    }

    private void ModeFilterComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ViewModel is not { } vm || sender is not ComboBox { SelectedValue: DownloadListOption item })
            return;

        vm.CurrentOption = item;
        vm.ResetFilter();
    }

    private void PauseAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.PauseSelectedItems();
    }

    private void ResumeAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.ResumeSelectedItems();
    }

    private void CancelAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.CancelSelectedItems();
    }

    private void ResetAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.ResetSelectedItems();
    }

    private void ClearDownloadListButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not DownloadViewViewModel { SelectedEntries.Count: var count and not 0 } vm)
            return;

        vm.RemoveSelectedItems(DeleteLocalFilesCheckBox.IsChecked == true);
        DownloadView.ListBox.UnselectAll();

        TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
            I18NManager.GetResource(DownloadPageResources.DeleteDownloadHistoryRecordsFormatted, count));
    }

    private void FilterTextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel?.FilterTask(FilterTextBox.Text);
    }

    private void FilterTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel is not { } vm)
            return;
        if (e.Key is not Key.Enter)
            return;

        if (string.IsNullOrWhiteSpace(FilterTextBox.Text))
        {
            ModeFilterComboBox.SelectedIndex = 0;
            vm.CurrentOption = DownloadListOption.AllQueued;
            vm.ResetFilter();
            return;
        }

        ModeFilterComboBox.SelectedItem = Options.First(t => t.Value is DownloadListOption.CustomSearch);
        vm.CurrentOption = DownloadListOption.CustomSearch;
        vm.ResetFilter(vm.FilteredTasks);
    }

    private void SelectAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        DownloadView.ListBox.SelectAll();
    }

    private void CancelSelectButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        DownloadView.ListBox.UnselectAll();
    }

    public sealed record DownloadOptionItem(DownloadListOption Value, string Text);
}
