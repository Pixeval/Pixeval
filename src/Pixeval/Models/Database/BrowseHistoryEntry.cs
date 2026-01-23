// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;

namespace Pixeval.Models.Database;

public class BrowseHistoryEntry : ArtworkHistoryEntry
{
    public BrowseHistoryEntry(IArtworkInfo entry) : base(entry)
    {
        Id = entry.Id;
    }

    public BrowseHistoryEntry() : base(null)
    {
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Id { get; init; } = null!;
}
