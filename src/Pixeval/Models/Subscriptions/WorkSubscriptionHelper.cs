// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Options;

namespace Pixeval.Models.Subscriptions;

public static class WorkSubscriptionHelper
{
    public static bool TryAddOrUpdate(
        UserBasicInfo user,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind)
    {
        if (user.Id is 0)
            return false;
        var serviceProvider = App.AppViewModel.AppServiceProvider;
        var subscriptionManager = serviceProvider.GetRequiredService<WorkSubscriptionPersistentManager>();
        _ = subscriptionManager.Upsert(new()
        {
            UserId = user.Id,
            SubscriptionType = subscriptionType,
            WorkKind = workKind,
            Name = user.Name,
            AvatarUrl = user.AvatarUrl,
            Account = user.Account
        });

        App.AppViewModel.QueueWorkSubscriptionSyncAll();
        return true;
    }
}
