// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Models.Options;
using SQLite;

namespace Pixeval.Models.Database;

public class WorkSubscriptionEntry : UserInfoEntry
{
    [Indexed("IX_WorkSubscriptionEntry_Key", 0, Unique = true)]
    public override long Id { get; set; }

    [Indexed("IX_WorkSubscriptionEntry_Key", 1, Unique = true)]
    public WorkSubscriptionType SubscriptionType { get; set; }

    [Indexed("IX_WorkSubscriptionEntry_Key", 2, Unique = true)]
    public WorkSubscriptionWorkKind WorkKind { get; set; }

    public void UpdateFrom(WorkSubscriptionEntry entry)
    {
        UpdateUserInfoFrom(entry);
        SubscriptionType = entry.SubscriptionType;
        WorkKind = entry.WorkKind;
    }
}
