// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using SQLite;

namespace Pixeval.Models.Database.Managers;

public class LoginUserPersistentManager(SQLiteConnection db) : SimplePersistentManager<LoginUserEntry>(db)
{
    public LoginUserEntry? GetByKey(int key) =>
        key <= 0 ? null : AccessDatabase(connection => connection.Find<LoginUserEntry>(key));

    public LoginUserEntry? GetByRefreshToken(string refreshToken) =>
        string.IsNullOrWhiteSpace(refreshToken)
            ? null
            : AccessDatabase(connection => connection.Table<LoginUserEntry>()
                .FirstOrDefault(entry => entry.RefreshToken == refreshToken));

    public LoginUserEntry? GetByUserId(long userId) =>
        userId <= 0
            ? null
            : AccessDatabase(connection => connection.Table<LoginUserEntry>()
                .FirstOrDefault(entry => entry.UserId == userId));

    public override void AddOrUpdate(LoginUserEntry entry)
    {
        AccessDatabase(connection => _ = AddOrUpdateCore(connection, entry));
    }

    public override LoginUserEntry Upsert(LoginUserEntry entry) =>
        AccessDatabase(connection => AddOrUpdateCore(connection, entry));

    private static LoginUserEntry AddOrUpdateCore(SQLiteConnection connection, LoginUserEntry entry)
    {
        var table = connection.Table<LoginUserEntry>();
        var existing = table.FirstOrDefault(item => item.UserId == entry.UserId)
                       ?? table.FirstOrDefault(item => item.RefreshToken == entry.RefreshToken);
        if (existing is null)
        {
            _ = connection.Insert(entry, typeof(LoginUserEntry));
            return entry;
        }

        existing.UpdateFrom(entry);
        _ = connection.Update(existing, typeof(LoginUserEntry));
        return existing;
    }
}
