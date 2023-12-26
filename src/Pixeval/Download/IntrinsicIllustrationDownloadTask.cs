#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IntrinsicIllustrationDownloadTask.cs
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

using Windows.Storage.Streams;
using Pixeval.Controls.IllustrationView;
using Pixeval.Database;
using Pixeval.Utilities.Threading;
using Pixeval.Utilities;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using WinUI3Utilities;

namespace Pixeval.Download;

/// <summary>
/// The disposal of <paramref name="stream" /> is not handled
/// </summary>
public class IntrinsicIllustrationDownloadTask(DownloadHistoryEntry entry, IllustrationItemViewModel illustrationViewModel, IRandomAccessStream stream)
    : IllustrationDownloadTask(entry, illustrationViewModel)
{
    public IRandomAccessStream Stream { get; } = stream;

    protected override async Task DownloadAsyncCore(Func<string, IProgress<double>?, CancellationHandle?, Task<Result<IRandomAccessStream>>> _, string url, string destination)
    {
        if (!App.AppViewModel.AppSetting.OverwriteDownloadedFile && File.Exists(destination))
            return;

        Stream.Seek(0);

        await ManageStream(Stream, destination);
    }
}

public class IntrinsicMangaDownloadTask : MangaDownloadTask
{
    public IList<IRandomAccessStream> Streams { get; }

    public IntrinsicMangaDownloadTask(DownloadHistoryEntry entry, IllustrationItemViewModel illustrationViewModel, IList<IRandomAccessStream> streams) : base(entry, illustrationViewModel)
    {
        if (streams.Count == Urls.Count)
            Streams = streams;
        else
            ThrowHelper.Argument(streams);
    }

    protected override async Task DownloadAsyncCore(Func<string, IProgress<double>?, CancellationHandle?, Task<Result<IRandomAccessStream>>> _, string url, string destination)
    {
        if (!App.AppViewModel.AppSetting.OverwriteDownloadedFile && File.Exists(destination))
            return;

        Streams[CurrentIndex].Seek(0);

        await ManageStream(Streams[CurrentIndex], destination);
    }
}
