// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Download.Macros;
using Pixeval.Models.Download;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;

namespace Pixeval.Utilities.IO;

public static partial class IoHelper
{
    public const string PixevalTempExtension = ".pixevaldownloading";

    extension(Image image)
    {
        public async Task UgoiraSaveToFileAsync(string path, UgoiraDownloadFormat? ugoiraDownloadFormat = null)
        {
            await using var fileStream = FileHelper.CreateAsyncWriteCreateParent(path);
            _ = await image.UgoiraSaveToStreamAsync(fileStream, ugoiraDownloadFormat);
        }

        public async Task<T> UgoiraSaveToStreamAsync<T>(T destination, UgoiraDownloadFormat? ugoiraDownloadFormat = null) where T : Stream
        {
            return await Task.Run(async () =>
            {
                await image.SaveAsync(destination, GetUgoiraEncoder(ugoiraDownloadFormat));
                image.Dispose();
                destination.Position = 0;
                return destination;
            });
        }

        public async Task IllustrationSaveToFileAsync(string path, IllustrationDownloadFormat? illustrationDownloadFormat = null)
        {
            await using var fileStream = FileHelper.CreateAsyncWriteCreateParent(path);
            _ = await image.IllustrationSaveToStreamAsync(fileStream, illustrationDownloadFormat);
        }

        public async Task<T> IllustrationSaveToStreamAsync<T>(T destination, IllustrationDownloadFormat? illustrationDownloadFormat = null) where T : Stream
        {
            return await Task.Run(async () =>
            {
                await image.SaveAsync(destination, GetIllustrationEncoder(illustrationDownloadFormat));
                image.Dispose();
                destination.Position = 0;
                return destination;
            });
        }
    }

    extension(IReadOnlyList<Stream> streams)
    {
        /// <summary>
        /// Writes the frames that are contained in <paramref name="streams" /> into <see cref="Stream"/> and encodes
        /// </summary>
        public async Task<Stream> UgoiraSaveToStreamAsync(IReadOnlyList<int> delays, Stream? target = null, IProgress<double>? progress = null, UgoiraDownloadFormat? ugoiraDownloadFormat = null)
        {
            using var image = await streams.UgoiraSaveToImageAsync(delays, progress);
            var s = await image.UgoiraSaveToStreamAsync(target ?? Streams.RentStream(), ugoiraDownloadFormat);
            progress?.Report(100);
            return s;
        }

        public async Task<Image> UgoiraSaveToImageAsync(IReadOnlyList<int> delays, IProgress<double>? progress = null, bool dispose = false)
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
                var delay = delays.Count > i ? (uint) delays[i] : 10u;
                streams[i].Position = 0;
                images[i] = await Image.LoadAsync(streams[i], token);
                if (dispose)
                    await streams[i].DisposeAsync();
                images[i].Frames[0].Metadata.GetFormatMetadata(WebpFormat.Instance).FrameDelay = delay;
                progressValue += average;
                progress?.Report((int) progressValue);
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
    }

    public static IImageEncoder GetUgoiraEncoder(UgoiraDownloadFormat? ugoiraDownloadFormat = null)
    {
        ugoiraDownloadFormat ??= GetAvailableUgoiraDownloadFormatToken().BuiltInFormat ?? UgoiraDownloadFormatToken.DefaultBuiltInFormat;
        return ugoiraDownloadFormat switch
        {
            UgoiraDownloadFormat.Tiff => new TiffEncoder(),
            UgoiraDownloadFormat.APng => new PngEncoder(),
            UgoiraDownloadFormat.Gif => new GifEncoder(),
            UgoiraDownloadFormat.WebPLossless => new WebpEncoder { FileFormat = WebpFileFormatType.Lossless },
            UgoiraDownloadFormat.WebPLossy => new WebpEncoder { FileFormat = WebpFileFormatType.Lossy },
            _ => throw new ArgumentOutOfRangeException(nameof(ugoiraDownloadFormat))
        };
    }

