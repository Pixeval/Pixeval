﻿#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ObservableDownloadTask.cs
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
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Database;
using Pixeval.Util.Threading;

namespace Pixeval.Download;

public class ObservableDownloadTask : ObservableObject, IDownloadTask
{
    protected ObservableDownloadTask(DownloadHistoryEntry entry)
    {
        DatabaseEntry = entry;
        CancellationHandle = new CancellationHandle();
        Completion = new TaskCompletionSource();
    }

    public DownloadHistoryEntry DatabaseEntry { get; }

    public string? Id => DatabaseEntry.Id;

    public string? Title => DatabaseEntry.Title;

    public string? Description => DatabaseEntry.Description;

    public string Url => DatabaseEntry.Url!;

    public string Destination => DatabaseEntry.Destination!;

    public string? Thumbnail => DatabaseEntry.Thumbnail;
         
    public CancellationHandle CancellationHandle { get; set; }

    public TaskCompletionSource Completion { get; }

    public DownloadState CurrentState
    {
        get => DatabaseEntry.State;
        set => SetProperty(DatabaseEntry.State, value, DatabaseEntry, (entry, state) => entry.State = state);
    }

    private Exception? _errorCause;

    public Exception? ErrorCause
    {
        get => _errorCause;
        set => SetProperty(_errorCause, value, DatabaseEntry, (entry, exception) =>
        {
            _errorCause = value;
            entry.ErrorCause = exception?.ToString();
        });
    }

    private double _progressPercentage;

    public double ProgressPercentage
    {
        get => _progressPercentage;
        set => SetProperty(ref _progressPercentage, value);
    }

    private bool _selected;

    public bool Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public virtual void DownloadStarting(DownloadStartingEventArgs args)
    {
    }

    protected bool Equals(ObservableDownloadTask other)
    {
        return Url == other.Url;
    }

    public override bool Equals(object? obj)
    {
        return obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((ObservableDownloadTask) obj));
    }

    public override int GetHashCode()
    {
        return Url.GetHashCode();
    }
}