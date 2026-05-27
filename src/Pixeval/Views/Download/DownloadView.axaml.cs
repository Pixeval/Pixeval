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
using Pixeval.Collections;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Download;

public partial class DownloadView : ContentPage
{
    public static readonly DirectProperty<DownloadView, int> SelectedCountProperty =
        AvaloniaProperty.RegisterDirect<DownloadView, int>(
            nameof(SelectedCount),
            o => o.SelectedCount);

    public static readonly DirectProperty<DownloadView, object?> DownloadItemsSourceProperty =
        AvaloniaProperty.RegisterDirect<DownloadView, object?>(
            nameof(DownloadItemsSource),
            o => o.DownloadItemsSource,
            (o, v) => o.DownloadItemsSource = v);

    public static readonly DirectProperty<DownloadView, bool> HasNoDownloadItemProperty =
        AvaloniaProperty.RegisterDirect<DownloadView, bool>(
            nameof(HasNoDownloadItem),
            o => o.HasNoDownloadItem);

    public static readonly DirectProperty<DownloadView, bool> DeleteLocalFilesProperty =
        AvaloniaProperty.RegisterDirect<DownloadView, bool>(
            nameof(DeleteLocalFiles),
            o => o.DeleteLocalFiles,
            (o, v) => o.DeleteLocalFiles = v);

    private readonly DownloadFolderViewModel? _folder;

    private readonly AdvancedObservableCollection<DownloadItemViewModel>? _folderView;

    private DownloadViewViewModel? _subscribedViewModel;

    private INotifyPropertyChanged? _subscribedItemsSource;

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

    public DownloadView()
    {
        InitializeComponent();
    }

    private DownloadView(DownloadFolderViewModel folder)
        : this()
    {
        _folder = folder;
        Header = folder.Title;
        _folderView = new(folder.Items, true);
        SetListSource();
        ApplyFolderFilter();
    }

    public void SelectAll() => ListBox.SelectAll();

    public void UnselectAll() => ListBox.UnselectAll();

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not DownloadViewViewModel vm)
            return;

        UpdateViewModelSubscription();
        SetListSource();
        if (_folder is null)
            RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, vm));
        else
            ApplyFolderFilter();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        UnsubscribeFromViewModel();
        UnsubscribeFromItemsSource();
        _folderView?.Dispose();
        base.OnUnloaded(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        UpdateViewModelSubscription();

        SetListSource();
        if (_folder is not null)
            ApplyFolderFilter();
    }

    private async void DownloadItem_OnOpenIllustrationRequested(DownloadItem sender, DownloadItemViewModel viewModel)
    {
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        if (viewModel.Entry is Novel novel)
            await viewContainer.CreateNovelPageAsync(novel.Id);
        else
            await viewContainer.CreateIllustrationPageAsync(viewModel.Entry);
    }

    private async void DownloadFolder_OnOpenRequested(DownloadFolder sender, DownloadFolderViewModel folder)
    {
        if (DataContext is DownloadViewViewModel
            && IsInNavigationPage
            && Parent is NavigationPage frame)
            await frame.PushAsync(new DownloadView(folder));
    }

    private void FilterTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not DownloadViewViewModel vm
            || e.Key is not Key.Enter
            || sender is not TextBox { Text: var filterText })
            return;

        vm.FilterText = filterText;
        vm.CurrentOption = string.IsNullOrWhiteSpace(filterText)
            ? DownloadListOption.AllQueued
            : DownloadListOption.CustomSearch;
    }

    private void PauseAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ExecuteForSelectedDownloadTasks(t => t.DownloadTask.Pause());
    }

    private void ResumeAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ExecuteForSelectedDownloadTasks(t => t.DownloadTask.Resume());
    }

    private void CancelAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ExecuteForSelectedDownloadTasks(t => t.DownloadTask.Cancel());
    }

    private void ResetAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        ExecuteForSelectedDownloadTasks(t => t.DownloadTask.Reset());
    }

    private void ClearDownloadListButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (SelectedCount is var count and not 0)
        {
            foreach (var item in GetSelectedEntries().SelectMany(t => t.DownloadItems).ToArray())
            {
                if (DeleteLocalFiles)
                {
                    try
                    {
                        item.DownloadTask.Delete();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                App.AppViewModel.HistoryPersistHelper.DownloadManager.RemoveTask(item.DownloadTask);
            }

            UnselectAll();
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(DownloadPageResources.DeleteDownloadHistoryRecordsFormatted, count));
        }
    }

    private void SelectAllButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        SelectAll();
    }

    private void CancelSelectButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        UnselectAll();
    }

    private void UpdateViewModelSubscription()
    {
        if (ReferenceEquals(_subscribedViewModel, DataContext as DownloadViewViewModel))
            return;

        UnsubscribeFromViewModel();
        _subscribedViewModel = DataContext as DownloadViewViewModel;
        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    private void UnsubscribeFromViewModel()
    {
        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        _subscribedViewModel = null;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_folder is not null
            && e.PropertyName is nameof(DownloadViewViewModel.CurrentOption) or nameof(DownloadViewViewModel.FilterText))
        {
            ApplyFolderFilter();
            UpdateHasNoDownloadItem();
        }
    }

    private void ItemsSourceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AdvancedObservableCollection<DownloadItemViewModel>.Count))
            UpdateHasNoDownloadItem();
    }

    private void SetListSource()
    {
        if (_folderView is not null)
        {
            SetDownloadItemsSource(_folderView);
            UpdateHasNoDownloadItem();
            return;
        }

        if (DataContext is not DownloadViewViewModel vm)
            return;

        SetDownloadItemsSource(vm.View);
        UpdateHasNoDownloadItem();
    }

    private void ApplyFolderFilter()
    {
        if (_folderView is null || DataContext is not DownloadViewViewModel vm)
            return;

        using (_folderView.DeferFiltersChange())
        {
            _folderView.Filters.Clear();
            _folderView.Filters.Add(IFilter<DownloadItemViewModel>.Create(vm.MatchesFolderItem, false));
        }

        UpdateHasNoDownloadItem();
    }

    private void UpdateHasNoDownloadItem()
    {
        HasNoDownloadItem = DownloadItemsSource is not ICollection collection || collection.Count is 0;
    }

    private void SetDownloadItemsSource(object source)
    {
        if (ReferenceEquals(DownloadItemsSource, source))
            return;

        UnsubscribeFromItemsSource();
        DownloadItemsSource = source;
        _subscribedItemsSource = source as INotifyPropertyChanged;
        if (_subscribedItemsSource is not null)
            _subscribedItemsSource.PropertyChanged += ItemsSourceOnPropertyChanged;
    }

    private void UnsubscribeFromItemsSource()
    {
        if (_subscribedItemsSource is not null)
            _subscribedItemsSource.PropertyChanged -= ItemsSourceOnPropertyChanged;
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

    private void ListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedCount = ListBox.SelectedItems?.OfType<IDownloadListEntryViewModel>().Count() ?? 0;
    }
}
