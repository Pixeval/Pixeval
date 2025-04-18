// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.Diagnostics;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Database.Managers;
using Pixeval.Download.Models;
using Pixeval.Util.IO;

namespace Pixeval.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<Illustration, IImageDownloadTaskGroup, UgoiraMetadata>
{
    public IImageDownloadTaskGroup Create(Illustration context, string rawPath, UgoiraMetadata? parameter)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(ArtworkMetaPathParser.Instance.Reduce(rawPath, context));
        _ = manager.Delete(entry => entry.Destination == path);

        IImageDownloadTaskGroup? task;
        switch (context.ImageType)
        {
            case ImageType.SingleAnimatedImage:
            {
                // 外部需要确保UgoiraMetadata已经加载
                ArgumentNullException.ThrowIfNull(parameter);
                task = new UgoiraDownloadTaskGroup(context, parameter, path);
                break;
            }
            case ImageType.ImageSet:
            {
                task = new MangaDownloadTaskGroup(context, path);
                break;
            }
            case ImageType.SingleImage:
            {
                task = new SingleImageDownloadTaskGroup(context, path);
                break;
            }
            default:
                return ThrowHelper.ThrowNotSupportedException<IImageDownloadTaskGroup>();
        }

        manager.Insert(task.DatabaseEntry);
        return task;
    }
}
