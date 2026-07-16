// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Diagnostics.CodeAnalysis;
using Misaki;
using SQLite;

namespace Pixeval.Models.Database;

public abstract class ArtworkHistoryEntry : HistoryEntry
{
    protected ArtworkHistoryEntry(IArtworkInfo? entry)
    {
        // 数据库构造
        if (entry is null)
            return;

        if (entry is not ISerializable serializable)
            throw new InvalidCastException($"{nameof(entry)} should be {nameof(ISerializable)}");

        SerializeKey = serializable.SerializeKey;
        Payload = new(serializable.Serialize());
        Entry = entry;
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    [Indexed]
    public string SerializeKey { get; init; } = null!;

    [Indexed(Unique = true)]
    public int ArtworkPayloadEntryId { get; set; }

    [Ignore]
    internal ArtworkPayloadEntry Payload { get; private set; } = null!;

    [Ignore]
    [field: AllowNull, MaybeNull]
    public IArtworkInfo Entry { get; private set; } = null!;

    internal void AttachPayload(ArtworkPayloadEntry payload)
    {
        Payload = payload;
        Entry = (IArtworkInfo) ArtworkSerializerTable.ArtworkTypeMethodsTable[SerializeKey](payload.SerializedArtwork);
    }
}
