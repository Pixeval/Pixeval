// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Net;
using Pixeval.Logging;
using Pixeval.Util.IO.Caching;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Util.IO;

public static class CacheHelper
{
    private static readonly Dictionary<string, Task<ImageSource?>> _Tasks = [];
    private static readonly SemaphoreSlim _Mutex = new(1, 1);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="memoryCache"></param>
    /// <param name="key"></param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Stream> GetStreamFromMemoryCacheAsync(
        this MemoryCache memoryCache,
        string key,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fileCache = App.AppViewModel.AppServiceProvider.GetRequiredService<FileCache>();
            return await fileCache.GetFromFileCacheAsync(key, progress, cancellationToken) ??
                   AppInfo.GetImageNotAvailableStream();
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetSourceFromMemoryCacheAsync), e);
        }
        return AppInfo.GetImageNotAvailableStream();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="memoryCache"></param>
    /// <param name="key"></param>
    /// <param name="progress"></param>
    /// <param name="desiredWidth"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<ImageSource> GetSourceFromMemoryCacheAsync(
        this MemoryCache memoryCache,
        string key,
        IProgress<double>? progress = null,
        int? desiredWidth = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetSourceFromMemoryCacheInnerAsync(memoryCache, key, progress, desiredWidth, cancellationToken) ??
                   memoryCache.ImageNotAvailable;
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception e)
        {
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(GetSourceFromMemoryCacheAsync), e);
        }

        return memoryCache.ImageNotAvailable;
    }

    /// <summary>
    /// 会先尝试读取<see cref="Stream"/>缓存
    /// </summary>
    /// <param name="memoryCache"></param>
    /// <param name="key"></param>
    /// <param name="progress"></param>
    /// <param name="desiredWidth"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async ValueTask<ImageSource?> GetSourceFromMemoryCacheInnerAsync(
        this MemoryCache memoryCache,
        string key,
        IProgress<double>? progress = null,
        int? desiredWidth = null,
        CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGet(key, out var result))
            return result;

        Task<ImageSource?>? imageSourcesTask;
        await _Mutex.WaitAsync(cancellationToken);
        try
        {
            if (!_Tasks.TryGetValue(key, out imageSourcesTask))
                imageSourcesTask = _Tasks[key] = MemoryCacheTask();
        }
        finally
        {
            _Mutex.Release();
        }

        // TODO: 能否让Task和CancellationToken任一完成就返回？
        var bitmapImages = await imageSourcesTask.To<Task<ImageSource?>>();

        if (bitmapImages is not null)
        {
            await _Mutex.WaitAsync(cancellationToken);
            try
            {
                memoryCache.CacheOrIncrease(key, bitmapImages);
            }
            finally
            {
                _Mutex.Release();
            }
        }

        return bitmapImages;

        async Task<ImageSource?> MemoryCacheTask()
        {
            var fileCache = App.AppViewModel.AppServiceProvider.GetRequiredService<FileCache>();
            // 由于所有线程等待同一个Task，所以这里传入None防止第一个任务取消后，直接将所有任务都被抛出异常
            var stream = await fileCache.GetFromFileCacheAsync(key, progress, CancellationToken.None);
            return stream is null
                ? null
                : await stream.DecodeBitmapImageAsync(true, desiredWidth);
        }
    }

    /// <summary>
    /// 本方法会根据<see cref="AppSettings.UseFileCache"/>判断是否使用文件缓存
    /// </summary>
    /// <returns><see langword="null"/>表示下载失败</returns>
    private static async ValueTask<Stream?> GetFromFileCacheAsync(
        this FileCache fileCache,
        string key,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var useFileCache = App.AppViewModel.AppSettings.UseFileCache;
        if (useFileCache && await fileCache.TryGetAsync<Stream>(key) is { } stream)
            return stream;

        if (await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi)
                .DownloadMemoryStreamAsync(key, progress, cancellationToken: cancellationToken) is
            Result<Stream>.Success(var s))
        {
            if (useFileCache)
            {
                await fileCache.AddAsync(key, s, TimeSpan.FromDays(1));
                s.Position = 0;
            }
            return s;
        }

        return null;
    }
}
