// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using SQLite;

namespace Pixeval.Models.Database.Managers;

public class WatchLaterPersistentManager(SQLiteConnection db)
    : SimplePersistentManager<WatchLaterEntry>(db)
{
    public WatchLaterEntry? GetByWorkKey(string workKey) =>
        string.IsNullOrWhiteSpace(workKey)
            ? null
            : Queryable.FirstOrDefault(t => t.WorkKey == workKey);
}
