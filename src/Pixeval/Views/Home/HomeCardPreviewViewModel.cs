// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Engine;
using Mako.Model;
using Pixeval.Collections;
using Pixeval.I18N;
using Pixeval.Models.Home;
using Pixeval.ViewModels;

namespace Pixeval.Views.Home;

public sealed partial class HomeCardPreviewViewModel(HomePageCardLayout card) : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _loadingCts = new();

    public HomeCardPreviewViewModel() : this(new())
    {
    }

    public HomePageCardLayout Card { get; } = card;

    [ObservableProperty]
    public partial IReadOnlyCollection<HomeCardPreviewItem> Items { get; private set; } = [];

    public IFetchEngine<IIdEntry>? Engine { get; private set; }

    [ObservableProperty]
    public partial string? PlaceholderText { get; private set; } = I18NManager.GetResource(HomePageResources.CardPreviewLoadingTextBlockText);

    public async Task LoadAsync()
    {
        try
        {
            PlaceholderText = I18NManager.GetResource(HomePageResources.CardPreviewLoadingTextBlockText);
            await HomePageCardSourceFactory.LoadPreviewItemsAsync(this);
            PlaceholderText = Items.Count is 0 ? I18NManager.GetResource(HomePageResources.CardPreviewEmptyTextBlockText) : null;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            PlaceholderText = I18NManager.GetResource(HomePageResources.CardPreviewFailedTextBlockText);
        }
    }

    public void SetEngine<T>(IFetchEngine<T> engine, Func<T, int, HomeCardPreviewItem> factory)
        where T : IIdEntry
    {
        Debug.Assert(Engine is null);
        Engine = (IFetchEngine<IIdEntry>?) engine;
        Items = new IncrementalLoadingCollection<HomeCardPreviewItem>(
            new IncrementalSource<T, HomeCardPreviewItem>(engine, factory));
    }

    public void SetItems(params IReadOnlyCollection<HomeCardPreviewItem> items)
    {
        Items = items;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _loadingCts.Cancel();
        _loadingCts.Dispose();
        Engine?.EngineHandle.Cancel();
    }
}
