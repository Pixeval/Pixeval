// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.IO;
using Mako.Model;
using Pixeval.Download;
using Pixeval.Models.Download.Tasks;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download;

public class NovelDownloadTaskFactory : IDownloadTaskFactory<Novel, NovelDownloadTaskGroup, NovelContent>
{
    public NovelDownloadTaskGroup Create(Novel context, string rawPath, NovelContent? parameter)
    {
        var path = IoHelper.NormalizePath(ArtworkMetaPathParser.Instance.Reduce(rawPath, context));
        // xxx.pdf\.png
        // xxx.pdf\<ext>
        // xxx\novel.txt\.png
        // xxx\novel.md\<ext>
        path = Path.Combine(path, IoHelper.GetIllustrationExtension());
        var task = new NovelDownloadTaskGroup(context, path, parameter);
        return task;
    }
}
