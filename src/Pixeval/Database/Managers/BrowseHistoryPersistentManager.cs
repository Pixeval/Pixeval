#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/BrowseHistoryPersistentManager.cs
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

using System.Collections.ObjectModel;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;

namespace Pixeval.Database.Managers;

public class BrowseHistoryPersistentManager(ILiteDatabase db, int maximumRecords)
    : SimplePersistentManager<BrowseHistoryEntry>(db, maximumRecords)
{
    public ObservableCollection<BrowseHistoryEntry> ObservableEntries { get; } = [];

    public static void AddHistory(IWorkEntry entry)
    {
        if (entry.Id is 0)
            return;
        var type = entry is Illustration ? SimpleWorkType.IllustAndManga : SimpleWorkType.Novel;
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        if (manager.TryDelete(x => x.Id == entry.Id && x.Type == type) is { } e)
            manager.ObservableEntries.Remove(e);
        var browseHistoryEntry = new BrowseHistoryEntry(entry);
        manager.Insert(browseHistoryEntry);
        manager.ObservableEntries.Insert(0, browseHistoryEntry);
    }
}
