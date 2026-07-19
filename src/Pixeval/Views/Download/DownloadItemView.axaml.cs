// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Model;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Download;

public partial class DownloadItemView : ContentPage, IDisposable
{
    public static readonly DirectProperty<DownloadItemView, int> SelectedCountProperty =
        AvaloniaProperty.RegisterDirect<DownloadItemView, int>(
            nameof(SelectedCount),
            view => view.SelectedCount);

    public static readonly DirectProperty<DownloadItemView, object?> DownloadItemsSourceProperty =
        AvaloniaProperty.RegisterDirect<DownloadItemView, object?>(
            nameof(DownloadItemsSource),
            view => view.DownloadItemsSource,
            (view, value) => view.DownloadItemsSource = value);

    public static readonly DirectProperty<DownloadItemView, bool> HasNoDownloadItemProperty =
        AvaloniaProperty.RegisterDirect<DownloadItemView, bool>(
            nameof(HasNoDownloadItem),
            view => view.HasNoDownloadItem);

    public static readonly DirectProperty<DownloadItemView, bool> DeleteLocalFilesProperty =
        AvaloniaProperty.RegisterDirect<DownloadItemView, bool>(
            nameof(DeleteLocalFiles),
            view => view.DeleteLocalFiles,
            (view, value) => view.DeleteLocalFiles = value);

    private INotifyPropertyChanged? _subscribedItemsSource;

    private bool _isDisposed;

    public DownloadFolderViewModel? Folder => (DataContext as DownloadItemPageViewModel)?.Folder;

    public int SelectedCount
    {
        get;
        private set => SetAndRaise(SelectedCountProperty, ref field, value);
    }

    public object? DownloadItemsSource
    {
        get;
        private set => SetAndRaise(DownloadItemsSourceProperty, ref field, value);
    }

    public bool HasNoDownloadItem
    {
        get;
        private set => SetAndRaise(HasNoDownloadItemProperty, ref field, value);
    } = true;

    public bool DeleteLocalFiles
    {
        get;
        set => SetAndRaise(DeleteLocalFilesProperty, ref field, value);
    }

    public static FuncValueConverter<int, string> SelectionCountLabelConverter { get; } = new(count =>
        count > 0
            ? I18NManager.GetResource(DownloadPageResources.CancelSelectionButtonFormatted, count)
            : I18NManager.GetResource(DownloadPageResources.CancelSelectionButtonDefaultLabel));

    public DownloadItemView() => InitializeComponent();

    public DownloadItemView(DownloadItemPageViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
        if (viewModel.Folder is { } folder)
            Header = folder.Title;
    }

    public void SelectAll() => ListBox.SelectAll();

    public void UnselectAll() => ListBox.UnselectAll();

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_isDisposed)
            return;

        UpdateItemsSourceSubscription();
        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, this));
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_isDisposed)
            return;

        UpdateItemsSourceSubscription();
        if (DataContext is DownloadItemPageViewModel { Folder: { } folder })
            Header = folder.Title;
    }

    private void DownloadItem_OnOpenIllustrationRequested(DownloadItem sender, DownloadItemViewModel viewModel)
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        if (viewModel.Entry is Novel novel)
            viewContainer.CreateNovelPage(novel.Id);
        else
            viewContainer.CreateIllustrationPage(viewModel.Entry);
    }

    private void FilterTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not DownloadItemPageViewModel vm
            || e.Key is not Key.Enter
            || sender is not TextBox { Text: var filterText })
            return;

        vm.FilterText = filterText;
        vm.CurrentOption = string.IsNullOrWhiteSpace(filterText)
            ? DownloadListOption.AllQueued
            : DownloadListOption.CustomSearch;
    }

    private void PauseAllButton_OnClicked(object? sender, RoutedEventArgs e) =>
        ExecuteForSelectedDownloadTasks(item => item.DownloadTask.Pause());

    private void ResumeAllButton_OnClicked(object? sender, RoutedEventArgs e) =>
        ExecuteForSelectedDownloadTasks(item => item.DownloadTask.Resume());

    private void CancelAllButton_OnClicked(object? sender, RoutedEventArgs e) =>
        ExecuteForSelectedDownloadTasks(item => item.DownloadTask.Cancel());

    private void ResetAllButton_OnClicked(object? sender, RoutedEventArgs e) =>
        ExecuteForSelectedDownloadTasks(item => item.DownloadTask.Reset());

    private void DeleteAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (SelectedCount is var count and not 0)
        {
            foreach (var item in GetSelectedEntries().SelectMany(entry => entry.DownloadItems))
            {
                if (DeleteLocalFiles)
                {
                    try
                    {
                        item.DownloadTask.Delete();
                    }
                    catch
                    {
                        // A missing or locked local file does not prevent history removal.
                    }
                }

                _ = App.AppViewModel.HistoryPersistHelper.DownloadManager.TryRemoveTask(item.DownloadTask);
            }

            UnselectAll();
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(DownloadPageResources.DeleteDownloadHistoryRecordsFormatted, count));
        }
    }

    private void SelectAllButton_OnClicked(object? sender, RoutedEventArgs e) => SelectAll();

    private void CancelSelectButton_OnClicked(object? sender, RoutedEventArgs e) => UnselectAll();

    private void UpdateItemsSourceSubscription()
    {
        UnsubscribeFromItemsSource();
        if (DataContext is not DownloadItemPageViewModel vm)
            return;

        SetDownloadItemsSource(vm.View);
    }

    private void ItemsSource_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ICollection.Count))
            UpdateHasNoDownloadItem();
    }

    private void UpdateHasNoDownloadItem() =>
        HasNoDownloadItem = DownloadItemsSource is not ICollection collection || collection.Count is 0;

    private void SetDownloadItemsSource(object source)
    {
        if (ReferenceEquals(DownloadItemsSource, source))
        {
            UpdateHasNoDownloadItem();
            return;
        }

        UnsubscribeFromItemsSource();
        DownloadItemsSource = source;
        _subscribedItemsSource = source as INotifyPropertyChanged;
        _subscribedItemsSource?.PropertyChanged += ItemsSource_OnPropertyChanged;
        UpdateHasNoDownloadItem();
    }

    private void UnsubscribeFromItemsSource()
    {
        if (_subscribedItemsSource is not null)
            _subscribedItemsSource.PropertyChanged -= ItemsSource_OnPropertyChanged;
        _subscribedItemsSource = null;
    }

    private IReadOnlyList<IDownloadListEntryViewModel> GetSelectedEntries() =>
        [.. ListBox.SelectedItems?.OfType<IDownloadListEntryViewModel>() ?? []];

    private void ExecuteForSelectedDownloadTasks(Action<DownloadItemViewModel> action)
    {
        foreach (var item in GetSelectedEntries())
        foreach (var downloadItem in item.DownloadItems)
            action(downloadItem);
    }

    private void ListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        SelectedCount = ListBox.SelectedItems?.OfType<IDownloadListEntryViewModel>().Count() ?? 0;

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        UnsubscribeFromItemsSource();
        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }
}
