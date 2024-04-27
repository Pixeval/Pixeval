#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/IntrinsicMangaDownloadTask.cs
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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pixeval.Controls;
using Pixeval.Database;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public sealed class IntrinsicMangaDownloadTask : MangaDownloadTask
{
    public IntrinsicMangaDownloadTask(DownloadHistoryEntry entry, IllustrationItemViewModel illustrationViewModel, IList<Stream> streams) : base(entry, illustrationViewModel)
    {
        Report(100);
        if (streams.Count == Urls.Length)
            Streams = streams;
        else
            ThrowHelper.Argument(streams);
    }

    public IList<Stream> Streams { get; }

    protected override async Task DownloadAsyncCore(Downloader _, string url, string destination)
    {
        if (!App.AppViewModel.AppSettings.OverwriteDownloadedFile && File.Exists(destination))
            return;

        Streams[CurrentIndex].Position = 0;

        await ManageStream(Streams[CurrentIndex], url, destination);
    }
}
