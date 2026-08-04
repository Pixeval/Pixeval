// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Mako.Engine;
using Mako.Model;
using Pixeval.Models.Database;

namespace Pixeval.Models.Subscriptions;

internal abstract record WorkSubscriptionSyncRequest
{
    public sealed record All : WorkSubscriptionSyncRequest
    {
        public static All Instance { get; } = new();

        private All()
        {
        }
    }

    public sealed record Subscription(
        WorkSubscriptionEntry Entry,
        IFetchEngine<IWorkEntry>? SourceEngine = null,
        bool UsesSharedSourceEngine = false,
        bool RefreshMetadata = false) : WorkSubscriptionSyncRequest;
}

internal sealed class WorkSubscriptionSyncRequestQueue
{
    private readonly Queue<WorkSubscriptionSyncRequest> _requests = [];

    private readonly HashSet<int> _subscriptionIds = [];

    private bool _hasAllRequest;

    public int Count => _requests.Count;

    public bool TryEnqueue(
        WorkSubscriptionSyncRequest request,
        WorkSubscriptionSyncRequest? activeRequest)
    {
        switch (request)
        {
            case WorkSubscriptionSyncRequest.All:
                if (activeRequest is WorkSubscriptionSyncRequest.All || _hasAllRequest)
                    return false;

                // A global sync that has not started yet will see every currently persisted subscription.
                Clear();
                _hasAllRequest = true;
                break;
            case WorkSubscriptionSyncRequest.Subscription { Entry.HistoryEntryId: var subscriptionId }:
                // A running global sync may already have passed a subscription inserted while it was fetching.
                if (_hasAllRequest
                    || (activeRequest is WorkSubscriptionSyncRequest.Subscription
                    {
                        Entry.HistoryEntryId: var activeSubscriptionId
                    } && activeSubscriptionId == subscriptionId)
                    || !_subscriptionIds.Add(subscriptionId))
                    return false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request));
        }

        _requests.Enqueue(request);
        return true;
    }

    public bool TryDequeue(out WorkSubscriptionSyncRequest request)
    {
        if (!_requests.TryDequeue(out var nextRequest))
        {
            request = null!;
            return false;
        }

        request = nextRequest;

        switch (request)
        {
            case WorkSubscriptionSyncRequest.All:
                _hasAllRequest = false;
                break;
            case WorkSubscriptionSyncRequest.Subscription { Entry.HistoryEntryId: var subscriptionId }:
                _ = _subscriptionIds.Remove(subscriptionId);
                break;
        }

        return true;
    }

    public void RemoveSubscription(int subscriptionId)
    {
        if (!_subscriptionIds.Remove(subscriptionId))
            return;

        var count = _requests.Count;
        for (var i = 0; i < count; i++)
        {
            var request = _requests.Dequeue();
            if (request is not WorkSubscriptionSyncRequest.Subscription
                {
                    Entry.HistoryEntryId: var requestSubscriptionId
                } || requestSubscriptionId != subscriptionId)
                _requests.Enqueue(request);
        }
    }

    public void Clear()
    {
        _requests.Clear();
        _subscriptionIds.Clear();
        _hasAllRequest = false;
    }
}
