using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB.Async;
using Pixeval.Models;

namespace Pixeval.Data
{
    internal class SearchHistoryRepository : BaseRepository<SearchHistory>
    {
        public SearchHistoryRepository(ILiteDatabaseAsync database) : base(database)
        {
        }
    }
}
