// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Mako.Model;

namespace Pixeval.Download.Models;

public interface IImageDownloadTaskGroup : IDownloadTaskGroup
{
    Illustration Entry { get; }
}
