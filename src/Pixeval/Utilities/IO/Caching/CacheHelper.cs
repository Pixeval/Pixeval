// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.AppManagement;

namespace Pixeval.Utilities.IO.Caching;

public static class CacheHelper
{
    public static string CachePath { get; } = Path.Combine(AppInfo.CacheFolder, "FileCache");

    private static readonly FileCache _FileCache = new(CachePath);

    /// <summary>
    /// Dispose无效果，可以反复用
    /// </summary>
    public static readonly Lazy<Bitmap> WrappedImageNotAvailable =
        new(() => new UndisposableBitmap(AppInfo.GetImageNotAvailableStream()));

    /// <summary>
    /// Dispose无效果，可以反复用
    /// </summary>
    public static readonly Lazy<IAnimatedBitmap> AnimatedImageNotAvailable =
        new(() => IAnimatedBitmap.Load([WrappedImageNotAvailable.Value], [100]));

    public static void PurgeCache() => _FileCache.Purge();

    public static void EnforceCacheSizeLimit()
    {
        var settings = App.AppViewModel.AppSettings.ApplicationSettings;
        if (!settings.LimitFileCacheSize)
            return;

        _FileCache.EnforceSizeLimit(GetCacheSizeLimitInBytes());
    }

    private static long GetCacheSizeLimitInBytes()
    {
        var sizeInMegabytes =
            Math.Max(1, App.AppViewModel.AppSettings.ApplicationSettings.FileCacheSizeLimitInMegabytes);
        return sizeInMegabytes * 1024L * 1024L;
    }

    /// <summary>
    /// 保证<see cref="Stream.Position"/>为0
    /// </summary>
    public static async ValueTask<IAnimatedBitmap> GetSingleAnimatedImageAsync(
        string platform,
        IAnimatedImageFrame frame,
        IProgress<double>? progress = null,
        CancellationToken token = default)
    {
        var key = frame.SingleImageUri;
        ArgumentNullException.ThrowIfNull(key);
        if (frame.PreferredAnimatedImageType is not SingleAnimatedImageType.SingleZipFile
            and not SingleAnimatedImageType.SingleFile)
            throw new InvalidOperationException(
                $"{nameof(IAnimatedImageFrame.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.SingleZipFile)} or {nameof(SingleAnimatedImageType.SingleFile)}");

        if (frame.PreferredAnimatedImageType is SingleAnimatedImageType.SingleZipFile)
        {
            if (await GetStreamAsync(platform, key.OriginalString, progress, token) is not { } stream)
                return AnimatedImageNotAvailable.Value;
            ArgumentNullException.ThrowIfNull(frame.ZipImageDelays);
            await frame.ZipImageDelays.TryPreloadListAsync(platform);
            var zip = await Streams.ReadZipAsync(stream, true);
            return IAnimatedBitmap.Load(zip, frame.ZipImageDelays, true);
        }

        // SingleAnimatedImageType.SingleFile
        if (await GetSingleImageAsync(platform, key, progress, token) is { } bitmap)
            return bitmap;

        return AnimatedImageNotAvailable.Value;
    }

    /// <summary>
    /// 保证<see cref="Stream.Position"/>为0
    /// </summary>
    public static ValueTask<IAnimatedBitmap> GetSingleImageAsync(
        string platform,
        IImageFrame frame,
        IProgress<double>? progress = null,
        CancellationToken token = default)
        => GetSingleImageAsync(platform, frame.ImageUri, progress, token);

