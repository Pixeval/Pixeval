using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Database.Managers
{
    public class SearchHistoryPersistentManager : SimplePersistentManager<SearchHistoryEntry>
    {
        public SearchHistoryPersistentManager(SQLiteAsyncConnection connection) : base(connection) {}
    }
}
