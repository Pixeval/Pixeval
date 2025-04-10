// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Mako.Net.Response;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Models;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IllustrationItemViewModel, IImageDownloadTaskGroup>
{
    public IMetaPathParser<IllustrationItemViewModel> PathParser => IllustrationMetaPathParser.Instance;

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
                task = new UgoiraDownloadTaskGroup(context.Entry, metadata, path);
                break;
            }
            case { IsManga: true, MangaIndex: -1 }:
            {
                task = new MangaDownloadTaskGroup(context.Entry, path);
                break;
            }
            default:
            {
                var stream = null as Stream;
                if (App.AppViewModel.AppSettings.UseFileCache)
                    stream = CacheHelper.TryGetStreamFromCache(context.IllustrationOriginalUrl);
                task = new SingleImageDownloadTaskGroup(context.Entry, path, stream);
                break;
            }
        }

        manager.Insert(task.DatabaseEntry);
        return task;
    }
}
