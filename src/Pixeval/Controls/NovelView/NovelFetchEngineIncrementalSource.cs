#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelFetchEngineIncrementalSource.cs
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

using System.Collections.Generic;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public class NovelFetchEngineIncrementalSource(IAsyncEnumerable<Novel> asyncEnumerator, int limit = -1) : FetchEngineIncrementalSource<Novel, NovelItemViewModel>(asyncEnumerator, limit)
{
    protected override long Identifier(Novel entity) => entity.Id;

    protected override NovelItemViewModel Select(Novel entity) => new(entity);
}