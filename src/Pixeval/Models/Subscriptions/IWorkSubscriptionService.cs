// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Pixeval.Models.Database;
using Pixeval.Models.Options;

namespace Pixeval.Models.Subscriptions;

public interface IWorkSubscriptionService
{
    WorkSubscriptionFetchState? CurrentFetchState { get; }

    event EventHandler<WorkSubscriptionFetchState>? FetchStateChanged;

    event EventHandler<WorkSubscriptionEntry>? SubscriptionUpdated;

    event EventHandler<int>? SubscriptionRemoved;

    WorkSubscriptionEntry? TryGetSubscription(
        long targetId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind);

    Task<WorkSubscriptionEntry?> TryRemoveAsync(int historyEntryId);
}
