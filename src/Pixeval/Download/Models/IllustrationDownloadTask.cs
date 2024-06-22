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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pixeval.Controls;
using Pixeval.Database;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using SixLabors.ImageSharp;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class IllustrationDownloadTask(DownloadHistoryEntry entry, IllustrationItemViewModel illustration)
    : DownloadTaskBase(entry)
{
    public override IWorkViewModel ViewModel => IllustrationViewModel;

    public override IReadOnlyList<string> ActualDestinations =>
    [
        IoHelper.ReplaceTokenExtensionFromUrl(Destination, IllustrationViewModel.IllustrationOriginalUrl)
    ];

    public override bool IsFolder => false;

    public IllustrationItemViewModel IllustrationViewModel { get; protected set; } = illustration;

    protected bool Dispose { get; set; } = true;

    public override async Task DownloadAsync(Downloader downloadStreamAsync)
    {
        var actualDestination = ActualDestination;
        if (await DownloadAsyncCore(downloadStreamAsync, IllustrationViewModel.IllustrationOriginalUrl, actualDestination) is { } stream)
        {
            await ManageStreamAsync(stream, actualDestination);
            if (Dispose)
                await stream.DisposeAsync();
        }
    }

    protected virtual async Task<Stream?> DownloadAsyncCore(Downloader downloadStreamAsync, string url, string? destination)
    {
        if (destination is not null && !ShouldOverwrite(destination))
            return null;

        if (App.AppViewModel.AppSettings.UseFileCache && await App.AppViewModel.Cache.TryGetAsync<Stream>(MakoHelper.GetOriginalCacheKey(url)) is { } stream)
            return stream;

        if (await downloadStreamAsync(url, this, CancellationHandle) is Result<Stream>.Success success)
            return success.Value;

        return ThrowHelper.Exception<Stream?>($"Error occurred when downloading {url}.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream">会自动Dispose</param>
    /// <param name="destination"></param>
    /// <returns></returns>
    protected async Task ManageStreamAsync(Stream stream, string destination)
    {
        if (App.AppViewModel.AppSettings.IllustrationDownloadFormat is IllustrationDownloadFormat.Original)
        {
            await stream.StreamSaveToFileAsync(destination);
        }
        else
        {
            using var image = await Image.LoadAsync(stream);
            image.SetIdTags(IllustrationViewModel.Entry);
            await image.IllustrationSaveToFileAsync(destination);
        }
    }

    public static bool ShouldOverwrite(string url) => App.AppViewModel.AppSettings.OverwriteDownloadedFile || !File.Exists(url);
}
