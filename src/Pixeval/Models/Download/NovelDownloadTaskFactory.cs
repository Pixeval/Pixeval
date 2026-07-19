// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Mako.Model;
using Pixeval.Models.Download.Tasks;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download;

public class NovelDownloadTaskFactory : IDownloadTaskFactory<Novel, NovelDownloadTaskGroup, NovelContent>
{
    public NovelDownloadTaskGroup Create(Novel context, string rawPath, NovelContent? parameter) =>
        Create(new ParserContext(context), rawPath, parameter);

    public NovelDownloadTaskGroup Create(ParserContext parserContext, string rawPath, NovelContent? parameter)
    {
        if (parserContext.ArtworkInfo is not Novel context)
            throw new ArgumentException($"The parser context must contain a {nameof(Novel)}.", nameof(parserContext));

        var path = IoHelper.NormalizePath(DownloadPathMacroParser.Reduce(rawPath, parserContext));
        var task = new NovelDownloadTaskGroup(
            context,
            path,
            parameter,
            parserContext.WorkSubscription?.HistoryEntryId);
        return task;
    }
}
