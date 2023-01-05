#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/JustifiedLayoutIllustrationViewDataProvider.cs
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Data;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;

namespace Pixeval.UserControls.IllustrationView;

public class JustifiedLayoutIllustrationViewDataProvider : GridIllustrationViewDataProvider
{
    private EventHandler<NotifyCollectionChangedEventArgs>? _onIllustrationsSourceCollectionChanged;

    public event EventHandler<NotifyCollectionChangedEventArgs> OnIllustrationsSourceCollectionChanged
    {
        add => _onIllustrationsSourceCollectionChanged += value;
        remove => _onIllustrationsSourceCollectionChanged -= value;
    }

    private EventHandler<IEnumerable<IllustrationViewModel>>? _onDeferLoadingCompleted;

    public event EventHandler<IEnumerable<IllustrationViewModel>> OnDeferLoadingCompleted
    {
        add => _onDeferLoadingCompleted += value;
        remove => _onDeferLoadingCompleted -= value;
    }

    public override async Task<int> FillAsync(int? itemsLimit = null)
    {
        var collection = new IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, IllustrationViewModel>, IllustrationViewModel>(new IllustrationFetchEngineIncrementalSource(FetchEngine!, itemsLimit));
        IllustrationsSource = collection;
        IllustrationsSource.CollectionChanged += OnIllustrationsSourceOnCollectionChanged;
        var result = await collection.LoadMoreItemsAsync(20);
        if (result.Count > 0)
        {
            _onDeferLoadingCompleted?.Invoke(IllustrationsView, IllustrationsView.TakeLast((int) result.Count).OfType<IllustrationViewModel>());
        }
        return (int) result.Count;
    }

    public override async Task<int> LoadMore()
    {
        if (IllustrationsSource is ISupportIncrementalLoading coll)
        {
            var count = (int) (await coll.LoadMoreItemsAsync(20)).Count;
            if (count > 0)
            {
                _onDeferLoadingCompleted?.Invoke(IllustrationsView, IllustrationsView.TakeLast(count).OfType<IllustrationViewModel>());
            }

            return count;
        }

        return 0;
    }

    protected override void OnIllustrationsSourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        base.OnIllustrationsSourceOnCollectionChanged(sender, args);
        _onIllustrationsSourceCollectionChanged?.Invoke(sender, args);
    }
}