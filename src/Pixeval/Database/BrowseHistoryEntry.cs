using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace Pixeval.Database;

public class BrowseHistoryEntry
{
    [BsonId(true)]
    public ObjectId? BrowseHistoryEntryId { get; set; }
    public string? Id { get; set; }
}