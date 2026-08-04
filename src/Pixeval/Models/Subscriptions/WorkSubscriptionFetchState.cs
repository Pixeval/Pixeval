// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;

namespace Pixeval.Models.Subscriptions;

public sealed class WorkSubscriptionFetchState(
    int workSubscriptionId,
    bool isFetching,
    int fetchedCount) : EventArgs
{
    public int WorkSubscriptionId { get; } = workSubscriptionId;

    public bool IsFetching { get; } = isFetching;

    public int FetchedCount { get; } = fetchedCount;
}
