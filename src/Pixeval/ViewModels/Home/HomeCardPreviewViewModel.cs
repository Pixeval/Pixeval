// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Collections;
using Pixeval.I18N;
using Pixeval.Models.Home;

namespace Pixeval.ViewModels.Home;

public sealed partial class HomeCardPreviewViewModel(HomePageCardLayout card) : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _loadingCts = new();
    private INotifyCollectionChanged? _itemsCollectionChanged;
    private Task? _loadingTask;
    private bool _isDisposed;

    public HomeCardPreviewViewModel() : this(new())
    {
    }

    public HomePageCardLayout Card { get; } = card;

    [ObservableProperty]
    public partial ISimpleViewViewModel? ViewModel { get; private set; }

    public IReadOnlyCollection<INotifyPropertyChanged> Items => ViewModel?.View ?? [];

    [ObservableProperty]
    public partial string? PlaceholderText { get; private set; } = I18NManager.GetResource(HomePageResources.CardPreviewLoadingTextBlockText);

    public async Task LoadAsync()
    {
        if (_isDisposed)
            return;

        if (_loadingTask is { } loadingTask)
        {
            await loadingTask;
            return;
        }

        _loadingTask = LoadCoreAsync();
        await _loadingTask;
    }

    public Task EnsureLoadedAsync() => ViewModel is null ? LoadAsync() : Task.CompletedTask;

    private async Task LoadCoreAsync()
    {
        try
        {
            PlaceholderText = I18NManager.GetResource(HomePageResources.CardPreviewLoadingTextBlockText);
            var oldViewModel = ViewModel;
            var viewModel = await Card.Definition.CreatePreviewViewModelAsync(Card);
            if (_isDisposed)
            {
                if (viewModel is IDisposable newDisposable)
                    newDisposable.Dispose();
                return;
            }

            DetachItems();
            ViewModel = viewModel;
            OnPropertyChanged(nameof(Items));
            AttachItems();
            if (!ReferenceEquals(oldViewModel, viewModel) && oldViewModel is IDisposable disposable)
                disposable.Dispose();

            await LoadInitialItemsAsync();
            UpdatePlaceholder();
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception)
        {
            if (!_isDisposed)
                PlaceholderText = I18NManager.GetResource(HomePageResources.CardPreviewFailedTextBlockText);
        }
    }

    private async Task LoadInitialItemsAsync()
    {
        if (!_isDisposed && Items is IIncrementalLoading { HasMoreItems: true } incremental)
            await incremental.LoadMoreItemsAsync(0, _loadingCts.Token);
    }

    private void AttachItems()
    {
        if (Items is not INotifyCollectionChanged items)
            return;

        _itemsCollectionChanged = items;
        _itemsCollectionChanged.CollectionChanged += Items_OnCollectionChanged;
    }

    private void DetachItems()
    {
        _itemsCollectionChanged?.CollectionChanged -= Items_OnCollectionChanged;
        _itemsCollectionChanged = null;
    }

    private void Items_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Items));
        UpdatePlaceholder();
    }

    private void UpdatePlaceholder()
    {
        PlaceholderText = Items.Count is 0
            ? I18NManager.GetResource(HomePageResources.CardPreviewEmptyTextBlockText)
            : null;
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _loadingCts.Cancel();
        _loadingCts.Dispose();
        DetachItems();
        var viewModel = ViewModel;
        ViewModel = null;
        OnPropertyChanged(nameof(Items));
        if (viewModel is IDisposable disposable)
            disposable.Dispose();
    }
}
