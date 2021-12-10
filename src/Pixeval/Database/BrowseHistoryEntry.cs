using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Pixeval.Database;

[Table("BrowseHistories")]
public class BrowseHistoryEntry
{
    [PrimaryKey]
    [Column("work_id")]
    public string? Id { get; set; }
}