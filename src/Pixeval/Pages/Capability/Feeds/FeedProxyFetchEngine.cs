#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/FeedProxyFetchEngine.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.Pages.Capability.Feeds;

public class FeedProxyFetchEngine(IFetchEngine<Feed?> feedFetchEngine) : IFetchEngine<IFeedEntry?>
{
    public IAsyncEnumerator<IFeedEntry?> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new FeedProxyFetchEngineEnumerator(feedFetchEngine.GetAsyncEnumerator(cancellationToken));
    }

    public MakoClient MakoClient => feedFetchEngine.MakoClient;

    public EngineHandle EngineHandle => feedFetchEngine.EngineHandle;

    public int RequestedPages
    {
        get => feedFetchEngine.RequestedPages;
        set => feedFetchEngine.RequestedPages = value;
    }

    private class FeedProxyFetchEngineEnumerator(IAsyncEnumerator<Feed?> feedEnumerator) : IAsyncEnumerator<IFeedEntry?>
    {
        private bool _started;
        private string? _currentPostUser;

        public IFeedEntry? Current { get; private set; }

        public ValueTask DisposeAsync()
        {
            return feedEnumerator.DisposeAsync();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_started is false)
            {
                // feedEnumerator is always one ahead.
                await feedEnumerator.MoveNextAsync();
                _currentPostUser = feedEnumerator.Current?.PostUserId;
                _started = true;
            }

            List<Feed?> list = [];
            while (feedEnumerator.Current?.PostUserId == _currentPostUser)
            {
                list.Add(feedEnumerator.Current);
                await feedEnumerator.MoveNextAsync();
            }

            // update
            _currentPostUser = feedEnumerator.Current?.PostUserId;

            if (list.Count is 0)
            {
                return false;
            }

            Current = list.Count == 1
                ? list.Single()?.Let(f => new IFeedEntry.SparseFeedEntry(f))
                : new IFeedEntry.CondensedFeedEntry(list);
            return true;
        }
    }
}
