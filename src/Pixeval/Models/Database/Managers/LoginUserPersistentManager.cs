// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using SQLite;

namespace Pixeval.Models.Database.Managers;

public class LoginUserPersistentManager(SQLiteConnection db) : SimplePersistentManager<LoginUserEntry>(db)
{
    public LoginUserEntry? GetByKey(int key) => key <= 0 ? null : Db.Find<LoginUserEntry>(key);

    public LoginUserEntry? GetByRefreshToken(string refreshToken) =>
        string.IsNullOrWhiteSpace(refreshToken)
            ? null
            : Queryable.FirstOrDefault(t => t.RefreshToken == refreshToken);

    public LoginUserEntry? GetByUserId(long userId) =>
        userId <= 0 ? null : Queryable.FirstOrDefault(t => t.UserId == userId);

    public override void AddOrUpdate(LoginUserEntry entry)
    {
        var existing = GetByUserId(entry.UserId) ?? GetByRefreshToken(entry.RefreshToken);
        if (existing is not null)
        {
            existing.UpdateFrom(entry);
            Update(existing);
            return;
        }

        Db.Insert(entry, typeof(LoginUserEntry));
    }

    public override LoginUserEntry Upsert(LoginUserEntry entry)
    {
        AddOrUpdate(entry);
        return GetByUserId(entry.UserId)
               ?? GetByRefreshToken(entry.RefreshToken)
               ?? entry;
    }
}
