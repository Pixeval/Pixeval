#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AnimatedIllustrationDownloadTask.cs
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.Controls;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Database;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using SixLabors.ImageSharp;

namespace Pixeval.Download.Models;

public partial class UgoiraDownloadTask(
    DownloadHistoryEntry entry,
    IllustrationItemViewModel illustration,
    UgoiraMetadataResponse metadata)
    : MangaDownloadTask(entry, illustration)
{
    protected UgoiraMetadataResponse Metadata { get; set; } = metadata;

    public override IReadOnlyList<string> Urls => IllustrationViewModel.UgoiraOriginalUrls;

    public override IReadOnlyList<string> ActualDestinations => [Destination.RemoveTokens()];

    public override async Task DownloadAsync(Downloader downloadStreamAsync)
    {
        StartProgress = 0;
        var urls = Urls;
        ProgressRatio = 0.9 / urls.Count;
        var actualDestination = ActualDestination;
        var list = new List<Stream>();

        if (!ShouldOverwrite(actualDestination))
            return;

        for (CurrentIndex = 0; CurrentIndex < urls.Count; ++CurrentIndex)
        {
            if (await base.DownloadAsyncCore(downloadStreamAsync, urls[CurrentIndex], null) is { } stream)
                list.Add(stream);

            StartProgress += 100 * ProgressRatio;
        }

        await ManageStreamsAsync(list, urls, actualDestination);
    }

    protected async Task ManageStreamsAsync(IReadOnlyList<Stream> streams, IReadOnlyList<string> urls, string destination)
    {
        if (App.AppViewModel.AppSettings.UgoiraDownloadFormat is UgoiraDownloadFormat.OriginalZip)
        {
            var names = urls.Select(Path.GetExtension).Select((extension, i) => $"{i}{extension}").ToArray();
            var zipStream = await IoHelper.WriteZipAsync(names, streams, Dispose);
            await zipStream.StreamSaveToFileAsync(destination);
            Report(100);
        }
        else
        {
            StartProgress = 90;
            ProgressRatio = .1;
            using var image = await streams.UgoiraSaveToImageAsync(Metadata.Delays, this, Dispose);
            await ManageImageAsync(image, destination);
        }
    }

    protected async Task ManageImageAsync(Image image, string destination)
    {
        image.SetTags(IllustrationViewModel.Entry);
        await image.UgoiraSaveToFileAsync(destination);
    }
}
