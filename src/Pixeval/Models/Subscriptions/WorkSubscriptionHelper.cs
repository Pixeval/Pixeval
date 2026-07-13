// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Options;
using Pixeval.Utilities;

namespace Pixeval.Models.Subscriptions;

public static class WorkSubscriptionHelper
{
    public static bool TryAddOrUpdate(
        UserBasicInfo user,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind) =>
        TryAddOrUpdate(
            user.Id,
            subscriptionType,
            workKind,
            user.Name,
            user.AvatarUrl,
            user.Account);

    public static bool TryAddOrUpdateSeries(
        long seriesId,
        WorkSubscriptionWorkKind workKind,
        SeriesDetailBase? seriesDetail = null,
        IWorkEntry? firstWork = null) =>
        TryAddOrUpdate(
            seriesId,
            WorkSubscriptionType.Series,
            workKind,
            seriesDetail?.Title ?? "",
            seriesDetail is MangaSeriesDetail manga
                ? manga.CoverImageUrls.Medium
                : firstWork?.GetThumbnailUrl() ?? "",
            seriesDetail?.User.Name ?? "");

    private static bool TryAddOrUpdate(
        long targetId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind,
        string name,
        string imageUrl,
        string description)
    {
        if (targetId is 0)
            return false;
        var serviceProvider = App.AppViewModel.AppServiceProvider;
        var subscriptionManager = serviceProvider.GetRequiredService<WorkSubscriptionPersistentManager>();
        _ = subscriptionManager.Upsert(new()
        {
            Id = targetId,
            SubscriptionType = subscriptionType,
            WorkKind = workKind,
            Name = name,
            AvatarUrl = imageUrl,
            Account = description
        });

        App.AppViewModel.QueueWorkSubscriptionSyncAll();
        return true;
    }
}
