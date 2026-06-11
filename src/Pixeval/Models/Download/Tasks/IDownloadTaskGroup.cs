// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Misaki;
using Pixeval.Download;
using Pixeval.Models.Database;

namespace Pixeval.Models.Download.Tasks;

public interface IDownloadTaskGroup : IDownloadTaskGroupBase, IIdentityInfo
{
    string IPlatformInfo.Platform => Pixiv;

    DownloadHistoryEntry DatabaseEntry { get; }
}
