#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/MangaDownloadTask.cs
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
using System.IO;
using System.Threading.Tasks;
using Pixeval.Controls;
using Pixeval.Database;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Download.Models;

public class MangaDownloadTask(DownloadHistoryEntry entry, IllustrationItemViewModel illustration)
    : IllustrationDownloadTask(entry, illustration)
{
    protected int CurrentIndex { get; private set; }

    public override async Task DownloadAsync(
        Func<string, IProgress<double>?, CancellationHandle?, Task<Result<Stream>>> downloadStreamAsync)
    {
        for (CurrentIndex = 0; CurrentIndex < Urls.Count; ++CurrentIndex)
        {
            var dest = IoHelper.GetPathFromUrlFormat(Destination, Url).Replace("<manga_index>", CurrentIndex.ToString());
            await base.DownloadAsyncCore((a1, a2, a3) => downloadStreamAsync(a1, new MyProgress(a2, Urls.Count, CurrentIndex), a3), Urls[CurrentIndex], dest);
        }
    }
}

file class MyProgress(IProgress<double>? dest, int count, int index) : IProgress<double>
{
    public void Report(double value) => dest?.Report((100 * index + value) / count);
}
