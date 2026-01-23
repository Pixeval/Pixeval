// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics.CodeAnalysis;
using Misaki;
using SQLite;

namespace Pixeval.Models.Database;

public abstract class HistoryEntry
{
    [PrimaryKey, AutoIncrement]
    public int HistoryEntryId { get; set; }
}

public abstract class ArtworkHistoryEntry : HistoryEntry
{
    protected ArtworkHistoryEntry(IArtworkInfo? entry)
    {
        // 数据库构造
        if (entry is null)
            return;

        if (entry is not ISerializable serializable)
            throw new InvalidCastException($"{nameof(entry)} should be {nameof(ISerializable)}");

        Entry = entry;
        SerializeKey = serializable.SerializeKey;
        EntryString = serializable.Serialize();
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string SerializeKey { get; init; } = null!;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string EntryString { get; init; } = null!;

    [Ignore]
    [field: AllowNull, MaybeNull]
    public IArtworkInfo Entry => field ??= (IArtworkInfo) ArtworkSerializerTable.ArtworkTypeMethodsTable[SerializeKey](EntryString);
}
