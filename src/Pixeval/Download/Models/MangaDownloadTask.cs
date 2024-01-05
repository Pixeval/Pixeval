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
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Pixeval.Controls.IllustrationView;
using Pixeval.Database;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Download.Models;

public class MangaDownloadTask(DownloadHistoryEntry entry, IllustrationItemViewModel illustration)
    : IllustrationDownloadTask(entry, illustration)
{
    protected int CurrentIndex { get; private set; }

    public override async Task DownloadAsync(Func<string, IProgress<double>?, CancellationHandle?, Task<Result<IRandomAccessStream>>> downloadRandomAccessStreamAsync)
    {
        for (CurrentIndex = 0; CurrentIndex < Urls.Count; ++CurrentIndex)
        {
            var dest = Destination.Format(CurrentIndex);
            await base.DownloadAsyncCore(downloadRandomAccessStreamAsync, Urls[CurrentIndex], dest);
        }
    }
}
