#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/LazyInitializedMangaDownloadTask.cs
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
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Pixeval.Database;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Download.Models;

public class LazyInitializedMangaDownloadTask(DownloadHistoryEntry entry)
    : MangaDownloadTask(entry, null!), ILazyLoadDownloadTask
{
    private readonly long _illustId = entry.Id;

    public override async Task DownloadAsync(
        Func<string, IProgress<double>?, CancellationHandle?, Task<Result<Stream>>> downloadStreamAsync)
    {
        await LazyLoadAsync(_illustId);

        await base.DownloadAsync(downloadStreamAsync);
    }

    public async Task LazyLoadAsync(long id)
    {
        IllustrationViewModel ??= new(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id));
    }
}
