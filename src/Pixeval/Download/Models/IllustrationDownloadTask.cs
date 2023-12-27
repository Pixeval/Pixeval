#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationDownloadTask.cs
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
using Pixeval.Controls.IllustrationView;
using Pixeval.Database;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Download.Models;

public class IllustrationDownloadTask(DownloadHistoryEntry entry, IllustrationItemViewModel illustration)
    : IllustrationDownloadTaskBase(entry)
{
    public string Url => Urls[0];

    public IllustrationItemViewModel IllustrationViewModel { get; protected set; } = illustration;

    public override async Task DownloadAsync(Func<string, IProgress<double>?, CancellationHandle?, Task<Result<IRandomAccessStream>>> downloadRandomAccessStreamAsync)
    {
        await DownloadAsyncCore(downloadRandomAccessStreamAsync, Url, Destination);
    }

    protected virtual async Task DownloadAsyncCore(Func<string, IProgress<double>?, CancellationHandle?, Task<Result<IRandomAccessStream>>> downloadRandomAccessStreamAsync, string url, string destination)
    {
        if (!App.AppViewModel.AppSetting.OverwriteDownloadedFile && File.Exists(destination))
            return;

        if (App.AppViewModel.AppSetting.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<IRandomAccessStream>(IllustrationViewModel.GetIllustrationOriginalImageCacheKey()) is { } stream)
        {
            using (stream)
                await ManageStream(stream, destination);
            return;
        }

        if (await downloadRandomAccessStreamAsync(url, this, CancellationHandle) is Result<IRandomAccessStream>.Success result)
        {
            using var ras = result.Value;
            await ManageStream(ras, destination);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream">会自动Dispose</param>
    /// <param name="destination"></param>
    /// <returns></returns>
    protected virtual async Task ManageStream(IRandomAccessStream stream, string destination)
    {
        await IoHelper.CreateAndWriteToFileAsync(stream, destination);
    }
}
