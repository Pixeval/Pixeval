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
using System.Threading.Tasks;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Database;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Download;

public abstract partial class IllustrationDownloadTaskBase(DownloadHistoryEntry entry) : ObservableObject, IDownloadTask, IProgress<double>
{
    private Exception? _errorCause;
    private double _progressPercentage;

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

    [ObservableProperty]
    private bool _selected;

    public DownloadHistoryEntry DatabaseEntry { get; } = entry;

    public long Id => DatabaseEntry.Id;

    public List<string> Urls => DatabaseEntry.Urls;

    public string Destination => DatabaseEntry.Destination;

    public CancellationHandle CancellationHandle { get; set; } = new();

    public TaskCompletionSource Completion { get; } = new();

    public DownloadState CurrentState
    {
        get => DatabaseEntry.State;
        set
        {
            if (value is DownloadState.Completed)
                ProgressPercentage = 100;
            _ = SetProperty(DatabaseEntry.State, value, DatabaseEntry, (entry, state) => entry.State = state);
        }
    }

    public Exception? ErrorCause
    {
        get => _errorCause;
        set => SetProperty(_errorCause, value, DatabaseEntry, (entry, exception) =>
        {
            _errorCause = value;
            entry.ErrorCause = exception?.ToString();
        });
    }

    public abstract Task DownloadAsync(Func<string, IProgress<double>?, CancellationHandle?, Task<Result<IRandomAccessStream>>> downloadRandomAccessStreamAsync);

    public void Report(double value) => ProgressPercentage = value;
}
