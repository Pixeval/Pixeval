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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.Controls;
using Pixeval.Database;
using Pixeval.Download.Macros;
using Pixeval.Util.IO;

namespace Pixeval.Download.Models;

public class MangaDownloadTask(DownloadHistoryEntry entry, IllustrationItemViewModel illustration)
    : IllustrationDownloadTask(entry, illustration)
{
    protected int CurrentIndex { get; set; }

    public override bool IsFolder => true;

    public virtual IReadOnlyList<string> Urls => IllustrationViewModel.MangaOriginalUrls;

    public override IReadOnlyList<string> ActualDestinations => Urls.Select((t, i) => IoHelper.ReplaceTokenExtensionFromUrl(Destination, t).Replace(MangaIndexMacro.NameConstToken, i.ToString())).ToArray();

    public override async Task DownloadAsync(Downloader downloadStreamAsync)
    {
        StartProgress = 0;
        var urls = Urls;
        ProgressRatio = 1.0 / urls.Count;
        var actualDestinations = ActualDestinations;
        for (CurrentIndex = 0; CurrentIndex < urls.Count; ++CurrentIndex)
        {
            if (await base.DownloadAsyncCore(downloadStreamAsync, urls[CurrentIndex], actualDestinations[CurrentIndex]) is { } stream)
            {
                await ManageStreamAsync(stream, actualDestinations[CurrentIndex]);
                if (Dispose)
                    await stream.DisposeAsync();
            }

            StartProgress += 100 * ProgressRatio;
        }
    }

    protected double StartProgress { get; set; }

    protected double ProgressRatio { get; set; }

    public override void Report(double value) => base.Report(StartProgress + ProgressRatio * value);
}
