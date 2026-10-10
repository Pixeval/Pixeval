using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.AppManagement.Settings;
using Pixeval.Controls;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;
using Pixeval.Utilities.IO.Caching;
using SkiaSharp;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class SizedImageDecodeTest
{
    [TestMethod]
    [DataRow(540, 540, 250, 250, 250, 250)]
    [DataRow(600, 300, 200, 200, 400, 200)]
    [DataRow(120, 80, 400, 400, 120, 80)]
    public async Task DecodeFillsTargetWithoutUpscaling(int width, int height, int targetWidth, int targetHeight, int expectedWidth, int expectedHeight)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(ImageViewerRenderingTest.ViewerTestApplication));
        _ = await session.Dispatch<bool>(async () =>
        {
            using var stream = new MemoryStream(CreateImage(width, height));
            using var bitmap = await stream.DecodeBitmapImageAsync(false, desiredSize: new(targetWidth, targetHeight));
            Assert.AreEqual(new PixelSize(expectedWidth, expectedHeight), bitmap.PixelSize);
            using var fullSize = await DecodeFullSizeAsync(width, height);
            Assert.AreEqual(new PixelSize(width, height), fullSize.PixelSize);
            return true;
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task LayoutAndDpiDriveDecodeAndResizeRetainsDisplayedBitmap()
    {
        await RunAsync(async (window, image, target, handler) =>
        {
            Source.SetCache(image, "https://example.com/first.png");
            target.Measure(new(250, 250));
            Dispatcher.UIThread.RunJobs();
            Assert.AreEqual(0, handler.Requests, "Measurement alone must not load an off-screen card.");

            window.Show();
            await WaitUntilAsync(() => image.Source is Bitmap { PixelSize.Width: 256 }, window);
            Assert.AreEqual(1, handler.Requests);
            var initial = image.Source;
            target.Width = target.Height = 240;
            window.UpdateLayout();
            await Task.Delay(200);
            Dispatcher.UIThread.RunJobs();
            Assert.AreSame(initial, image.Source);
            Assert.AreEqual(1, handler.Requests, "Changes in the same decode bucket must reuse the bitmap.");

            var replacement = handler.BlockNext();
            window.SetRenderScaling(1.5);
            await WaitUntilAsync(() => handler.Requests == 2, window);
            Assert.AreSame(initial, image.Source, "Keep the old bitmap while its replacement is loading.");
            Assert.IsTrue(Source.GetLoaded(image));
            replacement.SetResult();
            await WaitUntilAsync(() => image.Source is Bitmap { PixelSize.Width: 384 }, window);

            var large = image.Source;
            var shrinking = handler.BlockNext();
            target.Width = target.Height = 120;
            window.UpdateLayout();
            await WaitUntilAsync(() => handler.Requests == 3, window);
            target.Width = target.Height = 100;
            window.UpdateLayout();
            await Task.Delay(200);
            Dispatcher.UIThread.RunJobs();
            Assert.AreEqual(3, handler.Requests, "Resize must not duplicate the unfinished download.");
            Assert.AreSame(large, image.Source);
            shrinking.SetResult();
            await WaitUntilAsync(() => image.Source is Bitmap { PixelSize.Width: 160 }, window);
            using var copied = await CacheHelper.GetBitmapAsync(IPlatformInfo.Pixiv, Source.GetCache(image)!);
            Assert.AreEqual(new PixelSize(540, 540), copied.PixelSize, "The clipboard path still obtains the full thumbnail.");

            ViewModelDisposal.Dispose(window);
            Assert.IsNull(image.Source);
            var requests = handler.Requests;
            target.Width = 300;
            window.UpdateLayout();
            await Task.Delay(200);
            Dispatcher.UIThread.RunJobs();
            Assert.AreEqual(requests, handler.Requests);
        });
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ReturningToCanceledSizeRetriesUntilTargetIsLoaded(bool hasDisplayedBitmap)
    {
        await RunAsync(async (window, image, target, handler) =>
        {
            if (hasDisplayedBitmap)
            {
                Source.SetCache(image, "https://example.com/first.png");
                window.Show();
                await WaitUntilAsync(() => image.Source is Bitmap { PixelSize.Width: 256 }, window);
            }

            var displayed = image.Source;
            var requests = handler.Requests;
            var download = handler.BlockNext();
            target.Width = target.Height = 350;
            Source.SetCache(image, "https://example.com/first.png");
            window.Show();
            await WaitUntilAsync(() => handler.Requests == requests + 1, window);

            // Let each resize pass the debounce while the original download is blocked.
            target.Width = target.Height = 450;
            window.UpdateLayout();
            await Task.Delay(200);
            Dispatcher.UIThread.RunJobs();
            target.Width = target.Height = 350;
            window.UpdateLayout();
            await Task.Delay(200);
            Dispatcher.UIThread.RunJobs();
            Assert.AreEqual(requests + 1, handler.Requests);
            Assert.AreSame(displayed, image.Source);

            download.SetResult();
            await WaitUntilAsync(() => image.Source is Bitmap { PixelSize.Width: 352 }, window);
            Assert.IsTrue(Source.GetLoaded(image));
            Assert.AreEqual(requests + 2, handler.Requests, "The canceled size must be requested again.");
        });
    }

    [TestMethod]
    public async Task RebindingDoesNotWaitForOldDownloadOrInstallItsResult()
    {
        await RunAsync(async (window, image, target, handler) =>
        {
            var oldDownload = handler.BlockNext();
            Source.SetCache(image, "https://example.com/first.png");
            window.Show();
            await WaitUntilAsync(() => handler.Requests == 1, window);
            Source.SetCache(image, null);
            Source.SetCache(image, "https://example.com/second.png");
            await WaitUntilAsync(() => image.Source is Bitmap, window);
            Assert.AreEqual(2, handler.Requests);

            // Return to the same URL before its first request finishes. URL equality alone
            // must not let the abandoned operation overwrite the newly loaded bitmap.
            Source.SetCache(image, "https://example.com/first.png");
            await WaitUntilAsync(() => image.Source is Bitmap && handler.Requests == 3, window);
            var current = image.Source;
            oldDownload.SetResult();
            await WaitUntilAsync(() => handler.Completed == 3, window);
            await Task.Delay(100);
            Dispatcher.UIThread.RunJobs();
            Assert.AreSame(current, image.Source);

            window.Content = null;
            Assert.IsNull(image.Source);
            window.Content = target;
            await WaitUntilAsync(() => image.Source is Bitmap, window);
        });
    }

    private static async Task RunAsync(Func<Window, Image, Border, ImageHandler, Task> test)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(ImageViewerRenderingTest.ViewerTestApplication));
        _ = await session.Dispatch<bool>(async () =>
        {
            using var handler = new ImageHandler(CreateImage(540, 540));
            using var client = new HttpClient(handler);
            using var services = new ServiceCollection()
                .AddKeyedSingleton<IDownloadHttpClientService>(IPlatformInfo.Pixiv, new ImageClient(client))
                .AddSingleton(new FileLogger(AppContext.BaseDirectory))
                .BuildServiceProvider();
            var model = (AppViewModel) RuntimeHelpers.GetUninitializedObject(typeof(AppViewModel));
            typeof(AppViewModel).GetProperty(nameof(AppViewModel.AppServiceProvider))!.SetValue(model, services);
            typeof(AppViewModel).GetField("<AppSettings>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(model, new AppSettings { ApplicationSettings = new() { UseFileCache = false } });
            var appProperty = typeof(App).GetProperty(nameof(App.AppViewModel), BindingFlags.Public | BindingFlags.Static)!;
            var previous = appProperty.GetValue(null);
            appProperty.SetValue(null, model);
            var image = new Image { Stretch = Stretch.UniformToFill };
            var target = new Border
            {
                Width = 250,
                Height = 250,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Child = image
            };
            var window = new Window { Width = 800, Height = 600, Content = target };
            window.AddHandler(ViewModelDisposal.ViewModelDisposalEvent, (_, args) =>
            {
                ViewModelDisposal.Register(window, args.Disposable);
                args.Handled = true;
            });
            Source.SetDecodeTarget(image, target);
            try
            {
                await test(window, image, target, handler);
                return true;
            }
            finally
            {
                ViewModelDisposal.Dispose(window);
                window.Close();
                appProperty.SetValue(null, previous);
            }
        }, CancellationToken.None);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, Window window)
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            if (condition())
                return;
            await Task.Delay(10);
        }
        Assert.Fail("Image loading did not reach the expected state.");
    }

    private static byte[] CreateImage(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.CornflowerBlue);
        using var encoded = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    private static async Task<Bitmap> DecodeFullSizeAsync(int width, int height)
    {
        await using var stream = new MemoryStream(CreateImage(width, height));
        return await stream.DecodeBitmapImageAsync(false);
    }

    private sealed class ImageClient(HttpClient client) : IDownloadHttpClientService
    {
        public string Platform => IPlatformInfo.Pixiv;
        public HttpClient GetApiClient() => client;
        public HttpClient GetImageDownloadClient() => client;
    }

    private sealed class ImageHandler(byte[] data) : HttpMessageHandler
    {
        private TaskCompletionSource? _gate;
        private int _requests;
        private int _completed;
        public int Requests => Volatile.Read(ref _requests);
        public int Completed => Volatile.Read(ref _completed);

        public TaskCompletionSource BlockNext() => _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Interlocked.Increment(ref _requests);
            if (Interlocked.Exchange(ref _gate, null) is { } gate)
                await gate.Task.WaitAsync(token);
            Interlocked.Increment(ref _completed);
            return new(HttpStatusCode.OK) { Content = new ByteArrayContent(data) };
        }
    }
}
