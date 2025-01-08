// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Download.Models;

public interface IDownloadTaskBase
{
    /// <summary>
    /// 只有<see cref="CurrentState"/>是<see cref="DownloadState.Running"/>或<see cref="DownloadState.Paused"/>值有效
    /// </summary>
    double ProgressPercentage { get; }

    DownloadState CurrentState { get; }

    string Destination { get; }

    Exception? ErrorCause { get; }

    string OpenLocalDestination { get; }

    bool IsProcessing { get; }

    void TryReset();

    void Pause();

    void TryResume();

    void Cancel();

    void Delete();
}
