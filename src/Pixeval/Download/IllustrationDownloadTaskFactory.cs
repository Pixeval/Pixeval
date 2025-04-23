// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.Diagnostics;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Database.Managers;
using Pixeval.Download.Models;
using Pixeval.Util.IO;

namespace Pixeval.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<Illustration, IDownloadTaskGroup, object>
{
    public IDownloadTaskGroup Create(Illustration context, string rawPath, object? parameter = null)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(ArtworkMetaPathParser.Instance.Reduce(rawPath, context));
        _ = manager.Delete(entry => entry.Destination == path);

        IDownloadTaskGroup? task;
        switch (context.ImageType)
        {
            case ImageType.SingleImage:
            case ImageType.SingleAnimatedImage when context.PreferredAnimatedImageType.HasFlag(SingleAnimatedImageType.SingleFile):
            {
                task = new SingleImageDownloadTaskGroup(context, path);
                break;
            }
            case ImageType.ImageSet:
            {
                task = new MangaDownloadTaskGroup(context, path);
                break;
            }
            case ImageType.SingleAnimatedImage:
            {
                task = new UgoiraDownloadTaskGroup(context, path);
                break;
            }
            default:
                return ThrowHelper.ThrowNotSupportedException<IDownloadTaskGroup>();
        }

        manager.Insert(task.DatabaseEntry);
        return task;
    }
}
