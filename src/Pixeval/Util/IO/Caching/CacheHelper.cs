// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Net;
using Pixeval.Logging;
using Pixeval.Utilities;
using IllustrationCacheTable = Pixeval.Caching.CacheTable<
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheKey,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheHeader,
    Pixeval.Util.IO.Caching.PixevalIllustrationCacheProtocol>;

namespace Pixeval.Util.IO.Caching;

public static class CacheHelper
{
    public static readonly Lazy<Task<ImageSource>> ImageNotAvailableTask = new(() => AppInfo.GetImageNotAvailableStream().DecodeBitmapImageAsync(true));

    public const string CacheFolderName = "cache";

    public static void PurgeCache()
    {
        Directory.Delete(AppKnownFolders.Cache.CombinePath(CacheFolderName), true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="key"></param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Stream> GetStreamFromCacheAsync(
        this IllustrationCacheTable cache,
        string key,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await cache.GetFromFileCacheAsync(key, progress, cancellationToken) ??
                   AppInfo.GetImageNotAvailableStream();
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetSourceFromCacheAsync), e);
        }
        return AppInfo.GetImageNotAvailableStream();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="key"></param>
    /// <param name="progress"></param>
    /// <param name="desiredWidth"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<ImageSource> GetSourceFromCacheAsync(
        this IllustrationCacheTable cache,
        string key,
        IProgress<double>? progress = null,
        int? desiredWidth = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await cache.GetSourceFromCacheInnerAsync(key, progress, desiredWidth, cancellationToken) ??
                   await ImageNotAvailableTask.Value;
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

    /// <returns></returns>
    private static async ValueTask<ImageSource?> GetSourceFromCacheInnerAsync(
        this IllustrationCacheTable cache,
        string key,
        IProgress<double>? progress = null,
        int? desiredWidth = null,
        CancellationToken cancellationToken = default)
    {
        var stream = await cache.GetFromFileCacheAsync(key, progress, cancellationToken);
        return stream is null
            ? null
            : await stream.DecodeBitmapImageAsync(true, desiredWidth);
    }

    /// <summary>
    /// 本方法会根据<see cref="AppSettings.UseFileCache"/>判断是否使用文件缓存
    /// </summary>
    /// <returns><see langword="null"/>表示下载失败</returns>
    private static async ValueTask<Stream?> GetFromFileCacheAsync(
        this IllustrationCacheTable cacheTable,
        string key,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var useFileCache = App.AppViewModel.AppSettings.UseFileCache;
        if (useFileCache && cacheTable.TryReadCache(new PixevalIllustrationCacheKey(key), out Stream stream))
            return stream;

        if (await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi)
                .DownloadMemoryStreamAsync(key, progress, cancellationToken: cancellationToken) is Result<Stream>.Success(var s))
        {
            if (useFileCache)
            {
                var result = cacheTable.TryCache(new PixevalIllustrationCacheKey(key, (int) s.Length), s);
                s.Seek(0, SeekOrigin.Begin);
            }
            return s;
        }

        return null;
    }
}
