// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Models.Options;
using SQLite;

namespace Pixeval.Models.Database;

public class WorkSubscriptionEntry : HistoryEntry
{
    [Indexed("IX_WorkSubscriptionEntry_Key", 0, Unique = true)]
    public long Id { get; set; }

    [Indexed("IX_WorkSubscriptionEntry_Key", 1, Unique = true)]
    public WorkSubscriptionType SubscriptionType { get; set; }

    [Indexed("IX_WorkSubscriptionEntry_Key", 2, Unique = true)]
    public WorkSubscriptionWorkKind WorkKind { get; set; }

    public string Name { get; set; } = "";

    public string AvatarUrl { get; set; } = "";

    public string Account { get; set; } = "";

    [Ignore]
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Id.ToString() : Name;

    public void UpdateFrom(WorkSubscriptionEntry entry)
    {
        Id = entry.Id;
        SubscriptionType = entry.SubscriptionType;
        WorkKind = entry.WorkKind;
        Name = entry.Name;
        AvatarUrl = entry.AvatarUrl;
        Account = entry.Account;
    }
}