    extension(IEnumerable<string> files)
    {
        public async Task UgoiraSaveToFileAsync(IReadOnlyList<int> delays, string path, UgoiraDownloadFormat? ugoiraDownloadFormat = null)
        {
            using var image = await files.UgoiraSaveToImageAsync(delays);
            await image.UgoiraSaveToFileAsync(path, ugoiraDownloadFormat);
        }

        public async Task<Image> UgoiraSaveToImageAsync(IReadOnlyList<int> delays)
        {
            Image? image = null;
            var i = 0;
            foreach (var file in files)
            {
                var delay = delays.Count > i ? (uint) delays[i] : 10u;
                var img = await Image.LoadAsync(file);
                var rootFrame = img.Frames[0];
                var webpFrameMetadata = rootFrame.Metadata.GetWebpMetadata();
                webpFrameMetadata.FrameDelay = delay;
                webpFrameMetadata.BlendMethod = WebpBlendMethod.Source;
                if (image is null)
                {
                    image = img;
                    image.Metadata.GetWebpMetadata().RepeatCount = 0;
                }
                else
                {
                    _ = image.Frames.AddFrame(img.Frames[0]);
                    img.Dispose();
                }

                ++i;
            }
            return image!;
        }
    }

    extension(Stream stream)
    {
        public async Task StreamSaveToFileAsync(string path)
        {
            await using var fileStream = FileHelper.CreateAsyncWriteCreateParent(path);
            await stream.CopyToAsync(fileStream);
        }

        public async Task StreamsCompressSaveToFileAsync(string path)
        {
            await using var fileStream = FileHelper.CreateAsyncWriteCreateParent(path);
            await stream.CopyToAsync(fileStream);
        }

        public async Task<Bitmap> DecodeBitmapImageAsync(bool disposeOfImageStream, int? desiredWidth = null)
        {
            var bitmapImage = await Task.Run(() => desiredWidth is { } w ? Bitmap.DecodeToWidth(stream, w) : new(stream));
            if (disposeOfImageStream)
                await stream.DisposeAsync();
            return bitmapImage;
        }

        public async Task<Stream> UgoiraSaveToStreamAsync(IReadOnlyList<int> delays, Stream? target = null, IProgress<double>? progress = null, UgoiraDownloadFormat? ugoiraDownloadFormat = null)
        {
            using var image = await stream.UgoiraSaveToImageAsync(delays, progress);
            var s = await image.UgoiraSaveToStreamAsync(target ?? Streams.RentStream(), ugoiraDownloadFormat);
            progress?.Report(100);
            return s;
        }

        public async Task<Image> UgoiraSaveToImageAsync(IReadOnlyList<int> delays, IProgress<double>? progress = null, bool dispose = false)
        {
            var streams = await Streams.ReadZipAsync(stream, dispose);
            return await streams.UgoiraSaveToImageAsync(delays, progress, true);
        }

        public async Task UgoiraSaveToFileAsync(IReadOnlyList<int> delays, string path, UgoiraDownloadFormat? ugoiraDownloadFormat = null, IProgress<double>? progress = null, bool dispose = false)
        {
            var streams = await Streams.ReadZipAsync(stream, dispose);
            using var image = await streams.UgoiraSaveToImageAsync(delays, progress, true);
            await image.UgoiraSaveToFileAsync(path, ugoiraDownloadFormat);
        }
    }

