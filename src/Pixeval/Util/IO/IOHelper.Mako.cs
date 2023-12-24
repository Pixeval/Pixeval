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
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;

namespace Pixeval.Util.IO;

public static partial class IoHelper
{
    public static async Task<Result<Stream>> DownloadStreamResultAsync(
        this MakoClient client,
        string url,
        IProgress<int>? progress = null,
        CancellationHandle? cancellationHandle = null)
    {
        return await client.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsStreamAsync(url, progress, cancellationHandle);
    }

    public static async Task<Result<IRandomAccessStream>> DownloadRandomAccessStreamResultAsync(
        this MakoClient client,
        string url,
        IProgress<int>? progress = null,
        CancellationHandle? cancellationHandle = null)
    {
        return await client.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url, progress, cancellationHandle);
    }

    public static async Task<Result<SoftwareBitmapSource>> DownloadSoftwareBitmapSourceResultAsync(
        this MakoClient client,
        string url,
        IProgress<int>? progress = null,
        CancellationHandle? cancellationHandle = null)
    {
        return await (await client.DownloadRandomAccessStreamResultAsync(url, progress, cancellationHandle)).RewrapAsync(m => m.GetSoftwareBitmapSourceAsync(true));
    }

    public static async Task<Result<ImageSource>> DownloadBitmapImageResultAsync(
        this MakoClient client,
        string url,
        int? desiredWidth,
        IProgress<int>? progress = null,
        CancellationHandle? cancellationHandle = null)
    {
        return await (await client.DownloadRandomAccessStreamResultAsync(url, progress, cancellationHandle))
            .RewrapAsync(async m => (ImageSource)await m.GetBitmapImageAsync(true, desiredWidth));
    }
}
