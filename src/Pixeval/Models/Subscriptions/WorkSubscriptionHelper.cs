// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Engine;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Options;
using Pixeval.Utilities;

namespace Pixeval.Models.Subscriptions;

public static class WorkSubscriptionHelper
{
    public static bool TryAddOrUpdateUser(
        UserBasicInfo user,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind)
    {
        var subscription = new WorkSubscriptionEntry
        {
            Id = user.Id,
            SubscriptionType = subscriptionType,
            WorkKind = workKind
        };
        subscription.UpdateUserMetadata(user);
        return TryAddOrUpdate(subscription);
    }

    public static bool TryAddOrUpdateSeries(
        long seriesId,
        WorkSubscriptionWorkKind workKind,
        SeriesDetailBase? seriesDetail = null,
        IWorkEntry? firstWork = null,
        IFetchEngine<IWorkEntry>? sourceEngine = null)
    {
        var subscription = new WorkSubscriptionEntry
        {
            Id = seriesId,
            SubscriptionType = WorkSubscriptionType.Series,
            WorkKind = workKind
        };
        subscription.UpdateSeriesMetadata(seriesDetail, firstWork);
        return TryAddOrUpdate(subscription, sourceEngine);
    }

    private static bool TryAddOrUpdate(
        WorkSubscriptionEntry subscription,
        IFetchEngine<IWorkEntry>? sourceEngine = null)
    {
        if (subscription.Id is 0)
            return false;

        var serviceProvider = App.AppViewModel.AppServiceProvider;
        var subscriptionManager = serviceProvider.GetRequiredService<WorkSubscriptionPersistentManager>();
        subscription = subscriptionManager.Upsert(subscription);

        App.AppViewModel.QueueWorkSubscriptionInitialSync(subscription, sourceEngine);
        return true;
    }

    extension(WorkSubscriptionEntry subscription)
    {
        internal void UpdateUserMetadata(UserBasicInfo user)
        {
            subscription.Name = user.Name;
            subscription.AvatarUrl = user.AvatarUrl;
            subscription.Account = user.Account;
        }

        internal void UpdateSeriesMetadata(SeriesDetailBase? seriesDetail,
            IWorkEntry? firstWork)
        {
            subscription.Name = seriesDetail?.Title ?? "";
            subscription.AvatarUrl = seriesDetail is MangaSeriesDetail manga
                ? manga.CoverImageUrls.Medium
                : firstWork?.GetThumbnailUrl() ?? "";
            subscription.Account = seriesDetail?.User.Name ?? "";
        }
    }
}
