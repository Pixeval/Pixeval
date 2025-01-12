// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
    public IAsyncEnumerator<IFeedEntry?> GetAsyncEnumerator(CancellationToken cancellationToken = default)
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

        public ValueTask DisposeAsync() => feedEnumerator.DisposeAsync();

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
                return false;

            Current = list.Count == 1
                ? list.Single()?.Let(f => new IFeedEntry.SparseFeedEntry(f))
                : new IFeedEntry.CondensedFeedEntry(list);
            return true;
        }
    }
}
