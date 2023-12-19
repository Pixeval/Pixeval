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
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Net.Response;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Pixeval.Controls;
using Pixeval.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pixeval.Util.IO;

public static partial class IoHelper
{
    /// <summary>
    ///     Re-encode and decode the image that wrapped in <paramref name="imageStream" />. Note that this function
    ///     is intended to be used when the image is about to be displayed on the screen instead of saving to file
    /// </summary>
    /// <returns></returns>
    public static async Task<SoftwareBitmapSource> EncodeSoftwareBitmapSourceAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream)
    {
        using var ras = await EncodeBitmapStreamAsync(imageStream, disposeOfImageStream);
        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(await GetSoftwareBitmapFromStreamAsync(ras));

        return source;
    }

    public static async Task<IRandomAccessStream> EncodeBitmapStreamAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream)
    {
        var bitmap = await GetSoftwareBitmapFromStreamAsync(imageStream);
        var inMemoryRandomAccessStream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomAccessStream);
        encoder.SetSoftwareBitmap(bitmap);
        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
        await encoder.FlushAsync();
        inMemoryRandomAccessStream.Seek(0);
        if (disposeOfImageStream)
        {
            imageStream.Dispose();
        }

        return inMemoryRandomAccessStream;
    }

    public static async Task<SoftwareBitmapSource> GetSoftwareBitmapSourceAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream)
    {
        var bitmap = await GetSoftwareBitmapFromStreamAsync(imageStream);
        if (disposeOfImageStream)
            imageStream.Dispose();
        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(bitmap);
        return source;
    }

    /// <summary>
    ///     Writes the frames that are contained in <paramref name="frames" /> into <paramref name="target" />
    ///     and encodes to <paramref name="ugoiraDownloadFormat"/> format
    /// </summary>
    /// <returns></returns>
    public static async Task WriteBitmapAsync(IRandomAccessStream target, IEnumerable<(IRandomAccessStream Frame, uint Delay)> frames, UgoiraDownloadFormat ugoiraDownloadFormat)
    {
        var framesArray = frames as (IRandomAccessStream Frame, uint Delay)[] ?? frames.ToArray();

        if (framesArray.Length is 0)
            return;

        Image? image = null;

        foreach (var (frame, delay) in framesArray)
        {
            frame.Seek(0);
            var tempImage = await Image.LoadAsync(frame.AsStream());
            ImageFrame newFrame;
            if (image is null)
            {
                image = tempImage;
                newFrame = image.Frames[0];
            }
            else
            {
                newFrame = image.Frames.AddFrame(tempImage.Frames[0]);
                tempImage.Dispose();
            }
            newFrame.Metadata.GetFormatMetadata(WebpFormat.Instance).FrameDelay = delay;
        }

        await image!.SaveAsync(target.AsStreamForWrite(),
            ugoiraDownloadFormat switch
            {
                UgoiraDownloadFormat.Tiff => new TiffEncoder(),
                UgoiraDownloadFormat.APng => new PngEncoder(),
                UgoiraDownloadFormat.Gif => new GifEncoder(),
                UgoiraDownloadFormat.WebPLossless => new WebpEncoder { FileFormat = WebpFileFormatType.Lossless },
                UgoiraDownloadFormat.WebPLossy => new WebpEncoder { FileFormat = WebpFileFormatType.Lossy },
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<UgoiraDownloadFormat, IImageEncoder>(
                    ugoiraDownloadFormat)
            });
        image.Dispose();
        target.Seek(0);
    }

    public static string GetExtension(this UgoiraDownloadFormat ugoiraDownloadFormat)
    {
        return ugoiraDownloadFormat switch
        {
            UgoiraDownloadFormat.Tiff or UgoiraDownloadFormat.APng or UgoiraDownloadFormat.Gif => "." + ugoiraDownloadFormat.ToString().ToLower(),
            UgoiraDownloadFormat.WebPLossless or UgoiraDownloadFormat.WebPLossy => ".webp",
            _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<UgoiraDownloadFormat, string>(ugoiraDownloadFormat)
        };
    }

    public static async Task<BitmapImage> GetBitmapImageAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream, int? desiredWidth = 0)
    {
        var bitmapImage = new BitmapImage
        {
            DecodePixelType = DecodePixelType.Logical
        };
        if (desiredWidth is { } width)
        {
            bitmapImage.DecodePixelWidth = width;
        }
        await bitmapImage.SetSourceAsync(imageStream);
        if (disposeOfImageStream)
        {
            imageStream.Dispose();
        }

        return bitmapImage;
    }

    /// <summary>
    ///     Decodes the <paramref name="imageStream" /> to a <see cref="SoftwareBitmap" />
    /// </summary>
    public static async Task<SoftwareBitmap> GetSoftwareBitmapFromStreamAsync(IRandomAccessStream imageStream)
    {
        using var image = await Image.LoadAsync<Bgra32>(imageStream.AsStreamForRead());
        var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, image.Width, image.Height, BitmapAlphaMode.Premultiplied);
        var buffer = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(buffer);
        softwareBitmap.CopyFromBuffer(buffer.AsBuffer());
        return softwareBitmap;
        // BitmapDecoder Bug多
        // var decoder = await BitmapDecoder.CreateAsync(imageStream);
        // return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
    }

    public static async Task<IRandomAccessStream> GetStreamFromZipStreamAsync(Stream zipStream, UgoiraMetadataResponse ugoiraMetadataResponse)
    {
        var entryStreams = await ReadZipArchiveEntries(zipStream);
        var frames = ugoiraMetadataResponse.UgoiraMetadataInfo?.Frames?.ToArray();
        var inMemoryRandomAccessStream = new InMemoryRandomAccessStream();
        await WriteBitmapAsync(inMemoryRandomAccessStream, entryStreams.Select((s, i) =>
                (s.content.AsRandomAccessStream(), (uint)(frames?[i]?.Delay ?? 10))),
            App.AppViewModel.AppSetting.UgoiraDownloadFormat);
        return inMemoryRandomAccessStream;
    }

    public static async Task<IEnumerable<IRandomAccessStream>> GetStreamsFromZipStreamAsync(Stream zipStream)
    {
        var entryStreams = await ReadZipArchiveEntries(zipStream);
        return entryStreams.Select(s => s.content.AsRandomAccessStream());
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
            _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ItemsViewLayoutType, ThumbnailUrlOption>(illustrationViewOption)
        };
    }
}
