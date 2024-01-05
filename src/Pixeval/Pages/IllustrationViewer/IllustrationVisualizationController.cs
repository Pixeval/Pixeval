#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationVisualizationController.cs
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
using CommunityToolkit.WinUI.Collections;
using Pixeval.Controls.IllustrationView;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;

namespace Pixeval.Pages.IllustrationViewer;

public class IllustrationVisualizationController(IIllustrationVisualizer visualizer) : IDisposable
{
    public IFetchEngine<Illustration?>? FetchEngine { get; set; }

    public NotifyCollectionChangedEventHandler? CollectionChanged { get; set; }

    public void Dispose()
    {
        visualizer.DisposeCurrent();
        FetchEngine = null;
    }

    public async Task<bool> FillAsync(int itemsLimit = -1)
    {
        var collection = new IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationItemViewModel>, IllustrationItemViewModel>(new IllustrationFetchEngineIncrementalSource(FetchEngine!, itemsLimit));
        visualizer.Illustrations = collection;
        _ = await collection.LoadMoreItemsAsync(20);
        visualizer.Illustrations.CollectionChanged += CollectionChanged;
        return visualizer.Illustrations.Count > 0;
    }

    public async Task FillAsync(IFetchEngine<Illustration?>? newEngine, int itemsLimit = -1)
    {
        FetchEngine = newEngine;
        _ = await FillAsync(itemsLimit);
    }

    public Task<bool> ResetAndFillAsync(IFetchEngine<Illustration?>? newEngine, int itemsLimit = -1)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = newEngine;
        visualizer.DisposeCurrent();
        return FillAsync(itemsLimit);
    }
}
