#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/ImagePool.Extensions.cs
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
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi;
using Pixeval.Utilities;

namespace Pixeval.Util.IO.Pooling;

public static class ImagePoolExtensions
{
    public static async Task<ImageSource> GetBitmapImageAsync(this ImagePool<IImagePoolKey> imagePool, Stream imageStream,bool disposeOfImageStream, int? desiredWidth = null, string? url = null)
    {
        if (url is { Length: > 0} && imagePool.TryGet(new IImagePoolKey.UrlImageKey(url), out var result))
        {
            return (BitmapImage) result;
        }

        var bitmapImage = new BitmapImage
        {
            DecodePixelType = DecodePixelType.Logical
        };
        if (desiredWidth is { } width)
            bitmapImage.DecodePixelWidth = width;
        await bitmapImage.SetSourceAsync(imageStream.AsRandomAccessStream());
        if (disposeOfImageStream)
            await imageStream.DisposeAsync();

        if (url is { Length: > 0 })
            imagePool.Cache(new IImagePoolKey.UrlImageKey(url), bitmapImage);

        return bitmapImage;
    }

    public static async Task<Result<ImageSource>> DownloadBitmapImageAsync(
        this ImagePool<IImagePoolKey> imagePool,
        MakoClient client,
        string url,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await (await client.DownloadMemoryStreamAsync(url, progress, cancellationToken)).RewrapAsync(m => imagePool.GetBitmapImageAsync(m, true, url: url));
    }

    public static async Task<Result<ImageSource>> DownloadBitmapImageWithDesiredSizeAsync(
        this ImagePool<IImagePoolKey> imagePool,
        MakoClient client,
        string url,
        int? desiredWidth = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await (await client.DownloadMemoryStreamAsync(url, progress, cancellationToken))
            .RewrapAsync(async m => await imagePool.GetBitmapImageAsync(m, true, desiredWidth, url));
    }
}
