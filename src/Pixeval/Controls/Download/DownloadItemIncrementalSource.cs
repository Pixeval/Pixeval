#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadItemIncrementalSource.cs
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
using CommunityToolkit.WinUI.Collections;
using Pixeval.Download.Models;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public class DownloadItemIncrementalSource(IEnumerable<DownloadTaskBase> source)
    : IIncrementalSource<DownloadItemViewModel>
{
    public async Task<IEnumerable<DownloadItemViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new CancellationToken())
    {
        return await source
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(async o =>
            {
                if (o is ILazyLoadDownloadTask lazy)
                    await lazy.LazyLoadAsync(o.DatabaseEntry.Entry);
                return new DownloadItemViewModel(o);
            }).WhenAll();
    }
}
