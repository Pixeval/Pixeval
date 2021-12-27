#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IPersistentManager.cs
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
using System.Linq.Expressions;
using LiteDB;

namespace Pixeval.Database.Managers;

/// <summary>
///     Manage persistent data stored in SQLite.
///     You may call CreateTable in the constructor.
/// </summary>
/// <example>
///     This example shows how to get a registered manager.
///     <code>
/// using var scope = App.AppViewModel.AppServicesScope;
/// var manager = scope.ServiceProvider.GetRequiredService&lt;IPersistentManager&lt;Entry, Model&gt;&gt;();
/// </code>
/// </example>
/// <see cref="AppViewModel.CreateHostBuilder">Register the manager in AppViewModel</see>
/// <typeparam name="TEntry">Entry to be serialized in database</typeparam>
/// <typeparam name="TModel">Data model in the program</typeparam>
public interface IPersistentManager<TEntry, out TModel>
    where TEntry : new()
{
    ILiteCollection<TEntry> Collection { get; init; }

    int MaximumRecords { get; set; }

    void Insert(TEntry t);

    IEnumerable<TModel> Query(Expression<Func<TEntry, bool>> predicate);

    IEnumerable<TModel> Select(Expression<Func<TEntry, bool>>? predicate = null, int? count = null);

    int Delete(Expression<Func<TEntry, bool>> predicate);

    IEnumerable<TModel> Enumerate();

    void Purge(int limit);
}