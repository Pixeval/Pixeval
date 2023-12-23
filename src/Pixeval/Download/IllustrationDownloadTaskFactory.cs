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

using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls.IllustrationView;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IllustrationItemViewModel, ObservableDownloadTask>
{
    public IMetaPathParser<IllustrationItemViewModel> PathParser { get; } = new IllustrationMetaPathParser();

    public Task<ObservableDownloadTask> CreateAsync(IllustrationItemViewModel context, string rawPath)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        if (manager.Collection.Find(entry => entry.Destination == path).Any())
        {
            // delete the original entry
            _ = manager.Delete(entry => entry.Destination == path);
        }

        var task = Functions.Block<ObservableDownloadTask>(() =>
        {
            if (context.IsUgoira)
            {
                var ugoiraMetadata = App.AppViewModel.MakoClient.GetUgoiraMetadataAsync(context.Id).GetAwaiter().GetResult();
                if (ugoiraMetadata.UgoiraMetadataInfo?.ZipUrls?.Large is not { } url)
                    throw new DownloadTaskInitializationException(
                        DownloadTaskResources.GifSourceUrlNotFoundFormatted.Format(context.Id));

                var downloadHistoryEntry = new DownloadHistoryEntry(DownloadState.Created, null, path,
                    DownloadItemType.Ugoira,
                    context.Id, 
                    url);
                return new AnimatedIllustrationDownloadTask(downloadHistoryEntry, context, ugoiraMetadata);
            }
            else
            {
                var downloadHistoryEntry = new DownloadHistoryEntry(DownloadState.Created, null, path,
                    context.IsManga ? DownloadItemType.Manga : DownloadItemType.Illustration, 
                    context.Id, 
                    context.Illustrate.GetOriginalUrl()!);
                return new IllustrationDownloadTask(downloadHistoryEntry, context);
            }
        });

        manager.Insert(task.DatabaseEntry);
        return Task.FromResult(task);
    }

    public Task<ObservableDownloadTask> TryCreateIntrinsicAsync(IllustrationItemViewModel context, IRandomAccessStream stream, string rawPath)
    {
        var type = context switch
        {
            { IsUgoira: true } => DownloadItemType.Ugoira,
            { IsManga: true } => DownloadItemType.Manga,
            _ => DownloadItemType.Illustration
        };
        var entry = new DownloadHistoryEntry(DownloadState.Completed, null, rawPath, type, context.Id, null);
        return Task.FromResult<ObservableDownloadTask>(new IntrinsicIllustrationDownloadTask(entry, context, stream));
    }
}
