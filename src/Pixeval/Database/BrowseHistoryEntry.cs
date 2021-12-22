using LiteDB;

namespace Pixeval.Database;

public class BrowseHistoryEntry
{
    [BsonId(true)]
    public ObjectId? BrowseHistoryEntryId { get; set; }

    public string? Id { get; set; }
}