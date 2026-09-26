// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;

namespace Pixeval.Models.Subscriptions;

public sealed class WorkSubscriptionFetchState(
    int workSubscriptionId,
    bool isFetching,
    int fetchedCount,
    DateTimeOffset? retryAt = null) : EventArgs
{
    public int WorkSubscriptionId { get; } = workSubscriptionId;

    public bool IsFetching { get; } = isFetching;

    public int FetchedCount { get; } = fetchedCount;

    public DateTimeOffset? RetryAt { get; } = retryAt;
}
