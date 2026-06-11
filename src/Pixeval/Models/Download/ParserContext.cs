// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Misaki;
using Pixeval.Models.Options;

namespace Pixeval.Models.Download;

public sealed record ParserContext(
    IArtworkInfo ArtworkInfo,
    WorkSubscriptionDownloadContext WorkSubscription = default);

public readonly record struct WorkSubscriptionDownloadContext(bool IsGroup, WorkSubscriptionType? SubscriptionType)
{
    public static WorkSubscriptionDownloadContext FromSubscriptionType(WorkSubscriptionType subscriptionType) =>
        new(true, subscriptionType);
}