    public static IImageEncoder GetIllustrationEncoder(IllustrationDownloadFormat? illustrationDownloadFormat = null)
    {
        illustrationDownloadFormat ??= GetAvailableIllustrationDownloadFormatToken().BuiltInFormat ?? IllustrationDownloadFormatToken.DefaultBuiltInFormat;
        var quality = App.AppViewModel.AppSettings.LossyImageDownloadQuality;
        return illustrationDownloadFormat switch
        {
            IllustrationDownloadFormat.Jpg => new JpegEncoder
            {
                Quality = quality switch
                {
                    -1 => null,
                    0 => 1,
                    _ => quality
                }
            },
            IllustrationDownloadFormat.Png => new PngEncoder(),
            IllustrationDownloadFormat.Bmp => new BmpEncoder(),
            IllustrationDownloadFormat.WebPLossless => new WebpEncoder
            {
                FileFormat = WebpFileFormatType.Lossless,
                Quality = quality is -1 ? 75 : quality
            },
            IllustrationDownloadFormat.WebPLossy => new WebpEncoder
            {
                FileFormat = WebpFileFormatType.Lossy,
                Quality = quality is -1 ? 75 : quality
            },
            _ => throw new ArgumentOutOfRangeException(nameof(illustrationDownloadFormat))
        };
    }

    public static UgoiraDownloadFormatToken GetAvailableUgoiraDownloadFormatToken(string? ugoiraDownloadFormat = null)
    {
        ugoiraDownloadFormat ??= App.AppViewModel.AppSettings.UgoiraDownloadFormat;
        var token = new UgoiraDownloadFormatToken(ugoiraDownloadFormat);
        if (token.BuiltInFormat is not null)
            return token;

        if (token.ExtensionFormatExtension is { } extension
            && App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>().GetAnimatedImageFormatProvider(extension) is not null)
            return token;

        return UgoiraDownloadFormatToken.Default;
    }

    public static string GetUgoiraExtension(string? ugoiraDownloadFormat = null)
    {
        var token = GetAvailableUgoiraDownloadFormatToken(ugoiraDownloadFormat);
        if (token.ExtensionFormatExtension is { } extension)
            return extension;

        return GetUgoiraBuiltInExtension(token.BuiltInFormat ?? UgoiraDownloadFormatToken.DefaultBuiltInFormat);
    }

    private static string GetUgoiraBuiltInExtension(UgoiraDownloadFormat ugoiraDownloadFormat)
    {
        return ugoiraDownloadFormat switch
        {
            UgoiraDownloadFormat.Original => FileExtensionMacro.NameConstToken,
            UgoiraDownloadFormat.Tiff or UgoiraDownloadFormat.APng or UgoiraDownloadFormat.Gif => "." + ugoiraDownloadFormat.ToString()!.ToLowerInvariant(),
            UgoiraDownloadFormat.WebPLossless or UgoiraDownloadFormat.WebPLossy => ".webp",
            _ => throw new ArgumentOutOfRangeException(nameof(ugoiraDownloadFormat))
        };
    }

    public static UgoiraDownloadFormat GetUgoiraFormat(string extension)
    {
        if (TryGetUgoiraFormat(extension, out var format))
            return format;

        throw new ArgumentOutOfRangeException(nameof(extension));
    }

    public static bool TryGetUgoiraFormat(string extension, out UgoiraDownloadFormat format)
    {
        return extension switch
        {
            FileExtensionMacro.NameConstToken => Assign(UgoiraDownloadFormat.Original, out format),
            ".tiff" => Assign(UgoiraDownloadFormat.Tiff, out format),
            ".apng" => Assign(UgoiraDownloadFormat.APng, out format),
            ".gif" => Assign(UgoiraDownloadFormat.Gif, out format),
            ".webp" => Assign(UgoiraDownloadFormat.WebPLossless, out format),
            _ => Assign(default, out format, false)
        };

        static bool Assign(UgoiraDownloadFormat value, out UgoiraDownloadFormat format, bool result = true)
        {
            format = value;
            return result;
        }
    }

    public static IllustrationDownloadFormatToken GetAvailableIllustrationDownloadFormatToken(string? illustrationDownloadFormat = null)
    {
        illustrationDownloadFormat ??= App.AppViewModel.AppSettings.IllustrationDownloadFormat;
        var token = new IllustrationDownloadFormatToken(illustrationDownloadFormat);
        if (token.BuiltInFormat is not null)
            return token;

        if (token.ExtensionFormatExtension is { } extension
            && App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>().GetStaticImageFormatProvider(extension) is not null)
            return token;

        return IllustrationDownloadFormatToken.Default;
    }

