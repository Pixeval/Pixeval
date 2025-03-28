// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using LiteDB;
using Mako.Global.Enum;
using Mako.Model;
using WinUI3Utilities;

namespace Pixeval.Database;

public class BrowseHistoryEntry() : IHistoryEntry
{
    public BrowseHistoryEntry(IWorkEntry entry) : this()
    {
        ArgumentNullException.ThrowIfNull(entry);
        switch (entry)
        {
            case Illustration:
                Id = entry.Id;
                EntryDocument = BsonMapper.Global.ToDocument(entry);
                Type = SimpleWorkType.IllustAndManga;
                break;
            case Novel:
                Id = entry.Id;
                EntryDocument = BsonMapper.Global.ToDocument(entry);
                Type = SimpleWorkType.Novel;
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(entry);
                break;
        }
    }

    [BsonId(true)]
    public ObjectId? HistoryEntryId { get; set; }

    public long Id { get; set; }

    public BsonDocument? EntryDocument { get; set; }

    public SimpleWorkType Type { get; set; }

    [BsonIgnore]
    public IWorkEntry? TryGetEntry(SimpleWorkType type) =>
        Type switch
        {
            _ when type != Type || EntryDocument is null => null,
            SimpleWorkType.IllustAndManga => BsonMapper.Global.ToObject<Illustration>(EntryDocument),
            SimpleWorkType.Novel => BsonMapper.Global.ToObject<Novel>(EntryDocument),
            _ => null
        };
}
