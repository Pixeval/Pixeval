// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using SQLite;

namespace Pixeval.Models.Database.Managers;

/// <remarks>
/// 有必要保留的是“共享连接同步”，SqlitePersistentManager 这个类本身只是当前最简单的承载方式。不能因为启用了 FullMutex 就直接删除 AccessDatabase。
/// 原因：
/// `FullMutex` 只保证单次原生 SQLite API 调用可跨线程安全执行，不保证多次调用组成的业务操作不会交错。
/// 当前存在查询、删除、插入组成的事务，例如 `ArtworkHistoryPersistentManager`。sqlite-net 的 RunInTransaction 本身没有覆盖整个委托的全局锁，其他 manager 可能中途进入同一连接的事务。
/// sqlite-net 插入后还会调用 LastInsertRowid 回填自增主键。不同表并发插入时，FullMutex 无法保证“插入 + 读取 ID”这两个调用之间不被另一线程插入打断。
/// `WatchLaterPersistentManager` 还依赖同一临界区保护 _workKeys。
/// 所以 `lock (Db)` 和 FullMutex 保护的是不同层次，并不重复。当前直接删除反而会造成正确性风险，性能上也不会获得真正的数据库并行能力，因为仍然只有一个连接。
/// 结论：保留当前基类比较合理。若以后要真正并发读写，应改成多连接加 WAL，或者使用 SQLiteAsyncConnection 的连接池模型，再一起移除这层全局锁。
/// </remarks>
public abstract class SqlitePersistentManager(SQLiteConnection db)
{
    protected SQLiteConnection Db { get; } = db;

    protected void AccessDatabase(Action<SQLiteConnection> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        lock (Db)
            action(Db);
    }

    protected TResult AccessDatabase<TResult>(Func<SQLiteConnection, TResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        lock (Db)
            return action(Db);
    }
}
