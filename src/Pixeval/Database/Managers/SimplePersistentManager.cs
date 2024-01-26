#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SimplePersistentManager.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;

namespace Pixeval.Database.Managers;

/// <summary>
/// A simple persistent manager without mapping
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SimplePersistentManager<T>(ILiteDatabase db, int maximumRecords) : IPersistentManager<T, T>
    where T : new()
{
#nullable disable
    public ILiteCollection<T> Collection { get; init; } = db.GetCollection<T>(typeof(T).Name);
#nullable restore
    public int MaximumRecords { get; set; } = maximumRecords;

    public int Count => Collection.Count();

    public void Insert(T t)
    {
        if (Collection.Count() > MaximumRecords)
        {
            Purge(MaximumRecords);
        }

        _ = Collection.Insert(t);
    }

    public IEnumerable<T> Query(Expression<Func<T, bool>> predicate)
    {
        return Collection.Find(predicate);
    }

    public IEnumerable<T> Select(Expression<Func<T, bool>>? predicate = null, int? count = null)
    {
        var query = Collection.FindAll();
        if (count.HasValue)
        {
            query = query.TakeLast(count.Value);
        }

        if (predicate != null)
        {
            query = query.Where(predicate.Compile());
        }

        return query.ToList();
    }

    public int Delete(Expression<Func<T, bool>> predicate)
    {
        return Collection.DeleteMany(predicate);
    }

    public IEnumerable<T> Enumerate()
    {
        return Collection.FindAll();
    }

    public void Purge(int limit)
    {
        if (Collection.Count() > limit)
        {
            var last = Collection.FindAll().Take(^limit..).ToHashSet();
            _ = Delete(e => !last.Contains(e!));
        }
    }

    public void Clear()
    {
        _ = Collection.DeleteAll();
    }
}