// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using SQLite;

namespace Pixeval.Models.Database;

public class DownloadFolderEntry : HistoryEntry
{
    /// <summary>
    /// <see cref="WorkSubscriptionEntry"/> 1-1 Foreign Key
    /// </summary>
    [Indexed(Unique = true)]
    public int SubscriptionEntryId { get; set; }

    [Indexed]
    public long UserId { get; set; }

    public string Name { get; set; } = "";

    [Ignore]
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? UserId.ToString() : Name;

    public void UpdateFrom(DownloadFolderEntry entry)
    {
        SubscriptionEntryId = entry.SubscriptionEntryId;
        UserId = entry.UserId;
        Name = entry.Name;
    }
}