    public static string GetIllustrationExtension(string? illustrationDownloadFormat = null)
    {
        var token = GetAvailableIllustrationDownloadFormatToken(illustrationDownloadFormat);
        if (token.ExtensionFormatExtension is { } extension)
            return extension;

        return GetIllustrationBuiltInExtension(token.BuiltInFormat ?? IllustrationDownloadFormatToken.DefaultBuiltInFormat);
    }

    private static string GetIllustrationBuiltInExtension(IllustrationDownloadFormat illustrationDownloadFormat)
    {
        return illustrationDownloadFormat switch
        {
            IllustrationDownloadFormat.Original => FileExtensionMacro.NameConstToken,
            IllustrationDownloadFormat.Jpg or IllustrationDownloadFormat.Png or IllustrationDownloadFormat.Bmp => "." + illustrationDownloadFormat.ToString()!.ToLowerInvariant(),
            IllustrationDownloadFormat.WebPLossless or IllustrationDownloadFormat.WebPLossy => ".webp",
            _ => throw new ArgumentOutOfRangeException(nameof(illustrationDownloadFormat))
        };
    }

    public static IllustrationDownloadFormat GetIllustrationFormat(string extension)
    {
        if (TryGetIllustrationFormat(extension, out var format))
            return format;

        throw new ArgumentOutOfRangeException(nameof(extension));
    }

    public static bool TryGetIllustrationFormat(string extension, out IllustrationDownloadFormat format)
    {
        return extension switch
        {
            ".jpg" => Assign(IllustrationDownloadFormat.Jpg, out format),
            ".png" => Assign(IllustrationDownloadFormat.Png, out format),
            ".bmp" => Assign(IllustrationDownloadFormat.Bmp, out format),
            ".webp" => Assign(IllustrationDownloadFormat.WebPLossless, out format),
            FileExtensionMacro.NameConstToken => Assign(IllustrationDownloadFormat.Original, out format),
            _ => Assign(default, out format, false)
        };

        static bool Assign(IllustrationDownloadFormat value, out IllustrationDownloadFormat format, bool result = true)
        {
            format = value;
            return result;
        }
    }

    public static string GetNovelExtension(string? novelDownloadFormat = null)
    {
        var token = GetAvailableNovelDownloadFormatToken(novelDownloadFormat);
        if (token.ExtensionFormatExtension is { } extension)
            return extension;

        return token.BuiltInFormat switch
        {
            NovelDownloadFormat.OriginalTxt => "novel.txt",
            NovelDownloadFormat.Html or NovelDownloadFormat.Md => "\\novel." + token.BuiltInFormat.ToString()!.ToLowerInvariant(),
            _ => throw new ArgumentOutOfRangeException(nameof(novelDownloadFormat))
        };
    }

    public static NovelDownloadFormatToken GetAvailableNovelDownloadFormatToken(string? novelDownloadFormat = null)
    {
        novelDownloadFormat ??= App.AppViewModel.AppSettings.NovelDownloadFormat;
        var token = new NovelDownloadFormatToken(novelDownloadFormat);
        if (token.BuiltInFormat is not null)
            return token;

        if (token.ExtensionFormatExtension is { } extension
            && App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>().GetNovelFormatProvider(extension) is not null)
            return token;

        return NovelDownloadFormatToken.Default;
    }

    public static NovelDownloadFormat GetNovelFormat(string extension)
    {
        if (TryGetNovelFormat(extension, out var format))
            return format;

        throw new ArgumentOutOfRangeException(nameof(extension));
    }

    public static bool TryGetNovelFormat(string extension, out NovelDownloadFormat format)
    {
        return extension switch
        {
            ".txt" => Assign(NovelDownloadFormat.OriginalTxt, out format),
            ".html" => Assign(NovelDownloadFormat.Html, out format),
            ".md" => Assign(NovelDownloadFormat.Md, out format),
            _ => Assign(default, out format, false)
        };

        static bool Assign(NovelDownloadFormat value, out NovelDownloadFormat format, bool result = true)
        {
            format = value;
            return result;
        }
    }

