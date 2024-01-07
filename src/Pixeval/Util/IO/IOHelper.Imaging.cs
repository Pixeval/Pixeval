#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IOHelper.Imaging.cs
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Controls;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using WinUI3Utilities;

namespace Pixeval.Util.IO;

public static partial class IoHelper
{
    public static async Task<MemoryStream> SaveAsPngStreamAsync(this Stream imageStream, bool disposeOfImageStream)
    {
        var image = await Image.LoadAsync<Rgba32>(imageStream);
        var target = new MemoryStream();
        await image.SaveAsBmpAsync(target);
        target.Position = 0;
        return target;
    }

    public static async Task<SoftwareBitmapSource> GetSoftwareBitmapSourceAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream)
    {
        var stream = imageStream.AsStreamForRead();
        // 此处Position可能为负数
        stream.Position = 0;

        var bitmap = await GetSoftwareBitmapFromStreamAsync(stream);
        if (disposeOfImageStream)
            imageStream.Dispose();
        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(bitmap);
        return source;
    }

    public static async Task<MemoryStream> UgoiraSaveToStreamAsync(this IEnumerable<Stream> streams, UgoiraMetadataResponse ugoiraMetadataResponse, UgoiraDownloadFormat? ugoiraDownloadFormat = null)
    {
        return await streams.UgoiraSaveToStreamAsync(ugoiraMetadataResponse.UgoiraMetadataInfo.Frames.Select(t => (int)t.Delay));
    }

    /// <summary>
    ///     Writes the frames that are contained in <paramref name="streams" /> into <see cref="MemoryStream"/>
    ///     and encodes to <paramref name="ugoiraDownloadFormat"/> format
    /// </summary>
    public static async Task<MemoryStream> UgoiraSaveToStreamAsync(this IEnumerable<Stream> streams, IEnumerable<int> delays, IProgress<int>? progress = null, UgoiraDownloadFormat? ugoiraDownloadFormat = null)
    {
        return await Task.Run(async () =>
        {
            var s = streams as IList<Stream> ?? streams.ToArray();
            var average = 50d / s.Count;
            ugoiraDownloadFormat ??= App.AppViewModel.AppSetting.UgoiraDownloadFormat;
            var d = delays as IList<int> ?? delays.ToArray();
            var progressValue = 0d;

            var images = new Image[s.Count];
            await Parallel.ForAsync(0, s.Count, async (i, token) =>
            {
                var delay = d.Count > i ? (uint)d[i] : 10u;
                s[i].Position = 0;
                images[i] = await Image.LoadAsync(s[i], token);
                images[i].Frames[0].Metadata.GetFormatMetadata(WebpFormat.Instance).FrameDelay = delay;
                progressValue += average;
                progress?.Report((int)progressValue);
            });

            var image = images[0];
            foreach (var img in images.Skip(1))
            {
                using (img)
                    _ = image.Frames.AddFrame(img.Frames[0]);
                progressValue += average / 2;
                progress?.Report((int)progressValue);
            }

            var target = new MemoryStream();
            await image.SaveAsync(target,
                ugoiraDownloadFormat switch
                {
                    UgoiraDownloadFormat.Tiff => new TiffEncoder(),
                    UgoiraDownloadFormat.APng => new PngEncoder(),
                    UgoiraDownloadFormat.Gif => new GifEncoder(),
                    UgoiraDownloadFormat.WebPLossless => new WebpEncoder { FileFormat = WebpFileFormatType.Lossless },
                    UgoiraDownloadFormat.WebPLossy => new WebpEncoder { FileFormat = WebpFileFormatType.Lossy },
                    _ => ThrowHelper.ArgumentOutOfRange<UgoiraDownloadFormat?, IImageEncoder>(ugoiraDownloadFormat)
                });
            progress?.Report(100);
            image.Dispose();
            target.Position = 0;
            return target;
        });
    }

    /// <summary>
    ///     Writes the <paramref name="stream" /> into <see cref="InMemoryRandomAccessStream"/>
    ///     and encodes to PNG format
    /// </summary>
    public static async Task<InMemoryRandomAccessStream> SaveToStreamAsync(this Stream stream)
    {
        var image = await Image.LoadAsync<Rgba32>(stream);
        var target = new InMemoryRandomAccessStream();
        await image.SaveAsPngAsync(target.AsStreamForWrite());
        target.Seek(0);
        return target;
    }

    public static string GetUgoiraExtension(UgoiraDownloadFormat? ugoiraDownloadFormat = null)
    {
        ugoiraDownloadFormat ??= App.AppViewModel.AppSetting.UgoiraDownloadFormat;
        return ugoiraDownloadFormat switch
        {
            UgoiraDownloadFormat.Tiff or UgoiraDownloadFormat.APng or UgoiraDownloadFormat.Gif => "." + ugoiraDownloadFormat.ToString()!.ToLower(),
            UgoiraDownloadFormat.WebPLossless or UgoiraDownloadFormat.WebPLossy => ".webp",
            _ => ThrowHelper.ArgumentOutOfRange<UgoiraDownloadFormat?, string>(ugoiraDownloadFormat)
        };
    }

    public static async Task<BitmapImage> GetBitmapImageAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream, int? desiredWidth = null)
    {
        var bitmapImage = new BitmapImage
        {
            DecodePixelType = DecodePixelType.Logical
        };
        if (desiredWidth is { } width)
            bitmapImage.DecodePixelWidth = width;
        await bitmapImage.SetSourceAsync(imageStream);
        if (disposeOfImageStream)
            imageStream.Dispose();

        return bitmapImage;
    }

    /// <summary>
    ///     Decodes the <paramref name="stream" /> to a <see cref="SoftwareBitmap" />
    /// </summary>
    public static async Task<SoftwareBitmap> GetSoftwareBitmapFromStreamAsync(Stream stream)
    {
        using var image = await Image.LoadAsync<Bgra32>(stream);
        var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, image.Width, image.Height, BitmapAlphaMode.Premultiplied);
        var buffer = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(buffer);
        softwareBitmap.CopyFromBuffer(buffer.AsBuffer());
        return softwareBitmap;
        // BitmapDecoder Bug多
        // var decoder = await BitmapDecoder.CreateAsync(imageStream);
        // return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
    }

    public static async Task<MemoryStream> GetStreamFromZipStreamAsync(Stream zipStream, UgoiraMetadataResponse ugoiraMetadataResponse)
    {
        var entryStreams = await ReadZipArchiveEntries(zipStream);
        return await entryStreams.UgoiraSaveToStreamAsync(ugoiraMetadataResponse.UgoiraMetadataInfo.Frames.Select(t => (int)t.Delay));
    }

    public static async Task<IEnumerable<MemoryStream>> GetStreamsFromZipStreamAsync(Stream zipStream)
    {
        var entryStreams = await ReadZipArchiveEntries(zipStream);
        return entryStreams;
    }

    /// <summary>
    /// 根据显示模式获取缩略图所需的Url
    /// </summary>
    /// <param name="illustrationViewOption"></param>
    /// <returns></returns>
    public static ThumbnailUrlOption ToThumbnailUrlOption(this ItemsViewLayoutType illustrationViewOption)
    {
        return illustrationViewOption switch
        {
            ItemsViewLayoutType.LinedFlow or ItemsViewLayoutType.VerticalStack or ItemsViewLayoutType.HorizontalStack => ThumbnailUrlOption.Medium,
            ItemsViewLayoutType.Grid or ItemsViewLayoutType.VerticalUniformStack or ItemsViewLayoutType.HorizontalUniformStack => ThumbnailUrlOption.SquareMedium,
            _ => ThrowHelper.ArgumentOutOfRange<ItemsViewLayoutType, ThumbnailUrlOption>(illustrationViewOption)
        };
    }
}
