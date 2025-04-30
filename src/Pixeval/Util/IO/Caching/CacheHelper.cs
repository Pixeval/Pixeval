// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.Caching;
using Pixeval.Utilities;
using IllustrationCacheTable = Pixeval.Caching.CacheTable<
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheKey,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheHeader,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheProtocol>;
using WinUI3Utilities;
using Misaki;
using System.Collections.Generic;

namespace Pixeval.Util.IO.Caching;

public static class CacheHelper
{
    public static readonly Lazy<Task<ImageSource>> ImageNotAvailableTask = new(() => AppInfo.GetImageNotAvailableStream().DecodeBitmapImageAsync(true));

    private static IllustrationCacheTable CacheTable { get; } =
        App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationCacheTable>();

    public const string CacheFolderName = "cache";

    public static void PurgeCache()
    {
        Directory.Delete(AppKnownFolders.Cache.CombinePath(CacheFolderName), true);
    }

    /// <summary>
    /// 保证<see cref="Stream.Position"/>为0
    /// </summary>
    public static ValueTask<Stream> GetSingleImageAsync(
        IPlatformInfo image,
        IAnimatedImageFrame frame,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (frame.PreferredAnimatedImageType is not SingleAnimatedImageType.SingleZipFile and not  SingleAnimatedImageType.SingleFile)
            ThrowHelper.InvalidOperation($"{nameof(IAnimatedImageFrame.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.SingleZipFile)} or {nameof(SingleAnimatedImageType.SingleFile)}");
        return GetSingleImageAsync(image, frame.SingleImageUri!, progress, cancellationToken);
    }

    /// <summary>
    /// 保证<see cref="Stream.Position"/>为0
    /// </summary>
    public static ValueTask<Stream> GetSingleImageAsync(
        IPlatformInfo image,
        IImageFrame frame,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
        => GetSingleImageAsync(image, frame.ImageUri, progress, cancellationToken);

    private static async ValueTask<Stream> GetSingleImageAsync(
        IPlatformInfo image,
        Uri frameUri,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = frameUri.OriginalString;
            if (TryGetStreamFromCache(key) is { } stream)
                return stream;
            var useFileCache = App.AppViewModel.AppSettings.UseFileCache;

            var client = App.AppViewModel.GetRequiredPlatformService<IDownloadHttpClientService>(image.Platform)
                .GetImageDownloadClient();
            if (await client.DownloadMemoryStreamAsync(frameUri, progress, cancellationToken: cancellationToken) is Result<Stream>.Success(var s))
            {
                if (useFileCache)
                {
                    _ = TryCacheStream(key, s);
                    s.Position = 0;
                }
                return s;
            }

            return AppInfo.GetImageNotAvailableStream();
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetStreamFromCacheAsync), e);
        }
        return AppInfo.GetImageNotAvailableStream();
    }

    public static async Task<IReadOnlyList<(Stream Image, int MsDelay)>> GetAnimatedImageSeparatedAsync(
        IPlatformInfo image,
        IAnimatedImageFrame frame,
        IProgress<double>? progress = null,
        CancellationToken token = default)
    {
        if (frame.PreferredAnimatedImageType is not SingleAnimatedImageType.MultiFiles)
            ThrowHelper.InvalidOperation($"{nameof(IAnimatedImageFrame.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.MultiFiles)}");
        await frame.MultiImageUris!.TryPreloadListAsync(image);
        var client = App.AppViewModel.AppServiceProvider.GetRequiredKeyedService<IDownloadHttpClientService>(image.Platform)
            .GetImageDownloadClient();
        var count = frame.MultiImageUris!.Count;
        var list = new List<(Stream Image, int MsDelay)>(count);
        var ratio = 1d / count;
        var startProgress = 0d;
        foreach (var (uri, msDelay) in frame.MultiImageUris)
        {
            Stream stream;
            try
            {
                var key = uri.OriginalString;
                if (TryGetStreamFromCache(key) is { } s)
                    stream = s;
                else
                {
                    var useFileCache = App.AppViewModel.AppSettings.UseFileCache;
                    var sp = startProgress;
                    if (await client.DownloadMemoryStreamAsync(
                            uri,
                            progress?.Let(t => new Progress<double>(d => t.Report(sp + ratio * d))),
                            cancellationToken: token) is Result<Stream>.Success(var s2))
                    {
                        if (useFileCache)
                        {
                            _ = TryCacheStream(key, s2);
                            s2.Position = 0;
                        }

                        stream = s2;
                    }
                    else
                        stream = AppInfo.GetImageNotAvailableStream();
                }
            }
            catch (Exception e)
            {
                App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                    .LogError(nameof(GetStreamFromCacheAsync), e);
                stream = AppInfo.GetImageNotAvailableStream();
            }
            list.Add((stream, msDelay));
            startProgress += 100 * ratio;
        }

        return list;
    }

    /// <summary>
    /// 保证<see cref="Stream.Position"/>为0
    /// </summary>
    public static async ValueTask<Stream> GetStreamFromCacheAsync(
        string key,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetFromFileCacheAsync(key, progress, cancellationToken) ??
                   AppInfo.GetImageNotAvailableStream();
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetStreamFromCacheAsync), e);
        }
        return AppInfo.GetImageNotAvailableStream();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="progress"></param>
    /// <param name="desiredWidth"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<ImageSource> GetSourceFromCacheAsync(
        string key,
        IProgress<double>? progress = null,
        int? desiredWidth = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetFromFileCacheAsync(key, progress, cancellationToken) is { } stream
                ? await stream.DecodeBitmapImageAsync(true, desiredWidth)
                : await ImageNotAvailableTask.Value;
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetSourceFromCacheAsync), e);
        }

        return await ImageNotAvailableTask.Value;
    }

    /// <summary>
    /// 本方法会根据<see cref="AppSettings.UseFileCache"/>判断是否使用文件缓存
    /// </summary>
    /// <returns><see langword="null"/>表示下载失败</returns>
    private static async ValueTask<Stream?> GetFromFileCacheAsync(
        string key,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetStreamFromCache(key) is { } stream)
            return stream;
        var useFileCache = App.AppViewModel.AppSettings.UseFileCache;

        if (await App.AppViewModel.MakoClient.GetImageDownloadClient()
                .DownloadMemoryStreamAsync(key, progress, cancellationToken: cancellationToken) is Result<Stream>.Success(var s))
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
    public static Stream? TryGetStreamFromCache(string key)
    {
        try
        {
            if (App.AppViewModel.AppSettings.UseFileCache && CacheTable.TryReadCache(new PixevalIllustrationCacheKey(key), out Stream stream))
                return stream;
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(IllustrationCacheTable.TryReadCache), e);
        }
        return null;
    }

    /// <exception cref="InvalidOperationException"/>
    public static AllocatorState TryCacheStream(string key, Stream stream)
    {
        if (!App.AppViewModel.AppSettings.UseFileCache)
            ThrowHelper.InvalidOperation("check UseFileCache before TryCacheStream");
        return CacheTable.TryCache(new(key, (int) stream.Length), stream);
    }
}
