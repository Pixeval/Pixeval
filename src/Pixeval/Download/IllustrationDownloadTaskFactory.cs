// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Models;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IllustrationItemViewModel, IImageDownloadTaskGroup>
{
    public IMetaPathParser<IllustrationItemViewModel> PathParser { get; } = new IllustrationMetaPathParser();

    public IImageDownloadTaskGroup Create(IllustrationItemViewModel context, string rawPath)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        _ = manager.Delete(entry => entry.Destination == path);

        IImageDownloadTaskGroup? task;
        switch (context)
        {
            case { IsUgoira: true }:
            {
                // 外部需要确保UgoiraMetadata已经加载
                var metadata = context.UgoiraMetadata.Result;
                var streams = null as IReadOnlyList<Stream?>;
                if (App.AppViewModel.AppSettings.UseFileCache)
                    streams = context.UgoiraOriginalUrls
                        .Select(url => CacheHelper.TryGetStreamFromCache(MakoHelper.GetOriginalCacheKey(url)))
                        .ToArray();
                task = new UgoiraDownloadTaskGroup(context.Entry, metadata, path, streams);
                break;
            }
            case { IsManga: true, MangaIndex: -1 }:
            {
                var streams = null as IReadOnlyList<Stream?>;
                if (App.AppViewModel.AppSettings.UseFileCache)
                    streams = context.MangaOriginalUrls
                        .Select(url => CacheHelper.TryGetStreamFromCache(MakoHelper.GetOriginalCacheKey(url)))
                        .ToArray();
                task = new MangaDownloadTaskGroup(context.Entry, path, streams);
                break;
            }
            default:
            {
                var stream = null as Stream;
                if (App.AppViewModel.AppSettings.UseFileCache)
                    stream = CacheHelper.TryGetStreamFromCache(
                        MakoHelper.GetOriginalCacheKey(context.IllustrationOriginalUrl));
                task = new SingleImageDownloadTaskGroup(context.Entry, path, stream);
                break;
            }
        }

        manager.Insert(task.DatabaseEntry);
        return task;
    }

    public IImageDownloadTaskGroup CreateIntrinsic(IllustrationItemViewModel context, string rawPath, object param)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        _ = manager.Delete(entry => entry.Destination == path);

        IImageDownloadTaskGroup task;
        switch (context)
        {
            case { IsUgoira: true }:
            {
                var (streams, metadata) = ((IReadOnlyList<Stream>, UgoiraMetadataResponse)) param;
                task = new UgoiraDownloadTaskGroup(context.Entry, metadata, path, streams);
                break;
            }
            case { IsManga: true, MangaIndex: -1 }: // 下载一篇漫画（未使用的分支）
            {
                var streams = (IReadOnlyList<Stream>) param;
                task = new MangaDownloadTaskGroup(context.Entry, path, streams);
                break;
            }
            default:
            {
                var stream = (IReadOnlyList<Stream>) param;
                task = new SingleImageDownloadTaskGroup(context.Entry, path, stream[0]);
                break;
            }
        }

        manager.Insert(task.DatabaseEntry);
        return task;
    }
}
