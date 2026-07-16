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
            : AccessDatabase(connection => connection.Table<SearchHistoryEntry>()
                .FirstOrDefault(entry => entry.Value == value));

    public bool TryDeleteByValue(string value) =>
        !string.IsNullOrWhiteSpace(value)
        && AccessDatabase(connection => connection.Table<SearchHistoryEntry>()
            .Delete(entry => entry.Value == value) is not 0);

    public override void AddOrUpdate(SearchHistoryEntry entry) =>
        AccessDatabase(connection => AddOrUpdateCore(connection, entry));

    public override SearchHistoryEntry Upsert(SearchHistoryEntry entry)
    {
        AccessDatabase(connection => AddOrUpdateCore(connection, entry));
        return entry;
    }

    private static void AddOrUpdateCore(SQLiteConnection connection, SearchHistoryEntry entry) =>
        connection.RunInTransaction(() =>
        {
            _ = connection.Table<SearchHistoryEntry>().Delete(item => item.Value == entry.Value);
            _ = connection.Insert(entry, typeof(SearchHistoryEntry));
        });
}
