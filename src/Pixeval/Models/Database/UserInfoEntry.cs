// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using SQLite;

namespace Pixeval.Models.Database;

public abstract class UserInfoEntry : HistoryEntry
{
    public abstract long Id { get; set; }

    public string Name { get; set; } = "";

    public string AvatarUrl { get; set; } = "";

    public string Account { get; set; } = "";

    [Ignore]
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Id.ToString() : Name;

    protected void UpdateUserInfoFrom(UserInfoEntry entry)
    {
        Id = entry.Id;
        Name = entry.Name;
        AvatarUrl = entry.AvatarUrl;
        Account = entry.Account;
    }
}
