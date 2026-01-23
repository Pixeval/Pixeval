// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Download;

public class DownloadTaskInitializationException : Exception
{
    public DownloadTaskInitializationException()
    {
    }

    public DownloadTaskInitializationException(string? message) : base(message)
    {
    }

    public DownloadTaskInitializationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
