#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SearchHistoryPersistentManager.cs
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
using System.Linq;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace Pixeval.Database.Managers;

public class SearchHistoryPersistentManager(ILiteDatabase db, int maximumRecords)
    : SimplePersistentManager<SearchHistoryEntry>(db, maximumRecords)
{
    public static void AddHistory(string text, string? optTranslatedName = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        if (manager.Count is 0 || manager.SelectLast(1).FirstOrDefault() is { Value: var last } && last != text)
        {
            manager.Insert(new SearchHistoryEntry
            {
                Value = text,
                TranslatedName = optTranslatedName,
                Time = DateTime.Now
            });
        }
    }
}
