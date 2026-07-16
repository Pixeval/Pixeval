// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Diagnostics.CodeAnalysis;
using SQLite;

namespace Pixeval.Models.Database.Managers;

/// <summary>
/// A simple persistent manager without mapping
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SimplePersistentManager<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(SQLiteConnection db) : PersistentManagerBase<T, T>(db)
    where T : HistoryEntry, new()
{
    /// <inheritdoc />
    protected sealed override T ToModel(T entry) => entry;
}
