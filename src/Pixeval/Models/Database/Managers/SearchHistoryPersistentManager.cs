// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using SQLite;

namespace Pixeval.Models.Database.Managers;

public class SearchHistoryPersistentManager(SQLiteConnection db)
    : SimplePersistentManager<SearchHistoryEntry>(db)
{
    public SearchHistoryEntry? GetByValue(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : Queryable.FirstOrDefault(t => t.Value == value);
}
