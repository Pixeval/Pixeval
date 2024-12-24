#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IOHelper.Mako.cs
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
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Net;
using Pixeval.Util.IO.Pooling;
using Pixeval.Utilities;

namespace Pixeval.Util.IO;

public static partial class IoHelper
{
    public static Task<Result<Stream>> DownloadMemoryStreamAsync(
        this MakoClient client,
        string url,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return client.GetMakoHttpClient(MakoApiKind.ImageApi)
            .DownloadMemoryStreamAsync(url, cancellationToken, progress);
    }

    public static async Task<Result<IRandomAccessStream>> DownloadRandomAccessStreamAsync(
        this MakoClient client,
        string url,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return (await client.DownloadMemoryStreamAsync(url, progress, cancellationToken)).Rewrap(stream => stream.AsRandomAccessStream());
    }

    public static Task<Result<ImageSource>> DownloadBitmapImageAsync(
        this MakoClient client,
        string url,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return _imagePool.DownloadBitmapImageAsync(client, url, progress, cancellationToken);
    }

    public static Task<Result<ImageSource>> DownloadBitmapImageWithDesiredSizeAsync(
        this MakoClient client,
        string url,
        int? desiredWidth = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return _imagePool.DownloadBitmapImageWithDesiredSizeAsync(client, url, desiredWidth, progress, cancellationToken);
    }
}
