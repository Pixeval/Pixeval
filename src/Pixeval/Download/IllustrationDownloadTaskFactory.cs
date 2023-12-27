#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationDownloadTaskFactory.cs
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

using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls.IllustrationView;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Models;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>
{
    public IMetaPathParser<IllustrationItemViewModel> PathParser { get; } = new IllustrationMetaPathParser();

    public async Task<IllustrationDownloadTask> CreateAsync(IllustrationItemViewModel context, string rawPath)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        if (manager.Collection.Exists(entry => entry.Destination == path))
        {
            // delete the original entry
            _ = manager.Delete(entry => entry.Destination == path);
        }

        var task = await Functions.Block<Task<IllustrationDownloadTask>>(async () =>
        {
            if (context.IsUgoira)
            {
                var (metadata, url) = await context.GetUgoiraOriginalUrlAsync();
                var downloadHistoryEntry = new DownloadHistoryEntry(
                    DownloadState.Created,
                    path,
                    DownloadItemType.Ugoira,
                    context.Id,
                    url);
                return new AnimatedIllustrationDownloadTask(downloadHistoryEntry, context, metadata);
            }
            else if (context.MangaIndex is -1 && context.IsManga)
            {
                var downloadHistoryEntry = new DownloadHistoryEntry(
                    DownloadState.Created,
                    path,
                    DownloadItemType.Manga,
                    context.Id,
                    context.GetMangaImageUrls());
                return new MangaDownloadTask(downloadHistoryEntry, context);
            }
            else
            {
                var downloadHistoryEntry = new DownloadHistoryEntry(
                    DownloadState.Created,
                    path,
                    DownloadItemType.Illustration,
                    context.Id,
                    context.OriginalStaticUrl);
                return new IllustrationDownloadTask(downloadHistoryEntry, context);
            }
        });

        manager.Insert(task.DatabaseEntry);
        return task;
    }

    public async Task<IllustrationDownloadTask> TryCreateIntrinsicAsync(IllustrationItemViewModel context, IRandomAccessStream stream, string rawPath)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        if (manager.Collection.Exists(entry => entry.Destination == path))
        {
            // delete the original entry
            _ = manager.Delete(entry => entry.Destination == path);
        }

        var type = context switch
        {
            { IsUgoira: true } => DownloadItemType.Ugoira,
            { IsManga: true, MangaIndex: -1 } => DownloadItemType.Manga, // 未使用的分支
            _ => DownloadItemType.Illustration
        };
        string url;
        if (context.IsUgoira)
        {
            (var metadata, url) = await context.GetUgoiraOriginalUrlAsync();
        }
        else
        {
            url = context.OriginalStaticUrl;
        }
        var entry = new DownloadHistoryEntry(DownloadState.Completed, rawPath, type, context.Id, url);
        return type is DownloadItemType.Manga
            ? new IntrinsicMangaDownloadTask(entry, context, [stream]) // 未使用的分支
            : new IntrinsicIllustrationDownloadTask(entry, context, stream);
    }
}
