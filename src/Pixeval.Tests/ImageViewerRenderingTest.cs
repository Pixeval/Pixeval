using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Imouto.BooruParser;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.Models.Extensions;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Viewers;
using SmoothScroll.Avalonia.Controls;
using Size = Avalonia.Size;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ImageViewerRenderingTest
{
    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(true, false, false)]
    [DataRow(true, true, false)]
    [DataRow(true, false, true)]
    public async Task ViewerFitsPagesAndPreservesSizeAcrossSourceUpdates(
        bool sourceReadyBeforeAttachment, bool replaceDuringFit, bool detachDuringReplacement)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(ViewerTestApplication));
        await session.Dispatch(() =>
        {
            I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
            // Supply only the extension registry; do not initialize the real application's network/database services.
            using var extensions = new ExtensionService(null!, [], new HashSet<string>(), false);
            using var services = new ServiceCollection().AddSingleton(extensions).BuildServiceProvider();
            var appModel = (AppViewModel) RuntimeHelpers.GetUninitializedObject(typeof(AppViewModel));
            typeof(AppViewModel).GetProperty(nameof(AppViewModel.AppServiceProvider))!.SetValue(appModel, services);
            var appProperty = typeof(App).GetProperty(nameof(App.AppViewModel), BindingFlags.Public | BindingFlags.Static)!;
            var previous = appProperty.GetValue(null);
            appProperty.SetValue(null, appModel);
            var window = new Window { Width = 800, Height = 600 };
            try
            {
                window.Show();
                foreach (var size in new Size[] { new(640, 320), new(2000, 1000), new(500, 1000) })
                {
                    using var model = new SingleViewerViewModel("test", new Post(
                        new("1", "test", PlatformType.Danbooru), "https://example.com/test.jpg", null, null,
                        ExistState.Exist, DateTimeOffset.UtcNow, new("1", "test", PlatformType.Danbooru),
                        null, new(100, 100), 0, SafeRating.General, [], null), 0, (_, _) => Task.CompletedTask);
                    using var thumbnail = CreateSource(size);
                    if (sourceReadyBeforeAttachment)
                    {
                        typeof(SingleViewerViewModel).GetProperty(nameof(SingleViewerViewModel.LoadSuccessfully))!
                            .SetValue(model, true);
                        typeof(SingleViewerViewModel).GetProperty(nameof(SingleViewerViewModel.CachedPreviewSource))!
                            .SetValue(model, thumbnail);
                    }
                    var viewer = new SingleImageViewer { DataContext = model };
                    window.Content = viewer;
                    viewer.Measure(new(800, 600));
                    var displayedSize = size;
                    using var earlyReplacement = CreateSource(size * SingleImageViewer.GetPixelScale(new(800, 600), size));
                    var replacedDuringFit = false;
                    // Retarget to 100% while the first fit is still in flight. Equality alone must not finish the new target.
                    if (replaceDuringFit)
                        viewer.ViewerScrollView!.ZoomStarting += (_, _) =>
                        {
                            if (replacedDuringFit)
                                return;
                            replacedDuringFit = true;
                            displayedSize = earlyReplacement.Size;
                            model.DisplaySource.SetFallback(earlyReplacement);
                        };
                    if (!sourceReadyBeforeAttachment)
                    {
                        using var empty = window.CaptureRenderedFrame();
                        model.DisplaySource.SetFallback(thumbnail);
                    }
                    Dispatcher.UIThread.RunJobs();
                    AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
                    // A compositor zoom may take another frame. The un-fitted image must not flash at 100%.
                    Assert.IsTrue(viewer.ImageViewer!.Opacity is 0
                        || Math.Abs(viewer.ViewerScrollView!.ZoomFactor
                            - SingleImageViewer.GetPixelScale(new(800, 600), displayedSize)) < 0.000001);
                    using var rendered = window.CaptureRenderedFrame();
                    Assert.IsNotNull(rendered);
                    Assert.AreEqual(1d, viewer.ImageViewer.Opacity);
                    var zoom = SingleImageViewer.GetPixelScale(new(800, 600), displayedSize);
                    Assert.AreEqual(zoom, viewer.ViewerScrollView!.ZoomFactor, 0.000001);
                    Assert.AreEqual(zoom, model.ZoomFactor, 0.000001);
                    var before = GetRedBounds(rendered);
                    Assert.AreEqual((int) (displayedSize.Width * zoom), before.Width);
                    Assert.AreEqual((int) (displayedSize.Height * zoom), before.Height);

                    using var fullSize = CreateSource(displayedSize * 2);
                    model.DisplaySource.UpdateAsync(_ => Task.FromResult<IAnimatedBitmap?>(fullSize)).GetAwaiter().GetResult();
                    if (detachDuringReplacement)
                    {
                        Dispatcher.UIThread.RunJobs();
                        window.Content = null;
                        Dispatcher.UIThread.RunJobs();
                        window.Content = viewer;
                        for (var frameIndex = 0; frameIndex < 10; frameIndex++)
                        {
                            using var settling = window.CaptureRenderedFrame();
                            if (Math.Abs(viewer.ViewerScrollView.ZoomFactor - zoom / 2) < 0.000001)
                                break;
                        }
                    }
                    using var replaced = window.CaptureRenderedFrame();
                    Assert.IsNotNull(replaced);
                    Assert.AreEqual(zoom / 2, viewer.ViewerScrollView.ZoomFactor, 0.000001);
                    Assert.AreEqual(before, GetRedBounds(replaced));

                    using var original = CreateSource(displayedSize * 3);
                    model.DisplaySource.UpdateAsync(_ => Task.FromResult<IAnimatedBitmap?>(original)).GetAwaiter().GetResult();
                    using var originalFrame = window.CaptureRenderedFrame();
                    Assert.IsNotNull(originalFrame);
                    Assert.AreEqual(zoom / 3, viewer.ViewerScrollView.ZoomFactor, 0.000001);
                    Assert.AreEqual(before, GetRedBounds(originalFrame));
                    window.Content = null;
                }
            }
            finally
            {
                window.Close();
                appProperty.SetValue(null, previous);
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task ThumbnailIsFittedInTheFirstRenderedFrame()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(ViewerTestApplication));
        await session.Dispatch(() =>
        {
            using var source = CreateSource(new(640, 320));
            var image = CreateImage(source);
            var scrollView = CreateScrollView(image);
            scrollView.ZoomFactor = SingleImageViewer.GetPixelScale(new(800, 600), source.Size);
            var window = new Window { Width = 800, Height = 600, Content = scrollView };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
                using var rendered = window.GetLastRenderedFrame();
                Assert.IsNotNull(rendered);
                Assert.AreEqual(1.25, scrollView.ZoomFactor, 0.000001);
                Assert.AreEqual(new PixelRect(0, 100, 800, 400), GetRedBounds(rendered));
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task EachNewPageUsesItsOwnFitInsteadOfDefaultZoom()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(ViewerTestApplication));
        await session.Dispatch(() =>
        {
            var window = new Window { Width = 800, Height = 600 };
            try
            {
                window.Show();
                foreach (var size in new Size[] { new(2000, 1000), new(500, 1000), new(1000, 500) })
                {
                    using var source = CreateSource(size);
                    var scrollView = CreateScrollView(CreateImage(source));
                    var zoom = SingleImageViewer.GetPixelScale(new(800, 600), size);
                    scrollView.ZoomFactor = zoom;
                    window.Content = scrollView;
                    using var rendered = window.CaptureRenderedFrame();
                    Assert.IsNotNull(rendered);
                    Assert.AreEqual(zoom, scrollView.ZoomFactor, 0.000001);
                    var expected = size * zoom;
                    var bounds = GetRedBounds(rendered);
                    Assert.AreEqual((int) expected.Width, bounds.Width);
                    Assert.AreEqual((int) expected.Height, bounds.Height);
                    window.Content = null;
                }
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    private static AnimatedImage CreateImage(IAnimatedBitmap source) => new()
    {
        Source = source,
        Stretch = Stretch.Uniform,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };

    private static ScrollView CreateScrollView(AnimatedImage image) => new()
    {
        Content = image,
        IsZoomEnabled = true,
        IsHorizontalMeasureInfinite = true,
        IsVerticalMeasureInfinite = true,
        HorizontalContentAlignment = HorizontalAlignment.Stretch,
        VerticalContentAlignment = VerticalAlignment.Stretch
    };

    private static IAnimatedBitmap CreateSource(Size size)
    {
        var bitmap = new WriteableBitmap(new((int) size.Width, (int) size.Height), new(96, 96),
            PixelFormat.Bgra8888, AlphaFormat.Premul);
        using var pixels = bitmap.Lock();
        var data = new byte[pixels.RowBytes * pixels.Size.Height];
        for (var y = 0; y < pixels.Size.Height; y++)
            for (var x = 0; x < pixels.Size.Width; x++)
            {
                data[y * pixels.RowBytes + x * 4 + 2] = 255;
                data[y * pixels.RowBytes + x * 4 + 3] = 255;
            }
        Marshal.Copy(data, 0, pixels.Address, data.Length);
        return IAnimatedBitmap.Load([bitmap], [0]);
    }

    private static PixelRect GetRedBounds(WriteableBitmap bitmap)
    {
        var size = bitmap.PixelSize;
        var stride = size.Width * 4;
        var data = new byte[stride * size.Height];
        var address = Marshal.AllocHGlobal(data.Length);
        try
        {
            bitmap.CopyPixels(new(size), address, data.Length, stride);
            Marshal.Copy(address, data, 0, data.Length);
        }
        finally
        {
            Marshal.FreeHGlobal(address);
        }
        var left = size.Width;
        var top = size.Height;
        var right = -1;
        var bottom = -1;
        for (var y = 0; y < size.Height; y++)
            for (var x = 0; x < size.Width; x++)
            {
                var index = y * stride + x * 4;
                if (data[index + 1] > 60 || (data[index] < 200 && data[index + 2] < 200))
                    continue;
                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        Assert.IsTrue(right >= left && bottom >= top, "The image was not rendered.");
        return new(left, top, right - left + 1, bottom - top + 1);
    }

    public sealed class ViewerTestApplication : Application
    {
        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
            Styles.Add(new ScrollViewDefaultTheme());
            Resources["ViewerScrollViewTheme"] = new Avalonia.Styling.ControlTheme(typeof(ScrollView))
            {
                BasedOn = (Avalonia.Styling.ControlTheme) this.FindResource(typeof(ScrollView))!,
                Setters = { new Avalonia.Styling.Setter(ScrollView.IsZoomEnabledProperty, true) }
            };
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<ViewerTestApplication>()
            .UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }
}
