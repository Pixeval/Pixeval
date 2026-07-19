// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Download;

public partial class DownloadFolderListView : ContentPage, IDisposable
{
    public static readonly DirectProperty<DownloadFolderListView, bool> HasNoFolderProperty =
        AvaloniaProperty.RegisterDirect<DownloadFolderListView, bool>(
            nameof(HasNoFolder),
            view => view.HasNoFolder);

    private INotifyPropertyChanged? _subscribedItemsSource;

    private bool _isDisposed;

    public bool HasNoFolder
    {
        get;
        private set => SetAndRaise(HasNoFolderProperty, ref field, value);
    } = true;

    public DownloadFolderListView() => InitializeComponent();

    public DownloadFolderListView(DownloadFolderPageViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }

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
    }

    private async void DownloadFolder_OnOpenRequested(DownloadFolder sender, DownloadFolderViewModel folder)
    {
        if (DataContext is not DownloadFolderPageViewModel vm
            || !IsInNavigationPage
            || Parent is not NavigationPage frame)
            return;

        await frame.PushAsync(new DownloadItemView(new DownloadItemPageViewModel(vm.PageViewModel, folder)));
    }

    private void ResumeAll_OnClicked(object? sender, RoutedEventArgs e) =>
        ExecuteForFolder(sender, item => item.DownloadTask.Resume());

    private void PauseAll_OnClicked(object? sender, RoutedEventArgs e) =>
        ExecuteForFolder(sender, item => item.DownloadTask.Pause());

    private void CancelAll_OnClicked(object? sender, RoutedEventArgs e) =>
        ExecuteForFolder(sender, item => item.DownloadTask.Cancel());

    private void ResetAll_OnClicked(object? sender, RoutedEventArgs e) =>
        ExecuteForFolder(sender, item => item.DownloadTask.Reset());

    private static void SyncSubscription_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: DownloadFolderViewModel { Subscription: var subscription } })
            return;

        App.AppViewModel.QueueWorkSubscriptionSync(subscription);
    }

    private static void ExecuteForFolder(object? sender, Action<DownloadItemViewModel> action)
    {
        if (sender is not MenuItem { Tag: DownloadFolderViewModel folder })
            return;

        foreach (var item in folder.DownloadItems)
            action(item);
    }

    private void UpdateItemsSourceSubscription()
    {
        UnsubscribeFromItemsSource();
        if (DataContext is not DownloadFolderPageViewModel vm)
            return;

        _subscribedItemsSource = vm.View;
        _subscribedItemsSource.PropertyChanged += ItemsSource_OnPropertyChanged;
        UpdateHasNoFolder();
    }

    private void ItemsSource_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ICollection.Count))
            UpdateHasNoFolder();
    }

    private void UpdateHasNoFolder() =>
        HasNoFolder = DataContext is DownloadFolderPageViewModel { View: { Count: 0 } };

    private void UnsubscribeFromItemsSource()
    {
        _subscribedItemsSource?.PropertyChanged -= ItemsSource_OnPropertyChanged;
        _subscribedItemsSource = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        UnsubscribeFromItemsSource();
    }
}