    private static async ValueTask<IAnimatedBitmap> GetSingleImageAsync(
        string platform,
        Uri frameUri,
        IProgress<double>? progress = null,
        CancellationToken token = default)
    {
        try
        {
            var key = frameUri.OriginalString;
            if (TryGetStream(key) is { } stream)
                return IAnimatedBitmap.Load(stream, true);
            var useFileCache = App.AppViewModel.AppSettings.ApplicationSettings.UseFileCache;

            var client = App.AppViewModel.GetRequiredPlatformService<IDownloadHttpClientService>(platform)
                .GetImageDownloadClient();
            if (await client.DownloadMemoryStreamAsync(frameUri, progress, token: token) is
                Result<Stream>.Success(var s))
            {
                if (useFileCache)
                {
                    _ = TryCacheStream(key, s);
                    s.Position = 0;
                }

                return IAnimatedBitmap.Load(s, true);
            }

            return AnimatedImageNotAvailable.Value;
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetSingleImageAsync), e);
        }

        return AnimatedImageNotAvailable.Value;
    }

    public static async Task<IAnimatedBitmap> GetAnimatedImageSeparatedAsync(
        string platform,
        IAnimatedImageFrame frame,
        IProgress<double>? progress = null,
        CancellationToken token = default)
    {
        if (frame.PreferredAnimatedImageType is not SingleAnimatedImageType.MultiFiles)
            throw new InvalidOperationException(
                $"{nameof(IAnimatedImageFrame.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.MultiFiles)}");
        await frame.MultiImageUris!.TryPreloadListAsync(platform);
        var client = App.AppViewModel.AppServiceProvider.GetRequiredKeyedService<IDownloadHttpClientService>(platform)
            .GetImageDownloadClient();
        var count = frame.MultiImageUris!.Count;
        var imageList = new List<BitmapOrStream>(count);
        var delayList = new List<int>(count);
        var ratio = 1d / count;
        var startProgress = 0d;
        foreach (var (uri, msDelay) in frame.MultiImageUris)
        {
            BitmapOrStream stream;
            try
            {
                var key = uri.OriginalString;
                if (TryGetStream(key) is { } s)
                    stream = s;
                else
                {
                    var useFileCache = App.AppViewModel.AppSettings.ApplicationSettings.UseFileCache;
                    var sp = startProgress;
                    if (await client.DownloadMemoryStreamAsync(
                            uri,
                            progress?.Let(t => new Progress<double>(d => t.Report(sp + (ratio * d)))),
                            token: token) is Result<Stream>.Success(var s2))
                    {
                        if (useFileCache)
                        {
                            _ = TryCacheStream(key, s2);
                            s2.Position = 0;
                        }

                        stream = s2;
                    }
                    else
                        stream = WrappedImageNotAvailable.Value;
                }
            }
            catch (Exception e)
            {
                App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                    .LogError(nameof(GetAnimatedImageSeparatedAsync), e);
                stream = WrappedImageNotAvailable.Value;
            }

            imageList.Add(stream);
            delayList.Add(msDelay);
            startProgress += 100 * ratio;
        }

        return IAnimatedBitmap.Load(imageList, delayList, true);
    }

    /// <summary>
    /// 保证<see cref="Stream.Position"/>为0
    /// </summary>
    public static async ValueTask<Stream?> GetImageStreamAsync(
        string platform,
        string key,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetStreamAsync(platform, key, progress, cancellationToken);
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetImageStreamAsync), e);
        }

        return null;
    }

    public static async Task<IAnimatedBitmap> GetAnimatedBitmapAsync(
        string platform,
        string key,
        IProgress<double>? progress = null,
        CancellationToken token = default)
    {
        try
        {
            if (await GetImageStreamAsync(platform, key, progress, token) is { } stream)
                return IAnimatedBitmap.Load(stream, true);
            return AnimatedImageNotAvailable.Value;
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetAnimatedBitmapAsync), e);
        }

        return AnimatedImageNotAvailable.Value;
    }

    public static async ValueTask<Bitmap> GetBitmapAsync(
        string platform,
        string key,
        IProgress<double>? progress = null,
        int? desiredWidth = null,
        CancellationToken token = default)
    {
        try
        {
            return await GetStreamAsync(platform, key, progress, token) is { } stream
                ? await stream.DecodeBitmapImageAsync(true, desiredWidth)
                : WrappedImageNotAvailable.Value;
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetBitmapAsync), e);
        }

        return WrappedImageNotAvailable.Value;
    }

    /// <summary>
    /// 本方法会根据<see cref="ApplicationSettingsGroup.UseFileCache"/>判断是否使用文件缓存
    /// </summary>
    /// <returns><see langword="null"/>表示下载失败</returns>
    private static async ValueTask<Stream?> GetStreamAsync(
        string platform,
        string key,
        IProgress<double>? progress = null,
        CancellationToken token = default)
    {
        if (TryGetStream(key) is { } stream)
            return stream;
        var useFileCache = App.AppViewModel.AppSettings.ApplicationSettings.UseFileCache;

        if (await App.AppViewModel.AppServiceProvider.GetRequiredKeyedService<IDownloadHttpClientService>(platform)
                .GetImageDownloadClient()
                .DownloadMemoryStreamAsync(key, progress, token: token) is Result<Stream>.Success(var s))
        {
            if (useFileCache)
            {
                _ = TryCacheStream(key, s);
                s.Position = 0;
            }

            return s;
        }

        return null;
    }

    /// <summary>
    /// 保证<see cref="Stream.Position"/>为0
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static Stream? TryGetStream(string key)
    {
        try
        {
            return App.AppViewModel.AppSettings.ApplicationSettings.UseFileCache
                ? _FileCache.TryOpen(key)
                : null;
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(TryGetStream), e);
        }

        return null;
    }

    /// <exception cref="InvalidOperationException"/>
    internal static FileCacheWriteResult TryCacheStream(string key, Stream stream)
    {
        if (!App.AppViewModel.AppSettings.ApplicationSettings.UseFileCache)
            throw new InvalidOperationException(
                $"Check {nameof(App.AppViewModel.AppSettings.ApplicationSettings.UseFileCache)} before {nameof(TryCacheStream)}");

        var sizeLimitBytes = App.AppViewModel.AppSettings.ApplicationSettings.LimitFileCacheSize
            ? GetCacheSizeLimitInBytes()
            : (long?) null;

        return _FileCache.TryCache(key, stream, sizeLimitBytes);
    }
}

file class UndisposableBitmap(Stream stream) : Bitmap(stream)
{

    /// <inheritdoc />
    public override void Dispose()
    {
    }

    public void DisposeForce()
    {
        base.Dispose();
    }
}
