// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using SQLite;

namespace Pixeval.Models.Database.Managers;

public class LoginUserPersistentManager(SQLiteConnection db) : SimplePersistentManager<LoginUserEntry>(db, int.MaxValue)
{
    private readonly SQLiteConnection _db = db;

    public LoginUserEntry? GetByKey(int key)
    {
        return key <= 0 ? null : Queryable.FirstOrDefault(t => t.HistoryEntryId == key);
    }

    public LoginUserEntry? GetByRefreshToken(string refreshToken)
    {
        return string.IsNullOrWhiteSpace(refreshToken)
            ? null
            : Queryable.FirstOrDefault(t => t.RefreshToken == refreshToken);
    }

    public override void AddOrUpdate(LoginUserEntry entry)
    {
        var existing = Queryable.FirstOrDefault(t => t.UserId == entry.UserId)
                       ?? GetByRefreshToken(entry.RefreshToken);
        if (existing is not null)
        {
            existing.UpdateFrom(entry);
            Update(existing);
            return;
        }

        _db.Insert(entry, typeof(LoginUserEntry));
    }

    public LoginUserEntry Upsert(LoginUserEntry entry)
    {
        AddOrUpdate(entry);
        return Queryable.FirstOrDefault(t => t.UserId == entry.UserId)
               ?? GetByRefreshToken(entry.RefreshToken)
               ?? entry;
    }
}
