// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Download.Macros;
using Pixeval.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using WinUI3Utilities;
using QRCoder;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Utilities;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Util.IO;

public static partial class IoHelper
{
    public static async Task<ImageSource> DecodeBitmapImageAsync(this Stream imageStream, bool disposeOfImageStream, int? desiredWidth = null)
    {
        var bitmapImage = await imageStream.AsRandomAccessStream().DecodeBitmapImageAsync(false, desiredWidth);
        if (disposeOfImageStream)
            await imageStream.DisposeAsync();

        return bitmapImage;
    }

    public static async Task<ImageSource> DecodeBitmapImageAsync(this IRandomAccessStream imageStream, bool disposeOfImageStream, int? desiredWidth = null)
    {
        var bitmapImage = new BitmapImage { DecodePixelType = DecodePixelType.Logical };
        if (desiredWidth is { } width)
            bitmapImage.DecodePixelWidth = width;
        await bitmapImage.SetSourceAsync(imageStream);
        if (disposeOfImageStream)
            imageStream.Dispose();

        return bitmapImage;
    }

    public static async Task<ImageSource> GetFileThumbnailAsync(string path, uint size = 64)
    {
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, size);
            return await thumbnail.DecodeBitmapImageAsync(true);
        }
        catch
        {
            return await CacheHelper.ImageNotAvailableTask.Value;
        }
    }

    public static async Task UgoiraSaveToFileAsync(this Image image, string path, UgoiraDownloadFormat? ugoiraDownloadFormat = null)
    {
        CreateParentDirectories(path);
        await using var fileStream = OpenAsyncWrite(path);
        _ = await UgoiraSaveToStreamAsync(image, fileStream, ugoiraDownloadFormat);
    }

    /// <summary>
    /// Writes the frames that are contained in <paramref name="streams" /> into <see cref="Stream"/> and encodes
    /// </summary>
    public static async Task<Stream> UgoiraSaveToStreamAsync(this IReadOnlyList<Stream> streams, IReadOnlyList<int> delays, Stream? target = null, IProgress<double>? progress = null)
    {
        using var image = await streams.UgoiraSaveToImageAsync(delays, progress);
        var s = await image.UgoiraSaveToStreamAsync(target ?? Streams.RentStream());
        progress?.Report(100);
        return s;
    }
    
    public static async Task<Stream> UgoiraSaveToStreamAsync(this Stream stream, IReadOnlyList<int> delays, Stream? target = null, IProgress<double>? progress = null)
    {
        using var image = await stream.UgoiraSaveToImageAsync(delays, progress);
        var s = await image.UgoiraSaveToStreamAsync(target ?? Streams.RentStream());
        progress?.Report(100);
        return s;
    }

    public static IImageEncoder GetUgoiraEncoder(UgoiraDownloadFormat? ugoiraDownloadFormat = null)
    {
        ugoiraDownloadFormat ??= App.AppViewModel.AppSettings.UgoiraDownloadFormat;
        return ugoiraDownloadFormat switch
        {
            UgoiraDownloadFormat.Tiff => new TiffEncoder(),
            UgoiraDownloadFormat.APng => new PngEncoder(),
            UgoiraDownloadFormat.Gif => new GifEncoder(),
            UgoiraDownloadFormat.WebPLossless => new WebpEncoder { FileFormat = WebpFileFormatType.Lossless },
            UgoiraDownloadFormat.WebPLossy => new WebpEncoder { FileFormat = WebpFileFormatType.Lossy },
            _ => ThrowHelper.ArgumentOutOfRange<UgoiraDownloadFormat?, IImageEncoder>(ugoiraDownloadFormat)
        };
    }

    public static async Task<T> UgoiraSaveToStreamAsync<T>(this Image image, T destination, UgoiraDownloadFormat? ugoiraDownloadFormat = null) where T : Stream
    {
        return await Task.Run(async () =>
        {
            await image.SaveAsync(destination, GetUgoiraEncoder(ugoiraDownloadFormat));
            image.Dispose();
            destination.Position = 0;
            return destination;
        });
    }

    public static async Task<Image> UgoiraSaveToImageAsync(this Stream stream, IReadOnlyList<int> delays, IProgress<double>? progress = null, bool dispose = false)
    {
        var streams = await Streams.ReadZipAsync(stream, dispose);
        var average = 50d / streams.Count;
        var progressValue = 0d;

        var images = new Image[streams.Count];
        var options = new ParallelOptions();
        if (progress is not null)
            options.TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        await Parallel.ForAsync(0, streams.Count, options, async (i, token) =>
        {
            var delay = delays.Count > i ? (uint)delays[i] : 10u;
            streams[i].Position = 0;
            images[i] = await Image.LoadAsync(streams[i], token);
            await streams[i].DisposeAsync();
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
            progress?.Report((int) progressValue);
        }

        return image;
    }

    public static async Task<Image> UgoiraSaveToImageAsync(this IReadOnlyList<Stream> streams, IReadOnlyList<int> delays, IProgress<double>? progress = null, bool dispose = false)
    {
        var average = 50d / streams.Count;
        var progressValue = 0d;

        var images = new Image[streams.Count];
        var options = new ParallelOptions();
        // TODO: 能否用DispatcherQueue实现提示进度？
        if (progress is not null)
            options.TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        await Parallel.ForAsync(0, streams.Count, options, async (i, token) =>
        {
            var delay = delays.Count > i ? (uint)delays[i] : 10u;
            streams[i].Position = 0;
            images[i] = await Image.LoadAsync(streams[i], token);
            if (dispose)
                await streams[i].DisposeAsync();
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
            progress?.Report((int) progressValue);
        }

        return image;
    }

    public static async Task<Image> UgoiraSaveToImageAsync(this IEnumerable<string> files, IReadOnlyList<int> delays)
    {
        var image = null as Image;
        var i = 0;
        foreach (var file in files)
        {
            var delay = delays.Count > i ? (uint)delays[i] : 10u;
            if (image is null)
                image = await Image.LoadAsync(file);
            else
            {
                using var img = await Image.LoadAsync(file);
                img.Frames[0].Metadata.GetFormatMetadata(WebpFormat.Instance).FrameDelay = delay;
                img.Frames[0].Metadata.GetFormatMetadata(PngFormat.Instance).FrameDelay = new Rational(delay, 10);
                img.Frames[0].Metadata.GetFormatMetadata(GifFormat.Instance).FrameDelay = (int) (delay / 10);
                _ = image.Frames.AddFrame(img.Frames[0]);
            }

            ++i;
        }
        return image!;
    }

    public static async Task IllustrationSaveToFileAsync(this Image image, string path, IllustrationDownloadFormat? illustrationDownloadFormat = null)
    {
        CreateParentDirectories(path);
        await using var fileStream = OpenAsyncWrite(path);
        _ = await IllustrationSaveToStreamAsync(image, fileStream, illustrationDownloadFormat);
    }

    public static async Task StreamSaveToFileAsync(this Stream stream, string path)
    {
        CreateParentDirectories(path);
        await using var fileStream = OpenAsyncWrite(path);
        await stream.CopyToAsync(fileStream);
    }

    public static async Task StreamsCompressSaveToFileAsync(this Stream stream, string path)
    {
        CreateParentDirectories(path);
        await using var fileStream = OpenAsyncWrite(path);
        await stream.CopyToAsync(fileStream);
    }

    public static IImageEncoder GetIllustrationEncoder(IllustrationDownloadFormat? illustrationDownloadFormat = null)
    {
        illustrationDownloadFormat ??= App.AppViewModel.AppSettings.IllustrationDownloadFormat;
        return illustrationDownloadFormat switch
        {
            IllustrationDownloadFormat.Jpg => new JpegEncoder(),
            IllustrationDownloadFormat.Png => new PngEncoder(),
            IllustrationDownloadFormat.Bmp => new BmpEncoder(),
            IllustrationDownloadFormat.WebPLossless => new WebpEncoder { FileFormat = WebpFileFormatType.Lossless },
            IllustrationDownloadFormat.WebPLossy => new WebpEncoder { FileFormat = WebpFileFormatType.Lossy },
            _ => ThrowHelper.ArgumentOutOfRange<IllustrationDownloadFormat?, IImageEncoder>(illustrationDownloadFormat)
        };
    }

    public static async Task<T> IllustrationSaveToStreamAsync<T>(this Image image, T destination, IllustrationDownloadFormat? illustrationDownloadFormat = null) where T : Stream
    {
        return await Task.Run(async () =>
        {
            await image.SaveAsync(destination, GetIllustrationEncoder(illustrationDownloadFormat));
            image.Dispose();
            destination.Position = 0;
            return destination;
        });
    }

    public static string GetUgoiraExtension(UgoiraDownloadFormat? ugoiraDownloadFormat = null)
    {
        ugoiraDownloadFormat ??= App.AppViewModel.AppSettings.UgoiraDownloadFormat;
        return ugoiraDownloadFormat switch
        {
            UgoiraDownloadFormat.OriginalZip => ".zip",
            UgoiraDownloadFormat.Tiff or UgoiraDownloadFormat.APng or UgoiraDownloadFormat.Gif => "." + ugoiraDownloadFormat.ToString()!.ToLower(),
            UgoiraDownloadFormat.WebPLossless or UgoiraDownloadFormat.WebPLossy => ".webp",
            _ => ThrowHelper.ArgumentOutOfRange<UgoiraDownloadFormat?, string>(ugoiraDownloadFormat)
        };
    }

    public static UgoiraDownloadFormat GetUgoiraFormat(string extension)
    {
        return extension switch
        {
            ".zip" => UgoiraDownloadFormat.OriginalZip,
            ".tiff" => UgoiraDownloadFormat.Tiff,
            ".apng" => UgoiraDownloadFormat.APng,
            ".gif" => UgoiraDownloadFormat.Gif,
            ".webp" => UgoiraDownloadFormat.WebPLossless,
            _ => ThrowHelper.ArgumentOutOfRange<string, UgoiraDownloadFormat>(extension)
        };
    }

    public static string GetIllustrationExtension(IllustrationDownloadFormat? illustrationDownloadFormat = null)
    {
        illustrationDownloadFormat ??= App.AppViewModel.AppSettings.IllustrationDownloadFormat;
        return illustrationDownloadFormat switch
        {
            IllustrationDownloadFormat.Original => FileExtensionMacro.NameConstToken,
            IllustrationDownloadFormat.Jpg or IllustrationDownloadFormat.Png or IllustrationDownloadFormat.Bmp => "." + illustrationDownloadFormat.ToString()!.ToLower(),
            IllustrationDownloadFormat.WebPLossless or IllustrationDownloadFormat.WebPLossy => ".webp",
            _ => ThrowHelper.ArgumentOutOfRange<IllustrationDownloadFormat?, string>(illustrationDownloadFormat)
        };
    }

    public static IllustrationDownloadFormat GetIllustrationFormat(string extension)
    {
        return extension switch
        {
            ".jpg" => IllustrationDownloadFormat.Jpg,
            ".png" => IllustrationDownloadFormat.Png,
            ".bmp" => IllustrationDownloadFormat.Bmp,
            ".webp" => IllustrationDownloadFormat.WebPLossless,
            FileExtensionMacro.NameConstToken => IllustrationDownloadFormat.Original,
            _ => ThrowHelper.ArgumentOutOfRange<string, IllustrationDownloadFormat>(extension)
        };
    }

    public static string GetNovelExtension(NovelDownloadFormat? novelDownloadFormat = null)
    {
        novelDownloadFormat ??= App.AppViewModel.AppSettings.NovelDownloadFormat;
        return novelDownloadFormat switch
        {
            NovelDownloadFormat.OriginalTxt => "novel.txt",
            NovelDownloadFormat.Pdf => "." + novelDownloadFormat.ToString()!.ToLower(),
            NovelDownloadFormat.Html or NovelDownloadFormat.Md => "\\novel." + novelDownloadFormat.ToString()!.ToLower(),
            _ => ThrowHelper.ArgumentOutOfRange<NovelDownloadFormat?, string>(novelDownloadFormat)
        };
    }

    public static NovelDownloadFormat GetNovelFormat(string extension)
    {
        return extension switch
        {
            ".txt" => NovelDownloadFormat.OriginalTxt,
            ".pdf" => NovelDownloadFormat.Pdf,
            ".html" => NovelDownloadFormat.Html,
            ".md" => NovelDownloadFormat.Md,
            _ => ThrowHelper.ArgumentOutOfRange<string, NovelDownloadFormat>(extension)
        };
    }

    public static async Task<Image> GetImageFromZipStreamAsync(Stream zipStream, UgoiraMetadataResponse ugoiraMetadataResponse)
    {
        var entryStreams = await Streams.ReadZipAsync(zipStream, true);
        return await entryStreams.UgoiraSaveToImageAsync(ugoiraMetadataResponse.Delays.ToArray());
    }

    public static async Task<ImageSource> GenerateQrCodeForUrlAsync(string url)
    {
        var qrCodeGen = new QRCodeGenerator();
        var urlPayload = new PayloadGenerator.Url(url);
        var qrCodeData = qrCodeGen.CreateQrCode(urlPayload, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new BitmapByteQRCode(qrCodeData);
        var bytes = qrCode.GetGraphic(20);
        return await Streams.RentStream(bytes).DecodeBitmapImageAsync(true);
    }

    public static async Task<ImageSource> GenerateQrCodeAsync(string content)
    {
        var qrCodeGen = new QRCodeGenerator();
        var qrCodeData = qrCodeGen.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new BitmapByteQRCode(qrCodeData);
        var bytes = qrCode.GetGraphic(20);
        return await Streams.RentStream(bytes).DecodeBitmapImageAsync(true);
    }

    public static string ChangeExtensionFromUrl(string path, string url)
    {
        var index = url.LastIndexOf('.');
        return Path.ChangeExtension(path, url[index..]);
    }

    public static string ReplaceTokenExtensionFromUrl(string path, string url)
    {
        var index = url.LastIndexOf('.');
        return path.Replace(FileExtensionMacro.NameConstToken, url[index..]);
    }
}
