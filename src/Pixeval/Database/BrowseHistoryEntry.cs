// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Diagnostics.CodeAnalysis;
using LiteDB;
using Misaki;
using Pixeval.Util;
using WinUI3Utilities;

namespace Pixeval.Database;

public class BrowseHistoryEntry() : IHistoryEntry
{
    public BrowseHistoryEntry(IArtworkInfo entry) : this()
    {
        if (entry is not ISerializable serializable)
        {
            ThrowHelper.InvalidOperation($"{nameof(entry)} should be {nameof(ISerializable)}");
            return;
        }

        Entry = entry;
        Id = entry.Id;
        SerializeKey = serializable.SerializeKey;
        EntryString = serializable.Serialize();
    }

    [BsonId(true)]
    public ObjectId? HistoryEntryId { get; set; }

    [BsonIgnore]
    [field: AllowNull, MaybeNull]
    public IArtworkInfo Entry => field ??= (IArtworkInfo) ArtworkSerializerTable.ArtworkTypeMethodsTable[SerializeKey](EntryString);

    // private set 反序列化使用
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public string Id { get; private set; } = null!;

    // private set 反序列化使用
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public string SerializeKey { get; private set; } = null!;

    // private set 反序列化使用
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public string EntryString { get; private set; } = null!;
}
