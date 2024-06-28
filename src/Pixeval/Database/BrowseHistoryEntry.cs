#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/BrowseHistoryEntry.cs
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
using LiteDB;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using WinUI3Utilities;

namespace Pixeval.Database;

public class BrowseHistoryEntry() : IHistoryEntry
{
    public BrowseHistoryEntry(IWorkEntry entry) : this()
    {
        ArgumentNullException.ThrowIfNull(entry);
        switch (entry)
        {
            case Illustration:
                Id = entry.Id;
                EntryDocument = BsonMapper.Global.ToDocument(entry);
                Type = SimpleWorkType.IllustAndManga;
                break;
            case Novel:
                Id = entry.Id;
                EntryDocument = BsonMapper.Global.ToDocument(entry);
                Type = SimpleWorkType.Novel;
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(entry);
                break;
        }
    }

    [BsonId(true)]
    public ObjectId? HistoryEntryId { get; set; }

    public long Id { get; set; }

    public BsonDocument? EntryDocument { get; set; }

    public SimpleWorkType Type { get; set; }

    [BsonIgnore]
    public IWorkEntry? TryGetEntry(SimpleWorkType type) =>
        Type switch
        {
            _ when type != Type || EntryDocument is null => null,
            SimpleWorkType.IllustAndManga => BsonMapper.Global.ToObject<Illustration>(EntryDocument),
            SimpleWorkType.Novel => BsonMapper.Global.ToObject<Novel>(EntryDocument),
            _ => null
        };
}
