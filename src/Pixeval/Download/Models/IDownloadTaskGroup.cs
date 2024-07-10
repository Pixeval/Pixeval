#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/IDownloadTaskGroup.cs
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
using System.ComponentModel;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.Database;

namespace Pixeval.Download.Models;

public interface IDownloadTaskGroup : IDownloadTaskBase, IIdEntry, INotifyPropertyChanged, INotifyPropertyChanging, IReadOnlyCollection<ImageDownloadTask>, IDisposable
{
    DownloadHistoryEntry DatabaseEntry { get; }

    ValueTask InitializeTaskGroupAsync();

    void SubscribeProgress(ChannelWriter<DownloadToken> writer);

    DownloadToken GetToken();
}

public readonly record struct DownloadToken(IDownloadTaskGroup Task, CancellationToken Token);
