using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Database
{
    [Table("SearchHistories")]
    public class SearchHistoryEntry
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// Search value
        /// </summary>
        [Column("value")]
        public string? Value { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }
    }
}
