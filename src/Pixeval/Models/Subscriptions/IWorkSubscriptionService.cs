// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Pixeval.Models.Database;

namespace Pixeval.Models.Subscriptions;

public interface IWorkSubscriptionService
{
    WorkSubscriptionFetchState? CurrentFetchState { get; }

    event EventHandler<WorkSubscriptionFetchState>? FetchStateChanged;

    event EventHandler<WorkSubscriptionEntry>? SubscriptionUpdated;

    event EventHandler<int>? SubscriptionRemoved;

    Task<WorkSubscriptionEntry?> TryRemoveAsync(int historyEntryId);
}
