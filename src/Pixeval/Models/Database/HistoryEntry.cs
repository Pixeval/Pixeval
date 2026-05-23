// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Diagnostics.CodeAnalysis;
using Misaki;
using SQLite;

namespace Pixeval.Models.Database;

public abstract class HistoryEntry : IEquatable<HistoryEntry>
{
    [PrimaryKey, AutoIncrement]
    public int HistoryEntryId { get; init; }

    public bool Equals(HistoryEntry? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return HistoryEntryId == other.HistoryEntryId;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as HistoryEntry);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HistoryEntryId;
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
