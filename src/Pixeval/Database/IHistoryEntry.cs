// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using LiteDB;

namespace Pixeval.Database;

public interface IHistoryEntry
{
    [BsonId(true)]
    public ObjectId? HistoryEntryId { get; set; }
}
