// Copyright (c) Pixeval.Download.
// Licensed under the GPL-3.0 License.

using System.Net.Http;
using System.Threading.Tasks;

namespace Pixeval.Download;

public interface ISingleDownloadTaskBase : IDownloadTaskBase
{
    Task StartAsync(HttpClient httpClient, bool resumeBreakpoint = false);
}
