using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace Pixeval.Database;

public class SearchHistoryEntry
{
    [BsonId(true)]
    public ObjectId Id { get; set; }

    /// <summary>
    /// Search value
    /// </summary>
    public string? Value { get; set; }
    
    public DateTime Time { get; set; }
}