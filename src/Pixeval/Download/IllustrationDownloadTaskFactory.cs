﻿#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationDownloadTaskFactory.cs
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

using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Options;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IllustrationViewModel, ObservableDownloadTask>
{
    public IllustrationDownloadTaskFactory()
    {
        PathParser = new IllustrationMetaPathParser();
    }

    public IMetaPathParser<IllustrationViewModel> PathParser { get; }

    public async Task<ObservableDownloadTask> CreateAsync(IllustrationViewModel context, string rawPath)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IOHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        if (manager.Collection.Find(entry => entry.Destination == path).Any())
        {
            // delete the original entry
            manager.Delete(entry => entry.Destination == path);
        }

        ObservableDownloadTask task = context.Illustration.IsUgoira() switch
        {
            true => await Functions.Block(async () =>
            {
                var ugoiraMetadata = await App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(context.Id);
                if (ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Medium is { } url)
                {
                    var downloadHistoryEntry = new DownloadHistoryEntry(DownloadState.Created, null, path, DownloadItemType.Ugoira, context.Id, context.Illustration.Title, context.Illustration.User?.Name, url, context.Illustration.GetThumbnailUrl(ThumbnailUrlOption.SquareMedium));
                    return new AnimatedIllustrationDownloadTask(downloadHistoryEntry, context, ugoiraMetadata);
                }

                throw new DownloadTaskInitializationException(DownloadTaskResources.GifSourceUrlNotFoundFormatted.Format(context.Id));
            }),
            false => Functions.Block(() =>
            {
                var downloadHistoryEntry = new DownloadHistoryEntry(DownloadState.Created, null, path, context.IsManga ? DownloadItemType.Manga : DownloadItemType.Illustration, context.Id, context.Illustration.Title, context.Illustration.User?.Name, context.Illustration.GetOriginalUrl()!, context.Illustration.GetThumbnailUrl(ThumbnailUrlOption.SquareMedium));
                return new IllustrationDownloadTask(downloadHistoryEntry, context);
            })
        };

        // TODO Check for unique
        manager.Insert(task.DatabaseEntry);
        return task;
    }

    public Task<ObservableDownloadTask> TryCreateIntrinsicAsync(IllustrationViewModel context, IRandomAccessStream stream, string rawPath)
    {
        var type = context switch
        {
            { IsUgoira: true } => DownloadItemType.Ugoira,
            { IsManga: true } => DownloadItemType.Manga,
            _ => DownloadItemType.Illustration
        };
        var entry = new DownloadHistoryEntry(DownloadState.Completed, null, rawPath, type, context.Id,
            context.Illustration.Title, context.Illustration.User?.Name, null, null);
        return Task.FromResult<ObservableDownloadTask>(new IntrinsicIllustrationDownloadTask(entry, context, stream));
    }
}