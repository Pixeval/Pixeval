// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Misaki;
using SQLite;

namespace Pixeval.Models.Database;

public class BrowseHistoryEntry : ArtworkHistoryEntry
{
    public BrowseHistoryEntry(IArtworkInfo entry) : base(entry)
    {
        Id = entry.Id;
        WorkKey = CreateWorkKey(SerializeKey, Id);
    }

    public BrowseHistoryEntry() : base(null)
    {
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    [Indexed(Unique = true)]
    public string WorkKey { get; init; } = null!;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Id { get; init; } = null!;

    public static bool TryCreateWorkKey(IArtworkInfo entry, out string key)
    {
        key = "";
        if (entry is not ISerializable { SerializeKey: { } serializeKey }
            || string.IsNullOrEmpty(entry.Id))
            return false;

        key = CreateWorkKey(serializeKey, entry.Id);
        return true;
    }

    private static string CreateWorkKey(string serializeKey, string id) => $"{serializeKey}:{id}";
}
