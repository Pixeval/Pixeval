﻿#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustrationVisualizationController.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;
using Pixeval.UserControls;
using Pixeval.Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public class IllustrationVisualizationController : IDisposable
{
    private readonly IIllustrationVisualizer _visualizer;

    public IFetchEngine<Illustration?>? FetchEngine { get; set; }

    public NotifyCollectionChangedEventHandler? CollectionChanged { get; set; }

    public IllustrationVisualizationController(IIllustrationVisualizer visualizer)
    {
        _visualizer = visualizer;
    }

    public async Task<bool> FillAsync(int? itemsLimit = null)
    {
        var collection = new IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationViewModel>, IllustrationViewModel>(new IllustrationFetchEngineIncrementalSource(FetchEngine!, itemsLimit));
        _visualizer.IncrementalLoadingCollection = collection;
        await collection.LoadMoreItemsAsync((uint)(itemsLimit ?? 20));
        _visualizer.Illustrations.CollectionChanged += CollectionChanged;
        _visualizer.Illustrations.AddRange(collection);
        return _visualizer.Illustrations.Count > 0;
    }

    public async Task FillAsync(IFetchEngine<Illustration?>? newEngine, int? itemsLimit = null)
    {
        FetchEngine = newEngine;
        await FillAsync(itemsLimit);
    }

    public Task<bool> ResetAndFillAsync(IFetchEngine<Illustration?>? newEngine, int? itemLimit = null)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = newEngine;
        _visualizer.DisposeCurrent();
        return FillAsync(itemLimit);
    }

    public void Dispose()
    {
        _visualizer.DisposeCurrent();
        FetchEngine = null;
    }
}