#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ObservableDownloadTask.cs
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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Download.Models;

public abstract class IllustrationDownloadTaskBase(DownloadHistoryEntry entry) : ObservableObject, IDownloadTask, IProgress<double>, IIllustrate
{
    private Exception? _errorCause;
    private double _progressPercentage = entry.State is DownloadState.Completed ? 100 : 0;

    /// <summary>
    /// 只有<see cref="CurrentState"/>是<see cref="DownloadState.Running"/>或<see cref="DownloadState.Paused"/>值有效
    /// </summary>
    public double ProgressPercentage
    {
        get => _progressPercentage;
        private set
        {
            if (value.Equals(_progressPercentage))
                return;
            _progressPercentage = value;
            OnPropertyChanged();
        }
    }

    public DownloadHistoryEntry DatabaseEntry { get; } = entry;

    public long Id => DatabaseEntry.Id;

    public DownloadItemType Type => DatabaseEntry.Type;

    public List<string> Urls => DatabaseEntry.Urls;

    public string Destination => DatabaseEntry.Destination;

    public CancellationHandle CancellationHandle { get; set; } = new();

    public TaskCompletionSource Completion { get; private set; } = new();

    public DownloadState CurrentState
    {
        get => DatabaseEntry.State;
        set => SetProperty(DatabaseEntry.State, value, DatabaseEntry, (entry, state) =>
        {
            entry.State = state;
            if (value is DownloadState.Completed)
            {
                ProgressPercentage = 100;
                Completion.SetResult();
            }
        });
    }

    public Exception? ErrorCause
    {
        get => _errorCause;
        set
        {
            if (Equals(value, _errorCause))
                return;
            _errorCause = value;
            OnPropertyChanged();

            DatabaseEntry.ErrorCause = value?.ToString();
            if (value is not null)
            {
                CurrentState = DownloadState.Error;
                Completion.SetException(value);
            }
        }
    }

    public abstract Task DownloadAsync(Func<string, IProgress<double>?, CancellationHandle?, Task<Result<Stream>>> downloadStreamAsync);

    public async Task ResetAsync()
    {
        await IoHelper.DeleteIllustrationTaskAsync(this);
        ProgressPercentage = 0;
        CurrentState = DownloadState.Queued;
        ErrorCause = null;
        Completion = new();
    }

    public void Report(double value) => ProgressPercentage = value;
}
