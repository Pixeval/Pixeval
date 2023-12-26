#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/LazyInitializedIllustrationDownloadTask.cs
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
using Pixeval.Database;
using Pixeval.Utilities.Threading;
using Pixeval.Utilities;
using Windows.Storage.Streams;

namespace Pixeval.Download;

public class LazyInitializedSingleIllustrationDownloadTask(DownloadHistoryEntry entry)
    : IllustrationDownloadTask(entry, null!), ILazyLoadDownloadTask
{
    private readonly long _illustId = entry.Id;

    public override async Task DownloadAsync(Func<string, IProgress<double>?, CancellationHandle?, Task<Result<IRandomAccessStream>>> downloadRandomAccessStreamAsync)
    {
        await LazyLoadAsync(_illustId);

        await base.DownloadAsync(downloadRandomAccessStreamAsync);
    }

    public async Task LazyLoadAsync(long id)
    {
        IllustrationViewModel ??= new(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id));
    }
}
