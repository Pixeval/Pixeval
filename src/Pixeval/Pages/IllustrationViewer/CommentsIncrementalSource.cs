#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/CommentsIncrementalSource.cs
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Common.Collections;

namespace Pixeval.Pages.IllustrationViewer;

public class CommentsIncrementalSource : IIncrementalSource<CommentBlockViewModel>
{
    private readonly IAsyncEnumerable<CommentBlockViewModel?> _source;

    public CommentsIncrementalSource(IAsyncEnumerable<CommentBlockViewModel?> source)
    {
        _source = source;
    }

    public async Task<IEnumerable<CommentBlockViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new())
    {
        return (await _source.Skip(pageIndex * pageSize).Take(pageSize).ToArrayAsync(cancellationToken))!;
    }
}