    public static async Task<Image> GetImageFromZipStreamAsync(Stream zipStream, UgoiraMetadata ugoiraMetadata)
    {
        var entryStreams = await Streams.ReadZipAsync(zipStream, true);
        return await entryStreams.UgoiraSaveToImageAsync(ugoiraMetadata.Delays);
    }
    
    public static async Task<(IReadOnlyList<Stream>, IReadOnlyList<int>)> SplitAnimatedImageStreamAsync(Stream animatedStream)
    {
        using var image = await Image.LoadAsync(animatedStream);
        if (image.Frames.Count <= 1)
            throw new ArgumentException("Not animated image");
        var streams = new List<Stream>();
        var msDelays = new List<int>();
        try
        {
            while (image.Frames.Count is not 1)
            {
                using var exportFrame = image.Frames.ExportFrame(0);
                var (memoryStream, ms) = await GetBitmapAndDelayAsync(exportFrame);
                streams.Add(memoryStream);
                msDelays.Add(ms);
            }
            var (lastMemoryStream, lastMs) = await GetBitmapAndDelayAsync(image);
            streams.Add(lastMemoryStream);
            msDelays.Add(lastMs);
            return (streams, msDelays);

            static async Task<(Stream Stream, int Delay)> GetBitmapAndDelayAsync(Image frame)
            {
                var webpFrameMetadata = frame.Frames.RootFrame.Metadata.GetWebpMetadata();
                var delay = webpFrameMetadata.FrameDelay is var d && d < 1 ? 10 : (int) d;

                var memoryStream = Streams.RentStream();
                await frame.SaveAsPngAsync(memoryStream);
                memoryStream.Position = 0;
                return (memoryStream, delay);
            }
        }
        catch (Exception)
        {
            foreach (var stream in streams) 
                await stream.DisposeAsync();
            streams.Clear();
            return ([], []);
        }
    }

    public static string ChangeExtension(string path, string extension)
    {
        return path.Replace(FileExtensionMacro.NameConstToken, extension);
    }

    public static string ReplaceTokenExtensionFromUrl(string path, Uri uri, int setIndex)
    {
        var url = uri.OriginalString;
        return ReplaceTokenExtensionFromUrl(path, url, setIndex);
    }

    public static string ReplaceTokenExtensionFromUrl(string path, string url, int setIndex)
    {
        var index = url.LastIndexOf('.');
        return ReplaceTokenSetIndex(path.Replace(FileExtensionMacro.NameConstToken, url[index..]), setIndex);
    }

    public static async Task<string> ReplaceTempExtensionFromStreamAsync(string path, Stream stream, int setIndex)
    {
        var ext = (await Image.DetectFormatAsync(stream)).FileExtensions.First();
        return ReplaceTokenSetIndex(path.Replace(FileExtensionMacro.NameConstToken, $".{ext}"), setIndex);
    }

    public static string ReplaceTokenExtensionWithTempExtension(string path, int setIndex)
    {
        return ReplaceTokenSetIndex(path.Replace(FileExtensionMacro.NameConstToken, PixevalTempExtension), setIndex);
    }

    public static string ReplaceTokenSetIndex(string path, int setIndex)
    {
        // 替换完<token>之后必须清除其他尖括号
        return path.Replace(PicSetIndexMacro.NameConstToken, setIndex.ToString()).Replace("<", null).Replace(">", null);
    }

    public static string RemoveTokenExtension(string path)
    {
        return path.Replace(FileExtensionMacro.NameConstToken, null).Replace("<", null).Replace(">", null);
    }

    extension(IArtworkInfo artworkInfo)
    {
        public int TryGetSetIndex()
        {
            return artworkInfo is ISingleImage singleImage ? singleImage.SetIndex : -1;
        }
    }
}
