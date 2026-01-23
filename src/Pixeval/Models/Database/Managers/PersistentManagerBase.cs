using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public abstract class PersistentManagerBase<TEntry, TModel> : IPersistentManager<TEntry, TModel>
    where TEntry : HistoryEntry, new()
{
    private readonly SQLiteConnection _db;

    protected PersistentManagerBase(SQLiteConnection db, int maximumRecords)
    {
        _db = db;
        MaximumRecords = maximumRecords;
        _ = _db.CreateTable<TEntry>();
        return;
        // TODO
        // 不直接用 CreateTable<TEntry>()，因其 MigrateTable 在 AOT 下因 ColumnInfo 被裁剪
        // 会误判所有列不存在，导致 "duplicate column name" 异常
        CreateTableIfNotExists<TEntry>(_db);
    }

    private static void CreateTableIfNotExists<TTable>(SQLiteConnection db) where TTable : new()
    {
        var tableName = db.GetMapping<TTable>().TableName;
        var count = db.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=?", tableName);
        if (count == 0)
            db.CreateTable<TTable>();
    }

    /// <inheritdoc />
    public virtual TableQuery<TEntry> Queryable => _db.Table<TEntry>();

    /// <inheritdoc />
    public int MaximumRecords { get; set; }

    /// <inheritdoc />
    public virtual int Count => Queryable.Count();

    /// <inheritdoc />
    public virtual void Insert(TEntry t)
    {
        if (Queryable.Count() > MaximumRecords)
            Purge(MaximumRecords);

        _db.Insert(t);
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<TModel> Query(Expression<Func<TEntry, bool>> predicate, int skip = 0, int limit = int.MaxValue) => 
        Queryable.Where(predicate).Skip(skip).Take(limit).Select(ToModel).ToArray();

    /// <inheritdoc />
    public virtual void AddOrUpdate(TEntry entry) => _db.InsertOrReplace(entry);

    /// <inheritdoc />
    public virtual void Update(TEntry entry) => _db.Update(entry);

    /// <inheritdoc />
    public virtual IReadOnlyList<TModel> Take(int count) => Queryable.Take(count).Select(ToModel).ToArray();

    /// <inheritdoc />
    public virtual IReadOnlyList<TModel> TakeLast(int count)
    {
        var c = Count;
        if (count > c)
            count = c;
        return Queryable.Skip(c - count).Take(count).Select(ToModel).ToArray();
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<TModel> Select(Expression<Func<TEntry, bool>> predicate) => 
        Queryable.Where(predicate).Select(ToModel).ToArray();

    /// <inheritdoc />
    public virtual TEntry? TryDelete(Expression<Func<TEntry, bool>> predicate)
    {
        if (Queryable.Where(predicate).FirstOrDefault() is { } e)
        {
            _db.Delete(e);
            return e;
        }

        return null;
    }

    /// <inheritdoc />
    public virtual int Delete(Expression<Func<TEntry, bool>> predicate)
    {
        return Queryable.Delete(predicate);
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<TModel> ToArray()
    {
        return Queryable.Select(ToModel).ToArray();
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<TModel> Reverse()
    {
        return Queryable.OrderByDescending(t => t.HistoryEntryId).Select(ToModel).ToArray();
    }

    /// <inheritdoc />
    public virtual void Purge(int limit)
    {
        var deleteCount = Count - limit;
        if (deleteCount > 0)
        {
            var toDelete = Queryable.Take(deleteCount).ToArray();
            foreach (var item in toDelete)
                _db.Delete(item);
        }
    }

    /// <inheritdoc />
    public virtual void Clear() => _db.DeleteAll<TEntry>();

    protected abstract TModel ToModel(TEntry entry);
}
