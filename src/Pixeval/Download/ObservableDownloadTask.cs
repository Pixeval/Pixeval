#region Copyright (c) Pixeval/Pixeval
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
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Util.Threading;

namespace Pixeval.Download
{
    public class ObservableDownloadTask : ObservableObject, IDownloadTask
    {
        public ObservableDownloadTask(
            string? title,
            string? description, 
            string url, 
            string destination,
            string? thumbnail)
        {
            Title = title;
            Description = description;
            Url = url;
            Destination = destination;
            Thumbnail = thumbnail;
            CancellationHandle = new CancellationHandle();
            CurrentState = DownloadState.Created;
        }

        public string? Title { get; }

        public string? Description { get; }

        public string Url { get; }

        public string Destination { get; }

        public string? Thumbnail { get; }

        public CancellationHandle CancellationHandle { get; set; }

        private DownloadState _currentState;

        public DownloadState CurrentState
        {
            get => _currentState;
            set => SetProperty(ref _currentState, value);
        }

        private Exception? _errorCause;

        public Exception? ErrorCause
        {
            get => _errorCause;
            set => SetProperty(ref _errorCause, value);
        }

        private double _progressPercentage;

        public double ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref _progressPercentage, value);
        }

        public event Action? Paused;

        public event Action? Resumed;

        public void OnPaused()
        {
            Paused?.Invoke();
        }

        public void OnResumed()
        {
            Resumed?.Invoke();
        }

        protected bool Equals(ObservableDownloadTask other)
        {
            return Url == other.Url;
        }

        public override bool Equals(object? obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((ObservableDownloadTask) obj));
        }

        public override int GetHashCode()
        {
            return Url.GetHashCode();
        }
    }
}