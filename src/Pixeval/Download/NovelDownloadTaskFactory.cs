// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.Extensions.DependencyInjection;
using Mako.Model;
using Pixeval.Database.Managers;
using Pixeval.Download.Models;
using Pixeval.Util.IO;

namespace Pixeval.Download;

public class NovelDownloadTaskFactory : IDownloadTaskFactory<Novel, NovelDownloadTaskGroup, NovelContent>
{
    public NovelDownloadTaskGroup Create(Novel context, string rawPath, NovelContent? parameter)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(ArtworkMetaPathParser.Instance.Reduce(rawPath, context));
        // xxx.pdf\.png
        // xxx.pdf\<ext>
        // xxx\novel.txt\.png
        // xxx\novel.md\<ext>
        path += "\\" + IoHelper.GetIllustrationExtension();
        _ = manager.Delete(entry => entry.Destination == path);
        var task = new NovelDownloadTaskGroup(context, path, parameter);
        manager.Insert(task.DatabaseEntry);
        return task;
    }
}
