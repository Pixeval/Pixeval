#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IDownloadTask.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using Pixeval.Util.Threading;

namespace Pixeval.Download
{
    public interface IDownloadTask
    {
        string? Title { get; }

        string? Description { get; }

        string Url { get; }

        string Destination { get; }

        string? Thumbnail { get; }

        CancellationHandle CancellationHandle { get; set; }

        DownloadState CurrentState { get; set; }

        Exception? ErrorCause { get; set; }

        double ProgressPercentage { get; set; }

        event Action Paused;

        event Action Resumed;

        void OnPaused();

        void OnResumed();
    }

    public static class DownloadTaskHelper
    {
        public static void Reset(this IDownloadTask task)
        {
            task.CancellationHandle.Reset();
            task.ProgressPercentage = 0;
            task.CurrentState = DownloadState.Created;
            task.ErrorCause = null;
        }
    }
}