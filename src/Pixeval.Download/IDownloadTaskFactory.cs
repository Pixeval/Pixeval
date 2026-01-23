// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;

namespace Pixeval.Download;

public interface IDownloadTaskFactory<in TContext, out TDownloadTask, in TParameter> where TContext : IArtworkInfo where TDownloadTask : IDownloadTaskGroupBase
{
    TDownloadTask Create(TContext context, string rawPath, TParameter? parameter);
}
