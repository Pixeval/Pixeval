// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Download.MacroParser;
using Pixeval.Download.Models;

namespace Pixeval.Download;

public interface IDownloadTaskFactory<in TContext, out TDownloadTask> where TDownloadTask : IDownloadTaskGroup
{
    IMetaPathParser<TContext> PathParser { get; }

    TDownloadTask Create(TContext context, string rawPath);
}
