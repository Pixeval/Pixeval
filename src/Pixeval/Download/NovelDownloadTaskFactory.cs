// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Mako.Model;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Models;
using Pixeval.Util.IO;

namespace Pixeval.Download;

public class NovelDownloadTaskFactory : IDownloadTaskFactory<NovelItemViewModel, NovelDownloadTaskGroup>
{
    public IMetaPathParser<NovelItemViewModel> PathParser { get; } = new NovelMetaPathParser();

    public NovelDownloadTaskGroup Create(NovelItemViewModel context, string rawPath)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        path += "\\" + IoHelper.GetIllustrationExtension();
        _ = manager.Delete(entry => entry.Destination == path);
        var task = new NovelDownloadTaskGroup(context.Entry, path);
        manager.Insert(task.DatabaseEntry);
        return task;
    }

    public NovelDownloadTaskGroup CreateIntrinsic(NovelItemViewModel context, string rawPath, object param)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        // xxx.pdf\.png
        // xxx.pdf\<ext>
        // xxx\novel.txt\.png
        // xxx\novel.md\<ext>
        path += "\\" + IoHelper.GetIllustrationExtension();
        _ = manager.Delete(entry => entry.Destination == path);
        var content = (NovelContent) param;
        var task = new NovelDownloadTaskGroup(context.Entry, content, path);
        manager.Insert(task.DatabaseEntry);
        return task;
    }
}
