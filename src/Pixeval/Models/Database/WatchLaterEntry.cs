// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Misaki;

namespace Pixeval.Models.Database;

public class WatchLaterEntry : BrowseHistoryEntry
{
    public WatchLaterEntry(IArtworkInfo entry) : base(entry)
    {
    }

    public WatchLaterEntry()
    {
    }

    public static new bool TryCreateWorkKey(IArtworkInfo entry, out string key) => BrowseHistoryEntry.TryCreateWorkKey(entry, out key);
}
