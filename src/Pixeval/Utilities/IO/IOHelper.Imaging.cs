// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Download.Macros;
using Pixeval.Models.Download;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;
using SkiaSharp;

namespace Pixeval.Utilities.IO;

public static partial class IoHelper
{
    public const string PixevalTempExtension = ".pixevaldownloading";

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
    }

    public static UgoiraDownloadFormatToken GetAvailableUgoiraDownloadFormatToken(string? ugoiraDownloadFormat = null)
    {
        ugoiraDownloadFormat ??= App.AppViewModel.AppSettings.UgoiraDownloadFormat;
        var token = new UgoiraDownloadFormatToken(ugoiraDownloadFormat);
        if (token.BuiltInFormat is UgoiraDownloadFormat.Original)
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
            _ => throw new ArgumentOutOfRangeException(nameof(ugoiraDownloadFormat))
        };
    }

    public static IllustrationDownloadFormatToken GetAvailableIllustrationDownloadFormatToken(string? illustrationDownloadFormat = null)
    {
        illustrationDownloadFormat ??= App.AppViewModel.AppSettings.IllustrationDownloadFormat;
        var token = new IllustrationDownloadFormatToken(illustrationDownloadFormat);
        if (token.BuiltInFormat is IllustrationDownloadFormat.Original)
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
            _ => throw new ArgumentOutOfRangeException(nameof(illustrationDownloadFormat))
        };
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

    public static async Task<IReadOnlyDictionary<Stream, int>> SplitAnimatedImageStreamAsync(Stream animatedStream)
    {
        if (animatedStream.CanSeek)
            animatedStream.Position = 0;

        using var codec = SKCodec.Create(animatedStream)
            ?? throw new InvalidOperationException($"Unable to create {nameof(SKCodec)} from the provided stream.");
        var frameCount = Math.Max(codec.FrameCount, 1);
        if (frameCount <= 1)
            throw new ArgumentException("Not animated image");

        var imageInfo = codec.Info;
        var targetInfo = new SKImageInfo(imageInfo.Width, imageInfo.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var frameInfos = codec.FrameInfo;
        var streams = new Dictionary<Stream, int>(frameCount);
        try
        {
            for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                using var bitmap = new SKBitmap(targetInfo);
                var options = new SKCodecOptions(frameIndex);
                var result = codec.GetPixels(targetInfo, bitmap.GetPixels(), options);
                if (result is not SKCodecResult.Success and not SKCodecResult.IncompleteInput)
                    throw new InvalidOperationException($"Failed to decode frame {frameIndex}: {result}.");

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100)
                    ?? throw new InvalidOperationException($"Failed to encode frame {frameIndex} as PNG.");
                var memoryStream = Streams.RentStream();
                data.SaveTo(memoryStream);
                memoryStream.Position = 0;

                var delay = frameInfos.Length > frameIndex && frameInfos[frameIndex].Duration > 0
                    ? frameInfos[frameIndex].Duration
                    : 100;

                streams[memoryStream] = delay;
            }

            return streams;
        }
        catch
        {
            foreach (var stream in streams.Keys) 
                await stream.DisposeAsync();
            throw;
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

    public static Task<string> ReplaceTempExtensionFromStreamAsync(string path, Stream stream, int setIndex)
    {
        var originalPosition = stream.CanSeek ? stream.Position : 0;
        try
        {
            if (stream.CanSeek)
                stream.Position = 0;

            using var codec = SKCodec.Create(stream)
                ?? throw new InvalidOperationException($"Unable to create {nameof(SKCodec)} from the provided stream.");
            var extension = GetEncodedImageExtension(codec.EncodedFormat);
            return Task.FromResult(ReplaceTokenSetIndex(path.Replace(FileExtensionMacro.NameConstToken, extension), setIndex));
        }
        finally
        {
            if (stream.CanSeek)
                stream.Position = originalPosition;
        }
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

    private static string GetEncodedImageExtension(SKEncodedImageFormat encodedFormat) =>
        encodedFormat switch
        {
            SKEncodedImageFormat.Jpeg => ".jpg",
            SKEncodedImageFormat.Png => ".png",
            SKEncodedImageFormat.Gif => ".gif",
            SKEncodedImageFormat.Bmp => ".bmp",
            SKEncodedImageFormat.Webp => ".webp",
            SKEncodedImageFormat.Ico => ".ico",
            SKEncodedImageFormat.Wbmp => ".wbmp",
            SKEncodedImageFormat.Pkm => ".pkm",
            SKEncodedImageFormat.Ktx => ".ktx",
            SKEncodedImageFormat.Astc => ".astc",
            SKEncodedImageFormat.Dng => ".dng",
            SKEncodedImageFormat.Heif => ".heif",
            SKEncodedImageFormat.Avif => ".avif",
            _ => throw new NotSupportedException($"Unsupported image format: {encodedFormat}.")
        };

    extension(IArtworkInfo artworkInfo)
    {
        public int TryGetSetIndex()
        {
            return artworkInfo is ISingleImage singleImage ? singleImage.SetIndex : -1;
        }
    }
}
